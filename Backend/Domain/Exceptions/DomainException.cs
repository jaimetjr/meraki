namespace Domain.Exceptions;

/// <summary>A business rule was violated by the input. Surfaces to API clients as 400.</summary>
public class DomainException(string message) : Exception(message);
