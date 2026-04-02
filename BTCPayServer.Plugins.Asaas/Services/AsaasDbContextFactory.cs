using System;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace BTCPayServer.Plugins.Asaas.Services;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AsaasDbContext>
{
    public AsaasDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AsaasDbContext>();

        // FIXME: Somehow the DateTimeOffset column types get messed up when not using Postgres
        // https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers?tabs=dotnet-core-cli
        builder.UseNpgsql("User ID=postgres;Host=127.0.0.1;Port=39372;Database=designtimebtcpay");

        return new AsaasDbContext(builder.Options, true);
    }
}

public class AsaasDbContextFactory : BaseDbContextFactory<AsaasDbContext>
{
    public AsaasDbContextFactory(IOptions<DatabaseOptions> options) : base(options, "BTCPayServer.Plugins.Asaas")
    {
    }

    public override AsaasDbContext CreateContext(Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = null)
    {
        var builder = new DbContextOptionsBuilder<AsaasDbContext>();
        ConfigureBuilder(builder, npgsqlOptionsAction);
        return new AsaasDbContext(builder.Options);
    }
}
