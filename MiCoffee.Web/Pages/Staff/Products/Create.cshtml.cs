using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Products;

public class CreateModel : PageModel
{
	private readonly ProductService _productService = new();
	private readonly ProductCategoryService _categoryService = new();
	private readonly ProductOptionService _optionService = new();

	[BindProperty]
	public Product Product { get; set; } = new();

	// Ids of the options checked in the form. The allowed options are not
	// part of Product: they are saved with ProductService.SetAllowedOptions.
	[BindProperty]
	public List<int> SelectedOptionIds { get; set; } = new();

	public List<SelectListItem> Categories { get; private set; } = new();

	// All options, grouped by OptionGroup, for the checkboxes.
	public List<IGrouping<string, ProductOption>> OptionGroups { get; private set; } = new();

	// True when the form can be shown.
	public bool Loaded { get; private set; }

	// True when there is no category yet, so no product can be created.
	public bool NoCategories { get; private set; }

	public string? Error { get; private set; }

	public void OnGet()
	{
		Loaded = LoadLists();

		if (Loaded && Categories.Count == 0)
		{
			Loaded = false;
			NoCategories = true;
			Error = "Primero crea una categoría para poder agregar productos.";
		}
	}

	public IActionResult OnPost()
	{
		Loaded = LoadLists();
		if (!Loaded)
		{
			return Page();
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		int productId;
		try
		{
			productId = _productService.Create(Product);
		}
		catch (BusinessException ex)
		{
			Error = ex.Message;
			return Page();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible guardar el producto.";
			return Page();
		}

		// Creating the product and saving its options are two separate
		// operations. If the second one fails the product already exists, so
		// the user is sent to Edit instead of back to this form, where
		// saving again would create the product twice.
		try
		{
			_productService.SetAllowedOptions(productId, SelectedOptionIds);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			TempData["Error"] = ex is BusinessException
				? $"El producto se creó, pero sus opciones permitidas no se guardaron: {ex.Message}"
				: "El producto se creó, pero sus opciones permitidas no se guardaron.";
			return RedirectToPage("./Edit", new { id = productId });
		}

		TempData["Message"] = "Producto creado.";
		return RedirectToPage("./Index");
	}

	private bool LoadLists()
	{
		try
		{
			Categories = _categoryService.GetAll()
				.Select(category => new SelectListItem(category.Name, category.ProductCategoryId.ToString()))
				.ToList();

			OptionGroups = _optionService.GetAll()
				.GroupBy(option => option.OptionGroup)
				.ToList();

			return true;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar las categorías y las opciones.";
			return false;
		}
	}
}
