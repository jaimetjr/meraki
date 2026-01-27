namespace Domain.Interfaces;

public interface ISoftDeletable
{
    bool IsActive { get; }
    void Deactivate();
}

