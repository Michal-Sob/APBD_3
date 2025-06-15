using CW6.Data;
using CW6.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAppService, AppService>();

var app = builder.Build();

// Always enable Swagger for testing
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();