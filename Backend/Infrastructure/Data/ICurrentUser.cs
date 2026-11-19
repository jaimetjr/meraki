namespace Infrastructure.Data;

/// <summary>The authenticated caller of the current request, if any (used for audit columns).</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
}
