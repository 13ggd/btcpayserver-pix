namespace BTCPayServer.Plugins.Asaas.PaymentHandler;

/// <summary>
/// Modelo de configuração para método de pagamento Asaas PIX.
/// </summary>
public class AsaasPixPaymentMethodConfig
{
    /// <summary>
    /// Se os pagamentos PIX estão habilitados para esta loja.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// API Key do Asaas.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// ID do cliente padrão para criar cobranças.
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// Obtém se o método de pagamento está devidamente configurado.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(CustomerId);
}
