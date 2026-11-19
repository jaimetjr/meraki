using System.Security.Claims;
using Infrastructure.Data;

namespace API.Common;

/// <summary>Reads the caller's user id from the JWT (NameIdentifier, or the raw "sub" claim).</summary>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var user = accessor.HttpContext?.User;
            var value = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user?.FindFirst("sub")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
