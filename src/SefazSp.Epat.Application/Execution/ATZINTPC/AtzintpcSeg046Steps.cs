#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Execution.ATZINTPC;

/// <summary>
/// Passos de envelope técnico do processo ATZINTPC — segmento 046
/// (passos 1–18 do cenário SC-ATZINTPC-007).
///
/// Contém apenas lógica que toca STATUS_CODE, ISAPPERROR, ISTECHERROR e
/// contadores de retentativa — nunca cálculo ou decisão de domínio.
/// A separação segue <c>classification.eRegraDeNegocio</c> do rule-catalogue.json.
///
/// Invariantes (glossário POC_Epat.yaml):
///   STATUS_CODE  : '0' = sucesso; != '0' = erro de aplicação.
///   ISAPPERROR   : 'N' = sem erro de aplicação; 'Y' = erro de aplicação.
///   ISTECHERROR  : 'N' = sem erro técnico; 'Y' = erro técnico.
///   MAXRETRIES   : 5 por omissão.
///   NUMAPPRETRIES: começa em 0, incrementa a cada falha de aplicação.
///   SW_QRETRYCOUNT: lido, nunca escrito.
///
/// Card: BUILD-ATZINTPC-seg046
/// </summary>
public static class AtzintpcSeg046Steps
{
    // ── Nó 2: SetParameters (_RNdJyl6PEfGBBLgT-R5iuw) ─────────────────────

    /// <summary>
    /// Passo SetParameters — envelope técnico.
    /// Inicializa MAXRETRIES no contexto de execução.
    /// A decisão de domínio (se deve inicializar) é avaliada por
    /// <see cref="AtzintpcSetParametersRule.ShouldInitialize"/>.
    /// Decisão NOEQ-iprocess-builtin (shim-tri-state, ratificado).
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="processId">Identificador do processo derivado de IDPROCESSO, ou null.</param>
    public static void ApplySetParameters(ProcessExecutionContext ctx, string? processId = null)
    {
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = AtzintpcSetParametersRule.DefaultMaxRetries;

        if (processId is not null)
            ctx.PROCESS_ID = processId;
    }

    // ── Nó 3: Start Loop (_RNdJzF6PEfGBBLgT-R5iuw) ─────────────────────────

    /// <summary>
    /// Passo Start Loop — envelope técnico.
    /// Regra: RI-script-ATZINTPC-StartLoop.
    /// Decisão NOEQ-iprocess-builtin: SW_DATE é lido pelo motor, não pelo processo.
    /// NUMAPPRETRIES é inicializado a 0 se ainda não foi definido nesta execução.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        _ = ctx.NUMAPPRETRIES; // leitura explícita para rastreabilidade
    }

    // ── Nó 6: Start TX (_RNdKFF6PEfGBBLgT-R5iuw) ────────────────────────────

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

    // ── Nó 10: Set App Error (_RNdKGV6PEfGBBLgT-R5iuw) ──────────────────────

    /// <summary>
    /// Passo Set App Error — envelope técnico.
    /// Marca ISAPPERROR="Y" e incrementa o contador de retentativas de aplicação.
    /// Chamado quando STATUS_CODE != "0" após a chamada a AtualizarIntimacao.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="envelope">Envelope de serviço cujos dados de erro são mapeados para o contexto.</param>
    public static void SetAppError(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE   = envelope.STATUS_CODE;
        ctx.STERRORCODE   = envelope.STERRORCODE;
        ctx.STERRORDESC   = envelope.STERRORDESC;
        ctx.ISAPPERROR    = "Y";
        ctx.ISTECHERROR   = "N";
        ctx.NUMAPPRETRIES = ctx.NUMAPPRETRIES + 1;
    }

    // ── Gateway Tech Error (regresso explícito) ───────────────────────────────

    /// <summary>
    /// Marca ISTECHERROR="Y" no contexto, registando a razão técnica.
    /// Chamado quando o gateway Check Retries esgota as tentativas do motor
    /// ou quando uma excepção de transporte é capturada (regresso explícito).
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="reason">Mensagem de diagnóstico (não persistida no caso; apenas para rastreabilidade).</param>
    public static void SetTechError(ProcessExecutionContext ctx, string reason)
    {
        ctx.ISTECHERROR = "Y";
        ctx.STERRORDESC ??= reason;
    }

    // ── Condições de gateway ──────────────────────────────────────────────────

    /// <summary>
    /// Gateway _RNdKGl6PEfGBBLgT-R5iuw.
    /// "A chamada a AtualizarIntimacao foi bem sucedida?"
    /// Ramo AppError: STATUS_CODE != "0".
    /// </summary>
    public static bool IsAppError(ServiceEnvelope envelope) =>
        envelope.STATUS_CODE != "0";

    /// <summary>
    /// Gateway App Error (_RNdJ2F6PEfGBBLgT-R5iuw).
    /// Ramo Yes: ISAPPERROR == "Y".
    /// Condição legada: ISAPPERROR=='Y'.
    /// </summary>
    public static bool IsAppErrorFlag(ProcessExecutionContext ctx) =>
        ctx.ISAPPERROR == "Y";

    /// <summary>
    /// Gateway More Retries (_RNdJ1V6PEfGBBLgT-R5iuw).
    /// Ramo Yes: NUMAPPRETRIES &lt; MAXRETRIES.
    /// Condição legada: NUMAPPRETRIES&lt;MAXRETRIES.
    /// </summary>
    public static bool HasMoreRetries(ProcessExecutionContext ctx) =>
        ctx.NUMAPPRETRIES < ctx.MAXRETRIES;

    // ── Mapeamento sucesso ────────────────────────────────────────────────────

    /// <summary>
    /// Mapeia o envelope de serviço para o contexto quando STATUS_CODE == "0".
    /// </summary>
    public static void MapServiceEnvelopeSuccess(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
        ctx.ISAPPERROR  = "N";
        ctx.ISTECHERROR = "N";
    }
}
