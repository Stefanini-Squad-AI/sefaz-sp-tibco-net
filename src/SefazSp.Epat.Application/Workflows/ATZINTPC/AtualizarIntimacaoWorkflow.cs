#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Rules.ATZINTPC;

namespace SefazSp.Epat.Application.Workflows.ATZINTPC;

public sealed class AtualizarIntimacaoWorkflow
{
    private const string StartEvent = "_RNdJyV6PEfGBBLgT-R5iuw";
    private const string SetParameters = "_RNdJyl6PEfGBBLgT-R5iuw";
    private const string StartLoop = "_RNdJzF6PEfGBBLgT-R5iuw";
    private const string ControlSystemTaskCall = "_RNdJ2l6PEfGBBLgT-R5iuw";
    private const string InnerStartEvent = "_RNdKFl6PEfGBBLgT-R5iuw";
    private const string StartTx = "_RNdKFF6PEfGBBLgT-R5iuw";
    private const string CheckRetries = "_RNdKFV6PEfGBBLgT-R5iuw";
    private const string AtualizarIntimacao = "_RNdKHF6PEfGBBLgT-R5iuw";
    private const string ServiceGateway = "_RNdKGl6PEfGBBLgT-R5iuw";
    private const string SetAppError = "_RNdKGV6PEfGBBLgT-R5iuw";
    private const string InnerMergeGateway = "_RNdKG16PEfGBBLgT-R5iuw";
    private const string InnerEndEvent = "_RNdKF16PEfGBBLgT-R5iuw";
    private const string TechError = "_RNdJ2V6PEfGBBLgT-R5iuw";
    private const string AppError = "_RNdJ2F6PEfGBBLgT-R5iuw";
    private const string MoreRetries = "_RNdJ1V6PEfGBBLgT-R5iuw";
    private const string Pause = "_RNdJ1l6PEfGBBLgT-R5iuw";
    private const string LinkToTryTask = "_RNdJ1F6PEfGBBLgT-R5iuw";
    private const string TryTask = "_RNdJzV6PEfGBBLgT-R5iuw";

    private readonly IEpatServices _services;
    private readonly IClock _clock;

    public AtualizarIntimacaoWorkflow(IEpatServices services, IClock clock)
    {
        _services = services;
        _clock = clock;
    }

    public async Task<WorkflowTrace> ExecuteAsync(
        ProcessExecutionContext ctx,
        AiimCase aiimCase,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(aiimCase);

        var nodeIds = new List<string>(18);

        nodeIds.Add(StartEvent);

        nodeIds.Add(SetParameters);
        SetParametersRule.Apply(ctx);

        nodeIds.Add(StartLoop);
        StartLoopStep(ctx, _clock);

        nodeIds.Add(ControlSystemTaskCall);

        // explicit descida edge
        nodeIds.Add(InnerStartEvent);

        nodeIds.Add(StartTx);
        StartTxStep(ctx, aiimCase);

        nodeIds.Add(CheckRetries);
        if (!CheckRetriesSwQretrycountRule.IsStillGood(aiimCase.SW_QRETRYCOUNT, ctx.MAXRETRIES))
        {
            return new WorkflowTrace(nodeIds);
        }

        nodeIds.Add(AtualizarIntimacao);
        var envelope = await _services.AtualizarintimacaoAsync(CreateCaseRef(ctx, aiimCase), cancellationToken).ConfigureAwait(false);
        ApplyEnvelope(ctx, envelope);

        nodeIds.Add(ServiceGateway);
        if (string.Equals(ctx.STATUS_CODE, "0", StringComparison.Ordinal))
        {
            nodeIds.Add(InnerEndEvent);
            // explicit regresso edge
            nodeIds.Add(TechError);
            return new WorkflowTrace(nodeIds);
        }

        nodeIds.Add(SetAppError);
        SetAppErrorStep(ctx);

        nodeIds.Add(InnerMergeGateway);
        nodeIds.Add(InnerEndEvent);

        // explicit regresso edge
        nodeIds.Add(TechError);
        if (string.Equals(ctx.ISTECHERROR, "Y", StringComparison.Ordinal))
        {
            return new WorkflowTrace(nodeIds);
        }

        nodeIds.Add(AppError);
        if (!string.Equals(ctx.ISAPPERROR, "Y", StringComparison.Ordinal))
        {
            return new WorkflowTrace(nodeIds);
        }

        nodeIds.Add(MoreRetries);
        if (ctx.NUMAPPRETRIES >= ctx.MAXRETRIES)
        {
            return new WorkflowTrace(nodeIds);
        }

        nodeIds.Add(Pause);
        PauseStep(_clock);

        nodeIds.Add(LinkToTryTask);

        // explicit link edge
        nodeIds.Add(TryTask);
        return new WorkflowTrace(nodeIds);
    }

    private static void StartLoopStep(ProcessExecutionContext ctx, IClock clock)
    {
        ctx.ISAPPERROR = "N";
        ctx.ISTECHERROR = "N";
        ctx.OUTCOME = "R";
        ctx.DATETIME ??= clock.Now.ToString("O");
    }

    private static void StartTxStep(ProcessExecutionContext ctx, AiimCase aiimCase)
    {
        _ = aiimCase;
        ctx.STATUS_CODE = null;
        ctx.STERRORCODE = null;
        ctx.STERRORDESC = null;
    }

    private static void SetAppErrorStep(ProcessExecutionContext ctx)
    {
        ctx.ISAPPERROR = "Y";
        ctx.OUTCOME = "R";
    }

    private static void PauseStep(IClock clock)
    {
        _ = clock.Now;
    }

    private static AiimCaseRef CreateCaseRef(ProcessExecutionContext ctx, AiimCase aiimCase) =>
        new(aiimCase.IdAiim, ctx.PROCESS_ID ?? string.Empty);

    private static void ApplyEnvelope(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
    }
}
