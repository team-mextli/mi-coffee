using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Options;

public class IndexModel : PageModel
{
	private readonly ProductOptionService _optionService = new();

	public List<ProductOption> ProductOptions { get; private set; } = new();

	public string? Error { get; private set; }

	public void OnGet()
	{
		try
		{
			ProductOptions = _optionService.GetAll();
		}
		catch (BusinessException ex)
		{
			Error = ex.Message;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar las opciones.";
		}
	}
}
