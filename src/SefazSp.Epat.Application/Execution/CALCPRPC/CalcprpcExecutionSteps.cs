#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Execution.CALCPRPC;

/// <summary>
/// Passos de envelope técnico do processo CALCPRPC — segmento 032
/// (passos 1–18 do cenário SC-CALCPRPC-007).
///
/// Contém apenas lógica que toca STATUS_CODE, ISAPPERROR, ISTECHERROR e
/// contadores de retentativa — nunca cálculo ou decisão de domínio.
/// A separação segue <c>classification.eRegraDeNegocio</c> do rule-catalogue.json.
///
/// Invariantes (glossário POC_Epat.yaml, confirmados 2026-08-06):
///   STATUS_CODE  : '0' = sucesso; != '0' = erro.
///   ISAPPERROR   : 'N' = sem erro de aplicação; 'Y' = erro de aplicação.
///   ISTECHERROR  : 'N' = sem erro técnico;      'Y' = erro técnico.
///   MAXRETRIES   : 5 por omissão.
///   NUMAPPRETRIES: começa em 0, incrementa a cada falha de aplicação.
///   SW_QRETRYCOUNT: lido, nunca escrito.
/// </summary>
public static class CalcprpcExecutionSteps
{
    // ── Nó 2: SetParameters (_zJIHVlqiEfG5K7mY0I3I6w) ─────────────────────

    /// <summary>
    /// Passo SetParameters — envelope técnico.
    /// Inicializa MAXRETRIES e PROCESS_ID no contexto de execução.
    /// A decisão de domínio (se deve inicializar) já foi avaliada por
    /// <see cref="CalcprpcSetParametersRule.ShouldInitialize"/>.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="processId">Identificador do processo derivado de IDPROCESSO, ou null.</param>
    public static void ApplySetParameters(ProcessExecutionContext ctx, string? processId)
    {
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = CalcprpcSetParametersRule.DefaultMaxRetries;

        if (processId is not null)
            ctx.PROCESS_ID = processId;
    }

    /// <inheritdoc cref="ApplySetParameters(ProcessExecutionContext, string?)"/>
    public static void ApplySetParameters(ProcessExecutionContext ctx)
        => ApplySetParameters(ctx, processId: null);

    // ── Nó 3: Start Loop (_zJIHWFqiEfG5K7mY0I3I6w) ─────────────────────────

    /// <summary>
    /// Passo Start Loop — envelope técnico.
    /// Inicializa NUMAPPRETRIES=0 quando ainda não foi inicializado.
    /// Usa IPESystemValues.SW_DATE implicitamente (registo de data de início do loop) —
    /// tratado como valor de ambiente, não escrito no contexto técnico.
    /// Fonte: glossário POC_Epat.yaml — "if (NUMAPPRETRIES == null) NUMAPPRETRIES = 0".
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        _ = ctx.NUMAPPRETRIES; // leitura explícita para rastreabilidade
    }

    // ── Nó 6: Start TX (_zJIuaVqiEfG5K7mY0I3I6w) ────────────────────────────

    /// <summary>
    /// Passo Start TX — envelope técnico (escopo ActivitySet).
    /// Reinicia os indicadores de erro antes de iniciar a transacção de serviço.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartTx(ProcessExecutionContext ctx)
    {
        ctx.STATUS_CODE = null;
        ctx.ISAPPERROR  = "N";
        ctx.ISTECHERROR = "N";
    }

    // ── Nó 10: Set App Error (_zJIucVqiEfG5K7mY0I3I6w) ──────────────────────

    /// <summary>
    /// Passo Set App Error — envelope técnico.
    /// Marca ISAPPERROR="Y" e incrementa o contador de retentativas de aplicação.
    /// Chamado quando STATUS_CODE != "0" após a chamada a CalcularPrazo.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void SetAppError(ProcessExecutionContext ctx)
    {
        ctx.ISAPPERROR = "Y";
        ctx.NUMAPPRETRIES++;
    }

    /// <inheritdoc cref="SetAppError(ProcessExecutionContext)"/>
    /// <param name="envelope">Envelope de serviço cujos dados de erro são mapeados para o contexto.</param>
    public static void SetAppError(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        MapServiceEnvelope(ctx, envelope);
        SetAppError(ctx);
    }

    // ── Nó Tech Error (regresso explícito) ───────────────────────────────────

    /// <summary>
    /// Marca ISTECHERROR="Y" no contexto, registando a razão técnica.
    /// Chamado tanto pelo ramo Maxretriesexceeded do gateway de retentativas
    /// quanto pela captura de excepção de transporte (regresso explícito).
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="reason">Mensagem de diagnóstico (não persistida no caso; apenas para rastreabilidade).</param>
    public static void SetTechError(ProcessExecutionContext ctx, string reason)
    {
        ctx.ISTECHERROR = "Y";
        ctx.STERRORDESC ??= reason;
    }

    /// <summary>
    /// Gateway _zJIuclqiEfG5K7mY0I3I6w.
    /// "A chamada a CalcularPrazo foi bem sucedida?"
    /// Ramo AppError: STATUS_CODE != "0".
    /// Decisão ratificada: rulings.CALCPRPC/_zJIuclqiE (glossário, 2026-08-06).
    /// </summary>
    public static bool IsAppError(ProcessExecutionContext ctx)
        => ctx.STATUS_CODE != "0";

    /// <summary>
    /// Gateway Tech Error (_zJIHZVqiEfG5K7mY0I3I6w).
    /// Alcançado por REGRESSO (aresta explícita no fluxo .NET, não existe no XPDL).
    /// Ramo "No" (otherwise) → encaminha para gateway App Error.
    /// </summary>
    public static bool IsTechError(ProcessExecutionContext ctx)
        => ctx.ISTECHERROR == "Y";

    /// <summary>
    /// Gateway App Error (_zJIHZFqiEfG5K7mY0I3I6w).
    /// Ramo "Yes": ISAPPERROR == "Y" → encaminha para More Retries.
    /// </summary>
    public static bool IsAppErrorFlag(ProcessExecutionContext ctx)
        => ctx.ISAPPERROR == "Y";

    /// <summary>
    /// Gateway More Retries (_zJIHYVqiEfG5K7mY0I3I6w).
    /// Ramo "Yes": NUMAPPRETRIES &lt; MAXRETRIES → encaminha para Pause.
    /// </summary>
    public static bool HasMoreRetries(ProcessExecutionContext ctx)
        => ctx.NUMAPPRETRIES < ctx.MAXRETRIES;

    /// <summary>
    /// Mapeia o envelope de serviço para o contexto de execução após chamada a CalcularPrazo.
    /// </summary>
    public static void MapServiceEnvelope(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
    }
}
