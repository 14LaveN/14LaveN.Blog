using Application.ApiHelpers.Middlewares.DelegatingHandlers;
using EmailService;
using Refit;
using Web.Components;
using Web.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

builder.Services.AddMediatR(x=> x.RegisterServicesFromAssemblyContaining<Program>());

var refitSettings = new RefitSettings
{
    ExceptionFactory = async response =>
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return new Exception($"API Error: {content}");
        }
        return null;
    }
};

builder.Services.AddTransient<LoggingHandler>();
builder.Services
    .AddRefitClient<IArticlesApi>(refitSettings)
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:6001"))
    .AddHttpMessageHandler<LoggingHandler>();

builder.Services.AddEmailService(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();
