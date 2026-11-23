using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.SupplyCategories;

public class EditModel : PageModel
{
    private readonly SupplyCategoryService _categoryService = new();

    [BindProperty]
    public SupplyCategory Category { get; set; } = new();

    public bool CanEdit { get; private set; }

    public string? Error { get; private set; }

    public void OnGet(int id)
    {
        try
        {
            SupplyCategory? category = _categoryService.GetById(id);
            if (category is null)
            {
                Error = "La categoría no existe.";
                return;
            }

            Category = category;
            CanEdit = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo cargar la categoría. Inténtalo de nuevo.";
        }
    }

    public IActionResult OnPost(int id)
    {
        // The id comes from the route, never from the form.
        Category.SupplyCategoryId = id;
        CanEdit = true;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            _categoryService.Update(Category);
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
            return Page();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo guardar la categoría. Inténtalo de nuevo.";
            return Page();
        }

        TempData["Message"] = "Categoría actualizada.";
        return RedirectToPage("/Staff/SupplyCategories/Index");
    }
}
