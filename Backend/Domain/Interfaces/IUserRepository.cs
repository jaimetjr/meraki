using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> FindByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
    }
}

