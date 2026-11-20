using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Staff login and user management. Contract only.
/// </summary>
public class UserService
{
    /// <summary>
    /// Returns the user when the credentials are valid. Returns null when the
    /// username does not exist, the user is inactive or the password does not
    /// match; the cause is not distinguished.
    /// </summary>
    public User? Authenticate(string username, string password)
        => throw new NotImplementedException();

    public List<User> GetAll()
        => throw new NotImplementedException();

    public User? GetById(int userId)
        => throw new NotImplementedException();

    /// <summary>
    /// Creates a staff user and returns its UserId. The password is hashed
    /// here; user.PasswordHash is ignored.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing required fields or password, or username already in use.
    /// </exception>
    public int Create(User user, string password)
        => throw new NotImplementedException();

    /// <summary>
    /// Updates FullName, Username and Role (IsActive is changed only through
    /// SetActive). The password changes only when newPassword is not empty.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing required fields, username already in use, or the last active
    /// Admin would lose the Admin role.
    /// </exception>
    public void Update(User user, string? newPassword)
        => throw new NotImplementedException();

    /// <summary>
    /// Activates or deactivates a user. Users are never deleted.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The last active Admin cannot be deactivated.
    /// </exception>
    public void SetActive(int userId, bool isActive)
        => throw new NotImplementedException();
}