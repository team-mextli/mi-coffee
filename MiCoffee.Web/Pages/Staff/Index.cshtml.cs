using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiCoffee.Business;
using MiCoffee.Entities;

namespace MiCoffee.Web.Pages.Staff;

public class IndexModel : PageModel
{
	private readonly OrderService _orderService = new();
	private readonly SupplyService _supplyService = new();
	private readonly SupplierService _supplierService = new();
	private readonly ProductService _productService = new();
	private readonly ProductCategoryService _categoryService = new();
	private readonly ProductOptionService _optionService = new();
	private readonly UserService _userService = new();

	
	public int? ReceivedCount { get; private set; }

	public int? InPreparationCount { get; private set; }

	public int? ReadyCount { get; private set; }

	public int? TodayCount { get; private set; }

	public int? LowStockCount { get; private set; }

	public int? SupplierCount { get; private set; }

	// Admin only.
	public int? ProductCount { get; private set; }

	public int? UnavailableProductCount { get; private set; }

	public int? CategoryCount { get; private set; }

	public int? OptionCount { get; private set; }

	public int? UserCount { get; private set; }

	public string RoleName => User.IsInRole("Admin") ? "Administrador" : "Empleado";

	public string TodayText => DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

	public void OnGet()
	{
		LoadActiveOrders();

		DateOnly today = DateOnly.FromDateTime(DateTime.Today);
		TodayCount = Count(() => _orderService.Search(today, today, null).Count);
		LowStockCount = Count(() => _supplyService.GetAlerts().Count());
		SupplierCount = Count(() => _supplierService.GetAll().Count);

		// for an Admin.
		if (!User.IsInRole("Admin"))
		{
			return;
		}

		try
		{
			List<Product> products = _productService.GetAll();
			ProductCount = products.Count;
			UnavailableProductCount = products.Count(product => !product.IsAvailable);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex);
		}

		CategoryCount = Count(() => _categoryService.GetAll().Count);
		OptionCount = Count(() => _optionService.GetAll().Count);
		UserCount = Count(() => _userService.GetAll().Count);
	}

	private void LoadActiveOrders()
	{
		try
		{
			List<CustomerOrder> active = _orderService.GetActive();

			ReceivedCount = active.Count(order => order.Status == OrderStatus.Received);
			InPreparationCount = active.Count(order => order.Status == OrderStatus.InPreparation);
			ReadyCount = active.Count(order => order.Status == OrderStatus.Ready);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex);
		}
	}

	private static int? Count(Func<int> load)
	{
		try
		{
			return load();
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex);
			return null;
		}
	}
}
