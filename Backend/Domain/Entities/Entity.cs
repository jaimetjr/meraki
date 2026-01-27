namespace Domain.Entities
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }
        public Guid? CreatedBy { get; protected set; }
        public Guid? UpdatedBy { get; protected set; }

        protected void MarkAsModified()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCreatedBy(Guid? userId)
        {
            CreatedBy = userId;
        }

        public void SetUpdatedBy(Guid? userId)
        {
            UpdatedBy = userId;
            MarkAsModified();
        }
    }
}
