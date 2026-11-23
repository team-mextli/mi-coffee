using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Categories;

public class CreateModel : PageModel
{
	private readonly ProductCategoryService _categoryService = new();

	[BindProperty]
	public ProductCategory Category { get; set; } = new();

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
			TempData["Message"] = "Categoría creada.";
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
