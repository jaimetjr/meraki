namespace Domain.Entities
{
    public abstract class AggregateRoot : Entity
    {
        // Aggregate roots are entities that have their own repositories
        // and are the entry points for operations on the aggregate
    }
}

