using Explorer.Grpc.Stakeholders;

// Allow plaintext (h2c) gRPC to the backend services on the internal network.
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

const string corsPolicy = "_gatewayCors";

// CORS so the Angular client (on any localhost dev port) can talk to the gateway.
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        // Reflect any origin (dev): JWT travels in the Authorization header, no cookies.
        policy.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("AuthenticationTokens-Expired");
    });
});

// YARP reverse proxy: all REST routes/clusters defined in appsettings(.json).
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// gRPC client to the Stakeholders service (gateway -> service RPC).
builder.Services.AddGrpcClient<StakeholdersGrpc.StakeholdersGrpcClient>(o =>
{
    o.Address = new Uri(Environment.GetEnvironmentVariable("STAKEHOLDERS_GRPC") ?? "http://stakeholders:8081");
});

var app = builder.Build();

app.UseCors(corsPolicy);

// ── RPC endpoints (gateway -> Stakeholders over gRPC) ──
app.MapGet("/api/rpc/users/{id:long}", async (long id, StakeholdersGrpc.StakeholdersGrpcClient client) =>
{
    var reply = await client.GetUserAsync(new UserRequest { UserId = id });
    return Results.Ok(new { id = reply.Id, username = reply.Username });
});

app.MapGet("/api/rpc/persons/{id:long}/email", async (long id, StakeholdersGrpc.StakeholdersGrpcClient client) =>
{
    var reply = await client.GetPersonEmailAsync(new PersonRequest { PersonId = id });
    return Results.Ok(new { email = reply.Email });
});

app.MapReverseProxy();
app.MapGet("/health", () => Results.Ok("gateway up"));

app.Run();
