#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BTCPayServer.Plugins.Asaas.Services;

/// <summary>
/// Serviço para integração com API do Asaas para pagamentos PIX
/// </summary>
public class AsaasApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AsaasApiService> _logger;
    private const string ApiBaseUrl = "https://sandbox.asaas.com/api/v3"; // Sandbox URL

    public AsaasApiService(
        HttpClient httpClient,
        ILogger<AsaasApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Criar uma cobrança PIX no Asaas
    /// </summary>
    public async Task<AsaasPaymentResponse?> CreatePixPaymentAsync(
        string apiKey,
        string customerId,
        decimal value,
        string description)
    {
        try
        {
            var requestBody = new AsaasPaymentRequest
            {
                Customer = customerId,
                BillingType = "PIX",
                Value = value,
                DueDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
                Description = description
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/payments")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            _logger.LogInformation("Creating PIX payment: Customer={Customer}, Value={Value}", customerId, value);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error creating PIX payment: {Response}", responseContent);
                return null;
            }

            var paymentResponse = JsonSerializer.Deserialize<AsaasPaymentResponse>(responseContent);
            
            _logger.LogInformation("PIX payment created successfully: Id={Id}", paymentResponse?.Id);
            return paymentResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PIX payment");
            return null;
        }
    }

    /// <summary>
    /// Obter QR Code PIX para uma cobrança
    /// </summary>
    public async Task<AsaasQrCodeResponse?> GetPixQrCodeAsync(string apiKey, string paymentId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBaseUrl}/payments/{paymentId}/pixQrCode");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            _logger.LogInformation("Getting PIX QR code for payment: {PaymentId}", paymentId);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error getting PIX QR code: {Response}", responseContent);
                return null;
            }

            var qrCodeResponse = JsonSerializer.Deserialize<AsaasQrCodeResponse>(responseContent);
            
            _logger.LogInformation("PIX QR code retrieved successfully for payment: {PaymentId}", paymentId);
            return qrCodeResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PIX QR code");
            return null;
        }
    }

    /// <summary>
    /// Consultar status de um pagamento
    /// </summary>
    public async Task<AsaasPaymentResponse?> GetPaymentStatusAsync(string apiKey, string paymentId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBaseUrl}/payments/{paymentId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error getting payment status: {Response}", responseContent);
                return null;
            }

            return JsonSerializer.Deserialize<AsaasPaymentResponse>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status");
            return null;
        }
    }
}

/// <summary>
/// Request para criar pagamento PIX
/// </summary>
public class AsaasPaymentRequest
{
    [JsonPropertyName("customer")]
    public string Customer { get; set; } = string.Empty;

    [JsonPropertyName("billingType")]
    public string BillingType { get; set; } = "PIX";

    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("dueDate")]
    public string DueDate { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Resposta de criação de pagamento
/// </summary>
public class AsaasPaymentResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("customer")]
    public string Customer { get; set; } = string.Empty;

    [JsonPropertyName("billingType")]
    public string BillingType { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("dueDate")]
    public string DueDate { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Resposta do QR Code PIX
/// </summary>
public class AsaasQrCodeResponse
{
    [JsonPropertyName("encodedImage")]
    public string EncodedImage { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;

    [JsonPropertyName("expirationDate")]
    public string ExpirationDate { get; set; } = string.Empty;
}
