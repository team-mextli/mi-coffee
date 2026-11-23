using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Supplier management.
/// </summary>
public class SupplierService
{
    /// <summary>Returns all suppliers ordered by name.</summary>
    public List<Supplier> GetAll()
    {
        throw new NotImplementedException();
    }

    public Supplier? GetById(int supplierId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a supplier and returns its SupplierId.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing or too long name or contact, or note too long.
    /// </exception>
    public int Create(Supplier supplier)
    {
        throw new NotImplementedException();
    }

    /// <exception cref="BusinessException">
    /// The supplier does not exist, missing or too long name or contact, or note too long.
    /// </exception>
    public void Update(Supplier supplier)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes a supplier that has no supplies assigned.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The supplier does not exist or still has supplies assigned.
    /// </exception>
    public void Delete(int supplierId)
    {
        throw new NotImplementedException();
    }
}