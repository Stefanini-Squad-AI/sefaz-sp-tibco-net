#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Abstractions;

namespace SefazSp.Epat.Application.Workflows;

public sealed class BscenvpcSegment005Workflow
{
    public const string BuscaEnvolvidosVistaPorAiimNodeId = "_qIDu5F6BEfGBBLgT-R5iuw";
    public const string ServiceResultGatewayNodeId = "_qIDu4l6BEfGBBLgT-R5iuw";
    public const string SetAppErrorNodeId = "_qIDu4V6BEfGBBLgT-R5iuw";
    public const string ActivitySetExitGatewayNodeId = "_qIDu416BEfGBBLgT-R5iuw";
    public const string ActivitySetEndEventNodeId = "_qIDu316BEfGBBLgT-R5iuw";
    public const string TechErrorNodeId = "_qIDupF6BEfGBBLgT-R5iuw";
    public const string AppErrorNodeId = "_qIDuo16BEfGBBLgT-R5iuw";
    public const string MoreRetriesNodeId = "_qIDuoF6BEfGBBLgT-R5iuw";
    public const string PauseNodeId = "_qIDuoV6BEfGBBLgT-R5iuw";
    public const string LinkToTryTaskNodeId = "_qIDun16BEfGBBLgT-R5iuw";
    public const string TryTaskNodeId = "_qIDumF6BEfGBBLgT-R5iuw";

    private readonly IEpatServices _services;
    private readonly IClock _clock;

    public BscenvpcSegment005Workflow(IEpatServices services, IClock clock)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<BscenvpcSegment005Result> ExecuteAsync(
        ProcessExecutionContext context,
        AiimCaseRef caseRef,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var visitedNodeIds = new List<string>(capacity: 11);

        visitedNodeIds.Add(BuscaEnvolvidosVistaPorAiimNodeId);
        var envelope = await _services.BuscarvistasativasporaiimAsync(caseRef, cancellationToken).ConfigureAwait(false);
        ApplyEnvelope(context, envelope);

        visitedNodeIds.Add(ServiceResultGatewayNodeId);
        if (HasApplicationFailure(context))
        {
            visitedNodeIds.Add(SetAppErrorNodeId);
            SetAppErrorScript.Execute(context);
        }

        visitedNodeIds.Add(ActivitySetExitGatewayNodeId);
        visitedNodeIds.Add(ActivitySetEndEventNodeId);
        visitedNodeIds.Add(TechErrorNodeId);

        if (HasTechnicalFailure(context))
        {
            return new BscenvpcSegment005Result(visitedNodeIds, "ManualException");
        }

        visitedNodeIds.Add(AppErrorNodeId);
        if (!HasApplicationErrorFlag(context))
        {
            return new BscenvpcSegment005Result(visitedNodeIds, "DoneSuccess");
        }

        visitedNodeIds.Add(MoreRetriesNodeId);
        if (!HasMoreApplicationRetries(context))
        {
            return new BscenvpcSegment005Result(visitedNodeIds, "ManualException");
        }

        visitedNodeIds.Add(PauseNodeId);
        _ = _clock.Now;

        visitedNodeIds.Add(LinkToTryTaskNodeId);
        visitedNodeIds.Add(TryTaskNodeId);
        return new BscenvpcSegment005Result(visitedNodeIds, "TryTask");
    }

    private static void ApplyEnvelope(ProcessExecutionContext context, ServiceEnvelope envelope)
    {
        context.STATUS_CODE = envelope.STATUS_CODE;
        context.STERRORCODE = envelope.STERRORCODE;
        context.STERRORDESC = envelope.STERRORDESC;
    }

    private static bool HasApplicationFailure(ProcessExecutionContext context) =>
        !string.Equals(context.STATUS_CODE, "0", StringComparison.Ordinal);

    private static bool HasTechnicalFailure(ProcessExecutionContext context) =>
        string.Equals(context.ISTECHERROR, "Y", StringComparison.Ordinal);

    private static bool HasApplicationErrorFlag(ProcessExecutionContext context) =>
        string.Equals(context.ISAPPERROR, "Y", StringComparison.Ordinal);

    private static bool HasMoreApplicationRetries(ProcessExecutionContext context) =>
        context.NUMAPPRETRIES < context.MAXRETRIES;
}

public sealed record BscenvpcSegment005Result(IReadOnlyList<string> VisitedNodeIds, string Exit);
