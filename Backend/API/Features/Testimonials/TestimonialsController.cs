using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Testimonials;

[Route("api/testimonials")]
[ApiController]
public class TestimonialsController(TestimonialService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("top/{count}")]
    public async Task<IActionResult> GetTopRated(int count) => Ok(await service.GetTopRatedAsync(count));

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromForm] TestimonialDto dto, IFormFile? image = null)
    {
        var created = await service.CreateAsync(dto, image);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromForm] TestimonialDto dto, IFormFile? image = null, [FromForm] bool removeImage = false) =>
        await service.UpdateAsync(id, dto, image, removeImage) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id) =>
        await service.DeleteAsync(id) ? NoContent() : NotFound();
}
