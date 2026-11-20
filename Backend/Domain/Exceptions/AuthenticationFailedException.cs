namespace Domain.Exceptions;

/// <summary>Wrong credentials or inactive account at login. Surfaces as 401.</summary>
public class AuthenticationFailedException(string message) : Exception(message);
