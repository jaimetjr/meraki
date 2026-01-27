using Domain.Entities;
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
    public class TherapistRepository : GenericRepository<Therapist>, ITherapistRepository
    {
        public TherapistRepository(AppDbContext context) : base(context) { }

        public async Task<IReadOnlyCollection<Therapist>> FindBySpecialtyAsync(string specialtyName)
        {
            return (await _dbSet
                .Include(t => t.Specialties)
                .Where(t => t.IsActive && t.Specialties.Any(s => s.Name == specialtyName))
                .ToListAsync()).AsReadOnly();
        }

        public override async Task<IReadOnlyCollection<Therapist>> GetAllAsync()
        {
            return (await _dbSet
                .Where(t => t.IsActive)
                .Include(x => x.Specialties)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync()).AsReadOnly();
        }

        public override async Task<Therapist?> GetByIdAsync(Guid id)
        {
            var therapist = await _dbSet
                .Include(x => x.Specialties)
                .FirstOrDefaultAsync(x => x.Id == id);
            
            if (therapist == null || !therapist.IsActive)
                return null;
                
            return therapist;
        }
    }
}
