using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Products;

public class IndexModel : PageModel
{
	private readonly ProductService _productService = new();

	// GetAll loads the Category of each product.
	public List<Product> Products { get; private set; } = new();

	public string? Error { get; private set; }

	public void OnGet()
	{
		try
		{
			Products = _productService.GetAll();
		}
		catch (BusinessException ex)
		{
			Error = ex.Message;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar los productos.";
		}
	}
}
