#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.CRNOTPC;
using SefazSp.Epat.Domain.Abstractions;

namespace SefazSp.Epat.Application.Workflows.CRNOTPC;

/// <summary>
/// Topologia do segmento 019 do processo CRNOTPC: de 'CriaNotificacao'
/// a 'Try Task' (passos 8–18 do cenário SC-CRNOTPC-007, segmento de ordem 1).
///
/// Trata 11 nos:
///   1  serviceTask  _NcJxMF9KEfGqPfX31TKC3w  CriaNotificacao                    (entrouPor=fluxo)
///   2  gateway      _NcJxLl9KEfGqPfX31TKC3w  gateway _NcJxLl9KEfGqPfX31TKC3w   (entrouPor=fluxo)
///   3  scriptTask   _NcJxLV9KEfGqPfX31TKC3w  Set App Error                      (entrouPor=fluxo)
///   4  gateway      _NcJxL19KEfGqPfX31TKC3w  gateway _NcJxL19KEfGqPfX31TKC3w   (entrouPor=fluxo)
///   5  endEvent     _NcJxK19KEfGqPfX31TKC3w  endEvent _NcJxK19KEfGqPfX31TKC3w  (entrouPor=fluxo)
///   6  gateway      _NcJJ8V9KEfGqPfX31TKC3w  Tech Error                         (entrouPor=regresso)
///   7  gateway      _NcJJ8F9KEfGqPfX31TKC3w  App Error                          (entrouPor=fluxo)
///   8  gateway      _NcJJ7V9KEfGqPfX31TKC3w  More Retries                       (entrouPor=fluxo)
///   9  timerEvent   _NcJJ7l9KEfGqPfX31TKC3w  Pause                              (entrouPor=fluxo)
///  10  linkThrow    _NcJJ7F9KEfGqPfX31TKC3w  Link To: Try Task                  (entrouPor=fluxo)
///  11  linkCatch    _NcJJ5V9KEfGqPfX31TKC3w  Try Task                           (entrouPor=link)
///
/// Nos sem transicao XPDL — escritos como arestas explicitas (decisao NOEQ-link-goto, flatten-edge):
///   - Ordem 6  (_NcJJ8V9KEfGqPfX31TKC3w, regresso): aresta de retorno explicita desde o fim do ActivitySet.
///   - Ordem 11 (_NcJJ5V9KEfGqPfX31TKC3w, link): par linkThrow/linkCatch achatado em aresta directa.
/// </summary>
public sealed class CrnotpcSeg019Workflow
{
    private readonly IEpatServices _services;
    private readonly IClock _clock;

    public CrnotpcSeg019Workflow(IEpatServices services, IClock clock)
    {
        _services = services;
        _clock    = clock;
    }

