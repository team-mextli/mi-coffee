using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Order;

public class ConfirmationModel : PageModel
{
    // Only the order number is shown: the order itself is not read, so the
    // customer interface never exposes the data of other orders.
    public int OrderId { get; private set; }

    public IActionResult OnGet()
    {
        if (!int.TryParse(TempData["OrderId"] as string, out int id))
        {
            return RedirectToPage("/Index");
        }

        OrderId = id;
        return Page();
    }
}