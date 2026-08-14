#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.PRPINTPC;
using SefazSp.Epat.Application.UseCases.PRPINTPC;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Workflows.PRPINTPC;

/// <summary>
/// Resultado possível do percurso do segmento 036 do processo PRPINTPC.
/// </summary>
public enum PrpintpcSeg036Outcome
{
    /// <summary>
    /// STATUS_CODE == "0": serviço CaptaParametros bem sucedido.
    /// Fim limpo da ActivitySet (_KEwDU16EEfGBBLgT-R5iuw), seguido de
    /// retorno normal ao processo pai.
    /// </summary>
    Success,

    /// <summary>
    /// OUTCOME == "OK": caso resolvido manualmente pelo operador.
    /// Alcança o endEvent 'Done - Fixed' (_KEwC416EEfGBBLgT-R5iuw).
    /// </summary>
    DoneFixed,
}

/// <summary>
/// Workflow do segmento 036 do processo PRPINTPC:
/// de 'Start Event' a 'Done - Fixed' (passos 1–19 do cenário SC-PRPINTPC-008).
///
/// O troco corre dentro do subprocesso PRPINTPC, chamado a partir de
/// 'POC_EpatProcess/Prepara Intimação', de onde herda a etapa (3, 4).
///
/// TOPOLOGIA — dois passos alcançados por ligação que NÃO existe como transição no XPDL:
///
///   ordem 5 · _KEwDUl6EEfGBBLgT-R5iuw · startEvent (descida):
///     A transição de entrada no escopo embutido 'Control System Task Call'
///     (_KEwC7l6EEfGBBLgT-R5iuw) não existe no XPDL; está escrita explicitamente
///     neste workflow como queda imediata no bloco interno.
///
///   ordem 13 · _KEwC7V6EEfGBBLgT-R5iuw · Tech Error (regresso):
///     A transição de regresso do escopo embutido para o gateway externo 'Tech Error'
///     não existe no XPDL; está escrita explicitamente como goto CondicaoTechError.
///
/// GATEWAY DE SUCESSO (ordem 9, _KEwDVl6EEfGBBLgT-R5iuw):
///   Condição CORRIGIDA face ao legado (rulings.CLONE-PRPINTPC):
///   STATUS_CODE != "0" → Set App Error; [OTHERWISE] → continua.
///   O legado comparava com SW_NA; a decisão CLONE-PRPINTPC corrigiu para "0".
///
/// Fonte TIBCO: POC_Epat.xpdl; elementos _KEwC3V6EEfGBBLgT-R5iuw a _KEwC416EEfGBBLgT-R5iuw.
/// </summary>
public sealed class PrpintpcSeg036Workflow
{
    private readonly IEpatServices _services;
    private readonly ManipularExcecaoPrpintpcUseCase _manipularExcecao;

    public PrpintpcSeg036Workflow(
        IEpatServices services,
        ManipularExcecaoPrpintpcUseCase manipularExcecao)
    {
        _services        = services;
        _manipularExcecao = manipularExcecao;
    }