    /// <summary>
    /// Executa o segmento completo, incluindo retentativas ate esgotar MAXRETRIES ou sucesso.
    /// Retorna o resultado final do segmento.
    /// </summary>
    /// <param name="caseRef">Identidade do caso (correlacao com o legado).</param>
    /// <param name="ctx">Contexto de execucao mutavel partilhado com o resto do subprocesso.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Resultado do percurso: Sucesso, ErroNaoAplicacao ou EsgotouRetentativas.</returns>
    public async Task<CrnotpcSeg019Result> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        CancellationToken ct)
    {
        // ── No 11: linkCatch 'Try Task' (_NcJJ5V9KEfGqPfX31TKC3w, entrouPor=link) ──────────────
        // Aresta explicita: o par linkThrow (_NcJJ7F9KEfGqPfX31TKC3w) / linkCatch (_NcJJ5V9KEfGqPfX31TKC3w)
        // e achatado numa aresta de fluxo directa — nao usar evento de sinal intermediario.
        // Decisao NOEQ-link-goto, ratificada 2026-08-06: flatten-edge.
        // O label 'TryTaskEntry' e o ponto de reingresso do laco, equivalente ao linkCatch TIBCO.
        TryTaskEntry:

        // ── No 1: serviceTask 'CriaNotificacao' (_NcJxMF9KEfGqPfX31TKC3w) ──────────────────────
        var envelope = await _services.CriarnotificacoesaiimAsync(caseRef, ct);
        CrnotpcSeg019Steps.MapServiceEnvelopeToContext(ctx, envelope);

        // ── No 2: gateway _NcJxLl9KEfGqPfX31TKC3w — "A chamada a CriaNotificacao foi bem sucedida?" ─
        if (!CrnotpcSeg019Steps.IsAppError(ctx))
        {
            // Ramo sucesso: STATUS_CODE == "0" — sai do ActivitySet via endEvent directo.
            // ── No 4 (gateway _NcJxL19KEfGqPfX31TKC3w, ramo sem retentativa) + No 5 (endEvent) ─
            return CrnotpcSeg019Result.Sucesso;
        }

        // ── No 3: scriptTask 'Set App Error' (_NcJxLV9KEfGqPfX31TKC3w) ─────────────────────────
        CrnotpcSeg019Steps.SetAppError(ctx, envelope);

        // ── No 4: gateway _NcJxL19KEfGqPfX31TKC3w — encaminha para fim do ActivitySet ──────────
        // ── No 5: endEvent _NcJxK19KEfGqPfX31TKC3w — fim do ActivitySet ────────────────────────
        // (queda implicita para o regresso ao escopo MAIN)

        // ── No 6: gateway 'Tech Error' (_NcJJ8V9KEfGqPfX31TKC3w, entrouPor=regresso) ──────────
        // Aresta explicita de retorno: o iProcess nao tem transicao XPDL aqui; a aresta e escrita
        // explicitamente no fluxo .NET desde o fim do ActivitySet ate este gateway.
        // Ramo "No" (OTHERWISE) leva a App Error.

        // ── No 7: gateway 'App Error' (_NcJJ8F9KEfGqPfX31TKC3w) — ISAPPERROR == 'Y'? ──────────
        if (!CrnotpcSeg019Steps.IsAppErrorFlag(ctx))
        {
            // Ramo No em App Error: nao e erro aplicacional (e.g. erro tecnico nao retentavel).
            return CrnotpcSeg019Result.ErroNaoAplicacao;
        }

        // ── No 8: gateway 'More Retries' (_NcJJ7V9KEfGqPfX31TKC3w) — NUMAPPRETRIES < MAXRETRIES ─
        if (!CrnotpcSeg019Steps.HasMoreRetries(ctx))
        {
            return CrnotpcSeg019Result.EsgotouRetentativas;
        }

        // ── No 9: timerEvent 'Pause' (_NcJJ7l9KEfGqPfX31TKC3w) ──────────────────────────────────
        // IClock injectado — DateTime.Now proibido.
        await PauseAsync(_clock, ct);

        // ── No 10: linkThrow 'Link To: Try Task' (_NcJJ7F9KEfGqPfX31TKC3w) ─────────────────────
        // Aresta explicita: achatado em goto para o linkCatch Try Task acima (flatten-edge).
        goto TryTaskEntry;
    }

    /// <summary>
    /// Implementa a pausa do timerEvent Pause (_NcJJ7l9KEfGqPfX31TKC3w).
    /// O prazo e calculado a partir de <see cref="IClock.Now"/> — nunca de DateTime.Now.
    /// A duracao e fixada em 1 minuto por omissao (parametro de resiliencia, sem significado de negocio).
    /// </summary>
    private static async Task PauseAsync(IClock clock, CancellationToken ct)
    {
        var pauseDuration = TimeSpan.FromMinutes(1);
        var deadline      = clock.Now.Add(pauseDuration);
        var remaining     = deadline - clock.Now;
        if (remaining > TimeSpan.Zero)
            await Task.Delay(remaining, ct);
    }
}

/// <summary>
/// Resultado possivel do percurso do segmento 019 do CRNOTPC.
/// </summary>
public enum CrnotpcSeg019Result
{
    /// <summary>STATUS_CODE == "0": a chamada foi bem sucedida.</summary>
    Sucesso,

    /// <summary>ISAPPERROR != "Y" apos Set App Error: erro tecnico nao retentavel.</summary>
    ErroNaoAplicacao,

    /// <summary>NUMAPPRETRIES >= MAXRETRIES: retentativas esgotadas.</summary>
    EsgotouRetentativas,
}
