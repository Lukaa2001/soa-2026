using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.BuildingBlocks.Infrastructure.Database;
using Explorer.Payments.API.Internal;
using Explorer.Payments.API.Public.PaymentRecord;
using Explorer.Payments.API.Public.ShoppingCart;
using Explorer.Payments.API.Public.TourBundle;
using Explorer.Payments.Core.Domain;
using Explorer.Payments.Core.Domain.RepositoryInterfaces;
using Explorer.Payments.Core.Mappers;
using Explorer.Payments.Core.UseCases;
using Explorer.Payments.Infrastructure.Database;
using Explorer.Payments.Infrastructure.Database.Repositories;
using Explorer.Payments.Infrastructure.EmailSender;
using Explorer.Purchase.Service.Stubs;
using Explorer.Tours.API.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Purchase.Service;

public static class PaymentsServiceStartup
{
    public static IServiceCollection ConfigurePaymentsServiceModule(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(PaymentsProfile).Assembly);
        SetupCore(services);
        SetupInfrastructure(services);
        return services;
    }

    private static void SetupCore(IServiceCollection services)
    {
        services.AddScoped<IShoppingCartService, ShoppingCartService>();
        services.AddScoped<IInternalShoppingCartService, ShoppingCartService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IInternalWalletService, WalletService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentRecordService, PaymentRecordService>();
        services.AddScoped<IInternalShoppingSession, ShoppingSession>();
        services.AddScoped<IShoppingSession, ShoppingSession>();

        // Cross-service dependencies stubbed during decomposition (gRPC wired in Faza 7).
        services.AddScoped<IInternalCouponService, CouponServiceStub>();
        services.AddScoped<IInternalTourService, TourServiceStub>();
    }

    private static void SetupInfrastructure(IServiceCollection services)
    {
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped(typeof(ICrudRepository<Order>), typeof(CrudDatabaseRepository<Order, PaymentsContext>));
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IShoppingSessionRepository, ShoppingSessionRepository>();
        services.AddScoped<IPaymentRecordRepository, PaymentRecordRepository>();
        services.AddScoped<ITourBundleRepository, TourBundleRepository>();
        services.AddScoped<IHttpClientService, HttpClientService>();

        services.AddDbContext<PaymentsContext>(opt =>
            opt.UseNpgsql(DbConnectionStringBuilder.Build("payments"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "payments")));
    }
}
