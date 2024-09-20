using ArticleAPI.Services;
using Refit;
using Web.Components;
using Web.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

builder.Services.AddScoped<ArticlesService>();

builder.Services.AddMediatR(x=> x.RegisterServicesFromAssemblyContaining<Program>());


builder.Services.AddRefitClient<IArticlesApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:6000")); // Укажите URL вашего API

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
