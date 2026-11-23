using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Categories;

public class EditModel : PageModel
{
	private readonly ProductCategoryService _categoryService = new();

	[BindProperty]
	public ProductCategory Category { get; set; } = new();

	// True when the form can be shown (the category was loaded).
	public bool Loaded { get; private set; }

	public string? Error { get; private set; }

	public void OnGet(int id)
	{
		try
		{
			ProductCategory? category = _categoryService.GetById(id);
			if (category is null)
			{
				Error = "La categoría no existe.";
				return;
			}

			Category = category;
			Loaded = true;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar la categoría.";
		}
	}

	public IActionResult OnPost(int id)
	{
		// The id comes from the route, never from the form.
		Category.ProductCategoryId = id;
		Loaded = true;

		if (!ModelState.IsValid)
		{
			return Page();
		}

		try
		{
			_categoryService.Update(Category);
			TempData["Message"] = "Categoría actualizada.";
			return RedirectToPage("./Index");
		}
		catch (BusinessException ex)
		{
			Error = ex.Message;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible guardar la categoría.";
		}

		return Page();
	}
}
