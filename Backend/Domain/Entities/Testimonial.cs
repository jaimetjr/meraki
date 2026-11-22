using Domain.Exceptions;
using Domain.Interfaces;

namespace Domain.Entities
{
    public sealed class Testimonial : AggregateRoot, ISoftDeletable
    {
        public const int AuthorNameMaxLength = 100;
        public const int AuthorAvatarUrlMaxLength = 300;
        public const int AuthorBadgeMaxLength = 50;
        public const int ContentMaxLength = 1000;

        public string AuthorName { get; private set; } = null!;
        public string? AuthorAvatarUrl { get; private set; }
        public string? AuthorBadge { get; private set; }
        public int Rating { get; private set; }
        public string Content { get; private set; } = null!;
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private Testimonial() { }

        public Testimonial(string authorName, string? avatarUrl, int rating, string content, string? badge = null)
        {
            if (string.IsNullOrWhiteSpace(authorName)) throw new DomainException("O campo Nome do autor é obrigatório.");
            if (string.IsNullOrWhiteSpace(content)) throw new DomainException("O campo Conteúdo é obrigatório.");
            if (rating < 1 || rating > 5) throw new DomainException("A nota deve estar entre 1 e 5.");
            if (authorName.Length > AuthorNameMaxLength) throw new DomainException($"O campo Nome do autor deve ter no máximo {AuthorNameMaxLength} caracteres.");
            if (content.Length > ContentMaxLength) throw new DomainException($"O campo Conteúdo deve ter no máximo {ContentMaxLength} caracteres.");

            AuthorName = authorName;
            AuthorAvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl;
            Rating = rating;
            Content = content;
            AuthorBadge = badge;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string authorName, string? avatarUrl, int rating, string content, string? badge = null)
        {
            if (string.IsNullOrWhiteSpace(authorName)) throw new DomainException("O campo Nome do autor é obrigatório.");
            if (string.IsNullOrWhiteSpace(content)) throw new DomainException("O campo Conteúdo é obrigatório.");
            if (rating < 1 || rating > 5) throw new DomainException("A nota deve estar entre 1 e 5.");
            if (authorName.Length > AuthorNameMaxLength) throw new DomainException($"O campo Nome do autor deve ter no máximo {AuthorNameMaxLength} caracteres.");
            if (content.Length > ContentMaxLength) throw new DomainException($"O campo Conteúdo deve ter no máximo {ContentMaxLength} caracteres.");

            AuthorName = authorName;
            AuthorAvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl;
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
