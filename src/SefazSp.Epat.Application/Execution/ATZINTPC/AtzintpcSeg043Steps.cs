#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Execution.ATZINTPC;

/// <summary>
/// Passos de envelope técnico do processo ATZINTPC — segmento 043
/// (passos 1–15 do cenário SC-ATZINTPC-010).
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
///
/// Card: BUILD-ATZINTPC-seg043
/// </summary>
public static class AtzintpcSeg043Steps
{
    // ── Nó 2: SetParameters (_RNdJyl6PEfGBBLgT-R5iuw) ─────────────────────

    /// <summary>
    /// Passo SetParameters — envelope técnico.
    /// Inicializa MAXRETRIES e PROCESS_ID no contexto de execução.
    /// A decisão de domínio (se deve inicializar) já foi avaliada por
    /// <see cref="AtzintpcSetParametersRule.ShouldInitialize"/>.
    /// </summary>
    public static void ApplySetParameters(ProcessExecutionContext ctx, string? processId = null)
    {
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = AtzintpcSetParametersRule.DefaultMaxRetries;

        if (processId is not null)
            ctx.PROCESS_ID = processId;
    }

    // ── Nó 3: Start Loop (_RNdJzF6PEfGBBLgT-R5iuw) ──────────────────────────

    /// <summary>
    /// Passo Start Loop — envelope técnico.
    /// Regra: RI-script-ATZINTPC-StartLoop.
    /// NOEQ-iprocess-builtin: SW_DATE (data de sistema) — shim-tri-state ratificado 2026-08-06.
    /// </summary>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        _ = ctx.NUMAPPRETRIES;
    }

    // ── Nó 6: Start TX (_RNdKFF6PEfGBBLgT-R5iuw) ────────────────────────────

    /// <summary>
    /// Passo Start TX — envelope técnico (escopo ActivitySet).
    /// Reinicia os indicadores de erro antes de iniciar a transacção de serviço.
    /// </summary>
    public static void ApplyStartTx(ProcessExecutionContext ctx)
    {
        ctx.STATUS_CODE = null;
        ctx.ISAPPERROR  = "N";
        ctx.ISTECHERROR = "N";
    }

    // ── Nó 10: Set App Error (_RNdKGV6PEfGBBLgT-R5iuw) ──────────────────────

    /// <summary>
    /// Passo Set App Error — envelope técnico.
    /// Marca ISAPPERROR="Y" e incrementa o contador de retentativas de aplicação.
    /// Chamado quando STATUS_CODE != "0" após a chamada a AtualizarIntimacao.
    /// </summary>
    public static void SetAppError(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE   = envelope.STATUS_CODE;
        ctx.STERRORCODE   = envelope.STERRORCODE;
        ctx.STERRORDESC   = envelope.STERRORDESC;
        ctx.ISAPPERROR    = "Y";
        ctx.ISTECHERROR   = "N";
        ctx.NUMAPPRETRIES = ctx.NUMAPPRETRIES + 1;
    }

    // ── Nó Tech Error (regresso explícito) ───────────────────────────────────

    /// <summary>
    /// Marca ISTECHERROR="Y" após falha de transporte ou esgotamento de SW_QRETRYCOUNT.
    /// </summary>
    public static void SetTechError(ProcessExecutionContext ctx, string reason)
    {
        ctx.ISTECHERROR = "Y";
        ctx.ISAPPERROR  = "N";
        ctx.STERRORDESC = reason;
    }

    // ── Predicados de gateway (MAIN) ─────────────────────────────────────────

    /// <summary>
    /// Gateway "A chamada a AtualizarIntimacao foi bem sucedida?" (_RNdKGl6PEfGBBLgT-R5iuw).
    /// Condição de AppError: STATUS_CODE != "0".
    /// </summary>
    public static bool IsAppError(ServiceEnvelope envelope) =>
        envelope.STATUS_CODE != "0";
}
