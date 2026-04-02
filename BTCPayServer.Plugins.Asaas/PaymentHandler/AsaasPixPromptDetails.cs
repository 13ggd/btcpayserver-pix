namespace BTCPayServer.Plugins.Asaas.PaymentHandler;

/// <summary>
/// Detalhes passados para o checkout UI para renderizar pagamento Asaas PIX.
/// </summary>
public class AsaasPixPromptDetails
{
    /// <summary>
    /// ID do pagamento no Asaas
    /// </summary>
    public string PaymentId { get; set; } = string.Empty;

    /// <summary>
    /// API Key do Asaas para chamadas de API
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// QR Code em Base64
    /// </summary>
    public string? QrCodeImage { get; set; }

    /// <summary>
    /// Payload PIX (copia e cola)
    /// </summary>
    public string? PixPayload { get; set; }

    /// <summary>
    /// Valor a ser exibido
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Código da moeda
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Status do pagamento
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Data de expiração do QR Code
    /// </summary>
    public string? ExpirationDate { get; set; }
}
