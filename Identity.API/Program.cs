using Application.Core.Extensions;
using Identity.API;
using Identity.API.Configuration;
using Identity.API.Models;
using Identity.API.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Persistence.Core.Extensions;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionStringOrThrow("IdentityDB");

builder.Services.AddDbContext<ApplicationDbContext>((sp, o) =>
    o.UseNpgsql(connectionString, act
            =>
        {
            act.EnableRetryOnFailure(3);
            act.CommandTimeout(30);
            act.MigrationsAssembly("Identity.API");
            act.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        })
        .ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables))
        .LogTo(Console.WriteLine)
        .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
        .EnableServiceProviderCaching()
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors());

builder.AddServiceDefaults();

builder.Services.AddControllersWithViews();

// Apply database migration automatically. Note that this approach is not
// recommended for production scenarios. Consider generating SQL scripts from
// migrations instead.
builder.Services.AddMigration<ApplicationDbContext, UsersSeed>();

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddIdentityServer(options =>
    {
        //options.IssuerUri = "null";
        options.Authentication.CookieLifetime = TimeSpan.FromHours(2);

        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;

        // TODO: Remove this line in production.
        options.KeyManagement.Enabled = false;
    })
    .AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
    .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
    .AddInMemoryApiResources(IdentityConfiguration.ApiResources)
    .AddInMemoryClients(IdentityConfiguration.Clients)
    .AddAspNetIdentity<User>()
// TODO: Not recommended for production - you need to store your key material somewhere secure
    .AddDeveloperSigningCredential();

builder.Services.AddTransient<IProfileService, ProfileService>();
builder.Services.AddTransient<ILoginService<User>, EFLoginService>();
builder.Services.AddTransient<IRedirectService, RedirectService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseStaticFiles();

// This cookie policy fixes login issues with Chrome 80+ using HTTP
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();