#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Application.Workflows;

/// <summary>
/// Troco do workflow BSCENVPC: de 'Busca Envolvidos Vista Por AIIM' ate 'Done - Bail'.
/// Cobre os 13 nos da jornada (passos 8-20 do cenario SC-BSCENVPC-009).
///
/// Topologia derivada de POC_Epat.xpdl; implementada como maquina de estados explicita
/// conforme a nota de scaffold de Application/Workflows.
///
/// ATENCAO — no por regresso (ordem 6):
///   O gateway 'Tech Error' (_qIDupF6BEfGBBLgT-R5iuw) NAO existe como transicao no XPDL;
///   e alcancado por regresso desde o MAIN scope apos o endEvent da ActivitySet
///   (_qIDu316BEfGBBLgT-R5iuw). A aresta e escrita explicitamente aqui como ligacao
///   de regresso (backward link). Omiti-la deixaria o no inalcancavel e os casos de
///   erro tecnico nunca chegariam ao laco de retry.
/// </summary>
public sealed class BscenvpcWorkflowSeg002
{
    private readonly IEpatServices _services;

    public BscenvpcWorkflowSeg002(IEpatServices services)
        => _services = services;

    /// <summary>
    /// Executa o troco de 'Busca Envolvidos Vista Por AIIM' ate um dos dois terminais.
    /// O delegado <paramref name="requestManualHandling"/> suspende o fluxo ate o
    /// operador completar a tarefa humana 'Manipular Excecao'.
    /// </summary>
    /// <param name="ctx">Contexto de execucao mutavel do processo.</param>
    /// <param name="caseRef">Referencia do caso (correlacao AIIM + processo).</param>
    /// <param name="requestManualHandling">
    /// Chamado quando o laco de retry se esgota; deve bloquear ate o operador
    /// ter preenchido <see cref="Execution.ProcessExecutionContext.OUTCOME"/>.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O terminal alcancado e a razao.</returns>
    public async Task<BscenvpcSeg002Result> ExecuteAsync(
        Execution.ProcessExecutionContext ctx,
        AiimCaseRef caseRef,
        Func<Execution.ProcessExecutionContext, CancellationToken, Task> requestManualHandling,
        CancellationToken ct)
    {
        // ----------------------------------------------------------------
        // ordem 1 — serviceTask 'Busca Envolvidos Vista Por AIIM'
        //            _qIDu5F6BEfGBBLgT-R5iuw · entrouPor=fluxo
        // ----------------------------------------------------------------
        var envelope = await _services.BuscarvistasativasporaiimAsync(caseRef, ct);

        // ----------------------------------------------------------------
        // ordem 2 — gateway _qIDu4l6BEfGBBLgT-R5iuw · entrouPor=fluxo
        //            Condicao AppError: STATUS_CODE != "0"   (rulings.CLONE-PRPINTPC)
        // ----------------------------------------------------------------
        bool isAppError = envelope.STATUS_CODE != "0";

        if (!isAppError)
        {
            // Ramo de sucesso
            // ordem 4 — gateway _qIDu416BEfGBBLgT-R5iuw (convergencia)
            // ordem 5 — endEvent _qIDu316BEfGBBLgT-R5iuw (fim limpo da ActivitySet)
            ctx.STATUS_CODE = envelope.STATUS_CODE;
            ctx.STERRORCODE = envelope.STERRORCODE;
            ctx.STERRORDESC = envelope.STERRORDESC;
            return BscenvpcSeg002Result.CleanEnd(NodeId._qIDu316BEfGBBLgT_R5iuw);
        }

        // ----------------------------------------------------------------
        // ordem 3 — scriptTask 'Set App Error' _qIDu4V6BEfGBBLgT-R5iuw · entrouPor=fluxo
        //            Actualiza o envelope tecnico; nao decide regras de negocio.
        //            STATUS_CODE != "0" e erro de aplicacao neste troco (rulings.CLONE-PRPINTPC).
        //            Erros tecnicos (transporte JMS/rede) sao surfaced pelo executor como
        //            excepco e nao chegam a este passo; por isso IsTechError=false aqui.
        // ----------------------------------------------------------------
        Execution.BscenvpcSetAppErrorStep.Apply(ctx, new Execution.ServiceEnvelopeResult(
            StatusCode: envelope.STATUS_CODE,
            ErrorCode: envelope.STERRORCODE,
            ErrorDesc: envelope.STERRORDESC,
            IsTechError: false,
            IsAppError: true));

        // ordem 4 — gateway _qIDu416BEfGBBLgT-R5iuw (convergencia)
        // ordem 5 — endEvent _qIDu316BEfGBBLgT-R5iuw (sai da ActivitySet com estado de erro)

        // ----------------------------------------------------------------
        // ordem 6 — gateway 'Tech Error' _qIDupF6BEfGBBLgT-R5iuw · entrouPor=REGRESSO
        //            NAO existe como transicao no XPDL: aresta de regresso escrita explicitamente.
        //            Ramo 'No' (OTHERWISE) leva a 'App Error'; ramo 'Yes' (tech error) tambem
        //            converge no mesmo laco de retry via App Error.
        // ----------------------------------------------------------------

        // ordem 7 — gateway 'App Error' _qIDuo16BEfGBBLgT-R5iuw · entrouPor=fluxo
        //            Ramo 'Yes' (ISAPPERROR == 'Y') leva a 'More Retries'
        bool enterRetryLoop = string.Equals(ctx.ISAPPERROR, "Y", StringComparison.Ordinal);

        if (!enterRetryLoop)
        {
            // Caso sem ISAPPERROR='Y' nao tem laco de retry; termina em bail.
            return BscenvpcSeg002Result.Bail(NodeId._qIDumV6BEfGBBLgT_R5iuw,
                "App Error gateway: ISAPPERROR != 'Y', sem ramo de retry definido.");
        }

        // ----------------------------------------------------------------
        // ordem 8 — gateway 'More Retries' _qIDuoF6BEfGBBLgT-R5iuw · entrouPor=fluxo
        //            Ramo 'No' (OTHERWISE) — sem mais retentativas — leva a
        //            gateway _qIDupl6BEfGBBLgT-R5iuw → Manipular Excecao
        // ----------------------------------------------------------------
        bool hasMoreRetries = ctx.NUMAPPRETRIES < ctx.MAXRETRIES;

        if (hasMoreRetries)
        {
            // Ainda ha retentativas: sinaliza ao chamador para reiniciar o troco.
            return BscenvpcSeg002Result.Retry(ctx.NUMAPPRETRIES, ctx.MAXRETRIES);
        }

        // ordem 9 — gateway _qIDupl6BEfGBBLgT-R5iuw · entrouPor=fluxo
        // Retentativas esgotadas; encaminha para tarefa humana.

        // ----------------------------------------------------------------
        // ordem 10 — userTask 'Manipular Excecao' _qIDunF6BEfGBBLgT-R5iuw · entrouPor=fluxo
        //             Caso de uso em UseCases; suspende ate o operador decidir.
        // ----------------------------------------------------------------
        await requestManualHandling(ctx, ct);

        // ----------------------------------------------------------------
        // ordem 11 — gateway 'Manually Fixed' _qIDull6BEfGBBLgT-R5iuw · entrouPor=fluxo
        //             Ramo 'Yes' quando OUTCOME == 'OK'; ramo 'No' (OTHERWISE) → Try Again
        // ----------------------------------------------------------------
        if (string.Equals(ctx.OUTCOME, "OK", StringComparison.Ordinal))
        {
            return BscenvpcSeg002Result.CleanEnd(NodeId._qIDu316BEfGBBLgT_R5iuw);
        }

        // ----------------------------------------------------------------
        // ordem 12 — gateway 'Try Again' _qIDum16BEfGBBLgT-R5iuw · entrouPor=fluxo
        //             Ramo 'Yes' quando OUTCOME == 'R'; ramo 'No' (OTHERWISE) → Done - Bail
        // ----------------------------------------------------------------
        if (string.Equals(ctx.OUTCOME, "R", StringComparison.Ordinal))
        {
            ctx.NUMAPPRETRIES = 0;
            return BscenvpcSeg002Result.Retry(0, ctx.MAXRETRIES);
        }

        // ----------------------------------------------------------------
        // ordem 13 — endEvent 'Done - Bail' _qIDumV6BEfGBBLgT-R5iuw · entrouPor=fluxo
        //             Alcancado quando retentativas se esgotam e operador nao volta a tentar.
        // ----------------------------------------------------------------
        return BscenvpcSeg002Result.Bail(NodeId._qIDumV6BEfGBBLgT_R5iuw, reason: null);
    }
}

