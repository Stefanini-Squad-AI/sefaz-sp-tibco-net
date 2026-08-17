#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.BSCENVPC;
using SefazSp.Epat.Application.UseCases.BSCENVPC;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.BSCENVPC;

/// <summary>
/// Desfecho possível do percurso do segmento 047 do processo BSCENVPC:
/// de 'Start Event' a 'Done - Bail' (cenário SC-BSCENVPC-015, etapa 5).
/// </summary>
public enum BscenvpcSeg047Outcome
{
    /// <summary>
    /// SW_QRETRYCOUNT &lt; MAXRETRIES — o motor ainda tem tentativas disponíveis.
    /// O chamador é responsável por executar a chamada de serviço nesta iteração.
    /// Corresponde ao ramo Stillgood do gateway Check Retries (_qIDu3V6BEfGBBLgT-R5iuw).
    /// </summary>
    Stillgood,

    /// <summary>
    /// ISAPPERROR != 'Y' após regresso do subProcessScope — erro técnico sem classificação
    /// de aplicação; o fluxo encerra sem tratamento manual.
    /// </summary>
    AppErrorEnd,

    /// <summary>
    /// Operador considerou o caso resolvido manualmente (OUTCOME = 'OK').
    /// Encerra no endEvent 'Done - Fixed' (fora do escopo deste segmento).
    /// </summary>
    DoneFixed,

    /// <summary>
    /// Operador optou por repetir o ciclo (OUTCOME = 'R').
    /// O chamador reinvoca o segmento para uma nova iteração.
    /// </summary>
    TryAgain,

    /// <summary>
    /// OUTCOME != 'OK' e != 'R': operador encerrou sem repetir nem fixar.
    /// Terminus do segmento: endEvent 'Done - Bail' (_qIDumV6BEfGBBLgT-R5iuw).
    /// </summary>
    DoneBail,
}

/// <summary>
/// Workflow do segmento 047 do processo BSCENVPC:
/// de 'Start Event' a 'Done - Bail' (SC-BSCENVPC-015, etapa 5, 17 nós).
///
/// O processo é chamado a partir de 'POC_EpatProcess/Busca Emails' — a etapa é herdada.
///
/// Topologia completa (17 nós, escopo MAIN e ActivitySet):
///
///   MAIN
///   [1]  startEvent   Start Event                       _qIDulF6BEfGBBLgT-R5iuw
///   [2]  scriptTask   SetParameters                     _qIDulV6BEfGBBLgT-R5iuw  RI-script-BSCENVPC-SetParameters
///   [3]  scriptTask   Start Loop                        _qIDul16BEfGBBLgT-R5iuw
///   [4]  subProcess   Control System Task Call          _qIDupV6BEfGBBLgT-R5iuw
///   │
///   │   ActivitySet  (descida explícita — sem transição XPDL; AC3)
///   │   [5] startEvent  _qIDu3l6BEfGBBLgT-R5iuw        entrouPor=descida
///   │   [6] scriptTask  Start TX                        _qIDu3F6BEfGBBLgT-R5iuw
///   │   [7] gateway     Check Retries SW_QRETRYCOUNT    _qIDu3V6BEfGBBLgT-R5iuw  RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT
///   │        ↓ Maxretriesexceeded (OTHERWISE)
///   │   [8] scriptTask  Set Technical Error             _qIDu4F6BEfGBBLgT-R5iuw
///   │   [9] endEvent    _qIDu316BEfGBBLgT-R5iuw
///   │
///   MAIN (regresso explícito — sem transição XPDL; AC4/AC5)
///   [10] gateway     Tech Error                        _qIDupF6BEfGBBLgT-R5iuw   entrouPor=regresso
///   [11] gateway     App Error                         _qIDuo16BEfGBBLgT-R5iuw
///   [12] gateway     More Retries                      _qIDuoF6BEfGBBLgT-R5iuw
///   [13] gateway     _qIDupl6BEfGBBLgT-R5iuw
///   [14] userTask    Manipular Excecao                 _qIDunF6BEfGBBLgT-R5iuw   AC6
///   [15] gateway     Manually Fixed                    _qIDull6BEfGBBLgT-R5iuw
///   [16] gateway     Try Again                         _qIDum16BEfGBBLgT-R5iuw
///   [17] endEvent    Done - Bail                       _qIDumV6BEfGBBLgT-R5iuw   AC7
///
/// Nós sem transição XPDL — escritos como arestas explícitas:
///   - Ordem 5  (_qIDu3l6BEfGBBLgT-R5iuw, descida): entrada no ActivitySet do subProcessScope.
///   - Ordem 10 (_qIDupF6BEfGBBLgT-R5iuw, regresso): retorno do ActivitySet para o MAIN.
///
/// Regras de negócio:
///   - RI-script-BSCENVPC-SetParameters     → <see cref="BscenvpcSetParametersRule"/>
///   - RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT → <see cref="BscenvpcCheckRetriesRule"/>
///
/// Bloqueador NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06):
///   SW_QRETRYCOUNT é fornecido pelo runtime — nunca escrito pelo processo.
///   IDPROCESSO usa <see cref="FieldValue{T}"/> para o terceiro estado SW_NA.
///
/// Card: BUILD-BSCENVPC-seg047 · AC1–AC8
/// </summary>
public sealed class BscenvpcSeg047Workflow
{
    private readonly ManipularExcecaoUseCase _manipularExcecao;

