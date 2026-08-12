using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Rules.ATZINTPC;

namespace SefazSp.Epat.Application.Execution.ATZINTPC;

public static class ATZINTPCEnvelopeScripts
{
    public static void ApplySetParameters(AiimCase caseData, ProcessExecutionContext executionContext)
    {
        ArgumentNullException.ThrowIfNull(caseData);
        ArgumentNullException.ThrowIfNull(executionContext);

        var maxRetries = executionContext.MAXRETRIES;
        SetParametersRule.Apply(ref maxRetries);
        executionContext.MAXRETRIES = maxRetries;

        executionContext.PROCESS_ID = caseData.IDPROCESSO.Match(
            idProcesso => FormattableString.Invariant($"idAiim-{caseData.IDAIIM}idProc-{idProcesso}"),
            () => FormattableString.Invariant($"idAiim-{caseData.IDAIIM}idProc-NA"),
            () => FormattableString.Invariant($"idAiim-{caseData.IDAIIM}idProc-NA"));
    }

    public static void StartLoop(ATZINTPCWorkflowState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var executionContext = state.ExecutionContext;
        if (state.HasEnteredLoop)
        {
            executionContext.NUMAPPRETRIES++;
        }
        else
        {
            state.HasEnteredLoop = true;
        }

        if (executionContext.NUMAPPRETRIES < 0)
        {
            executionContext.NUMAPPRETRIES = 0;
        }

        executionContext.ISAPPERROR = "N";
        executionContext.ISTECHERROR = "N";
        executionContext.OUTCOME = "R";
        executionContext.DATETIME ??= state.CurrentDateTimeText;
    }

    public static void ApplyEnvelope(ServiceEnvelope envelope, ProcessExecutionContext executionContext)
    {
        ArgumentNullException.ThrowIfNull(executionContext);

        executionContext.STATUS_CODE = envelope.STATUS_CODE;
        executionContext.STERRORCODE = envelope.STERRORCODE;
        executionContext.STERRORDESC = envelope.STERRORDESC;
    }

    public static void SetTechnicalError(ProcessExecutionContext executionContext)
    {
        ArgumentNullException.ThrowIfNull(executionContext);

        executionContext.ISTECHERROR = "Y";
        executionContext.OUTCOME = "R";
    }

    public static void SetApplicationError(ProcessExecutionContext executionContext)
    {
        ArgumentNullException.ThrowIfNull(executionContext);

        executionContext.ISAPPERROR = "Y";
        executionContext.OUTCOME = "R";
    }
}
