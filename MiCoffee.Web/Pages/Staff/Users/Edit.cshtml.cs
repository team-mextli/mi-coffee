using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Users;

public class EditModel : PageModel
{
	private readonly UserService _userService = new();

	[BindProperty]
	public User StaffUser { get; set; } = new();

	[BindProperty]
	public string? NewPassword { get; set; }

	public string? Error { get; private set; }
	public bool Loaded { get; private set; }

	public List<SelectListItem> Roles { get; } = new()
	{
		new("Empleado", nameof(UserRole.Employee)),
		new("Administrador", nameof(UserRole.Admin))
	};

	public IActionResult OnGet(int id)
	{
		try
		{
			User? user = _userService.GetById(id);

			if (user is null)
			{
				Error = "El usuario no existe.";
				return Page();
			}

			StaffUser = user;
			Loaded = true;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible cargar el usuario.";
		}

		return Page();
	}

	public IActionResult OnPost()
	{
		Loaded = true;

		if (!ModelState.IsValid)
		{
			return Page();
		}

		try
		{
			_userService.Update(StaffUser, NewPassword);
			TempData["Message"] = "Usuario actualizado.";
			return RedirectToPage("./Index");
		}
		catch (BusinessException ex)
		{
			Error = ex.Message;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible guardar el usuario.";
		}

		return Page();
	}
}