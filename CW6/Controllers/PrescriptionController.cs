using CW6.Dtos;
using CW6.Services;
using Microsoft.AspNetCore.Mvc;

namespace CW6.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionController(IAppService service): ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> CreatePrescriptionAsync([FromBody] PrescriptionPostDto prescription)
    {
        try
        {
            var result = await service.CreatePrescriptionAsync(prescription);
            return Created($"prescription/{result.IdPrescription}", result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}