namespace Application.DTOs
{
    public class BenefitDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }

    }
}
