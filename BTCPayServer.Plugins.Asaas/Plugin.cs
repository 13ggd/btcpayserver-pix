using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Abstractions.Models;
using BTCPayServer.Abstractions.Services;
using BTCPayServer.Plugins.Asaas.PaymentHandler;
using BTCPayServer.Plugins.Asaas.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BTCPayServer.Plugins.Asaas;

public class Plugin : BaseBTCPayServerPlugin
{
    public static readonly PaymentMethodId AsaasPixPaymentMethodId = new("ASAAS_PIX");

    public override IBTCPayServerPlugin.PluginDependency[] Dependencies { get; } =
    {
        new IBTCPayServerPlugin.PluginDependency { Identifier = nameof(BTCPayServer), Condition = ">=2.3.7" }
    };

    public override void Execute(IServiceCollection services)
    {
        // Registrar HttpClient para AsaasApiService
        services.AddHttpClient<AsaasApiService>();
        
        // Registrar core services
        services.AddSingleton<AsaasApiService>();
        services.AddSingleton<AsaasService>();
        services.AddSingleton<AsaasDbContextFactory>();
        services.AddDbContext<AsaasDbContext>((provider, o) =>
        {
            var factory = provider.GetRequiredService<AsaasDbContextFactory>();
            factory.ConfigureBuilder(o);
        });

        // Registrar payment method handler
        services.AddSingleton<IPaymentMethodHandler, AsaasPixPaymentMethodHandler>();

        // Registrar checkout model extension
        services.AddSingleton<ICheckoutModelExtension, AsaasPixCheckoutModelExtension>();

        // Registrar UI extensions
        services.AddUIExtension("store-wallets-nav", "AsaasPix/StoreNavExtension");
        services.AddUIExtension("checkout-end", "AsaasPix/CheckoutPaymentMethodExtension");

        // Definir nome de exibição para o método de pagamento
        services.AddDefaultPrettyName(AsaasPixPaymentMethodId, "PIX (Asaas)");

        // UI Header
        services.AddSingleton<IUIExtension>(new UIExtension("AsaasPluginHeaderNav", "header-nav"));
        services.AddHostedService<ApplicationPartsLogger>();
        services.AddHostedService<PluginMigrationRunner>();
    }
}
