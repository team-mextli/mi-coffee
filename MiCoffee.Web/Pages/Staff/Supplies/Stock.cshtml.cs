using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Supplies;

public class StockModel : PageModel
{
    private readonly SupplyService _supplyService = new();

    public Supply? Supply { get; private set; }

    public string? Error { get; private set; }

    public void OnGet(int id)
    {
        Load(id);
    }

    public IActionResult OnPostAdd(int id, decimal? quantity)
    {
        if (quantity is null)
        {
            Load(id);
            Error ??= "Escribe la cantidad de la entrada.";
            return Page();
        }

        return Run(id, () => _supplyService.AddStock(id, quantity.Value), "Entrada registrada.");
    }

    public IActionResult OnPostSet(int id, decimal? newStock)
    {
        if (newStock is null)
        {
            Load(id);
            Error ??= "Escribe la existencia real.";
            return Page();
        }

        return Run(id, () => _supplyService.SetStock(id, newStock.Value), "Ajuste guardado.");
    }

    private IActionResult Run(int id, Action change, string successMessage)
    {
        try
        {
            change();
        }
        catch (BusinessException ex)
        {
            Load(id);
            Error ??= ex.Message;
            return Page();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Load(id);
            Error ??= "No se pudo guardar el cambio. Inténtalo de nuevo.";
            return Page();
        }

        TempData["Message"] = successMessage;
        return RedirectToPage(new { id });
    }

    private void Load(int id)
    {
        try
        {
            Supply = _supplyService.GetById(id);
            if (Supply is null)
            {
                Error = "El insumo no existe.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Supply = null;
            Error = "No se pudo cargar el insumo. Inténtalo de nuevo.";
        }
    }
}
