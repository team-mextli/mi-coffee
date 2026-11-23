using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Suppliers;

public class EditModel : PageModel
{
    private readonly SupplierService _supplierService = new();

    [BindProperty]
    public Supplier Supplier { get; set; } = new();

    // False when the supplier could not be loaded: the form is not shown.
    public bool CanEdit { get; private set; }

    public string? Error { get; private set; }

    public void OnGet(int id)
    {
        try
        {
            Supplier? supplier = _supplierService.GetById(id);
            if (supplier is null)
            {
                Error = "El proveedor no existe.";
                return;
            }

            Supplier = supplier;
            CanEdit = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo cargar el proveedor. Inténtalo de nuevo.";
        }
    }

    public IActionResult OnPost(int id)
    {
        // The id comes from the route, never from the form.
        Supplier.SupplierId = id;
        CanEdit = true;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            _supplierService.Update(Supplier);
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

        TempData["Message"] = "Proveedor actualizado.";
        return RedirectToPage("/Staff/Suppliers/Index");
    }
}
