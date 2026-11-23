using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Options;

public class EditModel : PageModel
{
	private readonly ProductOptionService _optionService = new();

	[BindProperty]
	public ProductOption ProductOption { get; set; } = new();

	// Existing groups, offered as suggestions so the same group is not
	// written in two different ways.
	public List<string> Groups { get; private set; } = new();

	// True when the form can be shown (the option was loaded).
	public bool Loaded { get; private set; }

	public string? Error { get; private set; }

	public void OnGet(int id)
	{
		try
		{
			ProductOption? option = _optionService.GetById(id);
			if (option is null)
			{
				Error = "La opción no existe.";
				return;
			}

			ProductOption = option;
			Loaded = true;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar la opción.";
			return;
		}

		LoadGroups();
	}

	public IActionResult OnPost(int id)
	{
		// The id comes from the route, never from the form.
		ProductOption.ProductOptionId = id;
		Loaded = true;
		LoadGroups();

		if (!ModelState.IsValid)
		{
			return Page();
		}

		try
		{
			_optionService.Update(ProductOption);
			TempData["Message"] = "Opción actualizada.";
			return RedirectToPage("./Index");
		}
		catch (BusinessException ex)
		{
			Error = ex.Message;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible guardar la opción.";
		}

		return Page();
	}

	// The suggestions are optional: if they cannot be loaded, the form still
	// works without them.
	private void LoadGroups()
	{
		try
		{
			Groups = _optionService.GetAll()
				.Select(option => option.OptionGroup)
				.Distinct()
				.OrderBy(group => group)
				.ToList();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
		}
	}
}
