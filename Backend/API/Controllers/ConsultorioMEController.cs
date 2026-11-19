using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConsultorioMEController(IConsultorioMEService consultorioMEService) : ControllerBase
{
    [HttpGet("professionals")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetProfessionals() => Ok(await consultorioMEService.GetProfessionalsAsync());
}
