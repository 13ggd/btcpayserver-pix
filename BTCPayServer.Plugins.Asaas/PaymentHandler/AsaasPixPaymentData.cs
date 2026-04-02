namespace BTCPayServer.Plugins.Asaas.PaymentHandler;

/// <summary>
/// Dados de pagamento para Asaas PIX.
/// </summary>
public class AsaasPixPaymentData
{
    /// <summary>
    /// ID do pagamento
    /// </summary>
    public string PaymentId { get; set; } = string.Empty;

    /// <summary>
    /// Status do pagamento
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
