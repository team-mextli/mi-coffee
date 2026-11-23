using MiCoffee.Data;
using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Supply category management.
/// </summary>
public class SupplyCategoryService
{
    private readonly SupplyCategoryRepository _repository = new();

    /// <summary>Returns all categories ordered by name.</summary>
    public List<SupplyCategory> GetAll()
    {
        return _repository.GetAll();
    }

    public SupplyCategory? GetById(int supplyCategoryId)
    {
        return _repository.GetById(supplyCategoryId);
    }

    /// <summary>
    /// Creates a category and returns its SupplyCategoryId.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing or too long name, or name already in use.
    /// </exception>
    public int Create(SupplyCategory category)
    {
        NormalizeAndValidate(category);
        EnsureNameAvailable(category);

        return _repository.Insert(category);
    }

    /// <exception cref="BusinessException">
    /// The category does not exist, missing or too long name, or name already in use.
    /// </exception>
    public void Update(SupplyCategory category)
    {
        NormalizeAndValidate(category);

        if (_repository.GetById(category.SupplyCategoryId) is null)
        {
            throw new BusinessException("La categoría no existe.");
        }

        EnsureNameAvailable(category);

        _repository.Update(category);
    }

    /// <summary>
    /// Deletes a category that has no supplies.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The category does not exist or still has supplies.
    /// </exception>
    public void Delete(int supplyCategoryId)
    {
        if (_repository.GetById(supplyCategoryId) is null)
        {
            throw new BusinessException("La categoría no existe.");
        }

        if (!_repository.Delete(supplyCategoryId))
        {
            throw new BusinessException(
                "No se puede eliminar la categoría porque tiene insumos. " +
                "Mueva o elimine sus insumos primero.");
        }
    }

    private static void NormalizeAndValidate(SupplyCategory category)
    {
        // Model binding turns empty form fields into null.
        category.Name = (category.Name ?? string.Empty).Trim();

        if (category.Name.Length == 0)
        {
            throw new BusinessException("El nombre de la categoría es obligatorio.");
        }

        if (category.Name.Length > 50)
        {
            throw new BusinessException("El nombre de la categoría admite hasta 50 caracteres.");
        }
    }

    // The lookup uses the database collation, the same one the UNIQUE
    // constraint applies.
    private void EnsureNameAvailable(SupplyCategory category)
    {
        SupplyCategory? existing = _repository.GetByName(category.Name);
        if (existing is not null && existing.SupplyCategoryId != category.SupplyCategoryId)
        {
            throw new BusinessException("Ya existe una categoría con ese nombre.");
        }
    }
}