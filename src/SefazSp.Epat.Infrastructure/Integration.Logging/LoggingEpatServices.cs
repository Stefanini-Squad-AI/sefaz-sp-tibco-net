#nullable enable

using System.Text.Json;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Domain.Abstractions;

namespace SefazSp.Epat.Infrastructure.Integration.Logging;

/// <summary>
/// Decorator over <see cref="IEpatServices"/> that records each of the 5 WSDL operation calls to the
/// <see cref="IServiceInteractionLog"/>. The inner implementation (double today, SOAP client later)
/// is untouched. Logging is best-effort — an audit failure never breaks the business call.
/// </summary>
public sealed class LoggingEpatServices(IEpatServices inner, IServiceInteractionLog log, IClock clock) : IEpatServices
{
    public Task<ServiceEnvelope> PrepararintimacaoAsync(AiimCaseRef c, CancellationToken ct)
        => Record("PrepararIntimacao", c, () => inner.PrepararintimacaoAsync(c, ct), ct);

    public Task<ServiceEnvelope> AtualizarintimacaoAsync(AiimCaseRef c, CancellationToken ct)
        => Record("AtualizarIntimacao", c, () => inner.AtualizarintimacaoAsync(c, ct), ct);

    public Task<ServiceEnvelope> BuscarvistasativasporaiimAsync(AiimCaseRef c, CancellationToken ct)
        => Record("BuscarVistasAtivasPorAiim", c, () => inner.BuscarvistasativasporaiimAsync(c, ct), ct);

    public Task<ServiceEnvelope> CriarnotificacoesaiimAsync(AiimCaseRef c, CancellationToken ct)
        => Record("CriarNotificacoesAiim", c, () => inner.CriarnotificacoesaiimAsync(c, ct), ct);

    public Task<ServiceEnvelope> ObterprimeirodiautilaposperiododediascorridosdeatAsync(AiimCaseRef c, CancellationToken ct)
        => Record("ObterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEAT", c,
            () => inner.ObterprimeirodiautilaposperiododediascorridosdeatAsync(c, ct), ct);

    private async Task<ServiceEnvelope> Record(
        string op, AiimCaseRef c, Func<Task<ServiceEnvelope>> call, CancellationToken ct)
    {
        var t0 = clock.Now;
        ServiceEnvelope resp = default;
        string? failure = null;
        try
        {
            resp = await call();
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
                    c.ProcessId, "IEpatServices", op,
                    JsonSerializer.Serialize(c),
                    failure is null ? JsonSerializer.Serialize(resp) : "null",
                    Success: failure is null && resp.STATUS_CODE == "0",
                    failure, t0, (long)(clock.Now - t0).TotalMilliseconds), ct);
            }
            catch { /* best-effort audit; never break the call */ }
        }
    }
}
