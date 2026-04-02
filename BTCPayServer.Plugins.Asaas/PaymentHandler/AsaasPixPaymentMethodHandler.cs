#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Data;
using BTCPayServer.Payments;
using BTCPayServer.Plugins.Asaas.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BTCPayServer.Plugins.Asaas.PaymentHandler;

/// <summary>
/// Payment method handler para Asaas PIX payments.
/// Cria cobranças PIX e configura checkout prompts.
/// </summary>
public class AsaasPixPaymentMethodHandler : IPaymentMethodHandler
{
    private readonly AsaasApiService _asaasService;
    private readonly ILogger<AsaasPixPaymentMethodHandler> _logger;

    public JsonSerializer Serializer { get; }
    public PaymentMethodId PaymentMethodId { get; }

    public AsaasPixPaymentMethodHandler(
        AsaasApiService asaasService,
        ILogger<AsaasPixPaymentMethodHandler> logger)
    {
        _asaasService = asaasService;
        _logger = logger;

        PaymentMethodId = new PaymentMethodId("ASAAS_PIX");

        // Use default serializer sem configuração de rede específica
        (_, Serializer) = BlobSerializer.CreateSerializer(null as NBitcoin.Network);
    }

    /// <summary>
    /// Chamado antes de buscar taxas. Configura moeda e divisibilidade.
    /// </summary>
    public Task BeforeFetchingRates(PaymentMethodContext context)
    {
        var config = ParsePaymentMethodConfigInternal(context.PaymentMethodConfig);
        
        _logger.LogInformation("BeforeFetchingRates called for {PaymentMethodId} - Enabled: {Enabled}, IsConfigured: {IsConfigured}", 
            PaymentMethodId, config.Enabled, config.IsConfigured);
        
        if (!config.Enabled || !config.IsConfigured)
        {
            _logger.LogWarning("ASAAS PIX method not available - Enabled: {Enabled}, IsConfigured: {IsConfigured}", 
                config.Enabled, config.IsConfigured);
            context.State = null;
            return Task.CompletedTask;
        }

        // PIX sempre usa BRL com 2 casas decimais
        context.Prompt.Currency = "BRL";
        context.Prompt.Divisibility = 2;
        context.State = new PrepareState { Config = config };
        
        _logger.LogInformation("ASAAS PIX method prepared successfully with currency BRL");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Configura o payment prompt com detalhes do pagamento PIX.
    /// </summary>
    public async Task ConfigurePrompt(PaymentMethodContext context)
    {
        _logger.LogInformation("ConfigurePrompt called for ASAAS PIX");
        
        if (context.State == null)
        {
            _logger.LogError("ConfigurePrompt failed: context.State is null");
            throw new PaymentMethodUnavailableException("Asaas PIX payment method is not prepared");
        }
        if (context.State is not PrepareState prepareState)
        {
            _logger.LogError("ConfigurePrompt failed: context.State is not PrepareState");
            throw new PaymentMethodUnavailableException("Asaas PIX payment method not properly initialized");
        }

        var config = prepareState.Config;
        if (!config.IsConfigured || !config.Enabled)
        {
            _logger.LogError("ConfigurePrompt failed: not configured or not enabled");
            throw new PaymentMethodUnavailableException("Asaas PIX is not configured for this store");
        }

        context.Prompt.Divisibility = 2; // BRL tem 2 casas decimais
        context.Prompt.PaymentMethodFee = 0m;

        // Calcular valor devido em BRL
        var invoice = context.InvoiceEntity;

        _logger.LogInformation("Creating ASAAS PIX payment for invoice {InvoiceId} - Price: {Price} {Currency}", 
            invoice.Id, invoice.Price, invoice.Currency);

        // Obter taxa da moeda da invoice para BRL
        var rate = invoice.GetRate(
            new BTCPayServer.Rating.CurrencyPair("BRL", invoice.Currency));

        _logger.LogInformation("Rate for BRL/{Currency}: {Rate}", invoice.Currency, rate);

        if (rate == 0)
        {
            _logger.LogError("Cannot get rate for BRL/{Currency}", invoice.Currency);
            throw new PaymentMethodUnavailableException(
                $"Cannot get rate for BRL/{invoice.Currency}");
        }

        // Calcular valor em BRL
        var amountInBrl = invoice.Price / rate;
        amountInBrl = Math.Round(amountInBrl, 2);

        _logger.LogInformation("Amount in BRL: {Amount}", amountInBrl);

        // Criar pagamento PIX via API Asaas
        try
        {
            var payment = await _asaasService.CreatePixPaymentAsync(
                config.ApiKey!,
                config.CustomerId!,
                amountInBrl,
                $"Payment for invoice {invoice.Id}");

            if (payment == null)
            {
                _logger.LogError("Failed to create ASAAS PIX payment");
                throw new PaymentMethodUnavailableException("Failed to create ASAAS PIX payment");
            }

            _logger.LogInformation("ASAAS PIX payment created successfully: {PaymentId}", payment.Id);

            // Obter QR Code
            var qrCode = await _asaasService.GetPixQrCodeAsync(config.ApiKey!, payment.Id);

            if (qrCode == null)
            {
                _logger.LogError("Failed to get ASAAS PIX QR code");
                throw new PaymentMethodUnavailableException("Failed to get ASAAS PIX QR code");
            }

            // Armazenar detalhes do prompt
            var promptDetails = new AsaasPixPromptDetails
            {
                PaymentId = payment.Id,
                ApiKey = config.ApiKey!,
                Amount = amountInBrl,
                Currency = "BRL",
                Status = payment.Status,
                QrCodeImage = qrCode.EncodedImage,
                PixPayload = qrCode.Payload,
                ExpirationDate = qrCode.ExpirationDate
            };

            context.Prompt.Destination = payment.Id;
            context.Prompt.Details = JObject.FromObject(promptDetails, Serializer);

            _logger.LogInformation(
                "Created ASAAS PIX payment {PaymentId} for invoice {InvoiceId} - {Amount} BRL",
                payment.Id, invoice.Id, amountInBrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ASAAS PIX payment: {Message}", ex.Message);
            throw;
        }
    }

    public Task AfterSavingInvoice(PaymentMethodContext context)
    {
        return Task.CompletedTask;
    }

    public object ParsePaymentPromptDetails(JToken details)
    {
        return details.ToObject<AsaasPixPromptDetails>(Serializer)!;
    }

    public void StripDetailsForNonOwner(object details)
    {
        // Remover dados sensíveis para não-proprietários
        if (details is AsaasPixPromptDetails promptDetails)
        {
            promptDetails.ApiKey = "[REDACTED]";
        }
    }

    public object ParsePaymentMethodConfig(JToken config)
    {
        return ParsePaymentMethodConfigInternal(config);
    }

    private static AsaasPixPaymentMethodConfig ParsePaymentMethodConfigInternal(JToken config)
    {
        return config.ToObject<AsaasPixPaymentMethodConfig>() ?? new AsaasPixPaymentMethodConfig();
    }

    public Task ValidatePaymentMethodConfig(PaymentMethodConfigValidationContext validationContext)
    {
        var config = ParsePaymentMethodConfigInternal(validationContext.Config);

        if (config.Enabled && !config.IsConfigured)
        {
            validationContext.ModelState.AddModelError("ApiKey", "API Key é obrigatória quando PIX está habilitado");
            validationContext.ModelState.AddModelError("CustomerId", "Customer ID é obrigatório quando PIX está habilitado");
        }

        return Task.CompletedTask;
    }

    public object ParsePaymentDetails(JToken details)
    {
        return details.ToObject<AsaasPixPaymentData>(Serializer)!;
    }

    private class PrepareState
    {
        public required AsaasPixPaymentMethodConfig Config { get; set; }
    }
}
