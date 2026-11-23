using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Suppliers;

public class DeleteModel : PageModel
{
    private readonly SupplierService _supplierService = new();

    public Supplier? Supplier { get; private set; }

    // Business only reports that a supplier is in use when the deletion is
    // attempted, so the button is hidden after a rejected attempt.
    public bool CanDelete { get; private set; }

    public string? Error { get; private set; }

    public void OnGet(int id)
    {
        Load(id);
    }

    public IActionResult OnPost(int id)
    {
        Load(id);
        if (Supplier is null)
        {
            return Page();
        }

        try
        {
            _supplierService.Delete(id);
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
            CanDelete = false;
            return Page();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo eliminar el proveedor. Inténtalo de nuevo.";
            CanDelete = false;
            return Page();
        }

        TempData["Message"] = "Proveedor eliminado.";
        return RedirectToPage("/Staff/Suppliers/Index");
    }

    private void Load(int id)
    {
        try
        {
            Supplier = _supplierService.GetById(id);
            if (Supplier is null)
            {
                Error = "El proveedor no existe.";
                return;
            }

            CanDelete = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Supplier = null;
            Error = "No se pudo cargar el proveedor. Inténtalo de nuevo.";
        }
    }
}
