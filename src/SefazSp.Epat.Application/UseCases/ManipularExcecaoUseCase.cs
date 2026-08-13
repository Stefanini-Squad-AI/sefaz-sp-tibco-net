#nullable enable

namespace SefazSp.Epat.Application.UseCases;

using System;
using System.Threading;
using System.Threading.Tasks;
using SefazSp.Epat.Application.Execution;

public sealed class ManipularExcecaoUseCase
{
    public Task<ManipularExcecaoResult> ExecuteAsync(
        ProcessExecutionContext context,
        ManipularExcecaoRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(request);

        ct.ThrowIfCancellationRequested();

        var normalizedOutcome = NormalizeOutcome(request.Outcome);
        context.OUTCOME = normalizedOutcome;

        return Task.FromResult(new ManipularExcecaoResult(
            normalizedOutcome,
            string.Equals(normalizedOutcome, ManipularExcecaoResult.ManuallyFixedOutcome, StringComparison.Ordinal),
            string.Equals(normalizedOutcome, ManipularExcecaoResult.TryAgainOutcome, StringComparison.Ordinal)));
    }

    private static string? NormalizeOutcome(string? outcome)
    {
        if (string.IsNullOrWhiteSpace(outcome))
        {
            return null;
        }

        return outcome.Trim().ToUpperInvariant();
    }
}

public sealed record ManipularExcecaoRequest(string? Outcome);

public sealed record ManipularExcecaoResult(string? Outcome, bool ManuallyFixed, bool TryAgain)
{
    public const string ManuallyFixedOutcome = "OK";
    public const string TryAgainOutcome = "R";
}
