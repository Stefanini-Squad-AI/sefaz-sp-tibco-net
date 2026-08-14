#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.BSCENVPC;
using SefazSp.Epat.Application.UseCases.BSCENVPC;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.BSCENVPC;

/// <summary>
/// Desfecho possivel do segmento 048 do processo BSCENVPC.
/// </summary>
public enum BscenvpcSeg048Outcome
{
    /// <summary>
    /// gateway Check Retries avaliou IsStillgood: as tentativas do motor (SW_QRETRYCOUNT)
    /// estao dentro do limite — o ActivitySet pode prosseguir para a chamada de servico.
    /// O chamador e responsavel por instanciar o proximo segmento.
    /// </summary>
    ScopeStillgood,

    /// <summary>
    /// Retentativas do motor esgotadas, caso escalado para tratamento manual e resolvido
    /// — encerrou no endEvent 'Done - Fixed' (_qIDuml6BEfGBBLgT-R5iuw).
    /// </summary>
    DoneFixed,

    /// <summary>
    /// O operador na tarefa 'Manipular Excecao' optou por repetir (OUTCOME = 'R')
    /// ou o ramo de App Error nao se aplica — o loop externo decide.
    /// </summary>
    RetryRequested,
}

/// <summary>
/// Workflow do segmento 048 de BSCENVPC: de 'Start Event' a 'Done - Fixed'
/// (16 passos, cenario SC-BSCENVPC-014, ordens 1–16).
///
/// Topologia (16 nos):
///   [1]  startEvent      Start Event                   _qIDulF6BEfGBBLgT-R5iuw   MAIN
///   [2]  scriptTask      SetParameters                 _qIDulV6BEfGBBLgT-R5iuw   MAIN   (RI-script-BSCENVPC-SetParameters)
///   [3]  scriptTask      Start Loop                    _qIDul16BEfGBBLgT-R5iuw   MAIN
///   [4]  subProcessScope Control System Task Call      _qIDupV6BEfGBBLgT-R5iuw   MAIN
///   [5]  startEvent      startEvent (descida)          _qIDu3l6BEfGBBLgT-R5iuw   ActivitySet   (sem transicao XPDL — AC4)
///   [6]  scriptTask      Start TX                      _qIDu3F6BEfGBBLgT-R5iuw   ActivitySet
///   [7]  gateway         Check Retries SW_QRETRYCOUNT  _qIDu3V6BEfGBBLgT-R5iuw   ActivitySet   (RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT)
///   [8]  scriptTask      Set Technical Error           _qIDu4F6BEfGBBLgT-R5iuw   ActivitySet   (ramo OTHERWISE)
///   [9]  endEvent        endEvent                      _qIDu316BEfGBBLgT-R5iuw   ActivitySet
///  [10]  gateway         Tech Error (regresso)         _qIDupF6BEfGBBLgT-R5iuw   MAIN          (sem transicao XPDL — AC6)
///  [11]  gateway         App Error                     _qIDuo16BEfGBBLgT-R5iuw   MAIN
///  [12]  gateway         More Retries                  _qIDuoF6BEfGBBLgT-R5iuw   MAIN
///  [13]  gateway         gateway                       _qIDupl6BEfGBBLgT-R5iuw   MAIN
///  [14]  userTask        Manipular Excecao             _qIDunF6BEfGBBLgT-R5iuw   MAIN
///  [15]  gateway         Manually Fixed                _qIDull6BEfGBBLgT-R5iuw   MAIN
///  [16]  endEvent        Done - Fixed                  _qIDuml6BEfGBBLgT-R5iuw   MAIN
///
/// Transicoes explicitas (sem equivalente XPDL):
///   AC4: descida para o startEvent interno (_qIDu3l6BEfGBBLgT-R5iuw, ordem 5) — escrito
///        como entrada no bloco de escopo embutido.
///   AC6: regresso do ActivitySet para o gateway Tech Error (_qIDupF6BEfGBBLgT-R5iuw, ordem 10)
///        — escrito como continuacao apos o fecho do bloco de escopo.
///
/// NOEQ-iprocess-builtin (decisao shim-tri-state, ratificado 2026-08-06):
///   - <paramref name="swQRetryCount"/> e o valor de IPESystemValues.SW_QRETRYCOUNT
///     fornecido pelo motor; e injectado como parametro e nunca escrito pelo processo.
///   - <paramref name="idProcesso"/> e o campo IDPROCESSO com sentinela SW_NA,
///     representado como <see cref="FieldValue{T}"/>.
/// </summary>
public sealed class BscenvpcSeg048Workflow
{
    private readonly ManipularExcecaoUseCase _manipularExcecao;

    public BscenvpcSeg048Workflow(ManipularExcecaoUseCase manipularExcecao)
    {
        _manipularExcecao = manipularExcecao;
    }

