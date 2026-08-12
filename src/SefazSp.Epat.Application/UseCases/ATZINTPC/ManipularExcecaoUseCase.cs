using SefazSp.Epat.Application.Execution.ATZINTPC;

namespace SefazSp.Epat.Application.UseCases.ATZINTPC;

public sealed class ManipularExcecaoUseCase
{
    public Task ExecuteAsync(ATZINTPCWorkflowState state, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(state);
        ct.ThrowIfCancellationRequested();

        if (!string.IsNullOrWhiteSpace(state.ManualExceptionOutcome))
        {
            state.ExecutionContext.OUTCOME = state.ManualExceptionOutcome;
        }

        return Task.CompletedTask;
    }
}
