using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IServiceRepository : IGenericRepository<Service>
    {
        Task<IReadOnlyCollection<Service>> GetByCategoryAsync(string category);
    }
}
