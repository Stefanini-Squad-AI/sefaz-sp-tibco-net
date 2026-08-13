#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.CRNOTPC;
using SefazSp.Epat.Application.UseCases.CRNOTPC;

namespace SefazSp.Epat.Application.Workflows.CRNOTPC;

/// <summary>
/// Topologia do segmento 017 do processo CRNOTPC: de 'CriaNotificacao'
/// a 'Done - Fixed' (passos 8–19 do cenário SC-CRNOTPC-008, segmento de ordem 1).
///
/// Trata 12 nós:
///   1  serviceTask  _NcJxMF9KEfGqPfX31TKC3w  CriaNotificacao                   (entrouPor=fluxo)
///   2  gateway      _NcJxLl9KEfGqPfX31TKC3w  A chamada foi bem sucedida?        (entrouPor=fluxo)
///   3  scriptTask   _NcJxLV9KEfGqPfX31TKC3w  Set App Error                      (entrouPor=fluxo)
///   4  gateway      _NcJxL19KEfGqPfX31TKC3w  Encaminha para fim do ActivitySet  (entrouPor=fluxo)
///   5  endEvent     _NcJxK19KEfGqPfX31TKC3w  Fim do ActivitySet                 (entrouPor=fluxo)
///   6  gateway      _NcJJ8V9KEfGqPfX31TKC3w  Tech Error                         (entrouPor=regresso)
///   7  gateway      _NcJJ8F9KEfGqPfX31TKC3w  App Error                          (entrouPor=fluxo)
///   8  gateway      _NcJJ7V9KEfGqPfX31TKC3w  More Retries                       (entrouPor=fluxo)
///   9  gateway      _NcJw8V9KEfGqPfX31TKC3w  gateway _NcJw8V9KEfGqPfX31TKC3w   (entrouPor=fluxo)
///  10  userTask     _NcJJ6V9KEfGqPfX31TKC3w  Manipular Excecao                  (entrouPor=fluxo)
///  11  gateway      _NcJJ419KEfGqPfX31TKC3w  Manually Fixed                     (entrouPor=fluxo)
///  12  endEvent     _NcJJ519KEfGqPfX31TKC3w  Done - Fixed                       (entrouPor=fluxo)
///
/// Nó sem transição XPDL — escrito como aresta explícita:
///   - Ordem 6  (_NcJJ8V9KEfGqPfX31TKC3w, regresso): aresta de retorno explícita desde
///     o fim do ActivitySet (escopo MAIN). Não existe como transição no XPDL;
///     a aresta é escrita explicitamente no fluxo .NET.
/// </summary>
public sealed class CrnotpcSeg017Workflow
{
    private readonly IEpatServices _services;
    private readonly ManipularExcecaoUseCase _manipularExcecao;

    public CrnotpcSeg017Workflow(IEpatServices services, ManipularExcecaoUseCase manipularExcecao)
    {
        _services         = services;
        _manipularExcecao = manipularExcecao;
    }

