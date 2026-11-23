using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using MiCoffee.Web.Cart;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ProductService _productService = new();

    public List<IGrouping<string, Product>> Groups { get; private set; } = new();

    public int CartCount { get; private set; }

    public string? Error { get; private set; }

    public void OnGet()
    {
        try
        {
            CartCount = CartSession.Count(HttpContext.Session);

            // GetMenu returns only available products, already ordered.
            Groups = _productService.GetMenu()
                .GroupBy(product => product.Category?.Name ?? "Otros")
                .ToList();
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo cargar el menú. Inténtalo de nuevo.";
        }
    }
}
