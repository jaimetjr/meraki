using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ITestimonialRepository : IGenericRepository<Testimonial>
    {
        Task<IReadOnlyCollection<Testimonial>> GetTopRatedAsync(int count);
    }

}
