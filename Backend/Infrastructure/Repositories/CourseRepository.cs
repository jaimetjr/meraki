using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context) { }

        public async Task<IReadOnlyCollection<Course>> GetByStatusAsync(CourseStatus status)
        {
            return (await _dbSet
                .Where(c => c.Status == status && c.IsActive)
                .ToListAsync()).AsReadOnly();
        }
    }
}
