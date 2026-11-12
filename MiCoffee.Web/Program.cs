using MiCoffee.Business;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

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

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
