using Microsoft.AspNetCore.Identity;
using MiCoffee.Data;
using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Staff login and user management.
/// </summary>
public class UserService
{
    private readonly UserRepository _repository = new();
    private readonly PasswordHasher<User> _hasher = new();

    /// <summary>
    /// Returns the user when the credentials are valid. Returns null when the
    /// username does not exist, the user is inactive or the password does not
    /// match; the cause is not distinguished.
    /// </summary>
    public User? Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        User? user = _repository.GetByUsername(username.Trim());
        if (user is null || !user.IsActive)
        {
            return null;
        }

        PasswordVerificationResult result =
            _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // The stored hash uses outdated parameters: upgrade it while the
            // plain password is available.
            string newHash = _hasher.HashPassword(user, password);
            _repository.UpdatePasswordHash(user.UserId, user.PasswordHash, newHash);
            user.PasswordHash = newHash;
        }

        return user;
    }

    public List<User> GetAll()
    {
        return _repository.GetAll();
    }

    public User? GetById(int userId)
    {
        return _repository.GetById(userId);
    }

    /// <summary>
    /// Creates a staff user and returns its UserId. The password is hashed
    /// here; user.PasswordHash is ignored.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing required fields or password, or username already in use.
    /// </exception>
    public int Create(User user, string password)
    {
        NormalizeAndValidate(user);

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new BusinessException("La contraseña es obligatoria.");
        }

        EnsureUsernameAvailable(user);

        user.PasswordHash = _hasher.HashPassword(user, password);
        return _repository.Insert(user);
    }

    /// <summary>
    /// Updates FullName, Username and Role (IsActive is changed only through
    /// SetActive). The password changes only when newPassword is not empty.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing required fields, username already in use, or the last active
    /// Admin would lose the Admin role.
    /// </exception>
    public void Update(User user, string? newPassword)
    {
        NormalizeAndValidate(user);

        User existing = _repository.GetById(user.UserId)
            ?? throw new BusinessException("El usuario no existe.");

        EnsureUsernameAvailable(user);

        // Start from the stored user so PasswordHash is never overwritten
        // with the empty value that comes from a form.
        existing.FullName = user.FullName;
        existing.Username = user.Username;
        existing.Role = user.Role;

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            existing.PasswordHash = _hasher.HashPassword(existing, newPassword);
        }

        if (!_repository.Update(existing))
        {
            throw new BusinessException(
                "No se puede quitar el rol de administrador al último administrador activo.");
        }
    }

    /// <summary>
    /// Activates or deactivates a user. Users are never deleted.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The last active Admin cannot be deactivated.
    /// </exception>
    public void SetActive(int userId, bool isActive)
    {
        if (_repository.GetById(userId) is null)
        {
            throw new BusinessException("El usuario no existe.");
        }

        if (!_repository.UpdateActive(userId, isActive))
        {
            throw new BusinessException(
                "No se puede desactivar al último administrador activo.");
        }
    }

    private static void NormalizeAndValidate(User user)
    {
        // Model binding turns empty form fields into null.
        user.FullName = (user.FullName ?? string.Empty).Trim();
        user.Username = (user.Username ?? string.Empty).Trim();

        if (user.FullName.Length == 0)
        {
            throw new BusinessException("El nombre completo es obligatorio.");
        }

        if (user.FullName.Length > 100)
        {
            throw new BusinessException("El nombre completo admite hasta 100 caracteres.");
        }

        if (user.Username.Length == 0)
        {
            throw new BusinessException("El nombre de usuario es obligatorio.");
        }

        if (user.Username.Length > 50)
        {
            throw new BusinessException("El nombre de usuario admite hasta 50 caracteres.");
        }

        if (!Enum.IsDefined(user.Role))
        {
            throw new BusinessException("El rol no es válido.");
        }
    }

    private void EnsureUsernameAvailable(User user)
    {
        User? existing = _repository.GetByUsername(user.Username);
        if (existing is not null && existing.UserId != user.UserId)
        {
            throw new BusinessException("El nombre de usuario ya está en uso.");
        }
    }
}