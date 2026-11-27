using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

internal static class DataReaderExtensions
{
    /// <summary>
    /// Reads a string column as a member of T. Throws InvalidOperationException
    /// when the stored value is not a member of T.
    /// </summary>
    public static T GetEnum<T>(this MySqlDataReader reader, int ordinal) where T : struct, Enum
    {
        string value = reader.GetString(ordinal);

        // TryParse also accepts numeric strings, so IsDefined is needed.
        if (!Enum.TryParse(value, out T result) || !Enum.IsDefined(result))
        {
            throw new InvalidOperationException(
                $"The value '{value}' in column '{reader.GetName(ordinal)}' is not a valid {typeof(T).Name}.");
        }

        return result;
    }
}