    // ── Constantes de nó (imutáveis — AC: invariantes) ───────────────────────

    /// <summary>Nó 1  — Start Event (startEvent, MAIN).</summary>
    public const string NodeStartEvent            = "_qIDulF6BEfGBBLgT-R5iuw";

    /// <summary>Nó 2  — SetParameters (scriptTask, MAIN). Regra: RI-script-BSCENVPC-SetParameters.</summary>
    public const string NodeSetParameters         = "_qIDulV6BEfGBBLgT-R5iuw";

    /// <summary>Nó 3  — Start Loop (scriptTask, MAIN).</summary>
    public const string NodeStartLoop             = "_qIDul16BEfGBBLgT-R5iuw";

    /// <summary>Nó 4  — Control System Task Call (subProcessScope, MAIN).</summary>
    public const string NodeControlSystemTaskCall = "_qIDupV6BEfGBBLgT-R5iuw";

    /// <summary>Nó 5  — startEvent interno (startEvent, ActivitySet). Descida explícita.</summary>
    public const string NodeStartEventInternal    = "_qIDu3l6BEfGBBLgT-R5iuw";

    /// <summary>Nó 6  — Start TX (scriptTask, ActivitySet).</summary>
    public const string NodeStartTx               = "_qIDu3F6BEfGBBLgT-R5iuw";

    /// <summary>Nó 7  — Check Retries SW_QRETRYCOUNT (gateway, ActivitySet). Regra: RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT.</summary>
    public const string NodeCheckRetries          = "_qIDu3V6BEfGBBLgT-R5iuw";

    /// <summary>Nó 8  — Set Technical Error (scriptTask, ActivitySet).</summary>
    public const string NodeSetTechnicalError     = "_qIDu4F6BEfGBBLgT-R5iuw";

    /// <summary>Nó 9  — endEvent interno (endEvent, ActivitySet).</summary>
    public const string NodeEndEventInternal      = "_qIDu316BEfGBBLgT-R5iuw";

    /// <summary>Nó 10 — Tech Error (gateway, MAIN). Alcançado por regresso explícito.</summary>
    public const string NodeTechError             = "_qIDupF6BEfGBBLgT-R5iuw";

    /// <summary>Nó 11 — App Error (gateway, MAIN).</summary>
    public const string NodeAppError              = "_qIDuo16BEfGBBLgT-R5iuw";

    /// <summary>Nó 12 — More Retries (gateway, MAIN).</summary>
    public const string NodeMoreRetries           = "_qIDuoF6BEfGBBLgT-R5iuw";

    /// <summary>Nó 13 — gateway de convergência (gateway, MAIN).</summary>
    public const string NodeGatewayConvergence    = "_qIDupl6BEfGBBLgT-R5iuw";

    /// <summary>Nó 14 — Manipular Excecao (userTask, MAIN). AC6.</summary>
    public const string NodeManipularExcecao      = "_qIDunF6BEfGBBLgT-R5iuw";

    /// <summary>Nó 15 — Manually Fixed (gateway, MAIN).</summary>
    public const string NodeManuallyFixed         = "_qIDull6BEfGBBLgT-R5iuw";

    /// <summary>Nó 16 — Try Again (gateway, MAIN).</summary>
    public const string NodeTryAgain              = "_qIDum16BEfGBBLgT-R5iuw";

    /// <summary>Nó 17 — Done - Bail (endEvent, MAIN). AC7.</summary>
    public const string NodeDoneBail              = "_qIDumV6BEfGBBLgT-R5iuw";

    // ─────────────────────────────────────────────────────────────────────────

    public BscenvpcSeg047Workflow(ManipularExcecaoUseCase manipularExcecao)
    {
        _manipularExcecao = manipularExcecao;
    }

