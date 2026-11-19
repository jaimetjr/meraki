using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await userService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await userService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var created = await userService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto) =>
        await userService.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await userService.DeleteAsync(id) ? NoContent() : NotFound();

    [HttpPut("{id}/password")]
    public async Task<IActionResult> UpdatePassword(Guid id, [FromBody] UpdatePasswordDto dto) =>
        await userService.UpdatePasswordAsync(id, dto) ? NoContent() : NotFound();
}
