using CW6.Services;
using Microsoft.AspNetCore.Mvc;

namespace CW6.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientController(IAppService service): ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetAllPatientsAsync()
    {
        return Ok(await service.GetAllPatientsAsync());
    }
}