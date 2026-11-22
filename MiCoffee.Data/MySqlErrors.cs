using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

internal static class MySqlErrors
{
    /// <summary>
    /// True when a DELETE was rejected because other rows reference the row.
    /// The server reports 1451; some versions report 1217 instead when the
    /// account lacks table privileges (MySQL bug #112589).
    /// </summary>
    public static bool IsRowReferenced(MySqlException ex)
    {
        return ex.Number is 1451 or 1217;
    }
}
