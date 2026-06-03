using Explorer.API.Middleware;
using Explorer.API.Startup;
using Explorer.Blog.Service;

// Allow plaintext (h2c) gRPC to the Stakeholders service on the internal network.
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.ConfigureSwagger(builder.Configuration);
const string corsPolicy = "_corsPolicy";
builder.Services.ConfigureCors(corsPolicy);
builder.Services.ConfigureAuth();
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureBlogServiceModule();

// Blog -> Followers HTTP client (comment allowed only if the user follows the author).
builder.Services.AddHttpClient<Explorer.Blog.Service.Clients.FollowersClient>(c =>
    c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("FOLLOWERS_URL") ?? "http://followers:8080/"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors(corsPolicy);
app.UseMiddleware<GlobalExceptionHandler>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("blog up"));

app.Run();
