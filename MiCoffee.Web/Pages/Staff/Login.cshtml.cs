using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff;

public class LoginModel : PageModel
{
	private readonly UserService _userService = new();

	[BindProperty]
	[Required(ErrorMessage = "El usuario es obligatorio.")]
	public string Username { get; set; } = string.Empty;

	[BindProperty]
	[Required(ErrorMessage = "La contraseña es obligatoria.")]
	public string Password { get; set; } = string.Empty;

	public string? Error { get; private set; }

	public void OnGet()
	{
	}

	public async Task<IActionResult> OnPostAsync()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		try
		{
			User? user = _userService.Authenticate(Username, Password);

			if (user is null)
			{
				Error = "Usuario o contraseña incorrectos.";
				return Page();
			}

			var claims = new List<Claim>
			{
				new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
				new(ClaimTypes.Name, user.FullName),
				new(ClaimTypes.Role, user.Role.ToString())
			};

			var identity = new ClaimsIdentity(
				claims, CookieAuthenticationDefaults.AuthenticationScheme);

			await HttpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				new ClaimsPrincipal(identity));

			return RedirectToPage("/Staff/Index");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			Error = "No fue posible iniciar sesión. Intenta de nuevo.";
			return Page();
		}
	}
}
