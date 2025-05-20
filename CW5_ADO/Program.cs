using CW5_ADO.models;
using CW5_ADO.models.DTO;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

app.MapGet("/api/trips", async () =>
{
    var trips = new List<TripWithDetailsDTO>();

    try
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;

                // Get trips
                command.CommandText = @"
                    SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople
                    FROM Trip t";

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        trips.Add(new TripWithDetailsDTO
                        {
                            IdTrip = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateFrom = reader.GetDateTime(3),
                            DateTo = reader.GetDateTime(4),
                            MaxPeople = reader.GetInt32(5),
                            Countries = new List<CountrySummaryDTO>()
                        });
                    }
                }

                // get countries for each trip
                foreach (var trip in trips)
                {
                    command.CommandText = @"
                        SELECT c.IdCountry, c.Name
                        FROM Country c
                        JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry
                        WHERE ct.IdTrip = @IdTrip";

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@IdTrip", trip.IdTrip);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            trip.Countries.Add(new CountrySummaryDTO
                            {
                                IdCountry = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }
        }

        return Results.Ok(trips);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Internal server error: {ex.Message}", statusCode: 500);
    }
});

app.MapGet("/api/clients/{id}/trips", async (int id) =>
{
    try
    {
        // check if client exists
        bool clientExists = false;
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var result = await CheckIfClientExists(connection, id);
            clientExists = result != null;
        }

        if (!clientExists)
        {
            return Results.NotFound($"Client with ID {id} not found");
        }

        var trips = new List<ClientTripDTO>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                
                // Get client trips
                command.CommandText = @"
                    SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                           ct.RegisteredAt, ct.PaymentDate
                    FROM Trip t
                    JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
                    WHERE ct.IdClient = @IdClient";
                command.Parameters.AddWithValue("@IdClient", id);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        trips.Add(new ClientTripDTO
                        {
                            IdTrip = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateFrom = reader.GetDateTime(3),
                            DateTo = reader.GetDateTime(4),
                            MaxPeople = reader.GetInt32(5),
                            RegisteredAt = reader.GetInt32(6),
                            PaymentDate = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                            Countries = new List<CountrySummaryDTO>()
                        });
                    }
                }

                // get countries for each trip
                foreach (var trip in trips)
                {
                    command.CommandText = @"
                        SELECT c.IdCountry, c.Name
                        FROM Country c
                        JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry
                        WHERE ct.IdTrip = @IdTrip";

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@IdTrip", trip.IdTrip);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            trip.Countries.Add(new CountrySummaryDTO
                            {
                                IdCountry = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }
        }

        return Results.Ok(trips);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Internal server error: {ex.Message}", statusCode: 500);
    }
});

app.MapPost("/api/clients", async (ClientCreateDTO clientDto) =>
{
    try
    {
        // Validate input
        if (string.IsNullOrEmpty(clientDto.FirstName))
            return Results.BadRequest("FirstName is required");
        
        if (string.IsNullOrEmpty(clientDto.LastName))
            return Results.BadRequest("LastName is required");
        
        if (string.IsNullOrEmpty(clientDto.Email))
            return Results.BadRequest("Email is required");

        // Basic email validation
        if (!clientDto.Email.Contains("@"))
            return Results.BadRequest("Invalid email format");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = @"
                    INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                    VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
                    SELECT SCOPE_IDENTITY();";

                command.Parameters.AddWithValue("@FirstName", clientDto.FirstName);
                command.Parameters.AddWithValue("@LastName", clientDto.LastName);
                command.Parameters.AddWithValue("@Email", clientDto.Email);
                command.Parameters.AddWithValue("@Telephone", clientDto.Telephone);
                command.Parameters.AddWithValue("@Pesel", clientDto.Pesel);

                // Get new ID
                var result = await command.ExecuteScalarAsync();
                var newId = Convert.ToInt32(result);

                return Results.Created($"/api/clients/{newId}", new { IdClient = newId });
            }
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Internal server error: {ex.Message}", statusCode: 500);
    }
});

app.MapPut("/api/clients/{id}/trips/{tripId}", async (int id, int tripId) =>
{
    try
    {
        await using (SqlConnection connection = new SqlConnection(connectionString))
        {
            var clientExists = false;
            var tripExists = false;
            var maxPeople = 0;
            
            await connection.OpenAsync();
            
            // Check if client exists
            var result = await CheckIfClientExists(connection, id);
            clientExists = result != null;

            if (!clientExists)
                return Results.NotFound($"Client with ID {id} not found");

            await using (SqlCommand command = new SqlCommand())
            {
                var currentParticipants = 0;
                var alreadyRegistered = false;
                
                command.Connection = connection;
                
                // Check if trip exists and get max people
                command.Parameters.Clear();
                command.CommandText = "SELECT MaxPeople FROM Trip WHERE IdTrip = @IdTrip";
                command.Parameters.AddWithValue("@IdTrip", tripId);
                result = await command.ExecuteScalarAsync();
                
                if (result != null)
                {
                    tripExists = true;
                    maxPeople = Convert.ToInt32(result);
                }
                
                if (!tripExists)
                    return Results.NotFound($"Trip with ID {tripId} not found");

                // Check if client is already registered for this trip
                command.Parameters.Clear();
                command.CommandText = "SELECT 1 FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
                command.Parameters.AddWithValue("@IdClient", id);
                command.Parameters.AddWithValue("@IdTrip", tripId);
                result = await command.ExecuteScalarAsync();
                alreadyRegistered = result != null;

                if (alreadyRegistered)
                    return Results.BadRequest($"Client is already registered for this trip");

                // Count participants
                command.Parameters.Clear();
                command.CommandText = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip";
                command.Parameters.AddWithValue("@IdTrip", tripId);
                currentParticipants = Convert.ToInt32(await command.ExecuteScalarAsync());

                if (currentParticipants >= maxPeople)
                    return Results.BadRequest("Trip has maximum number of participants");

                // Register client for trip
                command.Parameters.Clear();
                command.CommandText = @"
                    INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
                    VALUES (@IdClient, @IdTrip, @RegisteredAt, NULL)";
                
                command.Parameters.AddWithValue("@IdClient", id);
                command.Parameters.AddWithValue("@IdTrip", tripId);
                
                int today = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                command.Parameters.AddWithValue("@RegisteredAt", today);

                await command.ExecuteNonQueryAsync();
            }
        }

        return Results.Ok("Client successfully registered for trip");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Internal server error: {ex.Message}", statusCode: 500);
    }
});

app.MapDelete("/api/clients/{id}/trips/{tripId}", async (int id, int tripId) =>
{
    try
    {
        bool registrationExists = false;

        await using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            await using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                
                // Check if registration exists
                command.CommandText = "SELECT 1 FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
                command.Parameters.AddWithValue("@IdClient", id);
                command.Parameters.AddWithValue("@IdTrip", tripId);
                
                var result = await command.ExecuteScalarAsync();
                registrationExists = result != null;

                if (!registrationExists)
                    return Results.NotFound("Client is not registered for this trip");

                // Remove registration
                command.CommandText = "DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
                await command.ExecuteNonQueryAsync();
            }
        }

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Internal server error: {ex.Message}", statusCode: 500);
    }
});

async Task<object?> CheckIfClientExists(SqlConnection connection, int id)
{
    await using SqlCommand command = new SqlCommand();
    
    command.Connection = connection;
    command.CommandText = "SELECT 1 FROM Client WHERE IdClient = @IdClient";
    command.Parameters.AddWithValue("@IdClient", id);
    var result = await command.ExecuteScalarAsync();

    return result;
}

app.Run();