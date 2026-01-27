namespace Application.DTOs
{
    public class TherapistDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Bio { get; set; } = default!;
        public string? Image { get; set; }
        public string Experience { get; set; } = default!;
        public string Education { get; set; } = default!;
        public List<SpecialtyDto> Specialties { get; set; } = new();

    }
}
