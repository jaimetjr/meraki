using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TestimonialRepository : GenericRepository<Testimonial>, ITestimonialRepository
    {
        public TestimonialRepository(AppDbContext context) : base(context) { }

        public async Task<IReadOnlyCollection<Testimonial>> GetTopRatedAsync(int count)
        {
            return (await _dbSet
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.Rating)
                .Take(count)
                .ToListAsync()).AsReadOnly();
        }
    }

}
