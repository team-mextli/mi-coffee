using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Orders;

public class IndexModel : PageModel
{
    private readonly OrderService _orderService = new();

    public List<CustomerOrder> Orders { get; private set; } = new();

    public string? Error { get; private set; }

    public void OnGet()
    {
        try
        {
            Orders = _orderService.GetActive();
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudieron cargar las órdenes. Inténtalo de nuevo.";
        }
    }
}
