namespace Application.DTOs
{
    public class TestimonialDto
    {
        public Guid Id { get; set; }
        public string AuthorName { get; set; } = default!;
        public string? AuthorAvatarUrl { get; set; } = default!;
        public string? AuthorBadge { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; } = default!;
    }

}
