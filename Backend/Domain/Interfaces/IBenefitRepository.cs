using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IBenefitRepository : IGenericRepository<Benefit>
    {
        Task<IReadOnlyCollection<Benefit>> SearchByKeywordAsync(string keyword);
    }
}
