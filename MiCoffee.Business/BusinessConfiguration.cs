using MiCoffee.Data;

namespace MiCoffee.Business;

public static class BusinessConfiguration
{
    public static void Initialize(string connectionString)
    {
        Connection.Configure(connectionString);
    }

    public static void TestConnection()
    {
        Connection.Test();
    }
}