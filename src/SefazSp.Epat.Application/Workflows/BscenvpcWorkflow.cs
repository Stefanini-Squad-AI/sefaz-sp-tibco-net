#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.UseCases;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows;

public sealed class BscenvpcWorkflow
{
    private const string SegmentId = "BUILD-BSCENVPC-seg048";
    private const string StartEventId = "_qIDulF6BEfGBBLgT-R5iuw";
    private const string SetParametersId = "_qIDulV6BEfGBBLgT-R5iuw";
    private const string StartLoopId = "_qIDul16BEfGBBLgT-R5iuw";
    private const string ControlSystemTaskCallId = "_qIDupV6BEfGBBLgT-R5iuw";
    private const string InnerStartEventId = "_qIDu3l6BEfGBBLgT-R5iuw";
    private const string StartTxId = "_qIDu3F6BEfGBBLgT-R5iuw";
    private const string CheckRetriesId = "_qIDu3V6BEfGBBLgT-R5iuw";
    private const string SetTechnicalErrorId = "_qIDu4F6BEfGBBLgT-R5iuw";
    private const string InnerEndEventId = "_qIDu316BEfGBBLgT-R5iuw";
    private const string TechErrorGatewayId = "_qIDupF6BEfGBBLgT-R5iuw";
    private const string AppErrorGatewayId = "_qIDuo16BEfGBBLgT-R5iuw";
    private const string MoreRetriesGatewayId = "_qIDuoF6BEfGBBLgT-R5iuw";
    private const string ConvergenceGatewayId = "_qIDupl6BEfGBBLgT-R5iuw";
    private const string ManipularExcecaoId = "_qIDunF6BEfGBBLgT-R5iuw";
    private const string ManuallyFixedGatewayId = "_qIDull6BEfGBBLgT-R5iuw";
    private const string DoneFixedEndEventId = "_qIDuml6BEfGBBLgT-R5iuw";
    private const string DescidaTransition = "descida";
    private const string RegressoTransition = "regresso";
    private const string Yes = "Y";

    private readonly IClock _clock;
    private readonly ManipularExcecaoUseCase _manipularExcecaoUseCase;

