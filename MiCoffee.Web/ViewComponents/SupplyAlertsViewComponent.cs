using System.Diagnostics;
using MiCoffee.Business;
using Microsoft.AspNetCore.Mvc;

namespace MiCoffee.Web.ViewComponents;

/// <summary>
/// Low-stock alerts for the staff layout. The rule lives in
/// SupplyService.GetAlerts; this component only shows its result.
/// </summary>
public class SupplyAlertsViewComponent : ViewComponent
{
    private readonly SupplyService _supplyService = new();

    public IViewComponentResult Invoke()
    {
        try
        {
            return View(_supplyService.GetAlerts());
        }
        catch (Exception ex)
        {
            // An alert failure must never break the page that shows it.
            Debug.WriteLine(ex);
            return Content(string.Empty);
        }
    }
}
