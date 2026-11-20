using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Auth;

[Route("api/auth")]
[ApiController]
public class AuthController(AuthService authService) : ControllerBase
{
    /// <summary>Invalid credentials surface as 401 (AuthenticationFailedException) through the global exception handler.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto login) => Ok(await authService.LoginAsync(login));
}
