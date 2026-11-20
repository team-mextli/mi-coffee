namespace MiCoffee.Business;

/// <summary>
/// Raised by services when a validation or business rule fails.
/// The message is written in Spanish and is meant to be shown to the user.
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}