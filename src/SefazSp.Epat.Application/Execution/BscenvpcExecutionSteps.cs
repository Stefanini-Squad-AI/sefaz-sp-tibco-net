#nullable enable

using System;
using System.Globalization;
using SefazSp.Epat.Domain.Abstractions;

namespace SefazSp.Epat.Application.Execution;

public static class BscenvpcExecutionSteps
{
    private const string No = "N";
    private const string Yes = "Y";

    public static StartLoopStepResult StartLoop(ProcessExecutionContext context, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(clock);

        var initialized = string.IsNullOrWhiteSpace(context.DATETIME);
        if (initialized)
        {
            context.NUMAPPRETRIES = 0;
            context.DATETIME = clock.Now.ToString("O", CultureInfo.InvariantCulture);
            context.ISAPPERROR = No;
            context.ISTECHERROR = No;
            context.OUTCOME = null;
        }

        return new(
            initialized,
            context.NUMAPPRETRIES,
            context.DATETIME,
            context.ISAPPERROR,
            context.ISTECHERROR,
            context.OUTCOME);
    }

    public static StartTxStepResult StartTx(ProcessExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.STATUS_CODE = null;
        context.STERRORCODE = null;
        context.STERRORDESC = null;

        return new(context.STATUS_CODE, context.STERRORCODE, context.STERRORDESC);
    }

    public static SetTechnicalErrorStepResult SetTechnicalError(ProcessExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ISTECHERROR = Yes;
        return new(context.ISTECHERROR);
    }
}

public sealed record StartLoopStepResult(
    bool Initialized,
    int NumAppRetries,
    string? DateTime,
    string? IsAppError,
    string? IsTechError,
    string? Outcome);

public sealed record StartTxStepResult(string? StatusCode, string? StErrorCode, string? StErrorDesc);

public sealed record SetTechnicalErrorStepResult(string? IsTechError);
