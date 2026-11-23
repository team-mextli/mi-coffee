using MiCoffee.Data;
using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Supplier management.
/// </summary>
public class SupplierService
{
    private readonly SupplierRepository _repository = new();

    /// <summary>Returns all suppliers ordered by name.</summary>
    public List<Supplier> GetAll()
    {
        return _repository.GetAll();
    }

    public Supplier? GetById(int supplierId)
    {
        return _repository.GetById(supplierId);
    }

    /// <summary>
    /// Creates a supplier and returns its SupplierId.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing or too long name or contact, or note too long.
    /// </exception>
    public int Create(Supplier supplier)
    {
        NormalizeAndValidate(supplier);

        return _repository.Insert(supplier);
    }

    /// <exception cref="BusinessException">
    /// The supplier does not exist, missing or too long name or contact, or note too long.
    /// </exception>
    public void Update(Supplier supplier)
    {
        NormalizeAndValidate(supplier);

        if (_repository.GetById(supplier.SupplierId) is null)
        {
            throw new BusinessException("El proveedor no existe.");
        }

        _repository.Update(supplier);
    }

    /// <summary>
    /// Deletes a supplier that has no supplies assigned.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The supplier does not exist or still has supplies assigned.
    /// </exception>
    public void Delete(int supplierId)
    {
        if (_repository.GetById(supplierId) is null)
        {
            throw new BusinessException("El proveedor no existe.");
        }

        if (!_repository.Delete(supplierId))
        {
            throw new BusinessException(
                "No se puede eliminar el proveedor porque tiene insumos asignados. " +
                "Cambie o quite el proveedor de esos insumos primero.");
        }
    }

    private static void NormalizeAndValidate(Supplier supplier)
    {
        // Model binding turns empty form fields into null.
        supplier.Name = (supplier.Name ?? string.Empty).Trim();
        supplier.Contact = (supplier.Contact ?? string.Empty).Trim();
        supplier.Note = NormalizeOptional(supplier.Note);

        if (supplier.Name.Length == 0)
        {
            throw new BusinessException("El nombre del proveedor es obligatorio.");
        }

        if (supplier.Name.Length > 100)
        {
            throw new BusinessException("El nombre del proveedor admite hasta 100 caracteres.");
        }

        if (supplier.Contact.Length == 0)
        {
            throw new BusinessException("El contacto del proveedor es obligatorio.");
        }

        if (supplier.Contact.Length > 100)
        {
            throw new BusinessException("El contacto del proveedor admite hasta 100 caracteres.");
        }

        if (supplier.Note is { Length: > 255 })
        {
            throw new BusinessException("La nota del proveedor admite hasta 255 caracteres.");
        }
    }

    // Empty or whitespace-only text becomes null, as the column is nullable.
    private static string? NormalizeOptional(string? value)
    {
        string trimmed = (value ?? string.Empty).Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}