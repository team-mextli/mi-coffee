using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Product category management.
/// </summary>
public class ProductCategoryService
{
    /// <summary>Returns all categories ordered by name.</summary>
    public List<ProductCategory> GetAll()
    {
        throw new NotImplementedException();
    }

    public ProductCategory? GetById(int productCategoryId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a category and returns its ProductCategoryId.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing or too long name, or name already in use.
    /// </exception>
    public int Create(ProductCategory category)
    {
        throw new NotImplementedException();
    }

    /// <exception cref="BusinessException">
    /// The category does not exist, missing or too long name, or name already in use.
    /// </exception>
    public void Update(ProductCategory category)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes a category that has no products.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The category does not exist or still has products.
    /// </exception>
    public void Delete(int productCategoryId)
    {
        throw new NotImplementedException();
    }
}
