using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultorioMEController : ControllerBase
    {
        private readonly IConsultorioMEService _consultorioMEService;

        public ConsultorioMEController(IConsultorioMEService consultorioMEService)
        {
            _consultorioMEService = consultorioMEService;
        }

        [HttpGet("professionals")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetProfessionals()
        {
            try
            {
                var professionals = await _consultorioMEService.GetProfessionalsAsync();
                return Ok(professionals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve professionals from ConsultorioME", error = ex.Message });
            }
        }
    }
}
