using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TokenAnalyser.Components;
using TokenAnalyser.Data;
using TokenAnalyser.Services;
using ApexCharts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var dbPath = Path.Combine(builder.Environment.ContentRootPath, "tokenanalyser.db");
var connectionString = $"Data Source={dbPath}";

// AddDbContextFactory also registers AppDbContext as a scoped service (used by Identity)
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity & Authentication
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddIdentityCore<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<DailyUsageService>();
builder.Services.AddApexCharts();

var app = builder.Build();

// Ensure the database is up-to-date (applies any pending migrations) and seed admin user
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    const string adminEmail = "admin@tokenanalyser.com";
    const string adminPassword = "Admin@123!";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var user = new IdentityUser { UserName = adminEmail, Email = adminEmail };
        await userManager.CreateAsync(user, adminPassword);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Logout endpoint
app.MapGet("/Account/Logout", async (SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/Account/Login");
});

app.Run();