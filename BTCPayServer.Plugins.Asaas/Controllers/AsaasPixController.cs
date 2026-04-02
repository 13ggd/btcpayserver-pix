#nullable enable
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Controllers;
using BTCPayServer.Data;
using BTCPayServer.Plugins.Asaas.PaymentHandler;
using BTCPayServer.Services.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BTCPayServer.Plugins.Asaas.Controllers;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = "CanModifyStoreSettings")]
[Route("plugins/{storeId}/asaaspix")]
public class AsaasPixController : Controller
{
    private readonly StoreRepository _storeRepository;

    public AsaasPixController(
        StoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    [HttpGet("")]
    public async Task<IActionResult> Configure(string storeId)
    {
        var store = await _storeRepository.FindStore(storeId);
        if (store == null)
            return NotFound();

        // Obter configuração existente ou criar nova
        var configToken = store.GetPaymentMethodConfig(new PaymentMethodId("ASAAS_PIX"));
        var config = configToken?.ToObject<AsaasPixPaymentMethodConfig>() ?? new AsaasPixPaymentMethodConfig();

        return View(config);
    }

    [HttpPost("")]
    public async Task<IActionResult> Configure(string storeId, AsaasPixPaymentMethodConfig config)
    {
        var store = await _storeRepository.FindStore(storeId);
        if (store == null)
            return NotFound();

        store.SetPaymentMethodConfig(new PaymentMethodId("ASAAS_PIX"), JToken.FromObject(config));
        await _storeRepository.UpdateStore(store);
        
        TempData["SuccessMessage"] = "Configurações do Asaas PIX salvas com sucesso!";
        return RedirectToAction("Configure", new { storeId });
    }
}
