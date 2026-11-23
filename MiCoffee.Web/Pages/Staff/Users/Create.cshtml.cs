using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff.Users;

public class CreateModel : PageModel
{
	private readonly UserService _userService = new();

	[BindProperty]
	public User StaffUser { get; set; } = new() { Role = UserRole.Employee };

	[BindProperty]
	[Required(ErrorMessage = "La contraseña es obligatoria.")]
	public string Password { get; set; } = string.Empty;

	public string? Error { get; private set; }

	public List<SelectListItem> Roles { get; } = new()
	{
		new("Empleado", nameof(UserRole.Employee)),
		new("Administrador", nameof(UserRole.Admin))
	};

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
			_userService.Create(StaffUser, Password);
			TempData["Message"] = "Usuario creado.";
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