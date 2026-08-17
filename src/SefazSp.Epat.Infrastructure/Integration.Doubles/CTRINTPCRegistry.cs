#nullable enable

// Card: BUILD-POCEPATPROCESS-seg054
// AC3 + AC4 — Registo de destinos da interface CTRINTPC.
//
// Padrão: interface-registry-validated (NOEQ-dynamic-subprocess, ratificado 2026-08-06).
// Origem: callActivity _nQntZ16JEfGBBLgT-R5iuw 'Controlar Intimados', processo POC_EpatProcess.
//
// Destinos declarados (POC_Epat.xpdl, elemento _nQntZ16JEfGBBLgT-R5iuw):
//   "CONTROPC" → CONTROPCDouble (processo entregue no pacote)
//
// O registo é validado no arranque: um destino em falta lança InvalidOperationException
// imediatamente — NAO herda HaltOnBadSubProcess=false do legado TIBCO.

using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Registo de destinos da interface de processo <c>CTRINTPC</c>.
///
/// <para>
/// Validado no arranque da aplicação: um destino sem implementação concreta
/// ou duble lança <see cref="InvalidOperationException"/> de forma imediata e visível.
/// <b>NAO herda HaltOnBadSubProcess=false do legado TIBCO.</b>
/// </para>
///
/// <para>
/// Origem da chamada:
/// callActivity <c>_nQntZ16JEfGBBLgT-R5iuw</c> "Controlar Intimados" no processo POC_EpatProcess.
/// O callee é resolvido em runtime pelo campo do caso (dynamic-subprocess).
/// Card: BUILD-POCEPATPROCESS-seg054, etapa 6.
/// </para>
///
/// <para>
/// <b>Destinos esperados</b> (derivados de POC_Epat.xpdl · <c>review-dossier.json &gt; NOEQ-dynamic-subprocess</c>):
/// <list type="bullet">
///   <item><description><c>"CONTROPC"</c> — <see cref="CONTROPCDouble"/> (pacote entregue)</description></item>
/// </list>
/// </para>
/// </summary>
public sealed class CTRINTPCRegistry
{
    // Conjunto fechado de destinos declarados no XPDL para a callActivity _nQntZ16JEfGBBLgT-R5iuw.
    // Derivado de review-dossier.json > NOEQ-dynamic-subprocess (ratificado 2026-08-06).
    private static readonly IReadOnlySet<string> ExpectedDestinations =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "CONTROPC"
        };

    private readonly IReadOnlyDictionary<string, ICTRINTPC> _destinations;

    /// <summary>
    /// Inicializa o registo e valida que todos os destinos esperados estão presentes.
    /// Lança <see cref="InvalidOperationException"/> se algum destino estiver em falta.
    /// </summary>
    /// <param name="destinations">
    /// Dicionário chave→implementação. A chave corresponde ao valor do campo de caso
    /// que identifica o subprocesso destino em runtime.
    /// </param>
    public CTRINTPCRegistry(IReadOnlyDictionary<string, ICTRINTPC> destinations)
    {
        if (destinations is null || destinations.Count == 0)
            throw new InvalidOperationException(
                "CTRINTPCRegistry: nenhum destino registado. " +
                "Cada destino de CTRINTPC requer implementação concreta ou duble. " +
                "Destino sem implementação falha visivelmente no arranque, não em silêncio " +
                "(NOEQ-dynamic-subprocess, interface-registry-validated, 2026-08-06).");

        var missingKeys = ExpectedDestinations
            .Where(k => !destinations.ContainsKey(k))
            .OrderBy(k => k)
            .ToList();

        if (missingKeys.Count > 0)
            throw new InvalidOperationException(
                $"CTRINTPCRegistry: destinos em falta: {string.Join(", ", missingKeys)}. " +
                "Registar implementação concreta ou duble antes de iniciar a aplicação. " +
                "Destino sem registo falha visivelmente — " +
                "NAO herda HaltOnBadSubProcess=false do legado TIBCO " +
                "(BUILD-POCEPATPROCESS-seg054, AC3+AC4).");

        _destinations = destinations;
    }

    /// <summary>
    /// Resolve a implementação de <see cref="ICTRINTPC"/> para o <paramref name="destination"/> indicado.
    /// Lança <see cref="InvalidOperationException"/> se o destino não estiver registado.
    /// </summary>
    /// <param name="destination">
    /// Valor do campo de caso que identifica o subprocesso destino em runtime.
    /// </param>
    public ICTRINTPC Resolve(string destination)
    {
        if (!_destinations.TryGetValue(destination, out var impl))
            throw new InvalidOperationException(
                $"CTRINTPCRegistry: destino '{destination}' não tem implementação registada. " +
                "Registar implementação concreta ou duble antes de iniciar a aplicação " +
                "(NOEQ-dynamic-subprocess, interface-registry-validated, BUILD-POCEPATPROCESS-seg054).");

        return impl;
    }

    /// <summary>Destinos actualmente registados (para diagnóstico e testes).</summary>
    public IEnumerable<string> RegisteredDestinations => _destinations.Keys;
}
