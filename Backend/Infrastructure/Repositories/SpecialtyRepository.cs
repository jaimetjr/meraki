using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SpecialtyRepository : GenericRepository<Specialty>, ISpecialtyRepository
    {
        public SpecialtyRepository(AppDbContext context) : base(context) { }

        public async Task<Specialty?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.IsActive && s.Name == name);
        }
    }

}
