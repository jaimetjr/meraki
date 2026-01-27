using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<IReadOnlyCollection<Course>> GetByStatusAsync(CourseStatus status);
    }
}
