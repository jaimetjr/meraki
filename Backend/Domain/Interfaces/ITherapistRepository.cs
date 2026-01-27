using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ITherapistRepository : IGenericRepository<Therapist>
    {
        Task<IReadOnlyCollection<Therapist>> FindBySpecialtyAsync(string specialty);
    }
}
