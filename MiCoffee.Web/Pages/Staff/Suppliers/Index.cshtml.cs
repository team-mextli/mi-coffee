using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Suppliers;

public class IndexModel : PageModel
{
    private readonly SupplierService _supplierService = new();

    public List<Supplier> Suppliers { get; private set; } = new();

    public string? Error { get; private set; }

    public void OnGet()
    {
        try
        {
            Suppliers = _supplierService.GetAll();
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudieron cargar los proveedores. Inténtalo de nuevo.";
        }
    }
}
