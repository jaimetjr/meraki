using Infrastructure.Data;
using Npgsql;

namespace API.Tests;

/// <summary>Railway hands out the database as a URL; the API accepts it as well as a normal connection string.</summary>
public class DatabaseUrlTests
{
    [Fact]
    public void A_postgres_url_becomes_a_connection_string()
    {
        var result = new NpgsqlConnectionStringBuilder(
            DatabaseUrl.ToConnectionString("postgresql://railway_user:p%40ss%3Aword@shortline.proxy.rlwy.net:41234/railway"));

        Assert.Equal("shortline.proxy.rlwy.net", result.Host);
        Assert.Equal(41234, result.Port);
        Assert.Equal("railway", result.Database);
        Assert.Equal("railway_user", result.Username);
        Assert.Equal("p@ss:word", result.Password);
    }

    [Fact]
    public void A_url_without_a_port_uses_5432()
    {
        var result = new NpgsqlConnectionStringBuilder(DatabaseUrl.ToConnectionString("postgres://u:p@db.internal/meraki"));

        Assert.Equal(5432, result.Port);
    }

    [Fact]
    public void A_normal_connection_string_is_left_as_it_is()
    {
        const string connectionString = "Host=localhost;Database=meraki;Username=postgres;Password=a:b@c";

        Assert.Equal(connectionString, DatabaseUrl.ToConnectionString(connectionString));
    }
}
