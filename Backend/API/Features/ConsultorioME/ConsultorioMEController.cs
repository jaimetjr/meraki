using Infrastructure.ConsultorioME;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.ConsultorioME;

/// <summary>Proxy to the ConsultorioME scheduling platform (used to link therapists via ProId).</summary>
[Route("api/consultoriome")]
[ApiController]
public class ConsultorioMEController(IConsultorioMEService consultorioME) : ControllerBase
{
    [HttpGet("professionals")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetProfessionals() => Ok(await consultorioME.GetProfessionalsAsync());
}
