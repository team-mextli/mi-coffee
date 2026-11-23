using MiCoffee.Entities;
using MySql.Data.MySqlClient;

namespace MiCoffee.Data;

public class SupplierRepository
{
    // Column order matters: Map reads by position.
    private const string SelectColumns = "supplier_id, name, contact, note";

    public List<Supplier> GetAll()
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM supplier ORDER BY name;", cn);

        using MySqlDataReader reader = cmd.ExecuteReader();
        List<Supplier> suppliers = new();
        while (reader.Read())
        {
            suppliers.Add(Map(reader));
        }

        return suppliers;
    }

    public Supplier? GetById(int supplierId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "SELECT " + SelectColumns + " FROM supplier " +
            "WHERE supplier_id = @supplier_id;", cn);
        cmd.Parameters.AddWithValue("@supplier_id", supplierId);

        using MySqlDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public int Insert(Supplier supplier)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "INSERT INTO supplier (name, contact, note) " +
            "VALUES (@name, @contact, @note);", cn);
        cmd.Parameters.AddWithValue("@name", supplier.Name);
        cmd.Parameters.AddWithValue("@contact", supplier.Contact);
        cmd.Parameters.AddWithValue("@note", (object?)supplier.Note ?? DBNull.Value);
        cmd.ExecuteNonQuery();

        return (int)cmd.LastInsertedId;
    }

    public void Update(Supplier supplier)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "UPDATE supplier SET name = @name, contact = @contact, note = @note " +
            "WHERE supplier_id = @supplier_id;", cn);
        cmd.Parameters.AddWithValue("@name", supplier.Name);
        cmd.Parameters.AddWithValue("@contact", supplier.Contact);
        cmd.Parameters.AddWithValue("@note", (object?)supplier.Note ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@supplier_id", supplier.SupplierId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Deletes the supplier. Returns false, without changes, when supplies
    /// still reference it (the foreign key is the guard).
    /// </summary>
    public bool Delete(int supplierId)
    {
        using MySqlConnection cn = Connection.Create();
        cn.Open();

        using MySqlCommand cmd = new(
            "DELETE FROM supplier WHERE supplier_id = @supplier_id;", cn);
        cmd.Parameters.AddWithValue("@supplier_id", supplierId);

        try
        {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (MySqlException ex) when (MySqlErrors.IsRowReferenced(ex))
        {
            return false;
        }
    }

    private static Supplier Map(MySqlDataReader reader)
    {
        return new Supplier
        {
            SupplierId = reader.GetInt32(0),
            Name = reader.GetString(1),
            Contact = reader.GetString(2),
            Note = reader.IsDBNull(3) ? null : reader.GetString(3)
        };
    }
}