#nullable enable

using System.Globalization;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.CALCPRPC;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.CALCPRPC;

/// <summary>
/// Topologia do segmento 031 do processo CALCPRPC: de "Start Event" a "Done - Success"
/// (cenário SC-CALCPRPC-010, segmento de ordem 1).
///
/// Trata 15 nós de referência, mais o timer Pause do laço de retry:
///   1  startEvent      _zJIHVVqiEfG5K7mY0I3I6w  Start Event
///   2  scriptTask      _zJIHVlqiEfG5K7mY0I3I6w  SetParameters
///   3  scriptTask      _zJIHWFqiEfG5K7mY0I3I6w  Start Loop
///   4  subProcessScope _zJIHZlqiEfG5K7mY0I3I6w  Control System Task Call
///   5  startEvent      _zJIublqiEfG5K7mY0I3I6w  startEvent interno (descida)
///   6  scriptTask      _zJIuaVqiEfG5K7mY0I3I6w  Start TX
///   7  gateway         _zJIubVqiEfG5K7mY0I3I6w  Check Retries SW_QRETRYCOUNT
///   8  serviceTask     _AsZCkVqkEfG5K7mY0I3I6w  CalcularPrazo
///   9  gateway         _zJIuclqiEfG5K7mY0I3I6w  STATUS_CODE != "0"?
///  10  scriptTask      _zJIucVqiEfG5K7mY0I3I6w  Set App Error
///  11  gateway         _zJIuc1qiEfG5K7mY0I3I6w  More Retries
///  12  endEvent        _zJIub1qiEfG5K7mY0I3I6w  endEvent interno
///  13  gateway         _zJIHZVqiEfG5K7mY0I3I6w  Tech Error (regresso)
///  14  gateway         _zJIHZFqiEfG5K7mY0I3I6w  App Error
///  15  endEvent        _zJIHY1qiEfG5K7mY0I3I6w  Done - Success
///
/// Nó auxiliar do laço de retry:
///      timerEvent      _zJIHYlqiEfG5K7mY0I3I6w  Pause
/// </summary>
public sealed class CalcprpcSeg031Workflow : ICALCPRPC
{
    private readonly IEpatServices _services;
    private readonly IClock _clock;

    public CalcprpcSeg031Workflow(IEpatServices services, IClock clock)
    {
        _services = services;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        var ctx = new ProcessExecutionContext();
        var result = await RunAsync(caseRef, ctx, ct);

        return result == CalcprpcSeg031Result.DoneSuccess
            ? new ProcessCallResult(Started: true, ChildInstanceId: ctx.PROCESS_ID ?? caseRef.ProcessId, Failure: null)
            : new ProcessCallResult(Started: false, ChildInstanceId: null, Failure: result.ToString());
    }

