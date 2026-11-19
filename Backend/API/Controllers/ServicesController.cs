using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServicesController(IServiceManager service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category) => Ok(await service.GetByCategoryAsync(category));

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromForm] ServiceDto dto, IFormFile? image = null)
    {
        var created = await service.CreateAsync(dto, image);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromForm] ServiceDto dto, IFormFile? image = null) =>
        await service.UpdateAsync(id, dto, image) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id) =>
        await service.DeleteAsync(id) ? NoContent() : NotFound();
}
