using System.Collections.Generic;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Client;
using BTCPayServer.Plugins.Asaas.Data;
using BTCPayServer.Plugins.Asaas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.Asaas;

[Route("~/plugins/asaas")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = Policies.CanViewProfile)]
public class UIPluginController : Controller
{
    private readonly AsaasService _PluginService;

    public UIPluginController(AsaasService PluginService)
    {
        _PluginService = PluginService;
    }

    // GET
    public async Task<IActionResult> Index()
    {
        return View(new AsaasPluginPageViewModel { Data = await _PluginService.Get() });
    }
}

public class AsaasPluginPageViewModel
{
    public List<PluginData> Data { get; set; }
}
