using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using MiCoffee.Web.Cart;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Order;

public class CustomizeModel : PageModel
{
    private readonly ProductService _productService = new();
    private readonly OrderService _orderService = new();

    public Product? Product { get; private set; }

    public List<IGrouping<string, ProductOption>> OptionGroups { get; private set; } = new();

    public string? Error { get; private set; }

    [BindProperty]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public int Quantity { get; set; } = 1;

    [BindProperty]
    [StringLength(255, ErrorMessage = "La nota admite hasta 255 caracteres.")]
    public string? Note { get; set; }

    [BindProperty]
    public List<int> SelectedOptionIds { get; set; } = new();

    public void OnGet(int productId)
    {
        Load(productId);
    }

    public IActionResult OnPost(int productId)
    {
        Load(productId);
        if (Product is null || !ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            OrderItem item = new()
            {
                ProductId = productId,
                Quantity = Quantity,
                Note = Note,
                Options = SelectedOptionIds.Distinct()
                    .Select(id => new OrderItemOption { ProductOptionId = id })
                    .ToList()
            };

            // Validates the line against the catalog; the prices are not kept.
            _orderService.CalculateLineTotal(item);

            List<OrderItem> cart = CartSession.Get(HttpContext.Session);
            cart.Add(item);
            CartSession.Save(HttpContext.Session, cart);
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
            return Page();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo agregar el producto. Inténtalo de nuevo.";
            return Page();
        }

        TempData["Message"] = "Producto agregado al carrito.";
        return RedirectToPage("/Order/Cart");
    }

    private void Load(int productId)
    {
        try
        {
            Product? product = _productService.GetById(productId);
            if (product is null)
            {
                Error = "El producto no existe.";
                return;
            }

            if (!product.IsAvailable)
            {
                Error = $"El producto «{product.Name}» no está disponible.";
                return;
            }

            Product = product;
            OptionGroups = product.AllowedOptions
                .OrderBy(option => option.OptionGroup)
                .ThenBy(option => option.Name)
                .GroupBy(option => option.OptionGroup)
                .ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo cargar el producto. Inténtalo de nuevo.";
        }
    }
}
