using MiCoffee.Data;
using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Product option management. Which products allow each option is managed
/// through ProductService.SetAllowedOptions.
/// </summary>
public class ProductOptionService
{
    private const decimal MaxPrice = 99999999.99m;

    private readonly ProductOptionRepository _repository = new();

    /// <summary>Returns all options ordered by group and name.</summary>
    public List<ProductOption> GetAll()
    {
        return _repository.GetAll();
    }

    public ProductOption? GetById(int productOptionId)
    {
        return _repository.GetById(productOptionId);
    }

    /// <summary>
    /// Creates an option and returns its ProductOptionId.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing or too long group or name, or ExtraPrice out of range.
    /// </exception>
    public int Create(ProductOption option)
    {
        NormalizeAndValidate(option);

        return _repository.Insert(option);
    }

    /// <exception cref="BusinessException">
    /// The option does not exist, missing or too long group or name, or
    /// ExtraPrice out of range.
    /// </exception>
    public void Update(ProductOption option)
    {
        NormalizeAndValidate(option);

        if (_repository.GetById(option.ProductOptionId) is null)
        {
            throw new BusinessException("La opción no existe.");
        }

        _repository.Update(option);
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
        if (_repository.GetById(productOptionId) is null)
        {
            throw new BusinessException("La opción no existe.");
        }

        if (!_repository.Delete(productOptionId))
        {
            throw new BusinessException(
                "No se puede eliminar la opción porque aparece en órdenes. " +
                "Quítela de sus productos.");
        }
    }

    private static void NormalizeAndValidate(ProductOption option)
    {
        // Model binding turns empty form fields into null.
        option.OptionGroup = (option.OptionGroup ?? string.Empty).Trim();
        option.Name = (option.Name ?? string.Empty).Trim();

        if (option.OptionGroup.Length == 0)
        {
            throw new BusinessException("El grupo de la opción es obligatorio.");
        }

        if (option.OptionGroup.Length > 50)
        {
            throw new BusinessException("El grupo de la opción admite hasta 50 caracteres.");
        }

        if (option.Name.Length == 0)
        {
            throw new BusinessException("El nombre de la opción es obligatorio.");
        }

        if (option.Name.Length > 50)
        {
            throw new BusinessException("El nombre de la opción admite hasta 50 caracteres.");
        }

        if (option.ExtraPrice < 0 || option.ExtraPrice > MaxPrice)
        {
            throw new BusinessException(
                "El precio adicional debe estar entre 0 y 99,999,999.99.");
        }
    }
}