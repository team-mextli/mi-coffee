using MiCoffee.Data;
using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Product management, including the options each product allows.
/// </summary>
public class ProductService
{
    private const decimal MaxPrice = 99999999.99m;

    private readonly ProductRepository _repository = new();
    private readonly ProductCategoryRepository _categoryRepository = new();
    private readonly ProductOptionRepository _optionRepository = new();

    /// <summary>
    /// Returns all products, available or not, ordered by category name and
    /// product name. Category is loaded; AllowedOptions is not.
    /// </summary>
    public List<Product> GetAll()
    {
        return _repository.GetAll();
    }

    /// <summary>
    /// Returns one product with Category and AllowedOptions loaded.
    /// </summary>
    public Product? GetById(int productId)
    {
        return _repository.GetById(productId);
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
        NormalizeAndValidate(product);
        EnsureCategoryExists(product);

        return _repository.Insert(product);
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
        NormalizeAndValidate(product);

        if (_repository.GetById(product.ProductId) is null)
        {
            throw new BusinessException("El producto no existe.");
        }

        EnsureCategoryExists(product);

        _repository.Update(product);
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
        if (_repository.GetById(productId) is null)
        {
            throw new BusinessException("El producto no existe.");
        }

        if (!_repository.Delete(productId))
        {
            throw new BusinessException(
                "No se puede eliminar el producto porque aparece en órdenes. " +
                "Márquelo como no disponible.");
        }
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
        if (_repository.GetById(productId) is null)
        {
            throw new BusinessException("El producto no existe.");
        }

        List<int> ids = productOptionIds.Distinct().ToList();

        HashSet<int> existingIds = _optionRepository.GetAll()
            .Select(option => option.ProductOptionId)
            .ToHashSet();

        if (ids.Any(id => !existingIds.Contains(id)))
        {
            throw new BusinessException("Alguna de las opciones seleccionadas no existe.");
        }

        _repository.ReplaceAllowedOptions(productId, ids);
    }

    /// <summary>
    /// Menu for the customer interface: only available products, ordered by
    /// category name and product name, each with Category and AllowedOptions
    /// loaded. The interface groups the products by category.
    /// </summary>
    public List<Product> GetMenu()
    {
        return _repository.GetMenu();
    }

    private static void NormalizeAndValidate(Product product)
    {
        // Model binding turns empty form fields into null.
        product.Name = (product.Name ?? string.Empty).Trim();

        if (product.Name.Length == 0)
        {
            throw new BusinessException("El nombre del producto es obligatorio.");
        }

        if (product.Name.Length > 100)
        {
            throw new BusinessException("El nombre del producto admite hasta 100 caracteres.");
        }

        if (product.BasePrice < 0 || product.BasePrice > MaxPrice)
        {
            throw new BusinessException(
                "El precio base debe estar entre 0 y 99,999,999.99.");
        }
    }

    private void EnsureCategoryExists(Product product)
    {
        if (_categoryRepository.GetById(product.ProductCategoryId) is null)
        {
            throw new BusinessException("La categoría seleccionada no existe.");
        }
    }
}