/// <summary>Resultado do troco BSCENVPC-seg002.</summary>
public sealed class BscenvpcSeg002Result
{
    public BscenvpcSeg002Outcome Outcome { get; }
    public string TerminalNodeId { get; }
    public int? RetriesUsed { get; }
    public int? RetriesMax { get; }
    public string? FailureReason { get; }

    private BscenvpcSeg002Result(
        BscenvpcSeg002Outcome outcome,
        string terminalNodeId,
        int? retriesUsed = null,
        int? retriesMax = null,
        string? failureReason = null)
    {
        Outcome = outcome;
        TerminalNodeId = terminalNodeId;
        RetriesUsed = retriesUsed;
        RetriesMax = retriesMax;
        FailureReason = failureReason;
    }

    /// <summary>Processo termina normalmente no endEvent _qIDu316BEfGBBLgT-R5iuw.</summary>
    public static BscenvpcSeg002Result CleanEnd(string nodeId) =>
        new(BscenvpcSeg002Outcome.CleanEnd, nodeId);

    /// <summary>Retentativa necessaria; o chamador deve re-executar o troco.</summary>
    public static BscenvpcSeg002Result Retry(int retriesUsed, int retriesMax) =>
        new(BscenvpcSeg002Outcome.Retry, NodeId._qIDuoF6BEfGBBLgT_R5iuw,
            retriesUsed, retriesMax);

