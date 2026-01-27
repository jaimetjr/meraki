using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BenefitRepository : GenericRepository<Benefit>, IBenefitRepository
    {
        public BenefitRepository(AppDbContext context) : base(context) { }

        public async Task<IReadOnlyCollection<Benefit>> SearchByKeywordAsync(string keyword)
        {
            return (await _dbSet
                .Where(b => b.IsActive && (b.Title.Contains(keyword) || (b.Description != null && b.Description.Contains(keyword))))
                .ToListAsync()).AsReadOnly();
        }
    }

}
