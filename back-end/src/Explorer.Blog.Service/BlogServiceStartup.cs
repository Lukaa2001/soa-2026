using Explorer.Blog.API.Public;
using Explorer.Blog.Core.Domain;
using Explorer.Blog.Core.Domain.RepositoryInterfaces;
using Explorer.Blog.Core.Mappers;
using Explorer.Blog.Core.UseCases;
using Explorer.Blog.Service.Database;
using Explorer.Blog.Service.Stubs;
using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Grpc.Stakeholders;
using Explorer.Stakeholders.API.Internal;
using MongoDB.Driver;

namespace Explorer.Blog.Service;

public static class BlogServiceStartup
{
    public static IServiceCollection ConfigureBlogServiceModule(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(BlogProfile).Assembly);

        services.AddScoped<IBlogService, BlogService>();

        // Service -> service RPC: usernames come from Stakeholders over gRPC.
        services.AddGrpcClient<StakeholdersGrpc.StakeholdersGrpcClient>(o =>
        {
            o.Address = new Uri(Environment.GetEnvironmentVariable("STAKEHOLDERS_GRPC") ?? "http://stakeholders:8081");
        });
        services.AddScoped<IInternalUserService, GrpcUserService>();

        services.AddSingleton<IMongoDatabase>(_ => MongoConfiguration.BuildDatabase());
        services.AddScoped<MongoBlogRepository>();
        services.AddScoped<ICrudRepository<BlogDom>>(sp => sp.GetRequiredService<MongoBlogRepository>());
        services.AddScoped<IBlogRepository>(sp => sp.GetRequiredService<MongoBlogRepository>());

        return services;
    }
}
