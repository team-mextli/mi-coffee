using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Supply category management.
/// </summary>
public class SupplyCategoryService
{
    /// <summary>Returns all categories ordered by name.</summary>
    public List<SupplyCategory> GetAll()
    {
        throw new NotImplementedException();
    }

    public SupplyCategory? GetById(int supplyCategoryId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a category and returns its SupplyCategoryId.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing or too long name, or name already in use.
    /// </exception>
    public int Create(SupplyCategory category)
    {
        throw new NotImplementedException();
    }

    /// <exception cref="BusinessException">
    /// The category does not exist, missing or too long name, or name already in use.
    /// </exception>
    public void Update(SupplyCategory category)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes a category that has no supplies.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The category does not exist or still has supplies.
    /// </exception>
    public void Delete(int supplyCategoryId)
    {
        throw new NotImplementedException();
    }
}