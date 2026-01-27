namespace Application.DTOs
{
    public class ContactRequestDto
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Service { get; set; } = default!;
        public string? Message { get; set; }

    }
}
