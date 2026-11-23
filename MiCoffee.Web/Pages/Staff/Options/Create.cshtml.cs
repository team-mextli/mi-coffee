using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Options;

public class CreateModel : PageModel
{
	private readonly ProductOptionService _optionService = new();

	[BindProperty]
	public ProductOption ProductOption { get; set; } = new();

	// Existing groups, offered as suggestions so the same group is not
	// written in two different ways.
	public List<string> Groups { get; private set; } = new();

	public string? Error { get; private set; }

	public void OnGet()
	{
		LoadGroups();
	}

	public IActionResult OnPost()
	{
		LoadGroups();

		if (!ModelState.IsValid)
		{
			return Page();
		}

		try
		{
			_optionService.Create(ProductOption);
			TempData["Message"] = "Opción creada.";
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
