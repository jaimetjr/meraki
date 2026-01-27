using Application.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICourseService
    {
        Task<List<CourseDto>> GetAllAsync();
        Task<CourseDto?> GetByIdAsync(Guid id);
        Task<List<CourseDto>> GetByStatusAsync(string status);
        Task<CourseDto> CreateAsync(CourseDto dto, IFormFile? image = null);
        Task<bool> UpdateAsync(Guid id, CourseDto dto, IFormFile? image = null);
        Task<bool> DeleteAsync(Guid id);

    }
}
