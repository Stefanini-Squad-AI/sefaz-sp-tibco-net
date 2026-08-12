using SefazSp.Epat.Application.Execution.ATZINTPC;
using SefazSp.Epat.Application.UseCases.ATZINTPC;
using SefazSp.Epat.Domain.Rules.ATZINTPC;

namespace SefazSp.Epat.Application.Workflows.ATZINTPC;

public sealed class ATZINTPCWorkflow
{
    private const string StartEvent = "_RNdJyV6PEfGBBLgT-R5iuw";
    private const string SetParameters = "_RNdJyl6PEfGBBLgT-R5iuw";
    private const string StartLoop = "_RNdJzF6PEfGBBLgT-R5iuw";
    private const string ControlSystemTaskCall = "_RNdJ2l6PEfGBBLgT-R5iuw";
    private const string ActivitySetStartEvent = "_RNdKFl6PEfGBBLgT-R5iuw";
    private const string StartTx = "_RNdKFF6PEfGBBLgT-R5iuw";
    private const string CheckRetriesSwQRetryCount = "_RNdKFV6PEfGBBLgT-R5iuw";
    private const string AtualizarIntimacao = "_RNdKHF6PEfGBBLgT-R5iuw";
    private const string ActivityGateway = "_RNdKGl6PEfGBBLgT-R5iuw";
    private const string SetAppError = "_RNdKGV6PEfGBBLgT-R5iuw";
    private const string ActivityConvergence = "_RNdKG16PEfGBBLgT-R5iuw";
    private const string ActivitySetEndEvent = "_RNdKF16PEfGBBLgT-R5iuw";
    private const string TechError = "_RNdJ2V6PEfGBBLgT-R5iuw";
    private const string AppError = "_RNdJ2F6PEfGBBLgT-R5iuw";
    private const string MoreRetries = "_RNdJ1V6PEfGBBLgT-R5iuw";
    private const string MainConvergence = "_RNdJ216PEfGBBLgT-R5iuw";
    private const string ManipularExcecao = "_RNdJ0V6PEfGBBLgT-R5iuw";
    private const string ManuallyFixed = "_RNdJy16PEfGBBLgT-R5iuw";
    private const string TryAgain = "_RNdJ0F6PEfGBBLgT-R5iuw";
    private const string DoneBail = "_RNdJzl6PEfGBBLgT-R5iuw";

    private readonly IAtualizarIntimacaoOperation _atualizarIntimacaoOperation;
    private readonly ManipularExcecaoUseCase _manipularExcecaoUseCase;

    public ATZINTPCWorkflow(
        IAtualizarIntimacaoOperation atualizarIntimacaoOperation,
        ManipularExcecaoUseCase manipularExcecaoUseCase)
    {
        _atualizarIntimacaoOperation = atualizarIntimacaoOperation ?? throw new ArgumentNullException(nameof(atualizarIntimacaoOperation));
        _manipularExcecaoUseCase = manipularExcecaoUseCase ?? throw new ArgumentNullException(nameof(manipularExcecaoUseCase));
    }

    public async Task<IReadOnlyList<string>> ExecuteAsync(ATZINTPCWorkflowState state, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.VisitedNodeIds.Count != 0)
        {
            throw new InvalidOperationException("The workflow state must start with an empty trace.");
        }

        state.Visit(StartEvent);
        state.Visit(SetParameters);
        ATZINTPCEnvelopeScripts.ApplySetParameters(state.CaseData, state.ExecutionContext);

        await ExecuteFromStartLoopAsync(state, ct).ConfigureAwait(false);
        return state.VisitedNodeIds;
    }

    private async Task ExecuteFromStartLoopAsync(ATZINTPCWorkflowState state, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        state.Visit(StartLoop);
        ATZINTPCEnvelopeScripts.StartLoop(state);

        state.Visit(ControlSystemTaskCall);
        await ExecuteControlSystemTaskCallAsync(state, ct).ConfigureAwait(false);

        state.Visit(TechError);
        if (Is("Y", state.ExecutionContext.ISTECHERROR))
        {
            state.Visit(MainConvergence);
            await ExecuteManualHandlingAsync(state, ct).ConfigureAwait(false);
            return;
        }

        state.Visit(AppError);
        if (!Is("Y", state.ExecutionContext.ISAPPERROR))
        {
            return;
        }

        state.Visit(MoreRetries);
        if (state.ExecutionContext.NUMAPPRETRIES < state.ExecutionContext.MAXRETRIES)
        {
            await ExecuteFromStartLoopAsync(state, ct).ConfigureAwait(false);
            return;
        }

        state.Visit(MainConvergence);
        await ExecuteManualHandlingAsync(state, ct).ConfigureAwait(false);
    }

    private async Task ExecuteControlSystemTaskCallAsync(ATZINTPCWorkflowState state, CancellationToken ct)
    {
        state.Visit(ActivitySetStartEvent);
        state.Visit(StartTx);
        state.Visit(CheckRetriesSwQRetryCount);

        if (!CheckRetriesSWQRETRYCOUNTRule.IsStillGood(state.SW_QRETRYCOUNT, state.ExecutionContext.MAXRETRIES))
        {
            ATZINTPCEnvelopeScripts.SetTechnicalError(state.ExecutionContext);
            state.Visit(ActivitySetEndEvent);
            return;
        }

        state.ExecutionContext.SERVICE_NAME = "AtualizarIntimacao";

        state.Visit(AtualizarIntimacao);
        var envelope = await _atualizarIntimacaoOperation
            .ExecuteAsync(state.CaseData, state.ExecutionContext, ct)
            .ConfigureAwait(false);

        ATZINTPCEnvelopeScripts.ApplyEnvelope(envelope, state.ExecutionContext);

        state.Visit(ActivityGateway);
        if (!Is("0", state.ExecutionContext.STATUS_CODE))
        {
            state.Visit(SetAppError);
            ATZINTPCEnvelopeScripts.SetApplicationError(state.ExecutionContext);
        }

        state.Visit(ActivityConvergence);
        state.Visit(ActivitySetEndEvent);
    }

    private async Task ExecuteManualHandlingAsync(ATZINTPCWorkflowState state, CancellationToken ct)
    {
        state.Visit(ManipularExcecao);
        await _manipularExcecaoUseCase.ExecuteAsync(state, ct).ConfigureAwait(false);

        state.Visit(ManuallyFixed);
        if (Is("OK", state.ExecutionContext.OUTCOME))
        {
            return;
        }

        state.Visit(TryAgain);
        if (Is("R", state.ExecutionContext.OUTCOME))
        {
            await ExecuteFromStartLoopAsync(state, ct).ConfigureAwait(false);
            return;
        }

        state.Visit(DoneBail);
    }

    private static bool Is(string expected, string? actual)
        => string.Equals(expected, actual, StringComparison.Ordinal);
}
