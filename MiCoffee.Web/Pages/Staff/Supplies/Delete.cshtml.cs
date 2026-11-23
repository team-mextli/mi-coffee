using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Supplies;

public class DeleteModel : PageModel
{
    private readonly SupplyService _supplyService = new();

    public Supply? Supply { get; private set; }

    public bool CanDelete { get; private set; }

    public string? Error { get; private set; }

    public void OnGet(int id)
    {
        Load(id);
    }

    public IActionResult OnPost(int id)
    {
        Load(id);
        if (Supply is null)
        {
            return Page();
        }

        try
        {
            _supplyService.Delete(id);
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
            Error = "No se pudo eliminar el insumo. Inténtalo de nuevo.";
            CanDelete = false;
            return Page();
        }

        TempData["Message"] = "Insumo eliminado.";
        return RedirectToPage("/Staff/Supplies/Index");
    }

    private void Load(int id)
    {
        try
        {
            Supply = _supplyService.GetById(id);
            if (Supply is null)
            {
                Error = "El insumo no existe.";
                return;
            }

            CanDelete = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Supply = null;
            Error = "No se pudo cargar el insumo. Inténtalo de nuevo.";
        }
    }
}
