namespace Domain.Exceptions;

/// <summary>The request conflicts with existing state (duplicate, still in use). Surfaces as 409.</summary>
public class ConflictException(string message) : Exception(message);
