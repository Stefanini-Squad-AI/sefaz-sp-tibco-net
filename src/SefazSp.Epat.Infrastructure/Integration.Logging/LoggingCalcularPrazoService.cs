#nullable enable

using System.Text.Json;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Workflows.CALCPRPC;
using SefazSp.Epat.Domain.Abstractions;

namespace SefazSp.Epat.Infrastructure.Integration.Logging;

/// <summary>
/// Decorator over <see cref="ICalcularPrazoSoapService"/> that records the CalcularPrazo call to the
/// <see cref="IServiceInteractionLog"/>. Best-effort — an audit failure never breaks the call.
/// </summary>
public sealed class LoggingCalcularPrazoService(
    ICalcularPrazoSoapService inner, IServiceInteractionLog log, IClock clock) : ICalcularPrazoSoapService
{
    public async Task<ServiceEnvelope> InvokeAsync(AiimCaseRef c, CancellationToken ct)
    {
        var t0 = clock.Now;
        ServiceEnvelope resp = default;
        string? failure = null;
        try
        {
            resp = await inner.InvokeAsync(c, ct);
            return resp;
        }
        catch (Exception ex)
        {
            failure = ex.Message;
            throw;
        }
        finally
        {
            try
            {
                await log.RecordAsync(new ServiceInteraction(
                    c.ProcessId, "CalcularPrazo", "InvokeAsync",
                    JsonSerializer.Serialize(c),
                    failure is null ? JsonSerializer.Serialize(resp) : "null",
                    Success: failure is null && resp.STATUS_CODE == "0",
                    failure, t0, (long)(clock.Now - t0).TotalMilliseconds), ct);
            }
            catch { /* best-effort audit */ }
        }
    }
}
