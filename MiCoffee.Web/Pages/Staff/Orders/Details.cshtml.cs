using System.Diagnostics;
using System.Security.Claims;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Orders;

public class DetailsModel : PageModel
{
    private readonly OrderService _orderService = new();

    public CustomerOrder? Order { get; private set; }

    public IReadOnlyList<OrderStatus> NextStatuses { get; private set; } = Array.Empty<OrderStatus>();

    public string? Error { get; private set; }

    public void OnGet(int id)
    {
        Load(id);
    }

    public IActionResult OnPostChangeStatus(int id, OrderStatus newStatus)
    {
        // The user comes from the session claim, never from the form.
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
        {
            Load(id);
            Error = "No se pudo identificar al usuario. Cierra sesión y vuelve a entrar.";
            return Page();
        }

        try
        {
            _orderService.ChangeStatus(id, newStatus, userId);
        }
        catch (BusinessException ex)
        {
            Load(id);
            Error = ex.Message;
            return Page();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Load(id);
            Error = "No se pudo cambiar el estado. Inténtalo de nuevo.";
            return Page();
        }

        TempData["Message"] = "Estado actualizado.";
        return RedirectToPage(new { id });
    }

    private void Load(int id)
    {
        try
        {
            Order = _orderService.GetById(id);
            if (Order is null)
            {
                Error = "La orden no existe.";
                return;
            }

            // Business says which changes are valid; the page repeats no rule.
            NextStatuses = _orderService.GetNextStatuses(Order.Status);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Order = null;
            Error = "No se pudo cargar la orden. Inténtalo de nuevo.";
        }
    }
}
