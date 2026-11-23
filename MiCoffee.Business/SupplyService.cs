using MiCoffee.Entities;

namespace MiCoffee.Business;

/// <summary>
/// Supply management: catalog data, stock changes and low-stock alerts.
/// </summary>
public class SupplyService
{
    /// <summary>
    /// Returns all supplies ordered by category name and supply name. Category
    /// and Supplier are loaded.
    /// </summary>
    public List<Supply> GetAll()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns one supply with Category and Supplier loaded, or null.
    /// </summary>
    public Supply? GetById(int supplyId)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes a supply. No other record depends on it.
    /// </summary>
    /// <exception cref="BusinessException">The supply does not exist.</exception>
    public void Delete(int supplyId)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Registers an adjustment: replaces the current stock with newStock.
    /// </summary>
    /// <exception cref="BusinessException">
    /// The supply does not exist or newStock is out of range.
    /// </exception>
    public void SetStock(int supplyId, decimal newStock)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Supplies in alert (CurrentStock at or below MinimumStock), largest
    /// shortage first and then by name. Category and Supplier are loaded;
    /// Supplier is null when none is assigned. Alerts are not stored.
    /// </summary>
    public List<Supply> GetAlerts()
    {
        throw new NotImplementedException();
    }
}