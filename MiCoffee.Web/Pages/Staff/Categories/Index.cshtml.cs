using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Categories;

public class IndexModel : PageModel
{
	private readonly ProductCategoryService _categoryService = new();

	public List<ProductCategory> Categories { get; private set; } = new();

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
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar las categorías.";
		}
	}
}