    /// <summary>Bail-out apos exaustao de retentativas ou decisao de nao tentar novamente.</summary>
    public static BscenvpcSeg002Result Bail(string nodeId, string? reason) =>
        new(BscenvpcSeg002Outcome.Bail, nodeId, failureReason: reason);
}

/// <summary>Tipo de resultado do troco BSCENVPC-seg002.</summary>
public enum BscenvpcSeg002Outcome
{
    /// <summary>Fim limpo da jornada (endEvent _qIDu316BEfGBBLgT-R5iuw).</summary>
    CleanEnd,
    /// <summary>Retentativa: o chamador deve re-invocar o troco.</summary>
    Retry,
    /// <summary>Bail-out: endEvent 'Done - Bail' (_qIDumV6BEfGBBLgT-R5iuw).</summary>
    Bail,
}

/// <summary>
/// Identificadores dos nos TIBCO preservados sem renomeacao (invariante do card).
/// </summary>
internal static class NodeId
{
    internal const string _qIDu5F6BEfGBBLgT_R5iuw = "_qIDu5F6BEfGBBLgT-R5iuw";
    internal const string _qIDu4l6BEfGBBLgT_R5iuw = "_qIDu4l6BEfGBBLgT-R5iuw";
    internal const string _qIDu4V6BEfGBBLgT_R5iuw = "_qIDu4V6BEfGBBLgT-R5iuw";
    internal const string _qIDu416BEfGBBLgT_R5iuw = "_qIDu416BEfGBBLgT-R5iuw";
    internal const string _qIDu316BEfGBBLgT_R5iuw = "_qIDu316BEfGBBLgT-R5iuw";
    internal const string _qIDupF6BEfGBBLgT_R5iuw = "_qIDupF6BEfGBBLgT-R5iuw";
    internal const string _qIDuo16BEfGBBLgT_R5iuw = "_qIDuo16BEfGBBLgT-R5iuw";
    internal const string _qIDuoF6BEfGBBLgT_R5iuw = "_qIDuoF6BEfGBBLgT-R5iuw";
    internal const string _qIDupl6BEfGBBLgT_R5iuw = "_qIDupl6BEfGBBLgT-R5iuw";
    internal const string _qIDunF6BEfGBBLgT_R5iuw = "_qIDunF6BEfGBBLgT-R5iuw";
    internal const string _qIDull6BEfGBBLgT_R5iuw = "_qIDull6BEfGBBLgT-R5iuw";
    internal const string _qIDum16BEfGBBLgT_R5iuw = "_qIDum16BEfGBBLgT-R5iuw";
    internal const string _qIDumV6BEfGBBLgT_R5iuw = "_qIDumV6BEfGBBLgT-R5iuw";
}
