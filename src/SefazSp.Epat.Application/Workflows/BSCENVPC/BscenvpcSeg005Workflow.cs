#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.BSCENVPC;
using SefazSp.Epat.Domain.Abstractions;

namespace SefazSp.Epat.Application.Workflows.BSCENVPC;

/// <summary>
/// Topologia do segmento 005 do processo BSCENVPC: de 'Busca Envolvidos Vista Por AIIM'
/// a 'Try Task' (passos 8–18 do cenário SC-BSCENVPC-007, segmento de ordem 1).
///
/// Trata 11 nos:
///   1  serviceTask  _qIDu5F6BEfGBBLgT-R5iuw  Busca Envolvidos Vista Por AIIM   (entrouPor=fluxo)
///   2  gateway      _qIDu4l6BEfGBBLgT-R5iuw  Sucesso?                          (entrouPor=fluxo)
///   3  scriptTask   _qIDu4V6BEfGBBLgT-R5iuw  Set App Error                     (entrouPor=fluxo)
///   4  gateway      _qIDu416BEfGBBLgT-R5iuw  Encaminha para fim                (entrouPor=fluxo)
///   5  endEvent     _qIDu316BEfGBBLgT-R5iuw  Fim do ActivitySet                (entrouPor=fluxo)
///   6  gateway      _qIDupF6BEfGBBLgT-R5iuw  Tech Error                        (entrouPor=regresso)
///   7  gateway      _qIDuo16BEfGBBLgT-R5iuw  App Error                         (entrouPor=fluxo)
///   8  gateway      _qIDuoF6BEfGBBLgT-R5iuw  More Retries                      (entrouPor=fluxo)
///   9  timerEvent   _qIDuoV6BEfGBBLgT-R5iuw  Pause                             (entrouPor=fluxo)
///  10  linkThrow    _qIDun16BEfGBBLgT-R5iuw  Link To: Try Task                 (entrouPor=fluxo)
///  11  linkCatch    _qIDumF6BEfGBBLgT-R5iuw  Try Task                          (entrouPor=link)
///
/// Nos sem transicao XPDL — escritos como arestas explicitas (decisao NOEQ-link-goto, flatten-edge):
///   - Ordem 6  (_qIDupF6BEfGBBLgT-R5iuw, regresso): aresta de retorno explicita desde o fim do ActivitySet.
///   - Ordem 11 (_qIDumF6BEfGBBLgT-R5iuw, link): par linkThrow/linkCatch achatado em aresta directa.
/// </summary>
public sealed class BscenvpcSeg005Workflow
{
    private readonly IEpatServices _services;
    private readonly IClock _clock;

    public BscenvpcSeg005Workflow(IEpatServices services, IClock clock)
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
    /// <returns>Resultado do percurso: Sucesso, ErroAplicacaoSemRetentativa ou EsgotouRetentativas.</returns>
    public async Task<BscenvpcSeg005Result> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        CancellationToken ct)
    {
        // ── No 11: linkCatch 'Try Task' (_qIDumF6BEfGBBLgT-R5iuw, entrouPor=link) ──────────────
        // Aresta explicita: o par linkThrow (_qIDun16BEfGBBLgT-R5iuw) / linkCatch (_qIDumF6BEfGBBLgT-R5iuw)
        // e achatado numa aresta de fluxo directa — nao usar evento de sinal intermediario.
        // O label 'TryTaskEntry' e o ponto de reingresso do laco, equivalente ao linkCatch TIBCO.
        TryTaskEntry:

        // ── No 1: serviceTask 'Busca Envolvidos Vista Por AIIM' (_qIDu5F6BEfGBBLgT-R5iuw) ──────
        var envelope = await _services.BuscarvistasativasporaiimAsync(caseRef, ct);
        BscenvpcSeg005Steps.MapServiceEnvelopeToContext(ctx, envelope);

        // ── No 2: gateway _qIDu4l6BEfGBBLgT-R5iuw — "A chamada foi bem sucedida?" ──────────────
        if (!BscenvpcSeg005Steps.IsAppError(ctx))
        {
            // Ramo sucesso: STATUS_CODE == "0" — sai do ActivitySet via endEvent directo.
            // ── No 4 (gateway _qIDu416BEfGBBLgT-R5iuw, ramo sem retentativa) + No 5 (endEvent) ─
            return BscenvpcSeg005Result.Sucesso;
        }

        // ── No 3: scriptTask 'Set App Error' (_qIDu4V6BEfGBBLgT-R5iuw) ─────────────────────────
        BscenvpcSeg005Steps.SetAppError(ctx, envelope);

        // ── No 4: gateway _qIDu416BEfGBBLgT-R5iuw — encaminha para fim do ActivitySet ──────────
        // ── No 5: endEvent _qIDu316BEfGBBLgT-R5iuw — fim do ActivitySet ─────────────────────────
        // (queda implicita para o regresso ao escopo MAIN)

        // ── No 6: gateway 'Tech Error' (_qIDupF6BEfGBBLgT-R5iuw, entrouPor=regresso) ───────────
        // Aresta explicita de retorno: o iProcess nao tem transicao XPDL aqui; a aresta e escrita
        // explicitamente no fluxo .NET desde o fim do ActivitySet ate este gateway.
        // Ramo "No" (OTHERWISE) leva a App Error.

        // ── No 7: gateway 'App Error' (_qIDuo16BEfGBBLgT-R5iuw) — ISAPPERROR == 'Y'? ──────────
        if (!BscenvpcSeg005Steps.IsAppErrorFlag(ctx))
        {
            // Ramo No em App Error: nao e erro aplicacional (e.g. erro tecnico nao retentavel).
            return BscenvpcSeg005Result.ErroNaoAplicacao;
        }

        // ── No 8: gateway 'More Retries' (_qIDuoF6BEfGBBLgT-R5iuw) — NUMAPPRETRIES < MAXRETRIES ─
        if (!BscenvpcSeg005Steps.HasMoreRetries(ctx))
        {
            return BscenvpcSeg005Result.EsgotouRetentativas;
        }

        // ── No 9: timerEvent 'Pause' (_qIDuoV6BEfGBBLgT-R5iuw) ──────────────────────────────────
        // IClock injectado — DateTime.Now proibido.
        await PauseAsync(_clock, ct);

        // ── No 10: linkThrow 'Link To: Try Task' (_qIDun16BEfGBBLgT-R5iuw) ─────────────────────
        // Aresta explicita: achatado em goto para o linkCatch Try Task acima (flatten-edge).
        goto TryTaskEntry;
    }

    /// <summary>
    /// Implementa a pausa do timerEvent Pause (_qIDuoV6BEfGBBLgT-R5iuw).
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
/// Resultado possivel do percurso do segmento 005 do BSCENVPC.
/// </summary>
public enum BscenvpcSeg005Result
{
    /// <summary>STATUS_CODE == "0": a chamada foi bem sucedida.</summary>
    Sucesso,

    /// <summary>ISAPPERROR != "Y" apos Set App Error: erro tecnico nao retentavel.</summary>
    ErroNaoAplicacao,

    /// <summary>NUMAPPRETRIES >= MAXRETRIES: retentativas esgotadas.</summary>
    EsgotouRetentativas,
}
