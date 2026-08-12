#nullable enable

using Microsoft.Extensions.DependencyInjection;
using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Registo dos dubles de integration no container via Keyed DI.
/// Um destino sem duble registado lanca excecao verificavel no arranque,
/// impedindo falhas silenciosas em producao (gaps.dynamic-subprocess = interface-registry-validated).
/// </summary>
public static class IntegrationDoublesRegistration
{
    /// <summary>
    /// Regista todos os dubles de integration com as chaves correspondentes ao nome do processo.
    /// </summary>
    public static IServiceCollection AddIntegrationDoubles(this IServiceCollection services)
    {
        services.AddKeyedScoped<IAGURETPC, AGURETCPDouble>("AGURETPC");
        return services;
    }
}
