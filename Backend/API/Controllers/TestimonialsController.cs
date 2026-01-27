using Application.DTOs;
using Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestimonialsController : ControllerBase
    {
        private readonly ITestimonialService _service;
        private readonly IValidator<TestimonialDto> _validator;

        public TestimonialsController(ITestimonialService service, IValidator<TestimonialDto> validator)
        {
            _service = service;
            _validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("top/{count}")]
        public async Task<IActionResult> GetTopRated(int count)
        {
            var result = await _service.GetTopRatedAsync(count);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Create([FromForm] TestimonialDto dto, IFormFile? image = null)
        {
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid) return BadRequest(validation.Errors);

            var created = await _service.CreateAsync(dto, image);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Update(Guid id, [FromForm] TestimonialDto dto, IFormFile? image = null)
        {
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid) return BadRequest(validation.Errors);

            var success = await _service.UpdateAsync(id, dto, image);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }

    }
}
