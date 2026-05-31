using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Explorer.API.Startup;

public static class CorsConfiguration
{
    public static IServiceCollection ConfigureCors(this IServiceCollection services, string corsPolicy)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: corsPolicy,
                builder =>
                {
                    builder.WithOrigins(ParseCorsOrigins())
                        .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization, "access_token")
                        .WithMethods("GET", "PUT", "POST", "PATCH", "DELETE", "OPTIONS");
                });
        });
        services.Configure<FormOptions>(o =>
        {
            o.ValueLengthLimit = int.MaxValue;
            o.MultipartBodyLengthLimit = int.MaxValue;
            o.MemoryBufferThreshold = int.MaxValue;
        });
        return services;
    }

    private static string[] ParseCorsOrigins()
    {
        // Behind the gateway this is rarely hit directly, but keep it permissive for local/dev.
        var origins = Environment.GetEnvironmentVariable("EXPLORER_CORS_ORIGINS");
        if (!string.IsNullOrWhiteSpace(origins))
            return origins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new[] { "http://localhost:4200", "http://localhost:8080" };
    }
}
