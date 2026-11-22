using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Product management, including the options each product allows.
/// </summary>
public class ProductService
{
    /// <summary>
    /// Returns all products, available or not, ordered by category name and
    /// product name. Category is loaded; AllowedOptions is not.
    /// </summary>
    public List<Product> GetAll()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns one product with Category and AllowedOptions loaded.
    /// </summary>
    public Product? GetById(int productId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a product and returns its ProductId. AllowedOptions is ignored.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing or too long name, BasePrice out of range, or the category does
    /// not exist.
    /// </exception>
    public int Create(Product product)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates category, name, base price and availability. AllowedOptions is
    /// ignored.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The product does not exist, missing or too long name, BasePrice out of
    /// range, or the category does not exist.
    /// </exception>
    public void Update(Product product)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes a product that appears in no order. Its allowed options are
    /// deleted with it, in a single transaction.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The product does not exist or appears in an order.
    /// </exception>
    public void Delete(int productId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Replaces the options allowed for a product with the given ones, in a
    /// single transaction. An empty list leaves the product without options.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The product does not exist or an option does not exist.
    /// </exception>
    public void SetAllowedOptions(int productId, IEnumerable<int> productOptionIds)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Menu for the customer interface: only available products, ordered by
    /// category name and product name, each with Category and AllowedOptions
    /// loaded. The interface groups the products by category.
    /// </summary>
    public List<Product> GetMenu()
    {
        throw new NotImplementedException();
    }
}
