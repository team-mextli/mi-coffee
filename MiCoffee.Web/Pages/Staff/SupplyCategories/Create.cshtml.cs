using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.SupplyCategories;

public class CreateModel : PageModel
{
    private readonly SupplyCategoryService _categoryService = new();

    [BindProperty]
    public SupplyCategory Category { get; set; } = new();

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
            _categoryService.Create(Category);
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

        TempData["Message"] = "Categoría creada.";
        return RedirectToPage("/Staff/SupplyCategories/Index");
    }
}
