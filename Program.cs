using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Zeiterfassung.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Zeiterfassung.Components.Account;
using Zeiterfassung.Data;
using Zeiterfassung.Services;
using Microsoft.Extensions.Hosting;
using ApexCharts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MudBlazor-Dienste
builder.Services.AddMudServices();
builder.Services.AddApexCharts();

// DB Context als Factory registrieren (gut für Blazor Server)
builder.Services.AddDbContextFactory<ZeiterfassungContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity und Authentifizierungsdienste (aufgeräumt, keine Duplikate)
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<TimerStateService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.AddIdentityCore<User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ZeiterfassungContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();

// Antiforgery-Dienst hinzufügen
builder.Services.AddAntiforgery();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();

app.UseRouting();

app.MapControllers();

app.UseAuthentication();

app.UseAuthorization();

app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapRazorPages();
app.MapAdditionalIdentityEndpoints();


// Seed Data Logik (unverändert)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ZeiterfassungContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        // Stelle sicher, dass die Migrationen angewendet wurden, bevor geseedet wird.
        await context.Database.MigrateAsync();
        await DataSeeder.InitializeAsync(context, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}


app.Run();