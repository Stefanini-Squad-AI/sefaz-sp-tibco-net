#nullable enable

// fundacao-registo — composição da resolução de subprocesso dinâmico (CONTROPC 'Aguardar Retorno').
//
// Decisão NOEQ-dynamic-subprocess (interface-registry-validated, ratificada 2026-08-06):
//   O destino é resolvido em runtime pelo valor de AGUARDAR[IDX_AGUARDAR] via Keyed DI (.NET 8).
//   Os 7 destinos declarados em CONTROPC/ISetSubProc são registados como serviços com chave;
//   AGUARDARRegistry é construído a partir deles e VALIDA no arranque — um destino em falta
//   lança InvalidOperationException imediatamente (NÃO herda HaltOnBadSubProcess=false do TIBCO).

using Microsoft.Extensions.DependencyInjection;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Infrastructure.Integration.Doubles;

namespace SefazSp.Epat.Api.Composition;

/// <summary>
/// Regista a resolução de subprocesso dinâmico da interface <c>AGURETPC</c>
/// (callActivity 'Aguardar Retorno' do CONTROPC), seguindo o padrão ratificado
/// interface-registry-validated: Keyed DI para os 7 destinos + <see cref="AGUARDARRegistry"/>
/// validado no arranque.
/// </summary>
public static class DynamicSubprocessRegistration
{
    /// <summary>Os 7 destinos de <c>AGUARDAR</c> declarados em CONTROPC/ISetSubProc.</summary>
    private static readonly string[] Destinations =
        { "AgPecas", "AgPRJ", "AgRecPRJ", "AgPRJR", "AgRCRaz", "AgCRaz", "AgPetica" };

    public static IServiceCollection AddDynamicSubprocessRegistry(this IServiceCollection services)
    {
        // Keyed DI (.NET 8): cada destino → implementação. AgPecas é o pacote entregue (AGPECASPC);
        // os outros 6 são doubles de pacote externo que falham VISIVELMENTE ao serem invocados.
        services.AddKeyedSingleton<IAGURETPC, AgPecasDouble>("AgPecas");
        services.AddKeyedSingleton<IAGURETPC, AgPRJDouble>("AgPRJ");
        services.AddKeyedSingleton<IAGURETPC, AgRecPRJDouble>("AgRecPRJ");
        services.AddKeyedSingleton<IAGURETPC, AgPRJRDouble>("AgPRJR");
        services.AddKeyedSingleton<IAGURETPC, AgRCRazDouble>("AgRCRaz");
        services.AddKeyedSingleton<IAGURETPC, AgCRazDouble>("AgCRaz");
        services.AddKeyedSingleton<IAGURETPC, AgPeticaDouble>("AgPetica");

        // AGUARDARRegistry valida no arranque que todos os 7 destinos estão presentes.
        services.AddSingleton<AGUARDARRegistry>(sp =>
        {
            var dict = Destinations.ToDictionary(
                key => key,
                key => sp.GetRequiredKeyedService<IAGURETPC>(key));
            return new AGUARDARRegistry(dict);
        });

        return services;
    }
}
