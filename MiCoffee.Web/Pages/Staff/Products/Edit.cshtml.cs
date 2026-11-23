using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Products;

public class EditModel : PageModel
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

	// True when the form can be shown (the product and the lists were loaded).
	public bool Loaded { get; private set; }

	public string? Error { get; private set; }

	public void OnGet(int id)
	{
		// Set by Create when the product was saved but its options were not.
		Error = TempData["Error"] as string;

		if (!LoadLists())
		{
			return;
		}

		try
		{
			// GetById loads Category and AllowedOptions.
			Product? product = _productService.GetById(id);
			if (product is null)
			{
				Error = "El producto no existe.";
				return;
			}

			Product = product;
			SelectedOptionIds = product.AllowedOptions
				.Select(option => option.ProductOptionId)
				.ToList();
			Loaded = true;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar el producto.";
		}
	}

	public IActionResult OnPost(int id)
	{
		// The id comes from the route, never from the form.
		Product.ProductId = id;

		Loaded = LoadLists();
		if (!Loaded)
		{
			return Page();
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		try
		{
			_productService.Update(Product);
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

		// The product data and its options are saved by two separate
		// operations: if this one fails, the data above is already saved.
		try
		{
			_productService.SetAllowedOptions(id, SelectedOptionIds);
		}
		catch (BusinessException ex)
		{
			Error = $"Los datos del producto se guardaron, pero las opciones permitidas no: {ex.Message}";
			return Page();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "Los datos del producto se guardaron, pero no fue posible guardar las opciones permitidas.";
			return Page();
		}

		TempData["Message"] = "Producto actualizado.";
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
