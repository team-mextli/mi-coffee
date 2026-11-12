using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

public static class Connection
{
    private static string? _connectionString;

    public static void Configure(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static MySqlConnection Create()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "The MiCoffee connection string has not been configured.");
        }

        return new MySqlConnection(_connectionString);
    }

    public static void Test()
    {
        using MySqlConnection cn = Create();
        cn.Open();

        using MySqlCommand cmd = new("SELECT 1;", cn);
        cmd.ExecuteScalar();
    }
}