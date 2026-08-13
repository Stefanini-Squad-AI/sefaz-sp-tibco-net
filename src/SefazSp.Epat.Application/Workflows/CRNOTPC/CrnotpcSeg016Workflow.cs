#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.CRNOTPC;
using SefazSp.Epat.Application.UseCases.CRNOTPC;

namespace SefazSp.Epat.Application.Workflows.CRNOTPC;

/// <summary>
/// Topologia do segmento 016 do processo CRNOTPC: de 'CriaNotificacao'
/// a 'Done - Bail' (passos 8–20 do cenário SC-CRNOTPC-009, segmento de ordem 1).
///
/// Trata 13 nos:
///   1  serviceTask  _NcJxMF9KEfGqPfX31TKC3w  CriaNotificacao                  (entrouPor=fluxo)
///   2  gateway      _NcJxLl9KEfGqPfX31TKC3w  gateway _NcJxLl9KEfGqPfX31TKC3w (entrouPor=fluxo)
///   3  scriptTask   _NcJxLV9KEfGqPfX31TKC3w  Set App Error                    (entrouPor=fluxo)
///   4  gateway      _NcJxL19KEfGqPfX31TKC3w  gateway _NcJxL19KEfGqPfX31TKC3w (entrouPor=fluxo)
///   5  endEvent     _NcJxK19KEfGqPfX31TKC3w  endEvent _NcJxK19KEfGqPfX31TKC3w(entrouPor=fluxo)
///   6  gateway      _NcJJ8V9KEfGqPfX31TKC3w  Tech Error                       (entrouPor=regresso)
///   7  gateway      _NcJJ8F9KEfGqPfX31TKC3w  App Error                        (entrouPor=fluxo)
///   8  gateway      _NcJJ7V9KEfGqPfX31TKC3w  More Retries                     (entrouPor=fluxo)
///   9  gateway      _NcJw8V9KEfGqPfX31TKC3w  gateway _NcJw8V9KEfGqPfX31TKC3w (entrouPor=fluxo)
///  10  userTask     _NcJJ6V9KEfGqPfX31TKC3w  Manipular Excecao                (entrouPor=fluxo)
///  11  gateway      _NcJJ419KEfGqPfX31TKC3w  Manually Fixed                   (entrouPor=fluxo)
///  12  gateway      _NcJJ6F9KEfGqPfX31TKC3w  Try Again                        (entrouPor=fluxo)
///  13  endEvent     _NcJJ5l9KEfGqPfX31TKC3w  Done - Bail                      (entrouPor=fluxo)
///
/// Nos sem transicao XPDL — escritos como arestas explicitas (decisao NOEQ-link-goto, flatten-edge):
///   - Ordem 6  (_NcJJ8V9KEfGqPfX31TKC3w, regresso): aresta de retorno explicita desde o fim
///     do ActivitySet (endEvent _NcJxK19KEfGqPfX31TKC3w) de volta ao escopo MAIN. Escrita
///     explicitamente como queda implicita no fluxo .NET — nao existe como transicao no XPDL.
/// </summary>
public sealed class CrnotpcSeg016Workflow
{
    private readonly IEpatServices _services;
    private readonly ManipularExcecaoUseCase _manipularExcecao;

    public CrnotpcSeg016Workflow(IEpatServices services, ManipularExcecaoUseCase manipularExcecao)
    {
        _services         = services;
        _manipularExcecao = manipularExcecao;
    }