    public BscenvpcWorkflow(IClock clock, ManipularExcecaoUseCase manipularExcecaoUseCase)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _manipularExcecaoUseCase = manipularExcecaoUseCase ?? throw new ArgumentNullException(nameof(manipularExcecaoUseCase));
    }

    public async Task<BscenvpcWorkflowResult> ExecuteAsync(
        AiimCase aiimCase,
        ProcessExecutionContext executionContext,
        BscenvpcWorkflowRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(aiimCase);
        ArgumentNullException.ThrowIfNull(executionContext);
        ArgumentNullException.ThrowIfNull(request);

        ct.ThrowIfCancellationRequested();

        var visitedNodeIds = new List<string> { StartEventId, SetParametersId };
        var setParameters = ExecuteSetParameters(aiimCase, executionContext);

        visitedNodeIds.Add(StartLoopId);
        _ = BscenvpcExecutionSteps.StartLoop(executionContext, _clock);

        visitedNodeIds.Add(ControlSystemTaskCallId);
        var controlSystemTask = ExecuteControlSystemTaskCall(aiimCase, executionContext, visitedNodeIds);

        visitedNodeIds.Add(TechErrorGatewayId);
        if (string.Equals(executionContext.ISTECHERROR, Yes, StringComparison.Ordinal))
        {
            return await ExecuteManualExceptionPathAsync(
                executionContext,
                request,
                visitedNodeIds,
                ManualExceptionReason.TechnicalError,
                setParameters,
                controlSystemTask,
                ct).ConfigureAwait(false);
        }

        visitedNodeIds.Add(AppErrorGatewayId);
        if (string.Equals(executionContext.ISAPPERROR, Yes, StringComparison.Ordinal))
        {
            visitedNodeIds.Add(MoreRetriesGatewayId);
            if (executionContext.NUMAPPRETRIES < executionContext.MAXRETRIES)
            {
                return new BscenvpcWorkflowResult(
                    SegmentId,
                    null,
                    BscenvpcWorkflowDisposition.RetryRequested,
                    StartLoopId,
                    ManualExceptionReason.ApplicationErrorRetryAvailable,
                    setParameters,
                    controlSystemTask,
                    visitedNodeIds.AsReadOnly());
            }

            return await ExecuteManualExceptionPathAsync(
                executionContext,
                request,
                visitedNodeIds,
                ManualExceptionReason.ApplicationErrorMaxRetriesExceeded,
                setParameters,
                controlSystemTask,
                ct).ConfigureAwait(false);
        }

        return await ExecuteManualExceptionPathAsync(
            executionContext,
            request,
            visitedNodeIds,
            ManualExceptionReason.NoErrorBranchJoined,
            setParameters,
            controlSystemTask,
            ct).ConfigureAwait(false);
    }

    private static SetParametersStepResult ExecuteSetParameters(AiimCase aiimCase, ProcessExecutionContext executionContext)
    {
        int? currentMaxRetries = executionContext.MAXRETRIES > 0 ? executionContext.MAXRETRIES : null;
        var currentIdProcesso = aiimCase.IDPROCESSO.Match(
            hasValue: value => value.ToString(CultureInfo.InvariantCulture),
            notAvailable: static () => (string?)null,
            empty: static () => (string?)null);

        var (effectiveMaxRetries, isErrorBranch) = BscenvpcSetParametersRule.Apply(currentMaxRetries, currentIdProcesso);
        executionContext.MAXRETRIES = effectiveMaxRetries;

        aiimCase.IDPROCESSO = FieldValue<int>.Of(0);
        executionContext.PROCESS_ID = BuildProcessId(aiimCase.IDAIIM, 0);

        return new(
            effectiveMaxRetries,
            currentIdProcesso,
            0,
            isErrorBranch,
            executionContext.PROCESS_ID);
    }

    private static ControlSystemTaskCallResult ExecuteControlSystemTaskCall(
        AiimCase aiimCase,
        ProcessExecutionContext executionContext,
        ICollection<string> visitedNodeIds)
    {
        ArgumentNullException.ThrowIfNull(aiimCase);
        ArgumentNullException.ThrowIfNull(executionContext);
        ArgumentNullException.ThrowIfNull(visitedNodeIds);

        visitedNodeIds.Add($"{DescidaTransition}:{InnerStartEventId}");
        visitedNodeIds.Add(InnerStartEventId);
        visitedNodeIds.Add(StartTxId);
        _ = BscenvpcExecutionSteps.StartTx(executionContext);

        visitedNodeIds.Add(CheckRetriesId);
        var maxRetriesExceeded = aiimCase.SW_QRETRYCOUNT >= executionContext.MAXRETRIES;
        if (maxRetriesExceeded)
        {
            visitedNodeIds.Add(SetTechnicalErrorId);
            _ = BscenvpcExecutionSteps.SetTechnicalError(executionContext);
        }

        visitedNodeIds.Add(InnerEndEventId);
        visitedNodeIds.Add($"{RegressoTransition}:{TechErrorGatewayId}");

        return new(
            DescidaTransition,
            RegressoTransition,
            aiimCase.SW_QRETRYCOUNT,
            executionContext.MAXRETRIES,
            maxRetriesExceeded,
            executionContext.ISTECHERROR);
    }

    private async Task<BscenvpcWorkflowResult> ExecuteManualExceptionPathAsync(
        ProcessExecutionContext executionContext,
        BscenvpcWorkflowRequest request,
        IList<string> visitedNodeIds,
        ManualExceptionReason reason,
        SetParametersStepResult setParameters,
        ControlSystemTaskCallResult controlSystemTask,
        CancellationToken ct)
    {
        visitedNodeIds.Add(ConvergenceGatewayId);
        visitedNodeIds.Add(ManipularExcecaoId);

        var manualHandling = await _manipularExcecaoUseCase.ExecuteAsync(
            executionContext,
            new ManipularExcecaoRequest(request.OperatorOutcome),
            ct).ConfigureAwait(false);

        visitedNodeIds.Add(ManuallyFixedGatewayId);
        if (manualHandling.ManuallyFixed)
        {
            visitedNodeIds.Add(DoneFixedEndEventId);
            return new BscenvpcWorkflowResult(
                SegmentId,
                DoneFixedEndEventId,
                BscenvpcWorkflowDisposition.DoneFixed,
                null,
                reason,
                setParameters,
                controlSystemTask,
                visitedNodeIds.ToArray(),
                manualHandling);
        }

        return new BscenvpcWorkflowResult(
            SegmentId,
            null,
            BscenvpcWorkflowDisposition.RetryRequested,
            StartLoopId,
            reason,
            setParameters,
            controlSystemTask,
            visitedNodeIds.ToArray(),
            manualHandling);
    }

    private static string BuildProcessId(long idAiim, int idProcesso) =>
        FormattableString.Invariant($"idAiim-{idAiim}idProc-{idProcesso}");
}

public sealed record BscenvpcWorkflowRequest(string? OperatorOutcome);

public sealed record BscenvpcWorkflowResult(
    string SegmentId,
    string? EndEventId,
    BscenvpcWorkflowDisposition Disposition,
    string? RetryTargetNodeId,
    ManualExceptionReason Reason,
    SetParametersStepResult SetParameters,
    ControlSystemTaskCallResult ControlSystemTaskCall,
    IReadOnlyList<string> VisitedNodeIds,
    ManipularExcecaoResult? ManualHandling = null);

public sealed record SetParametersStepResult(
    int MaxRetries,
    string? OriginalIdProcesso,
    int NormalizedIdProcesso,
    bool IsErrorBranch,
    string? ProcessId);

public sealed record ControlSystemTaskCallResult(
    string DescidaTransition,
    string RegressoTransition,
    long QueueRetryCount,
    int MaxRetries,
    bool MaxRetriesExceeded,
    string? IsTechError);

public enum BscenvpcWorkflowDisposition
{
    DoneFixed,
    RetryRequested
}

public enum ManualExceptionReason
{
    TechnicalError,
    ApplicationErrorRetryAvailable,
    ApplicationErrorMaxRetriesExceeded,
    NoErrorBranchJoined
}
