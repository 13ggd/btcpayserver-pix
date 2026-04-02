using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BTCPayServer.Plugins.Asaas.Data;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.Asaas.Services;

public class AsaasService
{
    private readonly AsaasDbContextFactory _asaasDbContextFactory;

    public AsaasService(AsaasDbContextFactory asaasDbContextFactory)
    {
        _asaasDbContextFactory = asaasDbContextFactory;
    }

    public async Task AddTestDataRecord()
    {
        await using var context = _asaasDbContextFactory.CreateContext();

        await context.PluginRecords.AddAsync(new PluginData { Timestamp = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync();
    }

    public async Task<List<PluginData>> Get()
    {
        await using var context = _asaasDbContextFactory.CreateContext();

        return await context.PluginRecords.ToListAsync();
    }

    // TODO: Implementar integração com API do Asaas aqui
    public async Task<bool> ProcessPixPayment(decimal amount, string pixKey)
    {
        // Implementação da API do Asaas virá aqui
        await Task.CompletedTask;
        return true;
    }
}

