using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SpecialtiesController(ISpecialtyService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var result = await service.GetByNameAsync(name);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create(SpecialtyDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, SpecialtyDto dto) =>
        await service.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id) =>
        await service.DeleteAsync(id) ? NoContent() : NotFound();
}
