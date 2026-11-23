using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Categories;

public class DeleteModel : PageModel
{
	private readonly ProductCategoryService _categoryService = new();

	public ProductCategory? Category { get; private set; }

	// True while the delete button can be offered. Business rejects the
	// deletion when the category has products, and then it is hidden.
	public bool CanDelete { get; private set; }

	public string? Error { get; private set; }

	public void OnGet(int id)
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
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar la categoría.";
		}
	}

	public IActionResult OnPost(int id)
	{
		try
		{
			_categoryService.Delete(id);
			TempData["Message"] = "Categoría eliminada.";
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
			Error = "No fue posible eliminar la categoría.";
			CanDelete = true;
		}

		ReloadCategory(id);
		return Page();
	}

	// Loads the category to show its name again after a failed deletion.
	private void ReloadCategory(int id)
	{
		try
		{
			Category = _categoryService.GetById(id);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
		}
	}
}
