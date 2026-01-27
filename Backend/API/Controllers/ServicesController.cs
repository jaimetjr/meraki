using Application.DTOs;
using Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly IValidator<ServiceDto> _validator;

        public ServicesController(IServiceManager serviceManager, IValidator<ServiceDto> validator)
        {
            _serviceManager = serviceManager;
            _validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _serviceManager.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _serviceManager.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var result = await _serviceManager.GetByCategoryAsync(category);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Create([FromForm] ServiceDto dto, [FromForm] string? category = null, [FromForm] string? benefits = null, IFormFile? image = null)
        {
            // Deserialize Category if provided
            if (!string.IsNullOrEmpty(category))
            {
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    dto.Category = JsonSerializer.Deserialize<CategoryDto>(category, options);
                }
                catch (JsonException)
                {
                    return BadRequest("Invalid category JSON format");
                }
            }

            // Deserialize Benefits if provided
            if (!string.IsNullOrEmpty(benefits))
            {
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    dto.Benefits = JsonSerializer.Deserialize<List<BenefitDto>>(benefits, options) ?? new List<BenefitDto>();
                }
                catch (JsonException)
                {
                    return BadRequest("Invalid benefits JSON format");
                }
            }

            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors);

            var created = await _serviceManager.CreateAsync(dto, image);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Update(Guid id, [FromForm] ServiceDto dto, [FromForm] string? category = null, [FromForm] string? benefits = null, IFormFile? image = null)
        {
            // Deserialize Category if provided
            if (!string.IsNullOrEmpty(category))
            {
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    dto.Category = JsonSerializer.Deserialize<CategoryDto>(category, options);
                }
                catch (JsonException)
                {
                    return BadRequest("Invalid category JSON format");
                }
            }

            // Deserialize Benefits if provided
            if (!string.IsNullOrEmpty(benefits))
            {
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    dto.Benefits = JsonSerializer.Deserialize<List<BenefitDto>>(benefits, options) ?? new List<BenefitDto>();
                }
                catch (JsonException)
                {
                    return BadRequest("Invalid benefits JSON format");
                }
            }

            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors);

            var success = await _serviceManager.UpdateAsync(id, dto, image);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _serviceManager.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
