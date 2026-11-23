using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.Supplies;

public class EditModel : PageModel
{
    private readonly SupplyService _supplyService = new();
    private readonly SupplyCategoryService _categoryService = new();
    private readonly SupplierService _supplierService = new();

    [BindProperty]
    public Supply Supply { get; set; } = new();

    public SelectList Categories { get; private set; } = new(Array.Empty<SupplyCategory>());

    public SelectList Suppliers { get; private set; } = new(Array.Empty<Supplier>());

    // Shown as text only: the stock changes through the Stock page.
    public decimal ShownStock { get; private set; }

    // False when the supply could not be loaded: the form is not shown.
    public bool CanEdit { get; private set; }

    public string? Error { get; private set; }

    public void OnGet(int id)
    {
        try
        {
            Supply? supply = _supplyService.GetById(id);
            if (supply is null)
            {
                Error = "El insumo no existe.";
                return;
            }

            Supply = supply;
            ShownStock = supply.CurrentStock;
            LoadLists();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo cargar el insumo. Inténtalo de nuevo.";
        }
    }

    public IActionResult OnPost(int id)
    {
        // The id comes from the route, never from the form.
        Supply.SupplyId = id;

        try
        {
            ShownStock = _supplyService.GetById(id)?.CurrentStock ?? 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        LoadLists();
        if (!CanEdit || !ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // CurrentStock is ignored by Business on update.
            _supplyService.Update(Supply);
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

        TempData["Message"] = "Insumo actualizado.";
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
