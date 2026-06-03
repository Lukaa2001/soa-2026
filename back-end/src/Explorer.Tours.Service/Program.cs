using Explorer.API.Middleware;
using Explorer.API.Startup;
using Explorer.Tours.Infrastructure.Database;
using Explorer.Tours.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.ConfigureSwagger(builder.Configuration);
const string corsPolicy = "_corsPolicy";
builder.Services.ConfigureCors(corsPolicy);
builder.Services.ConfigureAuth();
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureToursServiceModule();

// Tours -> Purchase HTTP client (purchase prerequisite for starting a tour).
builder.Services.AddHttpClient<Explorer.Tours.Service.Clients.PurchaseClient>(c =>
    c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("PURCHASE_URL") ?? "http://purchase:8080/"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToursContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors(corsPolicy);
app.UseMiddleware<GlobalExceptionHandler>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("tours up"));

app.Run();
