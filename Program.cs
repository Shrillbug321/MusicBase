using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MusicBase.Database;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DbConnection>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddEntityFrameworkStores<DbConnection>();

var app = builder.Build();

var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
	var context = services.GetRequiredService<DbConnection>();
	DbInitializer.Initialize(context);
}
catch (Exception ex)
{
	var logger = services.GetRequiredService<ILogger<Program>>();
	logger.LogError(ex, "An error occurred creating the DB.");
}

if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

IList<CultureInfo> sc = new List<CultureInfo>();
sc.Add(new CultureInfo("pl-PL"));
var lo = new RequestLocalizationOptions
{
	DefaultRequestCulture = new RequestCulture("pl-PL"),
	SupportedCultures = sc,
	SupportedUICultures = sc
};
var cp = lo.RequestCultureProviders.OfType<CookieRequestCultureProvider>().First();
cp.CookieName = "UserCulture"; // Or whatever name that you like

app.UseRequestLocalization(lo);

app.Run();
