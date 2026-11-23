using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Orders;

public class HistoryModel : PageModel
{
    private readonly OrderService _orderService = new();

    [BindProperty(SupportsGet = true)]
    public DateOnly? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public OrderStatus? Status { get; set; }

    public List<CustomerOrder> Orders { get; private set; } = new();

    public string? Error { get; private set; }

    public void OnGet()
    {
        try
        {
            Orders = _orderService.Search(FromDate, ToDate, Status);
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo cargar el historial. Inténtalo de nuevo.";
        }
    }
}