    /// <summary>
    /// Executa o percurso completo de 'Start Event' a 'Done - Bail'
    /// (SC-BSCENVPC-015, etapa 5, 17 nós).
    /// </summary>
    /// <param name="caseRef">Identidade do caso (correlação com o legado).</param>
    /// <param name="ctx">Contexto de execução mutável partilhado com o chamador.</param>
    /// <param name="idProcesso">
    ///   Campo IDPROCESSO tri-estado do caso — usa <see cref="FieldValue{T}"/> por força
    ///   do bloqueador NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06).
    ///   SW_NA NUNCA é mapeado para null.
    /// </param>
    /// <param name="swQRetryCount">
    ///   Valor de <c>IPESystemValues.SW_QRETRYCOUNT</c> fornecido pelo runtime iProcess.
    ///   Lido pelo gateway Check Retries (_qIDu3V6BEfGBBLgT-R5iuw); nunca escrito pelo processo.
    ///   NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06).
    /// </param>
    /// <param name="decideManipularExcecao">
    ///   Delegate de interação humana para a userTask 'Manipular Excecao' (_qIDunF6BEfGBBLgT-R5iuw).
    ///   Em produção suspende o workflow até o operador submeter o formulário MANEXC;
    ///   em testes é substituído pelo valor configurado no cenário.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O desfecho do percurso.</returns>
    public async Task<BscenvpcSeg047Outcome> ExecuteAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        FieldValue<long> idProcesso,
        long swQRetryCount,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoResult>> decideManipularExcecao,
        CancellationToken ct)
    {
        // ── Nó 1: startEvent 'Start Event' (_qIDulF6BEfGBBLgT-R5iuw) ──────────
        // Ponto de entrada do fluxo. Não há transição anterior. AC1.

        // ── Nó 2: scriptTask 'SetParameters' (_qIDulV6BEfGBBLgT-R5iuw) ────────
        // Regra: RI-script-BSCENVPC-SetParameters (classification.eRegraDeNegocio=true).
        // A decisão de domínio é avaliada em BscenvpcSetParametersRule.ShouldInitialize.
        // O efeito sobre o envelope técnico (MAXRETRIES, PROCESS_ID) fica em
        // BscenvpcExecutionSteps.ApplySetParameters. AC2.
        var maxRetriesNullable = ctx.MAXRETRIES == 0 ? (int?)null : ctx.MAXRETRIES;
        if (BscenvpcSetParametersRule.ShouldInitialize(idProcesso, maxRetriesNullable))
        {
            var processId = idProcesso.Match(
                hasValue:      v => v.ToString(),
                notAvailable:  ()  => null,
                empty:         ()  => null);
            BscenvpcExecutionSteps.ApplySetParameters(ctx, processId);
        }

        // ── Nó 3: scriptTask 'Start Loop' (_qIDul16BEfGBBLgT-R5iuw) ─────────
        // Inicializa NUMAPPRETRIES na primeira entrada no laço. AC3.
        BscenvpcExecutionSteps.ApplyStartLoop(ctx);

        // ── Nó 4: subProcessScope 'Control System Task Call' (_qIDupV6BEfGBBLgT-R5iuw) ──
        // ── Nó 5: startEvent interno (_qIDu3l6BEfGBBLgT-R5iuw, descida) ────────
        // DESCIDA EXPLÍCITA (AC3): não existe transição XPDL do subProcessScope
        // para o startEvent interno. A aresta é escrita explicitamente aqui.
        var subResult = ExecuteControlSystemTaskCall(ctx, swQRetryCount);

        // ── Nó 10: gateway 'Tech Error' (_qIDupF6BEfGBBLgT-R5iuw, regresso) ───
        // REGRESSO EXPLÍCITO (AC4/AC5): não existe transição XPDL do endEvent
        // interno de volta ao MAIN. A aresta é escrita explicitamente aqui.
        // Ramo "No" (OTHERWISE) → App Error.
        if (subResult == SubProcessResult.Stillgood)
        {
            // SW_QRETRYCOUNT < MAXRETRIES: o motor ainda tem tentativas.
            // Este segmento não cobre a chamada de serviço — o chamador executa-a.
            return BscenvpcSeg047Outcome.Stillgood;
        }

        // ── Nó 11: gateway 'App Error' (_qIDuo16BEfGBBLgT-R5iuw) ──────────────
        // Ramo Yes (CONDITION): ISAPPERROR == 'Y' → More Retries.
        // Ramo No  (OTHERWISE): ISAPPERROR != 'Y' → encerra sem retentativa manual.
        if (!BscenvpcSeg047Steps.IsAppErrorFlag(ctx))
            return BscenvpcSeg047Outcome.AppErrorEnd;

        // ── Nó 12: gateway 'More Retries' (_qIDuoF6BEfGBBLgT-R5iuw) ──────────
        // Ramo Yes: NUMAPPRETRIES < MAXRETRIES → ainda há retentativas de aplicação;
        //   o chamador reinicia o laço nesta iteração.
        // Ramo No (OTHERWISE): retentativas esgotadas → tratamento manual.
        if (BscenvpcSeg047Steps.HasMoreRetries(ctx))
            return BscenvpcSeg047Outcome.TryAgain;

        // ── Nó 13: gateway de convergência (_qIDupl6BEfGBBLgT-R5iuw) ──────────
        // (encaminha para userTask Manipular Excecao)

        // ── Nó 14: userTask 'Manipular Excecao' (_qIDunF6BEfGBBLgT-R5iuw) ─────
        // Implementado como caso de uso em Application/UseCases/BSCENVPC. AC6.
        await _manipularExcecao
            .ExecuteAsync(caseRef, ctx, decideManipularExcecao, ct)
            .ConfigureAwait(false);

        // ── Nó 15: gateway 'Manually Fixed' (_qIDull6BEfGBBLgT-R5iuw) ──────────
        // Ramo Yes (CONDITION): OUTCOME == 'OK' → operador resolveu manualmente.
        // Ramo No  (OTHERWISE): operador não resolveu → Try Again.
        if (BscenvpcSeg047Steps.IsManuallyFixed(ctx))
            return BscenvpcSeg047Outcome.DoneFixed;

        // ── Nó 16: gateway 'Try Again' (_qIDum16BEfGBBLgT-R5iuw) ────────────────
        // Ramo Yes (CONDITION): OUTCOME == 'R' → operador quer repetir.
        // Ramo No  (OTHERWISE): operador encerra sem repetir → Done - Bail. AC7.
        if (BscenvpcSeg047Steps.IsTryAgain(ctx))
            return BscenvpcSeg047Outcome.TryAgain;

        // ── Nó 17: endEvent 'Done - Bail' (_qIDumV6BEfGBBLgT-R5iuw) ────────────
        // Terminus do segmento: alcançado por transição de fluxo a partir de Try Again. AC7.
        return BscenvpcSeg047Outcome.DoneBail;
    }

    // ── ActivitySet interno do subProcessScope ────────────────────────────────

    private enum SubProcessResult { Stillgood, TechError }

    /// <summary>
    /// Executa os nós 5–9 do ActivitySet embutido no subProcessScope
    /// 'Control System Task Call' (_qIDupV6BEfGBBLgT-R5iuw).
    ///
    /// Nó 5 (descida explícita) e nó 9 (endEvent → regresso explícito) não têm
    /// equivalente como transição XPDL; estão escritos explicitamente aqui (AC3/AC5).
    /// </summary>
    private static SubProcessResult ExecuteControlSystemTaskCall(
        ProcessExecutionContext ctx,
        long swQRetryCount)
    {
        // ── Nó 5: startEvent interno (_qIDu3l6BEfGBBLgT-R5iuw) ─────────────────
        // DESCIDA EXPLÍCITA: ponto de entrada do ActivitySet.

        // ── Nó 6: scriptTask 'Start TX' (_qIDu3F6BEfGBBLgT-R5iuw) ──────────────
        BscenvpcExecutionSteps.ApplyStartTx(ctx);

        // ── Nó 7: gateway 'Check Retries SW_QRETRYCOUNT' (_qIDu3V6BEfGBBLgT-R5iuw)
        // Regra: RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT.
        // Ramo Stillgood: SW_QRETRYCOUNT < MAXRETRIES → continua para a chamada de serviço.
        // Ramo Maxretriesexceeded (OTHERWISE): retentativas do motor esgotadas → Set Technical Error.
        if (BscenvpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            // Ramo Stillgood: a chamada de serviço seria executada aqui.
            // Este segmento não cobre o nó de serviço — sinaliza ao chamador.
            return SubProcessResult.Stillgood;
        }

        // ── Nó 8: scriptTask 'Set Technical Error' (_qIDu4F6BEfGBBLgT-R5iuw) ───
        // Ramo Maxretriesexceeded: retentativas do motor (SW_QRETRYCOUNT) esgotadas.
        // Define ISTECHERROR = 'Y' e ISAPPERROR = 'Y' para activar o gateway
        // App Error (_qIDuo16BEfGBBLgT-R5iuw) a jusante (SC-BSCENVPC-015, decisions).
        BscenvpcSeg047Steps.SetTechnicalError(
            ctx,
            $"SW_QRETRYCOUNT ({swQRetryCount}) >= MAXRETRIES ({ctx.MAXRETRIES})");

        // ── Nó 9: endEvent interno (_qIDu316BEfGBBLgT-R5iuw) ────────────────────
        // Fim do ActivitySet. O controlo regressa ao MAIN via aresta de regresso explícita.
        return SubProcessResult.TechError;
    }
}