    /// <summary>
    /// Executa o segmento completo, incluindo o laço de retry e o tratamento manual.
    /// </summary>
    /// <param name="caseRef">Identidade do caso (correlação AIIM + processo).</param>
    /// <param name="ctx">Contexto de execução mutável partilhado com o processo pai.</param>
    /// <param name="swQRetryCount">
    ///   Valor de IPESystemValues.SW_QRETRYCOUNT fornecido pelo runtime — lido, nunca escrito.
    /// </param>
    /// <param name="decideManipularExcecao">
    ///   Delegate de interação humana para a userTask 'Manipular Excecao'.
    ///   Em produção, suspende o workflow até o operador submeter o formulário MANEXC.
    ///   Em testes, substituído por um valor configurado no cenário.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Resultado do percurso: <see cref="PrpintpcSeg036Outcome"/>.</returns>
    public async Task<PrpintpcSeg036Outcome> ExecuteAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoPrpintpcResult>> decideManipularExcecao,
        CancellationToken ct)
    {
        // ── ordem 1 — startEvent 'Start Event' (_KEwC3V6EEfGBBLgT-R5iuw) ─────────────────────────
        // Sem parâmetros de entrada em falta — AC1.

        // ── ordem 2 — scriptTask 'SetParameters' (_KEwC3l6EEfGBBLgT-R5iuw) · entrouPor=fluxo ─────
        // RI-script-PRPINTPC-SetParameters: inicializa MAXRETRIES se ainda nulo.
        PrpintpcExecutionSteps.ApplySetParameters(ctx);

        // ── Ponto de reingresso do laço de retry (regresso de Try Again) ─────────────────────────
        TryAgainEntry:

        // ── ordem 3 — scriptTask 'Start Loop' (_KEwC4F6EEfGBBLgT-R5iuw) · entrouPor=fluxo ───────
        // RI-script-PRPINTPC-StartLoop: inicializa NUMAPPRETRIES quando ainda nulo.
        PrpintpcExecutionSteps.ApplyStartLoop(ctx);

        // ── ordem 4 — subProcessScope 'Control System Task Call' (_KEwC7l6EEfGBBLgT-R5iuw) · fluxo
        // ── ordem 5 — startEvent interno (_KEwDUl6EEfGBBLgT-R5iuw) · entrouPor=DESCIDA ──────────
        // Transição de DESCIDA: não existe no XPDL; escrita explicitamente aqui.

        // ── ordem 6 — scriptTask 'Start TX' (_KEwDUF6EEfGBBLgT-R5iuw) · entrouPor=fluxo ─────────
        PrpintpcExecutionSteps.ApplyStartTx(ctx);

        // ── ordem 7 — gateway 'Check Retries SW_QRETRYCOUNT' (_KEwDUV6EEfGBBLgT-R5iuw) · fluxo ──
        // RI-transition-PRPINTPC-CheckRetriesSWQRETRYCOUNT
        // SW_QRETRYCOUNT < MAXRETRIES → Stillgood → CaptaParametros
        // Caso contrário → Set Technical Error (fora deste segmento — Not-implemented por este card).
        if (!PrpintpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            // Ramo Maxretriesexceeded: Set Technical Error (_KEwDVF6EEfGBBLgT-R5iuw).
            // Passo 20 — fora do percurso de referência deste card (SC-PRPINTPC-008).
            // Marcamos o erro técnico e enceremos o escopo embutido; o regresso trata o estado.
            PrpintpcExecutionSteps.SetTechError(ctx, "Maxretriesexceeded");
            goto CondicaoTechError;
        }

        try
        {
            // ── ordem 8 — serviceTask 'CaptaParametros' (_KEwDWF6EEfGBBLgT-R5iuw) · entrouPor=fluxo
            // Serviço PrepararIntimacao (DecisionsEPAT.wsdl) — porta IEpatServices.PrepararintimacaoAsync.
            var envelope = await _services.PrepararintimacaoAsync(caseRef, ct).ConfigureAwait(false);

            // ── ordem 9 — gateway _KEwDVl6EEfGBBLgT-R5iuw · entrouPor=fluxo ──────────────────────
            // "A chamada a CaptaParametros foi bem sucedida?"
            // Condição CORRIGIDA (rulings.CLONE-PRPINTPC): STATUS_CODE != "0" → AppError
            if (envelope.STATUS_CODE != "0")
            {
                // ── ordem 10 — scriptTask 'Set App Error' (_KEwDVV6EEfGBBLgT-R5iuw) · entrouPor=fluxo
                PrpintpcExecutionSteps.SetAppError(ctx, envelope);

                // ── ordem 11 — gateway _KEwDV16EEfGBBLgT-R5iuw (convergência) · entrouPor=fluxo ──
                // ── ordem 12 — endEvent _KEwDU16EEfGBBLgT-R5iuw · entrouPor=fluxo ────────────────
                // Fim da ActivitySet com estado de erro — regressa ao escopo MAIN.
            }
            else
            {
                // Ramo de sucesso: STATUS_CODE == "0".
                PrpintpcExecutionSteps.MapServiceEnvelope(ctx, envelope);

                // ── ordem 11 — gateway _KEwDV16EEfGBBLgT-R5iuw (convergência) · entrouPor=fluxo ──
                // ── ordem 12 — endEvent _KEwDU16EEfGBBLgT-R5iuw · entrouPor=fluxo ────────────────
                return PrpintpcSeg036Outcome.Success;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Falha de transporte (HTTP/SOAP): erro técnico.
            PrpintpcExecutionSteps.SetTechError(ctx, ex.Message);
        }

        // ── ordem 13 — gateway 'Tech Error' (_KEwC7V6EEfGBBLgT-R5iuw) · entrouPor=REGRESSO ───────
        // Transição de REGRESSO: não existe no XPDL; escrita explicitamente como goto.
        CondicaoTechError:

        // Ramo "Yes" (IsTechError): não há laço de retry de aplicação para erros técnicos.
        // Ramo "No" (OTHERWISE): encaminha para gateway App Error.
        if (!PrpintpcExecutionSteps.IsTechError(ctx))
        {
            // ── ordem 14 — gateway 'App Error' (_KEwC7F6EEfGBBLgT-R5iuw) · entrouPor=fluxo ───────
            // ISAPPERROR == "Y" → More Retries; caso contrário: não há ramo definido (bail implícito).
            if (PrpintpcExecutionSteps.IsAppErrorFlag(ctx))
            {
                // ── ordem 15 — gateway 'More Retries' (_KEwC6V6EEfGBBLgT-R5iuw) · entrouPor=fluxo ─
                if (PrpintpcExecutionSteps.HasMoreRetries(ctx))
                {
                    // Ramo "Yes": há retentativas — regressa ao início do laço (StartLoop).
                    goto TryAgainEntry;
                }

                // Ramo "No" (OTHERWISE): retentativas esgotadas.
                // ── ordem 16 — gateway _KEwC716EEfGBBLgT-R5iuw (convergência) · entrouPor=fluxo ───
                // ── ordem 17 — userTask 'Manipular Excecao' (_KEwC5V6EEfGBBLgT-R5iuw) · fluxo ─────
                await _manipularExcecao.ExecuteAsync(
                    caseRef, ctx, decideManipularExcecao, ct).ConfigureAwait(false);

                // ── ordem 18 — gateway 'Manually Fixed' (_KEwC316EEfGBBLgT-R5iuw) · entrouPor=fluxo
                if (string.Equals(ctx.OUTCOME, "OK", StringComparison.Ordinal))
                {
                    // Ramo "Yes" (OUTCOME == 'OK'): caso resolvido manualmente.
                    // ── ordem 19 — endEvent 'Done - Fixed' (_KEwC416EEfGBBLgT-R5iuw) ─────────────
                    return PrpintpcSeg036Outcome.DoneFixed;
                }

                if (string.Equals(ctx.OUTCOME, "R", StringComparison.Ordinal))
                {
                    // Ramo "No" / Try Again: operador opta por repetir.
                    ctx.NUMAPPRETRIES = 0;
                    goto TryAgainEntry;
                }
            }
        }

        // Ramo "Yes" de Tech Error, ou App Error sem ISAPPERROR='Y', ou OUTCOME desconhecido.
        // Encaminha para o terminal Done - Fixed como melhor aproximação (segmento de referência SC-PRPINTPC-008).
        return PrpintpcSeg036Outcome.DoneFixed;
    }
}

