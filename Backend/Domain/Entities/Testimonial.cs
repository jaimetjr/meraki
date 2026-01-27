using Domain.Interfaces;

namespace Domain.Entities
{
    public sealed class Testimonial : AggregateRoot, ISoftDeletable
    {
        public string AuthorName { get; private set; }
        public string AuthorAvatarUrl { get; private set; }
        public string? AuthorBadge { get; private set; }
        public int Rating { get; private set; }
        public string Content { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private Testimonial() { }

        public Testimonial(string authorName, string avatarUrl, int rating, string content, string? badge = null)
        {
            if (string.IsNullOrWhiteSpace(authorName)) throw new ArgumentException("AuthorName is required", nameof(authorName));
            if (string.IsNullOrWhiteSpace(avatarUrl)) throw new ArgumentException("AvatarUrl is required", nameof(avatarUrl));
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content is required", nameof(content));
            if (rating < 1 || rating > 5) throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));
            if (authorName.Length > 100) throw new ArgumentException("AuthorName cannot exceed 100 characters", nameof(authorName));
            if (content.Length > 1000) throw new ArgumentException("Content cannot exceed 1000 characters", nameof(content));

            AuthorName = authorName;
            AuthorAvatarUrl = avatarUrl;
            Rating = rating;
            Content = content;
            AuthorBadge = badge;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string authorName, string avatarUrl, int rating, string content, string? badge = null)
        {
            if (string.IsNullOrWhiteSpace(authorName)) throw new ArgumentException("AuthorName is required", nameof(authorName));
            if (string.IsNullOrWhiteSpace(avatarUrl)) throw new ArgumentException("AvatarUrl is required", nameof(avatarUrl));
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content is required", nameof(content));
            if (rating < 1 || rating > 5) throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));
            if (authorName.Length > 100) throw new ArgumentException("AuthorName cannot exceed 100 characters", nameof(authorName));
            if (content.Length > 1000) throw new ArgumentException("Content cannot exceed 1000 characters", nameof(content));

            AuthorName = authorName;
            AuthorAvatarUrl = avatarUrl;
            Rating = rating;
            Content = content;
            AuthorBadge = badge;
            MarkAsModified();
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkAsModified();
        }
    }

}
