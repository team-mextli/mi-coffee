using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Users;

public class IndexModel : PageModel
{
	private readonly UserService _userService = new();

	public List<User> StaffUsers { get; private set; } = new();
	public string? Error { get; private set; }

	public void OnGet()
	{
		Load();
	}

	public IActionResult OnPostSetActive(int id, bool active)
	{
		try
		{
			_userService.SetActive(id, active);
			TempData["Message"] = active ? "Usuario activado." : "Usuario desactivado.";
			return RedirectToPage();
		}
		catch (BusinessException ex)
		{
			Error = ex.Message;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible actualizar el usuario.";
		}

		Load();
		return Page();
	}

	private void Load()
	{
		try
		{
			StaffUsers = _userService.GetAll();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar los usuarios.";
		}
	}
}