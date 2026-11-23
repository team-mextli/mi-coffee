using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.SupplyCategories;

public class DeleteModel : PageModel
{
    private readonly SupplyCategoryService _categoryService = new();

    public SupplyCategory? Category { get; private set; }

    public bool CanDelete { get; private set; }

    public string? Error { get; private set; }

    public void OnGet(int id)
    {
        Load(id);
    }

    public IActionResult OnPost(int id)
    {
        Load(id);
        if (Category is null)
        {
            return Page();
        }

        try
        {
            _categoryService.Delete(id);
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
            Error = "No se pudo eliminar la categoría. Inténtalo de nuevo.";
            CanDelete = false;
            return Page();
        }

        TempData["Message"] = "Categoría eliminada.";
        return RedirectToPage("/Staff/SupplyCategories/Index");
    }

    private void Load(int id)
    {
        try
        {
            Category = _categoryService.GetById(id);
            if (Category is null)
            {
                Error = "La categoría no existe.";
                return;
            }

            CanDelete = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Category = null;
            Error = "No se pudo cargar la categoría. Inténtalo de nuevo.";
        }
    }
}
