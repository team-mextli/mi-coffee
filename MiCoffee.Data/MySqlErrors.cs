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
    /// True when an INSERT or UPDATE was rejected because the row it
    /// references does not exist. The server reports 1452; it reports 1216
    /// instead when the account lacks table privileges on the referenced
    /// table.
    /// </summary>
    public static bool IsReferenceMissing(MySqlException ex)
    {
        return ex.Number is 1452 or 1216;
    }

    /// <summary>
    /// True when a trigger or a stored procedure rejected the statement with
    /// SIGNAL. Without an explicit MYSQL_ERRNO, SIGNAL SQLSTATE '45000' is
    /// reported as 1644 (confirmed on MySQL 8.0 with the triggers and
    /// procedures of routines_tests.sql).
    /// </summary>
    public static bool IsRejectedByTrigger(MySqlException ex)
    {
        return ex.Number == 1644;
    }
}