using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Supplies;

public class CreateModel : PageModel
{
    private readonly SupplyService _supplyService = new();
    private readonly SupplyCategoryService _categoryService = new();
    private readonly SupplierService _supplierService = new();

    [BindProperty]
    public Supply Supply { get; set; } = new();

    public SelectList Categories { get; private set; } = new(Array.Empty<SupplyCategory>());

    public SelectList Suppliers { get; private set; } = new(Array.Empty<Supplier>());

    // False when the lists could not be loaded: the form is not shown.
    public bool CanEdit { get; private set; }

    public string? Error { get; private set; }

    public void OnGet()
    {
        LoadLists();
    }

    public IActionResult OnPost()
    {
        LoadLists();
        if (!CanEdit || !ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            _supplyService.Create(Supply);
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
            return Page();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo guardar el insumo. Inténtalo de nuevo.";
            return Page();
        }

        TempData["Message"] = "Insumo creado.";
        return RedirectToPage("/Staff/Supplies/Index");
    }

    private void LoadLists()
    {
        try
        {
            Categories = new SelectList(_categoryService.GetAll(), "SupplyCategoryId", "Name");
            Suppliers = new SelectList(_supplierService.GetAll(), "SupplierId", "Name");
            CanEdit = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo cargar el formulario. Inténtalo de nuevo.";
        }
    }
}
