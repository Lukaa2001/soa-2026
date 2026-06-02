using Explorer.API.Middleware;
using Explorer.API.Startup;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Infrastructure;
using Explorer.Stakeholders.Infrastructure.Database;
using Explorer.Stakeholders.Service.Grpc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// REST (HTTP/1.1) on 8080, gRPC (HTTP/2 plaintext) on 8081.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, o => o.Protocols = HttpProtocols.Http1);
    options.ListenAnyIP(8081, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddGrpc();
// Don't auto-require non-nullable reference-type properties (e.g. UpdatePersonDTO.Image),
// so multipart profile updates without a new image aren't rejected with 400.
builder.Services.AddControllers(o =>
    o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.ConfigureSwagger(builder.Configuration);
const string corsPolicy = "_corsPolicy";
builder.Services.ConfigureCors(corsPolicy);
builder.Services.ConfigureAuth();
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureStakeholdersModule();
// IInternalUserService was registered by the Blog module in the monolith; the
// Stakeholders service now exposes it (used by the gRPC server).
builder.Services.AddScoped<Explorer.Stakeholders.API.Internal.IInternalUserService, Explorer.Stakeholders.Core.UseCases.UserService>();

var app = builder.Build();

// Create schema (no migrations in this project) and seed a default admin.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StakeholdersContext>();
    db.Database.EnsureCreated();
    SeedDefaults(db);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors(corsPolicy);
app.UseMiddleware<GlobalExceptionHandler>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<StakeholdersGrpcService>();
app.MapGet("/health", () => Results.Ok("stakeholders up"));

app.Run();

static void SeedDefaults(StakeholdersContext db)
{
    if (db.Users.Any()) return;

    // Admin is inserted directly into the DB (spec: admins are not self-registered).
    var admin = new User("admin", "admin", UserRole.Administrator, true);
    var author = new User("autor", "autor", UserRole.Author, true);
    var tourist = new User("turista", "turista", UserRole.Tourist, true);
    db.Users.AddRange(admin, author, tourist);
    db.SaveChanges();

    db.People.AddRange(
        new Person(author.Id, "Ana", "Vodic", "ana@explorer.com", "", "", "", 0, 0),
        new Person(tourist.Id, "Marko", "Turista", "marko@explorer.com", "", "", "", 0, 0));
    db.SaveChanges();
}

namespace Explorer.Stakeholders.Service
{
    public partial class Program { }
}
