using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure database connection (Azure SQL)
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("AzureSQL"))
);

var app = builder.Build();

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Set up MVC routing (Default route to LoanController)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Loan}/{action=Index}/{id?}"
);

app.Run();