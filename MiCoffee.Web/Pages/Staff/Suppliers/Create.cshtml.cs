using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Suppliers;

public class CreateModel : PageModel
{
    private readonly SupplierService _supplierService = new();

    [BindProperty]
    public Supplier Supplier { get; set; } = new();

    public string? Error { get; private set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            _supplierService.Create(Supplier);
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
            return Page();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo guardar el proveedor. Inténtalo de nuevo.";
            return Page();
        }

        TempData["Message"] = "Proveedor creado.";
        return RedirectToPage("/Staff/Suppliers/Index");
    }
}
