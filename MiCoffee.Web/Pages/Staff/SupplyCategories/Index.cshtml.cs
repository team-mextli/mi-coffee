using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Staff.SupplyCategories;

public class IndexModel : PageModel
{
    private readonly SupplyCategoryService _categoryService = new();

    public List<SupplyCategory> Categories { get; private set; } = new();

    public string? Error { get; private set; }

    public void OnGet()
    {
        try
        {
            Categories = _categoryService.GetAll();
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudieron cargar las categorías. Inténtalo de nuevo.";
        }
    }
}