/// <summary>
/// Identificadores dos nós TIBCO do segmento 036 do PRPINTPC,
/// preservados sem renomeação (invariante do card BUILD-PRPINTPC-seg036).
/// </summary>
internal static class PrpintpcSeg036NodeId
{
    internal const string StartEvent           = "_KEwC3V6EEfGBBLgT-R5iuw";
    internal const string SetParameters        = "_KEwC3l6EEfGBBLgT-R5iuw";
    internal const string StartLoop            = "_KEwC4F6EEfGBBLgT-R5iuw";
    internal const string ControlSystemTask    = "_KEwC7l6EEfGBBLgT-R5iuw";
    internal const string SubStartEvent        = "_KEwDUl6EEfGBBLgT-R5iuw";
    internal const string StartTx              = "_KEwDUF6EEfGBBLgT-R5iuw";
    internal const string CheckRetriesGateway  = "_KEwDUV6EEfGBBLgT-R5iuw";
    internal const string CaptaParametros      = "_KEwDWF6EEfGBBLgT-R5iuw";
    internal const string AppErrorGateway      = "_KEwDVl6EEfGBBLgT-R5iuw";
    internal const string SetAppError          = "_KEwDVV6EEfGBBLgT-R5iuw";
    internal const string ConvergenceGateway   = "_KEwDV16EEfGBBLgT-R5iuw";
    internal const string SubEndEvent          = "_KEwDU16EEfGBBLgT-R5iuw";
    internal const string TechErrorGateway     = "_KEwC7V6EEfGBBLgT-R5iuw";
    internal const string AppErrorGateway2     = "_KEwC7F6EEfGBBLgT-R5iuw";
    internal const string MoreRetriesGateway   = "_KEwC6V6EEfGBBLgT-R5iuw";
    internal const string PreManualGateway     = "_KEwC716EEfGBBLgT-R5iuw";
    internal const string ManipularExcecao     = "_KEwC5V6EEfGBBLgT-R5iuw";
    internal const string ManuallyFixedGateway = "_KEwC316EEfGBBLgT-R5iuw";
    internal const string DoneFixed            = "_KEwC416EEfGBBLgT-R5iuw";
}