    /// <summary>
    /// Executa o segmento completo, incluindo retentativas e tratamento de excecao.
    /// Retorna o resultado final do segmento.
    /// </summary>
    /// <param name="caseRef">Identidade do caso (correlacao com o legado).</param>
    /// <param name="ctx">Contexto de execucao mutavel partilhado com o resto do subprocesso.</param>
    /// <param name="decideOutcome">
    /// Delegate que representa a interacao humana em 'Manipular Excecao'.
    /// Em producao, suspende o workflow ate o operador submeter o formulario MANEXC.
    /// Em testes, substituido por um valor configurado no cenario.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Resultado do percurso: Sucesso, ErroAplicacaoSemRetentativa, EsgotouRetentativas ou DoneBail.</returns>
    public async Task<CrnotpcSeg016Result> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoResult>> decideOutcome,
        CancellationToken ct)
    {
        // ── Ponto de reingresso do laco de retry ──────────────────────────────
        // Equivalente ao regresso TIBCO — aresta explicita no fluxo .NET.
        CriaNotificacaoEntry:

        // ── No 1: serviceTask 'CriaNotificacao' (_NcJxMF9KEfGqPfX31TKC3w) ──────────────────────
        var envelope = await _services.CriarnotificacoesaiimAsync(caseRef, ct);
        CrnotpcSeg016Steps.MapServiceEnvelopeToContext(ctx, envelope);

        // ── No 2: gateway _NcJxLl9KEfGqPfX31TKC3w — "A chamada a CriaNotificacao foi bem sucedida?" ─
        if (!CrnotpcSeg016Steps.IsAppError(ctx))
        {
            // Ramo sucesso: STATUS_CODE == "0" — sai do ActivitySet via endEvent directo.
            // ── No 4: gateway _NcJxL19KEfGqPfX31TKC3w (ramo sem retentativa) ─────────────────────
            // ── No 5: endEvent _NcJxK19KEfGqPfX31TKC3w — fim do ActivitySet ────────────────────
            return CrnotpcSeg016Result.Sucesso;
        }

        // ── No 3: scriptTask 'Set App Error' (_NcJxLV9KEfGqPfX31TKC3w) ──────────────────────────
        CrnotpcSeg016Steps.SetAppError(ctx, envelope);

        // ── No 4: gateway _NcJxL19KEfGqPfX31TKC3w — encaminha para fim do ActivitySet ────────────
        // ── No 5: endEvent _NcJxK19KEfGqPfX31TKC3w — fim do ActivitySet ────────────────────────
        // (queda implicita para o regresso ao escopo MAIN)

        // ── No 6: gateway 'Tech Error' (_NcJJ8V9KEfGqPfX31TKC3w, entrouPor=regresso) ────────────
        // Aresta explicita de retorno: o iProcess NAO tem transicao XPDL aqui; a aresta e escrita
        // explicitamente no fluxo .NET desde o fim do ActivitySet ate este gateway.
        // Ramo "No" (OTHERWISE) leva a App Error — nao ha ramo "Yes" neste cenario.

        // ── No 7: gateway 'App Error' (_NcJJ8F9KEfGqPfX31TKC3w) — ISAPPERROR == 'Y'? ────────────
        if (!CrnotpcSeg016Steps.IsAppErrorFlag(ctx))
        {
            // Ramo No em App Error: nao e erro aplicacional (erro tecnico nao retentavel).
            return CrnotpcSeg016Result.ErroNaoAplicacao;
        }

        // ── No 8: gateway 'More Retries' (_NcJJ7V9KEfGqPfX31TKC3w) — NUMAPPRETRIES < MAXRETRIES ─
        if (CrnotpcSeg016Steps.HasMoreRetries(ctx))
        {
            // Ramo Yes: ainda ha retentativas — regressa ao inicio do laco.
            goto CriaNotificacaoEntry;
        }

        // Ramo No (OTHERWISE): retentativas esgotadas — avanca para gateway _NcJw8V9KEfGqPfX31TKC3w.

        // ── No 9: gateway _NcJw8V9KEfGqPfX31TKC3w — encaminha para Manipular Excecao ────────────
        // (pass-through: a decisao e implicita no alcance deste ponto)

        // ── No 10: userTask 'Manipular Excecao' (_NcJJ6V9KEfGqPfX31TKC3w) ──────────────────────
        await _manipularExcecao.ExecuteAsync(caseRef, ctx, decideOutcome, ct);

        // ── No 11: gateway 'Manually Fixed' (_NcJJ419KEfGqPfX31TKC3w) — OUTCOME == 'OK'? ────────
        if (CrnotpcSeg016Steps.IsManuallyFixed(ctx))
        {
            // Ramo Yes: caso resolvido manualmente.
            return CrnotpcSeg016Result.ManuallyFixed;
        }

        // Ramo No (OTHERWISE): avanca para gateway Try Again.

        // ── No 12: gateway 'Try Again' (_NcJJ6F9KEfGqPfX31TKC3w) — OUTCOME == 'R'? ────────────
        if (CrnotpcSeg016Steps.IsTryAgain(ctx))
        {
            // Ramo Yes: operador quer repetir a chamada — regressa ao inicio do laco.
            goto CriaNotificacaoEntry;
        }

        // Ramo No (OTHERWISE): avanca para Done - Bail.

        // ── No 13: endEvent 'Done - Bail' (_NcJJ5l9KEfGqPfX31TKC3w) ─────────────────────────────
        return CrnotpcSeg016Result.DoneBail;
    }
}

/// <summary>
/// Resultado possivel do percurso do segmento 016 do CRNOTPC.
/// </summary>
public enum CrnotpcSeg016Result
{
    /// <summary>STATUS_CODE == "0": a chamada CriaNotificacao foi bem sucedida.</summary>
    Sucesso,

    /// <summary>ISAPPERROR != "Y" apos Set App Error: erro tecnico nao retentavel.</summary>
    ErroNaoAplicacao,

    /// <summary>OUTCOME == "OK": caso resolvido manualmente pelo operador.</summary>
    ManuallyFixed,

    /// <summary>
    /// OUTCOME != "R" e OUTCOME != "OK" apos Manipular Excecao (OTHERWISE):
    /// encerra em Done - Bail (_NcJJ5l9KEfGqPfX31TKC3w).
    /// </summary>
    DoneBail,
}
