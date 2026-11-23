using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Options;

public class DeleteModel : PageModel
{
	private readonly ProductOptionService _optionService = new();

	public ProductOption? ProductOption { get; private set; }

	// True while the delete button can be offered. Business rejects the
	// deletion when the option appears in an order, and then it is hidden.
	public bool CanDelete { get; private set; }

	public string? Error { get; private set; }

	public void OnGet(int id)
	{
		try
		{
			ProductOption = _optionService.GetById(id);
			if (ProductOption is null)
			{
				Error = "La opción no existe.";
				return;
			}

			CanDelete = true;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar la opción.";
		}
	}

	public IActionResult OnPost(int id)
	{
		try
		{
			_optionService.Delete(id);
			TempData["Message"] = "Opción eliminada.";
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
			Error = "No fue posible eliminar la opción.";
			CanDelete = true;
		}

		ReloadOption(id);
		return Page();
	}

	// Loads the option to show its name again after a failed deletion.
	private void ReloadOption(int id)
	{
		try
		{
			ProductOption = _optionService.GetById(id);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
		}
	}
}
