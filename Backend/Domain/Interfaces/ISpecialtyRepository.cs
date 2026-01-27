using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ISpecialtyRepository : IGenericRepository<Specialty>
    {
        Task<Specialty?> GetByNameAsync(string name);
    }
}
