using System.Globalization;
using GymWeb.Components;
using GymWeb.Services;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Everything the gateway does not support yet is served by the in-memory mock.
builder.Services.AddSingleton<IGymService, MockGymService>();

// Users: real gateway calls when Gateway:UseMockData is false, otherwise mock data.
if (builder.Configuration.GetValue<bool>("Gateway:UseMockData"))
{
    builder.Services.AddSingleton<IUserService, MockUserService>();
}
else
{
    var baseUrl = builder.Configuration["Gateway:BaseUrl"] ?? "http://localhost:8080";
    builder.Services.AddHttpClient<IUserService, GatewayUserService>(c => c.BaseAddress = new Uri(baseUrl));
}

builder.Services.AddScoped<AuthState>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
