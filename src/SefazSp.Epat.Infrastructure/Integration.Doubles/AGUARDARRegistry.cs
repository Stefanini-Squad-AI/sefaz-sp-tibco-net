#nullable enable

// Card: BUILD-CONTROPC-seg039
// AC1 + AC2 — Registo de destinos da interface AGURETPC.
//
// Padrão: interface-registry-validated (NOEQ-dynamic-subprocess, ratificado 2026-08-06).
// Origem: callActivity _-bkw-V6JEfGBBLgT-R5iuw 'Aguardar Retorno', processo CONTROPC.
// Campo de resolução: AGUARDAR[IDX_AGUARDAR] (escrito por ISetSubProc, passo seg045).
//
// Destinos declarados em CONTROPC/ISetSubProc (POC_Epat.xpdl):
//   "AgPecas"  → AGPECASPC (pacote entregue, implementa IAGURETPC)
//   "AgPRJ"    → AgPRJDouble    (pacote externo não entregue na POC)
//   "AgRecPRJ" → AgRecPRJDouble (pacote externo não entregue na POC)
//   "AgPRJR"   → AgPRJRDouble   (pacote externo não entregue na POC)
//   "AgRCRaz"  → AgRCRazDouble  (pacote externo não entregue na POC)
//   "AgCRaz"   → AgCRazDouble   (pacote externo não entregue na POC)
//   "AgPetica" → AgPeticaDouble (pacote externo não entregue na POC)
//
// O registo é validado no arranque: um destino em falta lança InvalidOperationException
// imediatamente — NAO herda HaltOnBadSubProcess=false do legado TIBCO.
//
// LIMITAÇÃO DOCUMENTADA DA POC (AC2, BUILD-CONTROPC-seg039):
//   Os 6 doubles para processos externos lançam excepção ao serem invocados.
//   Isto é comportamento intencional — torna visível a ausência do pacote,
//   em contraste com a falha silenciosa do legado (HaltOnBadSubProcess=false).

using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Registo de destinos da interface de processo <c>AGURETPC</c>.
///
/// <para>
/// Validado no arranque da aplicação: um destino sem implementação concreta
/// ou duble lança <see cref="InvalidOperationException"/> de forma imediata e visível.
/// <b>NAO herda HaltOnBadSubProcess=false do legado TIBCO.</b>
/// </para>
///
/// <para>
/// Origem da chamada:
/// callActivity <c>_-bkw-V6JEfGBBLgT-R5iuw</c> "Aguardar Retorno" no processo CONTROPC.
/// O callee é resolvido em runtime pelo campo <c>AGUARDAR[IDX_AGUARDAR]</c>.
/// Card: BUILD-CONTROPC-seg039, etapa 4.
/// </para>
///
/// <para>
/// <b>Destinos esperados</b> (derivados de CONTROPC/ISetSubProc · <c>review-dossier.json &gt; NOEQ-dynamic-subprocess</c>):
/// <list type="bullet">
///   <item><description><c>"AgPecas"</c>  — AGPECASPC (entregue)</description></item>
///   <item><description><c>"AgPRJ"</c>    — <see cref="AgPRJDouble"/> (pacote externo não entregue)</description></item>
///   <item><description><c>"AgRecPRJ"</c> — <see cref="AgRecPRJDouble"/> (pacote externo não entregue)</description></item>
///   <item><description><c>"AgPRJR"</c>   — <see cref="AgPRJRDouble"/> (pacote externo não entregue)</description></item>
///   <item><description><c>"AgRCRaz"</c>  — <see cref="AgRCRazDouble"/> (pacote externo não entregue)</description></item>
///   <item><description><c>"AgCRaz"</c>   — <see cref="AgCRazDouble"/> (pacote externo não entregue)</description></item>
///   <item><description><c>"AgPetica"</c> — <see cref="AgPeticaDouble"/> (pacote externo não entregue)</description></item>
/// </list>
/// </para>
/// </summary>
public sealed class AGUARDARRegistry
{
    // Conjunto fechado de destinos declarados no XPDL (CONTROPC/ISetSubProc).
    // Derivado de review-dossier.json > NOEQ-dynamic-subprocess (ratificado 2026-08-06).
    private static readonly IReadOnlySet<string> ExpectedDestinations =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "AgPecas", "AgPRJ", "AgRecPRJ", "AgPRJR", "AgRCRaz", "AgCRaz", "AgPetica"
        };

    private readonly IReadOnlyDictionary<string, IAGURETPC> _destinations;

    /// <summary>
    /// Inicializa o registo e valida que todos os destinos esperados estão presentes.
    /// Lança <see cref="InvalidOperationException"/> se algum destino estiver em falta.
    /// </summary>
    /// <param name="destinations">
    /// Dicionário chave→implementação. A chave corresponde ao valor de
    /// <c>AGUARDAR[IDX_AGUARDAR]</c> escrito por <c>ISetSubProc</c>.
    /// </param>
    public AGUARDARRegistry(IReadOnlyDictionary<string, IAGURETPC> destinations)
    {
        if (destinations is null || destinations.Count == 0)
            throw new InvalidOperationException(
                "AGUARDARRegistry: nenhum destino registado. " +
                "Cada destino de AGUARDAR requer implementação concreta ou duble. " +
                "Destino sem implementação falha visivelmente no arranque, não em silêncio " +
                "(NOEQ-dynamic-subprocess, interface-registry-validated, 2026-08-06).");

        var missingKeys = ExpectedDestinations
            .Where(k => !destinations.ContainsKey(k))
            .OrderBy(k => k)
            .ToList();

        if (missingKeys.Count > 0)
            throw new InvalidOperationException(
                $"AGUARDARRegistry: destinos em falta: {string.Join(", ", missingKeys)}. " +
                "Registar implementação concreta ou duble antes de iniciar a aplicação. " +
                "Destino sem registo falha visivelmente — " +
                "NAO herda HaltOnBadSubProcess=false do legado TIBCO " +
                "(BUILD-CONTROPC-seg039, AC1+AC2).");

        _destinations = destinations;
    }

    /// <summary>
    /// Resolve a implementação de <see cref="IAGURETPC"/> para o <paramref name="destination"/> indicado.
    /// Lança <see cref="InvalidOperationException"/> se o destino não estiver registado.
    /// </summary>
    /// <param name="destination">
    /// Valor de <c>AGUARDAR[IDX_AGUARDAR]</c> determinado por <c>ISetSubProc</c>.
    /// </param>
    public IAGURETPC Resolve(string destination)
    {
        if (!_destinations.TryGetValue(destination, out var impl))
            throw new InvalidOperationException(
                $"AGUARDARRegistry: destino '{destination}' não tem implementação registada. " +
                "Registar implementação concreta ou duble antes de iniciar a aplicação " +
                "(NOEQ-dynamic-subprocess, interface-registry-validated, BUILD-CONTROPC-seg039).");

        return impl;
    }

    /// <summary>Destinos actualmente registados (para diagnóstico e testes).</summary>
    public IEnumerable<string> RegisteredDestinations => _destinations.Keys;
}
