using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Product option management. Which products allow each option is managed
/// through ProductService.SetAllowedOptions.
/// </summary>
public class ProductOptionService
{
    /// <summary>Returns all options ordered by group and name.</summary>
    public List<ProductOption> GetAll()
    {
        throw new NotImplementedException();
    }

    public ProductOption? GetById(int productOptionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates an option and returns its ProductOptionId.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing or too long group or name, or ExtraPrice out of range.
    /// </exception>
    public int Create(ProductOption option)
    {
        throw new NotImplementedException();
    }

    /// <exception cref="BusinessException">
    /// The option does not exist, missing or too long group or name, or
    /// ExtraPrice out of range.
    /// </exception>
    public void Update(ProductOption option)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes an option that appears in no order. Its assignments to products
    /// are deleted with it, in a single transaction.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The option does not exist or appears in an order.
    /// </exception>
    public void Delete(int productOptionId)
    {
        throw new NotImplementedException();
    }
}
