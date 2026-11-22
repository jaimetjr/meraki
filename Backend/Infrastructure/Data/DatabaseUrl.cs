using Npgsql;

namespace Infrastructure.Data;

/// <summary>
/// Railway (like Heroku) hands out the database as a URL, <c>postgresql://user:password@host:port/database</c>,
/// which Npgsql does not read. Anything else is returned unchanged as a normal connection string.
/// </summary>
public static class DatabaseUrl
{
    public static string ToConnectionString(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var url) || url.Scheme is not ("postgres" or "postgresql"))
            return value;

        var user = url.UserInfo.Split(':', 2);
        return new NpgsqlConnectionStringBuilder
        {
            Host = url.Host,
            Port = url.Port > 0 ? url.Port : 5432,
            Database = Uri.UnescapeDataString(url.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(user[0]),
            Password = user.Length > 1 ? Uri.UnescapeDataString(user[1]) : null,
        }.ConnectionString;
    }
}