    /// <summary>
    /// Executa o segmento completo, incluindo retentativas até esgotar MAXRETRIES ou
    /// resolução manual pelo operador.
    /// Retorna o resultado final do segmento.
    /// </summary>
    /// <param name="caseRef">Identidade do caso (correlação com o legado).</param>
    /// <param name="ctx">Contexto de execução mutável partilhado com o resto do subprocesso.</param>
    /// <param name="decideOutcome">
    /// Delegate de interação humana para a userTask 'Manipular Excecao'.
    /// Em produção suspende o workflow; em testes é substituído por valor do cenário.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Resultado do percurso: Sucesso, ErroNaoAplicacao ou DoneFixed.</returns>
    public async Task<CrnotpcSeg017Result> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoResult>> decideOutcome,
        CancellationToken ct)
    {
        // ── Nó 1: serviceTask 'CriaNotificacao' (_NcJxMF9KEfGqPfX31TKC3w) ────────────────────
        // Chamada ao serviço definido em IEpatServices.CriarnotificacoesaiimAsync (porta final).
        var envelope = await _services.CriarnotificacoesaiimAsync(caseRef, ct);
        CrnotpcSeg017Steps.MapServiceEnvelopeToContext(ctx, envelope);

        // ── Nó 2: gateway _NcJxLl9KEfGqPfX31TKC3w — "A chamada a CriaNotificacao foi bem sucedida?" ─
        if (!CrnotpcSeg017Steps.IsCallFailed(ctx))
        {
            // Ramo sucesso: STATUS_CODE == "0".
            // ── Nó 4 (gateway _NcJxL19KEfGqPfX31TKC3w, ramo sem retentativa) + Nó 5 (endEvent) ─
            // Subprocesso termina normalmente — regressa ao chamador POC_EpatProcess/Criar Notificacao.
            return CrnotpcSeg017Result.Sucesso;
        }

        // ── Nó 3: scriptTask 'Set App Error' (_NcJxLV9KEfGqPfX31TKC3w) ─────────────────────────
        // ISAPPERROR='Y', incrementa NUMAPPRETRIES. Sem valores literais fixos (SCRIPT-HARDCODED verificado).
        CrnotpcSeg017Steps.SetAppError(ctx, envelope);

        // ── Nó 4: gateway _NcJxL19KEfGqPfX31TKC3w — encaminha para fim do ActivitySet ──────────
        // ── Nó 5: endEvent _NcJxK19KEfGqPfX31TKC3w — fim do ActivitySet ─────────────────────────
        // (queda implícita para o regresso ao escopo MAIN)

        // ── Nó 6: gateway 'Tech Error' (_NcJJ8V9KEfGqPfX31TKC3w, entrouPor=regresso) ────────────
        // Aresta explícita de retorno: o iProcess não tem transição XPDL aqui; a aresta é escrita
        // explicitamente no fluxo .NET desde o fim do ActivitySet até este gateway.
        // Ramo "No" (OTHERWISE) leva a App Error.

        // ── Nó 7: gateway 'App Error' (_NcJJ8F9KEfGqPfX31TKC3w) — ISAPPERROR == 'Y'? ──────────
        if (!CrnotpcSeg017Steps.IsAppErrorFlag(ctx))
        {
            // Ramo No em App Error: erro técnico não retentável.
            return CrnotpcSeg017Result.ErroNaoAplicacao;
        }

        // ── Nó 8: gateway 'More Retries' (_NcJJ7V9KEfGqPfX31TKC3w) — NUMAPPRETRIES < MAXRETRIES ─
        if (CrnotpcSeg017Steps.HasMoreRetries(ctx))
        {
            // Ramo Yes: ainda há retentativas — o chamador (prologue / Start Loop) gere o laço.
            return CrnotpcSeg017Result.RetentativaDisponivel;
        }

        // Ramo No (OTHERWISE): retentativas esgotadas.

        // ── Nó 9: gateway _NcJw8V9KEfGqPfX31TKC3w ────────────────────────────────────────────────
        // Encaminha para Manipular Excecao (único ramo neste percurso).

        // ── Nó 10: userTask 'Manipular Excecao' (_NcJJ6V9KEfGqPfX31TKC3w) ──────────────────────
        await _manipularExcecao.ExecuteAsync(caseRef, ctx, decideOutcome, ct);

        // ── Nó 11: gateway 'Manually Fixed' (_NcJJ419KEfGqPfX31TKC3w) — OUTCOME == 'OK'? ───────
        if (CrnotpcSeg017Steps.IsManuallyFixed(ctx))
        {
            // ── Nó 12: endEvent 'Done - Fixed' (_NcJJ519KEfGqPfX31TKC3w) ─────────────────────────
            return CrnotpcSeg017Result.DoneFixed;
        }

        // Ramo Retry (OUTCOME != 'OK'): o chamador trata o reingresso no laço.
        return CrnotpcSeg017Result.RetentativaDisponivel;
    }
}

/// <summary>
/// Resultado possível do percurso do segmento 017 do CRNOTPC.
/// </summary>
public enum CrnotpcSeg017Result
{
    /// <summary>STATUS_CODE == "0": a chamada foi bem sucedida.</summary>
    Sucesso,

    /// <summary>ISAPPERROR != "Y" após Set App Error: erro técnico não retentável.</summary>
    ErroNaoAplicacao,

    /// <summary>
    /// NUMAPPRETRIES &lt; MAXRETRIES: retentativa disponível (chamador gere o laço),
    /// ou OUTCOME != "OK" após Manipular Excecao (operador optou por repetir).
    /// </summary>
    RetentativaDisponivel,

    /// <summary>OUTCOME == "OK" após Manipular Excecao: caso encerrado manualmente.</summary>
    DoneFixed,
}
