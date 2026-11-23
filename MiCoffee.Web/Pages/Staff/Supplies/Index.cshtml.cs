using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Supplies;

public class IndexModel : PageModel
{
    private readonly SupplyService _supplyService = new();

    public List<Supply> Supplies { get; private set; } = new();

    // Supplies in alert, as decided by Business. The page does not compare stocks.
    public HashSet<int> AlertIds { get; private set; } = new();

    public string? Error { get; private set; }

    public void OnGet()
    {
        try
        {
            Supplies = _supplyService.GetAll();
            AlertIds = _supplyService.GetAlerts().Select(s => s.SupplyId).ToHashSet();
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo cargar el inventario. Inténtalo de nuevo.";
        }
    }
}