    /// <summary>
    /// Executa o segmento completo, incluindo o laço interno de retry até sucesso,
    /// erro técnico, erro aplicacional ou esgotamento de retentativas.
    /// </summary>
    public async Task<CalcprpcSeg031Result> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        CancellationToken ct)
    {
        // ── Nó 1: startEvent 'Start Event' (_zJIHVVqiEfG5K7mY0I3I6w) ───────────────────────────
        var idProcesso = ParseIdProcesso(caseRef.ProcessId);

        // ── Nó 2: scriptTask 'SetParameters' (_zJIHVlqiEfG5K7mY0I3I6w) ──────────────────────────
        if (CalcprpcSetParametersRule.ShouldInitialize(idProcesso, ctx.MAXRETRIES == 0 ? null : ctx.MAXRETRIES))
            CalcprpcSeg031Steps.ApplySetParameters(ctx, caseRef.ProcessId);

        // ── Nó 3: scriptTask 'Start Loop' (_zJIHWFqiEfG5K7mY0I3I6w) ─────────────────────────────
        CalcprpcSeg031Steps.ApplyStartLoop(ctx);

        // ── Nó 4: subProcessScope 'Control System Task Call' (_zJIHZlqiEfG5K7mY0I3I6w) ─────────
        // ── Nó 5: startEvent interno (_zJIublqiEfG5K7mY0I3I6w, descida) ────────────────────────
        // Aresta explícita de descida: o XPDL não traz transição para o startEvent interno.
        // O fluxo .NET escreve a descida directamente do subProcessScope para o ActivitySet.
        StartTxEntry:

        // ── Nó 6: scriptTask 'Start TX' (_zJIuaVqiEfG5K7mY0I3I6w) ───────────────────────────────
        CalcprpcSeg031Steps.ApplyStartTx(ctx);

        // ── Nó 7: gateway 'Check Retries SW_QRETRYCOUNT' (_zJIubVqiEfG5K7mY0I3I6w) ─────────────
        // RI-transition-CALCPRPC-CheckRetriesSWQRETRYCOUNT.
        // SW_QRETRYCOUNT pertence ao runtime; neste troço síncrono entra como primeira tentativa (0).
        if (!CalcprpcCheckRetriesRule.IsStillgood(swQRetryCount: 0, maxRetries: ctx.MAXRETRIES))
            return CalcprpcSeg031Result.RetriesExhausted;

        ServiceEnvelope envelope;
        try
        {
            // ── Nó 8: serviceTask 'CalcularPrazo' (_AsZCkVqkEfG5K7mY0I3I6w) ────────────────────
            envelope = await _services.ObterprimeirodiautilaposperiododediascorridosdeatAsync(caseRef, ct);
        }
        catch (Exception ex)
        {
            ctx.STATUS_CODE = "TRANSPORT_ERROR";
            ctx.STERRORCODE = ex.GetType().Name;
            ctx.STERRORDESC = ex.Message;
            ctx.ISAPPERROR = "N";
            ctx.ISTECHERROR = "Y";

            // ── Nó 12: endEvent interno (_zJIub1qiEfG5K7mY0I3I6w) ───────────────────────────────
            // queda implícita para o regresso ao escopo MAIN

            // ── Nó 13: gateway 'Tech Error' (_zJIHZVqiEfG5K7mY0I3I6w, regresso) ─────────────────
            // Regresso explícito: o XPDL não traz a aresta do fim do ActivitySet de volta ao MAIN.
            return CalcprpcSeg031Result.TechError;
        }

        CalcprpcSeg031Steps.MapServiceEnvelopeToContext(ctx, envelope);

        // ── Nó 9: gateway _zJIuclqiEfG5K7mY0I3I6w — STATUS_CODE != "0"? ────────────────────────
        var retriesExhausted = false;
        if (CalcprpcSeg031Steps.IsCallError(ctx))
        {
            // ── Nó 10: scriptTask 'Set App Error' (_zJIucVqiEfG5K7mY0I3I6w) ─────────────────────
            CalcprpcSeg031Steps.SetAppError(ctx, envelope);

            // ── Nó 11: gateway _zJIuc1qiEfG5K7mY0I3I6w — More Retries ────────────────────────────
            if (CalcprpcSeg031Steps.HasMoreRetries(ctx))
            {
                // ── Nó auxiliar: timerEvent 'Pause' (_zJIHYlqiEfG5K7mY0I3I6w) ───────────────────
                await PauseAsync(_clock, ct);
                goto StartTxEntry;
            }

            retriesExhausted = true;
        }

        // ── Nó 12: endEvent interno (_zJIub1qiEfG5K7mY0I3I6w) ───────────────────────────────────
        // queda implícita para o regresso ao escopo MAIN

        // ── Nó 13: gateway 'Tech Error' (_zJIHZVqiEfG5K7mY0I3I6w, regresso) ─────────────────────
        // Regresso explícito: o XPDL não traz a aresta do fim do ActivitySet de volta ao MAIN.
        if (CalcprpcSeg031Steps.IsTechError(ctx))
            return CalcprpcSeg031Result.TechError;

        // ── Nó 14: gateway 'App Error' (_zJIHZFqiEfG5K7mY0I3I6w) ────────────────────────────────
        if (CalcprpcSeg031Steps.IsAppError(ctx))
            return retriesExhausted
                ? CalcprpcSeg031Result.RetriesExhausted
                : CalcprpcSeg031Result.AppError;

        // ── Nó 15: endEvent 'Done - Success' (_zJIHY1qiEfG5K7mY0I3I6w) ─────────────────────────
        return CalcprpcSeg031Result.DoneSuccess;
    }

    private static async Task PauseAsync(IClock clock, CancellationToken ct)
    {
        var pauseDuration = TimeSpan.FromMinutes(1);
        var deadline = clock.Now.Add(pauseDuration);
        var remaining = deadline - clock.Now;
        if (remaining > TimeSpan.Zero)
            await Task.Delay(remaining, ct);
    }

    private static FieldValue<long> ParseIdProcesso(string processId)
    {
        const string marker = "idProc-";
        var markerIndex = processId.LastIndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
            return FieldValue<long>.Empty;

        var rawValue = processId[(markerIndex + marker.Length)..];
        if (string.Equals(rawValue, "NA", StringComparison.OrdinalIgnoreCase))
            return FieldValue<long>.NotAvailable;

        return long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? FieldValue<long>.Of(value)
            : FieldValue<long>.Empty;
    }
}

/// <summary>
/// Resultado possível do percurso do segmento 031 do CALCPRPC.
/// </summary>
public enum CalcprpcSeg031Result
{
    DoneSuccess,
    TechError,
    AppError,
    RetriesExhausted,
}
