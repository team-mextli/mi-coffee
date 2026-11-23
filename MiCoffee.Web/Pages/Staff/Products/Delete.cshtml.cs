using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Products;

public class DeleteModel : PageModel
{
	private readonly ProductService _productService = new();

	public Product? Product { get; private set; }

	// True while the delete button can be offered. Business rejects the
	// deletion when the product appears in an order, and then it is hidden
	// and the user is pointed to Edit to mark the product as unavailable.
	public bool CanDelete { get; private set; }

	public string? Error { get; private set; }

	public void OnGet(int id)
	{
		try
		{
			Product = _productService.GetById(id);
			if (Product is null)
			{
				Error = "El producto no existe.";
				return;
			}

			CanDelete = true;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar el producto.";
		}
	}

	public IActionResult OnPost(int id)
	{
		try
		{
			_productService.Delete(id);
			TempData["Message"] = "Producto eliminado.";
			return RedirectToPage("./Index");
		}
		catch (BusinessException ex)
		{
			Error = ex.Message;
			CanDelete = false;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible eliminar el producto.";
			CanDelete = true;
		}

		ReloadProduct(id);
		return Page();
	}

	// Loads the product to show its data again after a failed deletion.
	private void ReloadProduct(int id)
	{
		try
		{
			Product = _productService.GetById(id);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
		}
	}
}
