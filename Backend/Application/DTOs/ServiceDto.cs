namespace Application.DTOs
{
    public class ServiceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string? Image { get; set; }
        public string? LongDescription { get; set; }
        public string? Duration { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "BRL";
        public CategoryDto? Category { get; set; }
        public List<BenefitDto>? Benefits { get; set; }

    }
}
