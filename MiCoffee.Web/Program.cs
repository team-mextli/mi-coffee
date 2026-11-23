using Microsoft.AspNetCore.Authentication.Cookies;
using MiCoffee.Business;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
	options.Conventions.AuthorizeFolder("/Staff", "StaffOnly");
	options.Conventions.AllowAnonymousToPage("/Staff/Login");
	options.Conventions.AuthorizeFolder("/Staff/Users", "AdminOnly");
	options.Conventions.AuthorizeFolder("/Staff/Categories", "AdminOnly");
	options.Conventions.AuthorizeFolder("/Staff/Products", "AdminOnly");
	options.Conventions.AuthorizeFolder("/Staff/Options", "AdminOnly");
});

builder.Services
	.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Staff/Login";
		options.AccessDeniedPath = "/Staff/Login";
		options.ExpireTimeSpan = TimeSpan.FromHours(8);
		options.SlidingExpiration = false;
	});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("StaffOnly", policy => policy.RequireRole("Employee", "Admin"));
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

string? connectionString =
	builder.Configuration.GetConnectionString("MiCoffee");

if (!string.IsNullOrWhiteSpace(connectionString))
{
	BusinessConfiguration.Initialize(connectionString);

	if (builder.Environment.IsDevelopment())
	{
		try
		{
			BusinessConfiguration.TestConnection();
			Console.WriteLine("MYSQL OK.");
		}
		catch (Exception ex)
		{
			Console.WriteLine(
				$"MYSQL ERROR: {ex.Message}");
		}
	}
}
else if (builder.Environment.IsDevelopment())
{
	Console.WriteLine(
		"MYSQL WARNING: ConnectionStrings:MiCoffee was not found. " +
		"Configure User Secrets before testing the connection.");
}

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(60);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();