using MiCoffee.Data;
using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Supply management: catalog data, stock changes and low-stock alerts.
/// </summary>
public class SupplyService
{
    // Limit of the DECIMAL(10,2) columns.
    private const decimal MaxStock = 99999999.99m;

    private readonly SupplyRepository _repository = new();
    private readonly SupplyCategoryRepository _categoryRepository = new();
    private readonly SupplierRepository _supplierRepository = new();

    /// <summary>
    /// Returns all supplies ordered by category name and supply name. Category
    /// and Supplier are loaded.
    /// </summary>
    public List<Supply> GetAll()
    {
        return _repository.GetAll();
    }

    /// <summary>
    /// Returns one supply with Category and Supplier loaded, or null.
    /// </summary>
    public Supply? GetById(int supplyId)
    {
        return _repository.GetById(supplyId);
    }

    /// <summary>
    /// Creates a supply and returns its SupplyId. CurrentStock is the initial
    /// stock.
    /// </summary>
    /// <exception cref="BusinessException">
    /// Missing or too long name or unit, CurrentStock or MinimumStock out of
    /// range, or the category or the supplier does not exist.
    /// </exception>
    public int Create(Supply supply)
    {
        NormalizeAndValidate(supply);

        if (supply.CurrentStock < 0 || supply.CurrentStock > MaxStock)
        {
            throw new BusinessException("La existencia debe estar entre 0 y 99,999,999.99.");
        }

        EnsureCategoryExists(supply);
        EnsureSupplierExists(supply);

        return _repository.Insert(supply);
    }

    /// <summary>
    /// Updates category, supplier, name, unit and minimum stock. CurrentStock
    /// is ignored: use AddStock and SetStock.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The supply does not exist, missing or too long name or unit,
    /// MinimumStock out of range, or the category or the supplier does not
    /// exist.
    /// </exception>
    public void Update(Supply supply)
    {
        NormalizeAndValidate(supply);

        if (_repository.GetById(supply.SupplyId) is null)
        {
            throw new BusinessException("El insumo no existe.");
        }

        EnsureCategoryExists(supply);
        EnsureSupplierExists(supply);

        _repository.Update(supply);
    }

    /// <summary>
    /// Deletes a supply. No other record depends on it.
    /// </summary>
    /// <exception cref="BusinessException">The supply does not exist.</exception>
    public void Delete(int supplyId)
    {
        if (_repository.GetById(supplyId) is null)
        {
            throw new BusinessException("El insumo no existe.");
        }

        _repository.Delete(supplyId);
    }

    /// <summary>
    /// Registers a stock entry: adds quantity to the current stock.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The supply does not exist, quantity is not greater than 0, or the
    /// resulting stock exceeds the maximum.
    /// </exception>
    public void AddStock(int supplyId, decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessException("La cantidad de la entrada debe ser mayor a 0.");
        }

        Supply? supply = _repository.GetById(supplyId);
        if (supply is null)
        {
            throw new BusinessException("El insumo no existe.");
        }

        // Compared this way so a huge quantity cannot overflow the addition.
        if (quantity > MaxStock - supply.CurrentStock)
        {
            throw new BusinessException("La existencia no puede superar 99,999,999.99.");
        }

        _repository.AddStock(supplyId, quantity);
    }

    /// <summary>
    /// Registers an adjustment: replaces the current stock with newStock.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The supply does not exist or newStock is out of range.
    /// </exception>
    public void SetStock(int supplyId, decimal newStock)
    {
        if (newStock < 0 || newStock > MaxStock)
        {
            throw new BusinessException("La existencia debe estar entre 0 y 99,999,999.99.");
        }

        if (_repository.GetById(supplyId) is null)
        {
            throw new BusinessException("El insumo no existe.");
        }

        _repository.SetStock(supplyId, newStock);
    }

    /// <summary>
    /// Supplies in alert (CurrentStock at or below MinimumStock), largest
    /// shortage first and then by name. Category and Supplier are loaded;
    /// Supplier is null when none is assigned. Alerts are not stored.
    /// </summary>
    public List<Supply> GetAlerts()
    {
        return _repository.GetAlerts();
    }

    // Validates the fields Create and Update share. CurrentStock is checked
    // only by Create, because Update ignores it.
    private static void NormalizeAndValidate(Supply supply)
    {
        // Model binding turns empty form fields into null.
        supply.Name = (supply.Name ?? string.Empty).Trim();
        supply.Unit = (supply.Unit ?? string.Empty).Trim();

        if (supply.Name.Length == 0)
        {
            throw new BusinessException("El nombre del insumo es obligatorio.");
        }

        if (supply.Name.Length > 100)
        {
            throw new BusinessException("El nombre del insumo admite hasta 100 caracteres.");
        }

        if (supply.Unit.Length == 0)
        {
            throw new BusinessException("La unidad de medida es obligatoria.");
        }

        if (supply.Unit.Length > 20)
        {
            throw new BusinessException("La unidad de medida admite hasta 20 caracteres.");
        }

        if (supply.MinimumStock < 0 || supply.MinimumStock > MaxStock)
        {
            throw new BusinessException("El límite mínimo debe estar entre 0 y 99,999,999.99.");
        }
    }

    private void EnsureCategoryExists(Supply supply)
    {
        if (_categoryRepository.GetById(supply.SupplyCategoryId) is null)
        {
            throw new BusinessException("La categoría seleccionada no existe.");
        }
    }

    // The supplier is optional: only a given SupplierId is checked.
    private void EnsureSupplierExists(Supply supply)
    {
        if (supply.SupplierId is int supplierId
            && _supplierRepository.GetById(supplierId) is null)
        {
            throw new BusinessException("El proveedor seleccionado no existe.");
        }
    }
}