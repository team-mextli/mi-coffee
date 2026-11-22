using MiCoffee.Entities;
using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

public class UserRepository
{
    // Column order matters: Map reads by position.
    private const string SelectColumns =
        "user_id, full_name, username, password_hash, role, is_active";

    public User? GetByUsername(string username)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM `user` WHERE username = @username;", cn);
        cmd.Parameters.AddWithValue("@username", username);

        using MySqlDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public User? GetById(int userId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM `user` WHERE user_id = @user_id;", cn);
        cmd.Parameters.AddWithValue("@user_id", userId);

        using MySqlDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public List<User> GetAll()
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM `user` ORDER BY full_name;", cn);

        using MySqlDataReader reader = cmd.ExecuteReader();
        List<User> users = new();
        while (reader.Read())
        {
            users.Add(Map(reader));
        }

        return users;
    }

    public int Insert(User user)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "INSERT INTO `user` (full_name, username, password_hash, role, is_active) " +
            "VALUES (@full_name, @username, @password_hash, @role, @is_active);", cn);
        cmd.Parameters.AddWithValue("@full_name", user.FullName);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@password_hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@role", user.Role.ToString());
        cmd.Parameters.AddWithValue("@is_active", user.IsActive);
        cmd.ExecuteNonQuery();

        return (int)cmd.LastInsertedId;
    }

    /// <summary>
    /// Updates the user. Returns false, without changes, when the user is the
    /// last active Admin and the new role is not Admin.
    /// </summary>
    public bool Update(User user)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();
        using MySqlTransaction tx = cn.BeginTransaction();

        if (user.Role != UserRole.Admin && IsLastActiveAdmin(cn, tx, user.UserId))
        {
            tx.Rollback();
            return false;
        }

        using MySqlCommand cmd = new(
            "UPDATE `user` SET full_name = @full_name, username = @username, " +
            "password_hash = @password_hash, role = @role " +
            "WHERE user_id = @user_id;", cn, tx);
        cmd.Parameters.AddWithValue("@full_name", user.FullName);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@password_hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@role", user.Role.ToString());
        cmd.Parameters.AddWithValue("@user_id", user.UserId);
        cmd.ExecuteNonQuery();

        tx.Commit();
        return true;
    }

    /// <summary>
    /// Activates or deactivates the user. Returns false, without changes, when
    /// the user is the last active Admin and would be deactivated.
    /// </summary>
    public bool UpdateActive(int userId, bool isActive)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();
        using MySqlTransaction tx = cn.BeginTransaction();

        if (!isActive && IsLastActiveAdmin(cn, tx, userId))
        {
            tx.Rollback();
            return false;
        }

        using MySqlCommand cmd = new(
            "UPDATE `user` SET is_active = @is_active WHERE user_id = @user_id;", cn, tx);
        cmd.Parameters.AddWithValue("@is_active", isActive);
        cmd.Parameters.AddWithValue("@user_id", userId);
        cmd.ExecuteNonQuery();

        tx.Commit();
        return true;
    }

    // Locks the active Admin rows until the transaction ends, so two concurrent
    // changes cannot both see another Admin remaining.
    private static bool IsLastActiveAdmin(MySqlConnection cn, MySqlTransaction tx, int userId)
    {
        using MySqlCommand cmd = new(
            "SELECT user_id FROM `user` WHERE role = 'Admin' AND is_active = TRUE FOR UPDATE;",
            cn, tx);
        using MySqlDataReader reader = cmd.ExecuteReader();

        int admins = 0;
        bool isTargetAdmin = false;
        while (reader.Read())
        {
            admins++;
            if (reader.GetInt32(0) == userId)
            {
                isTargetAdmin = true;
            }
        }

        return isTargetAdmin && admins == 1;
    }

    private static User Map(MySqlDataReader reader)
    {
        return new User
        {
            UserId = reader.GetInt32(0),
            FullName = reader.GetString(1),
            Username = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            Role = Enum.Parse<UserRole>(reader.GetString(4)),
            IsActive = reader.GetBoolean(5)
        };
    }
}