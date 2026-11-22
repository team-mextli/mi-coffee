using MiCoffee.Data;
using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Product category management.
/// </summary>
public class ProductCategoryService
{
    private readonly ProductCategoryRepository _repository = new();

    /// <summary>Returns all categories ordered by name.</summary>
    public List<ProductCategory> GetAll()
    {
        return _repository.GetAll();
    }

    public ProductCategory? GetById(int productCategoryId)
    {
        return _repository.GetById(productCategoryId);
    }

    /// <summary>
    /// Creates a category and returns its ProductCategoryId.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing or too long name, or name already in use.
    /// </exception>
    public int Create(ProductCategory category)
    {
        NormalizeAndValidate(category);
        EnsureNameAvailable(category);

        return _repository.Insert(category);
    }

    /// <exception cref="BusinessException">
    /// The category does not exist, missing or too long name, or name already in use.
    /// </exception>
    public void Update(ProductCategory category)
    {
        NormalizeAndValidate(category);

        if (_repository.GetById(category.ProductCategoryId) is null)
        {
            throw new BusinessException("La categoría no existe.");
        }

        EnsureNameAvailable(category);

        _repository.Update(category);
    }

    /// <summary>
    /// Deletes a category that has no products.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The category does not exist or still has products.
    /// </exception>
    public void Delete(int productCategoryId)
    {
        if (_repository.GetById(productCategoryId) is null)
        {
            throw new BusinessException("La categoría no existe.");
        }

        if (!_repository.Delete(productCategoryId))
        {
            throw new BusinessException(
                "No se puede eliminar la categoría porque tiene productos. " +
                "Mueva o elimine sus productos primero.");
        }
    }

    private static void NormalizeAndValidate(ProductCategory category)
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
    private void EnsureNameAvailable(ProductCategory category)
    {
        ProductCategory? existing = _repository.GetByName(category.Name);
        if (existing is not null && existing.ProductCategoryId != category.ProductCategoryId)
        {
            throw new BusinessException("Ya existe una categoría con ese nombre.");
        }
    }
}