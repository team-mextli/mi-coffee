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

    /// <summary>
    /// True when a trigger rejected the statement with SIGNAL. Without an
    /// explicit MYSQL_ERRNO, SIGNAL SQLSTATE '45000' is reported as 1644.
    /// Confirm the number against the real trigger before relying on it.
    /// </summary>
    public static bool IsRejectedByTrigger(MySqlException ex)
    {
        return ex.Number == 1644;
    }
}