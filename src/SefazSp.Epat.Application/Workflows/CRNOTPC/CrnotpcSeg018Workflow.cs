#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.CRNOTPC;

namespace SefazSp.Epat.Application.Workflows.CRNOTPC;

/// <summary>
/// Topologia do segmento 018 do processo CRNOTPC: de 'CriaNotificacao'
/// a 'Done - Success' (passos 8–15 do cenário SC-CRNOTPC-010, segmento de ordem 1).
///
/// Trata 8 nos:
///   1  serviceTask  _NcJxMF9KEfGqPfX31TKC3w  CriaNotificacao                   (entrouPor=fluxo)
///   2  gateway      _NcJxLl9KEfGqPfX31TKC3w  A chamada foi bem sucedida?       (entrouPor=fluxo)
///   3  scriptTask   _NcJxLV9KEfGqPfX31TKC3w  Set App Error                     (entrouPor=fluxo)
///   4  gateway      _NcJxL19KEfGqPfX31TKC3w  Converge ramos de erro            (entrouPor=fluxo)
///   5  endEvent     _NcJxK19KEfGqPfX31TKC3w  Fim do ActivitySet                (entrouPor=fluxo)
///   6  gateway      _NcJJ8V9KEfGqPfX31TKC3w  Tech Error                        (entrouPor=regresso)
///   7  gateway      _NcJJ8F9KEfGqPfX31TKC3w  App Error                         (entrouPor=fluxo)
///   8  endEvent     _NcJJ719KEfGqPfX31TKC3w  Done - Success                    (entrouPor=fluxo)
///
/// Nos sem transicao XPDL — escritos como arestas explicitas (decisao flatten-edge):
///   - Ordem 6  (_NcJJ8V9KEfGqPfX31TKC3w, regresso): aresta de retorno explicita desde o fim
///     do ActivitySet (endEvent _NcJxK19KEfGqPfX31TKC3w). NAO existe transicao XPDL aqui;
///     a ligacao de regresso e escrita explicitamente no fluxo .NET.
/// </summary>
public sealed class CrnotpcSeg018Workflow
{
    private readonly IEpatServices _services;

    public CrnotpcSeg018Workflow(IEpatServices services)
    {
        _services = services;
    }

    /// <summary>
    /// Executa o segmento completo, de CriaNotificacao a Done - Success.
    /// Retorna o resultado final do segmento.
    /// </summary>
    /// <param name="caseRef">Identidade do caso (correlacao com o legado).</param>
    /// <param name="ctx">Contexto de execucao mutavel partilhado com o resto do subprocesso.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Resultado do percurso: Sucesso ou ErroAplicacao (ambos convergem em Done - Success).</returns>
    public async Task<CrnotpcSeg018Result> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        CancellationToken ct)
    {
        // ── No 1: serviceTask 'CriaNotificacao' (_NcJxMF9KEfGqPfX31TKC3w, entrouPor=fluxo) ─────
        var envelope = await _services.CriarnotificacoesaiimAsync(caseRef, ct);
        CrnotpcSeg018Steps.MapServiceEnvelopeToContext(ctx, envelope);

        // ── No 2: gateway _NcJxLl9KEfGqPfX31TKC3w — "A chamada a CriaNotificacao foi bem sucedida?" ─
        // Ramo AppError: STATUS_CODE != "0"
        if (CrnotpcSeg018Steps.IsAppError(ctx))
        {
            // ── No 3: scriptTask 'Set App Error' (_NcJxLV9KEfGqPfX31TKC3w) ─────────────────────
            // Lógica de envelope técnico (STATUS_CODE, ISAPPERROR, contador NUMAPPRETRIES) — Application/Execution.
            CrnotpcSeg018Steps.SetAppError(ctx, envelope);

            // ── No 4: gateway _NcJxL19KEfGqPfX31TKC3w — converge ramos de erro sem lógica adicional ─
            // ── No 5: endEvent _NcJxK19KEfGqPfX31TKC3w — termina o ActivitySet ─────────────────────
            // (queda implicita: fim do ActivitySet, sem prosseguir inadvertidamente para Done - Success)

            // ── No 6: gateway 'Tech Error' (_NcJJ8V9KEfGqPfX31TKC3w, entrouPor=regresso) ─────────
            // Aresta explicita de retorno: NAO existe transicao XPDL deste endEvent para Tech Error.
            // A ligacao de regresso e escrita aqui explicitamente no fluxo .NET.
            // Ramo No (OTHERWISE) leva directamente a App Error — sem condicao de guarda.

            // ── No 7: gateway 'App Error' (_NcJJ8F9KEfGqPfX31TKC3w) ─────────────────────────────
            // Ramo No (OTHERWISE) leva a Done - Success — sem condicao de guarda.

            // ── No 8: endEvent 'Done - Success' (_NcJJ719KEfGqPfX31TKC3w) ───────────────────────
            return CrnotpcSeg018Result.ErroAplicacao;
        }

        // Ramo sucesso: STATUS_CODE == "0" — a chamada foi bem sucedida.
        // O fluxo avanca directamente para os gateways de saida (Tech Error → App Error → Done - Success).

        // ── No 6: gateway 'Tech Error' (_NcJJ8V9KEfGqPfX31TKC3w, entrouPor=regresso) ───────────
        // Aresta explicita de regresso a partir do ramo de sucesso do ActivitySet.
        // Ramo No (OTHERWISE) leva directamente a App Error — sem condicao de guarda.

        // ── No 7: gateway 'App Error' (_NcJJ8F9KEfGqPfX31TKC3w) ─────────────────────────────────
        // Ramo No (OTHERWISE) leva a Done - Success — sem condicao de guarda.

        // ── No 8: endEvent 'Done - Success' (_NcJJ719KEfGqPfX31TKC3w) ───────────────────────────
        return CrnotpcSeg018Result.Sucesso;
    }
}

/// <summary>
/// Resultado possivel do percurso do segmento 018 do CRNOTPC.
/// Ambos os valores convergem no endEvent 'Done - Success' (_NcJJ719KEfGqPfX31TKC3w);
/// a distinção é mantida para rastreabilidade do percurso e verificação do oráculo.
/// </summary>
public enum CrnotpcSeg018Result
{
    /// <summary>STATUS_CODE == "0": CriaNotificacao foi bem sucedida; Done - Success atingido via ramo directo.</summary>
    Sucesso,

    /// <summary>STATUS_CODE != "0": Set App Error executado; Done - Success atingido via Tech Error → App Error.</summary>
    ErroAplicacao,
}
