namespace API.Common;

/// <summary>
/// A reference to another resource: the services a therapist offers, the therapists who offer a service.
/// When the admin sends one, only <see cref="Id"/> is read.
/// </summary>
public class IdNameDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}
