#nullable enable
using System;
using BTCPayServer.Payments;
using BTCPayServer.Plugins.Asaas.PaymentHandler;
using BTCPayServer.Services.Invoices;
using Microsoft.AspNetCore.Routing;

namespace BTCPayServer.Plugins.Asaas.PaymentHandler;

/// <summary>
/// Checkout model extension para pagamentos Asaas PIX.
/// </summary>
public class AsaasPixCheckoutModelExtension : ICheckoutModelExtension
{
    public PaymentMethodId PaymentMethodId => new PaymentMethodId("ASAAS_PIX");
    public string Image => "/img/asaas-pix.svg";
    public string Badge => null!;

    /// <summary>
    /// Modifica o checkout model para incluir detalhes do Asaas PIX.
    /// </summary>
    public void ModifyCheckoutModel(CheckoutModelContext context)
    {
        if (context.Prompt.PaymentMethodId != new PaymentMethodId("ASAAS_PIX"))
            return;

        context.Model.CheckoutBodyComponentName = "AsaasPixCheckout";

        var details = context.Prompt.Details?.ToObject<AsaasPixPromptDetails>();
        if (details != null)
        {
            context.Model.AdditionalData.Add("paymentId", details.PaymentId);
            context.Model.AdditionalData.Add("qrCodeImage", details.QrCodeImage);
            context.Model.AdditionalData.Add("pixPayload", details.PixPayload);
            context.Model.AdditionalData.Add("amount", details.Amount);
            context.Model.AdditionalData.Add("expirationDate", details.ExpirationDate);
        }

        context.Model.PaymentMethodId = new PaymentMethodId("ASAAS_PIX").ToString();
        context.Model.InvoiceId = context.InvoiceEntity.Id;
    }
}
