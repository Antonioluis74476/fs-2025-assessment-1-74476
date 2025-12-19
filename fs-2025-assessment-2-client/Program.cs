using fs_2025_assessment_2_client.Services;
using fs_2025_assessment_2_client.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Required by default Blazor project template (used in FetchData page)
builder.Services.AddSingleton<WeatherForecastService>();

// ------------------------------
// API CLIENT CONFIGURATION
// ------------------------------
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
                 ?? "https://localhost:7259/"; // API/Swagger port

builder.Services.AddHttpClient<StationsApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Build application
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
