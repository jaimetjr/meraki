using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Common;

/// <summary>
/// Turns exceptions into RFC 7807 problem responses. Business-rule and conflict messages are
/// meant for the client; anything unexpected is logged and answered with a generic 500.
/// </summary>
public sealed class ApiExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            DomainException e => (StatusCodes.Status400BadRequest, "Invalid request", e.Message),
            ConflictException e => (StatusCodes.Status409Conflict, "Conflict", e.Message),
            AuthenticationFailedException e => (StatusCodes.Status401Unauthorized, "Unauthorized", e.Message),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error", "Ocorreu um erro inesperado. Tente novamente mais tarde."),
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail },
        });
    }
}