    /// <summary>
    /// Executa o segmento 048 (passos 1–16) do processo BSCENVPC.
    /// </summary>
    /// <param name="caseRef">Referencia do caso (correlacao com o legado).</param>
    /// <param name="ctx">Contexto de execucao mutavel partilhado com o chamador.</param>
    /// <param name="idProcesso">
    ///   Campo IDPROCESSO do caso, com sentinela SW_NA preservado como FieldValue.
    ///   NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06).
    /// </param>
    /// <param name="swQRetryCount">
    ///   Valor de IPESystemValues.SW_QRETRYCOUNT fornecido pelo runtime do motor.
    ///   Nunca e escrito pelo processo — injectado aqui como parametro.
    ///   NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06).
    /// </param>
    /// <param name="decideManipularExcecao">
    ///   Delegate de interacao humana para a userTask 'Manipular Excecao' (_qIDunF6BEfGBBLgT-R5iuw).
    ///   Em producao suspende o workflow; em testes e substituido por valor de cenario.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O desfecho do segmento.</returns>
    public async Task<BscenvpcSeg048Outcome> ExecuteAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        FieldValue<long> idProcesso,
        long swQRetryCount,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoResult>> decideManipularExcecao,
        CancellationToken ct)
    {
        // ── passo 1: startEvent _qIDulF6BEfGBBLgT-R5iuw ─────────────────────
        // Ponto de entrada do fluxo BSCENVPC (AC1).
        // O controlo chega aqui vindo de POC_EpatProcess/Busca Emails.

        // ── passo 2: scriptTask SetParameters _qIDulV6BEfGBBLgT-R5iuw ───────
        // RI-script-BSCENVPC-SetParameters (AC2).
        BscenvpcSeg048Steps.ApplySetParameters(ctx, idProcesso);

        // ── passo 3: scriptTask Start Loop _qIDul16BEfGBBLgT-R5iuw ──────────
        // Inicializa o contador de retentativas de aplicacao para o ciclo (AC3).
        BscenvpcSeg048Steps.ApplyStartLoop(ctx);

        // ── passo 4: subProcessScope Control System Task Call _qIDupV6BEfGBBLgT-R5iuw ─
        // Descida para o ActivitySet embutido (AC3).
        //
        // ── passo 5: startEvent _qIDu3l6BEfGBBLgT-R5iuw (descida) ───────────
        // Transicao de descida escrita explicitamente — nao existe como transicao XPDL (AC4).
        {
            // ── passo 6: scriptTask Start TX _qIDu3F6BEfGBBLgT-R5iuw ─────────
            BscenvpcSeg048Steps.ApplyStartTx(ctx);

            // ── passo 7: gateway Check Retries SW_QRETRYCOUNT _qIDu3V6BEfGBBLgT-R5iuw ─
            // RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT (AC5).
            if (BscenvpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
            {
                // Ramo Stillgood: tentativas do motor dentro do limite.
                // O ActivitySet prosseguiria para a chamada de servico — fora do scope deste segmento.
                return BscenvpcSeg048Outcome.ScopeStillgood;
            }

            // Ramo OTHERWISE (Maxretriesexceeded): tentativas do motor esgotadas.
            // ── passo 8: scriptTask Set Technical Error _qIDu4F6BEfGBBLgT-R5iuw ─
            BscenvpcSeg048Steps.ApplySetTechnicalError(ctx);

            // ── passo 9: endEvent _qIDu316BEfGBBLgT-R5iuw ─────────────────────
            // Fim do ActivitySet. O controlo regressa ao escopo MAIN (AC5).
        }
        // ── fim do ActivitySet ─────────────────────────────────────────────────

        // ── passo 10: gateway Tech Error _qIDupF6BEfGBBLgT-R5iuw (regresso) ─
        // Transicao de regresso escrita explicitamente — nao existe como transicao XPDL (AC6).
        // Ramo OTHERWISE/No → encaminha para gateway App Error.

        // ── passo 11: gateway App Error _qIDuo16BEfGBBLgT-R5iuw ──────────────
        // Condicao Yes: ISAPPERROR == 'Y' → More Retries.
        // Ramo No: nao e erro de aplicacao; encerra este segmento.
        if (ctx.ISAPPERROR != "Y")
        {
            return BscenvpcSeg048Outcome.RetryRequested;
        }

        // ── passo 12: gateway More Retries _qIDuoF6BEfGBBLgT-R5iuw ──────────
        // Ramo OTHERWISE/No: sem mais retentativas de aplicacao → tratamento manual.
        // (Ramo Yes levaria de volta ao laco externo, fora do scope deste segmento.)

        // ── passo 13: gateway _qIDupl6BEfGBBLgT-R5iuw ────────────────────────
        // (anonimo — encaminha para userTask Manipular Excecao)

        // ── passo 14: userTask Manipular Excecao _qIDunF6BEfGBBLgT-R5iuw (AC7) ─
        await _manipularExcecao
            .ExecuteAsync(caseRef, ctx, decideManipularExcecao, ct)
            .ConfigureAwait(false);

        // ── passo 15: gateway Manually Fixed _qIDull6BEfGBBLgT-R5iuw ─────────
        // Condicao Yes: OUTCOME == 'OK' → Done - Fixed (AC7).
        if (ctx.OUTCOME == "OK")
        {
            // ── passo 16: endEvent Done - Fixed _qIDuml6BEfGBBLgT-R5iuw ─────
            return BscenvpcSeg048Outcome.DoneFixed;
        }

        // OUTCOME == 'R': operador quer repetir; o loop externo retoma o controlo.
        return BscenvpcSeg048Outcome.RetryRequested;
    }
}
