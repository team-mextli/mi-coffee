using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using MiCoffee.Business;
using MiCoffee.Entities;
using MiCoffee.Web.Cart;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiCoffee.Web.Pages.Order;

public class CartModel : PageModel
{
    private readonly ProductService _productService = new();
    private readonly OrderService _orderService = new();

    public List<CartLine> Lines { get; private set; } = new();

    public decimal? Total { get; private set; }

    public bool CanConfirm { get; private set; }

    public string? Error { get; private set; }

    [BindProperty]
    [StringLength(50, ErrorMessage = "El nombre admite hasta 50 caracteres.")]
    public string? CustomerName { get; set; }

    public void OnGet()
    {
        LoadCart();
    }

    public IActionResult OnPostRemove(int index)
    {
        List<OrderItem> cart = CartSession.Get(HttpContext.Session);
        if (index >= 0 && index < cart.Count)
        {
            cart.RemoveAt(index);
            CartSession.Save(HttpContext.Session, cart);
        }

        return RedirectToPage();
    }

    public IActionResult OnPostConfirm()
    {
        if (!ModelState.IsValid)
        {
            LoadCart();
            return Page();
        }

        try
        {
            List<OrderItem> cart = CartSession.Get(HttpContext.Session);

            CustomerOrder order = new() { CustomerName = CustomerName };
            order.Items.AddRange(cart);

            // Business reads the prices from the catalog and calculates the total.
            int orderId = _orderService.Create(order);

            CartSession.Clear(HttpContext.Session);
            return RedirectToPage("/Order/Confirmation", new { id = orderId });
        }
        catch (BusinessException ex)
        {
            LoadCart();
            Error = ex.Message;
            return Page();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            LoadCart();
            Error = "No se pudo enviar el pedido. Inténtalo de nuevo.";
            return Page();
        }
    }

    private void LoadCart()
    {
        try
        {
            List<OrderItem> cart = CartSession.Get(HttpContext.Session);
            CanConfirm = cart.Count > 0;

            for (int i = 0; i < cart.Count; i++)
            {
                OrderItem item = cart[i];
                Product? product = _productService.GetById(item.ProductId);

                CartLine line = new()
                {
                    Index = i,
                    Item = item,
                    ProductName = product?.Name ?? "Producto no disponible"
                };

                if (product is not null)
                {
                    line.OptionNames = item.Options
                        .Select(o => product.AllowedOptions
                            .FirstOrDefault(a => a.ProductOptionId == o.ProductOptionId)?.Name)
                        .Where(name => name is not null)
                        .Select(name => name!)
                        .ToList();
                }

                try
                {
                    line.LineTotal = _orderService.CalculateLineTotal(item);
                }
                catch (BusinessException ex)
                {
                    // The product or an option changed after it was added.
                    Error = ex.Message + " Quita ese producto del carrito.";
                    CanConfirm = false;
                }

                Lines.Add(line);
            }

            if (CanConfirm)
            {
                CustomerOrder order = new();
                order.Items.AddRange(cart);
                Total = _orderService.CalculateTotal(order);
            }
        }
        catch (BusinessException ex)
        {
            Error = ex.Message;
            CanConfirm = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudo cargar el carrito. Inténtalo de nuevo.";
            CanConfirm = false;
        }
    }

    public class CartLine
    {
        public int Index { get; set; }

        public OrderItem Item { get; set; } = new();

        public string ProductName { get; set; } = string.Empty;

        public List<string> OptionNames { get; set; } = new();

        public decimal? LineTotal { get; set; }
    }
}
