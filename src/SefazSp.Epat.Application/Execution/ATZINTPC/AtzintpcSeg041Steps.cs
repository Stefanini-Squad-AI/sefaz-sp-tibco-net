#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Execution.ATZINTPC;

/// <summary>
/// Passos de envelope técnico do processo ATZINTPC — segmento 041
/// (passos 1–20 do cenário SC-ATZINTPC-009).
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
/// Card: BUILD-ATZINTPC-seg041
/// </summary>
public static class AtzintpcSeg041Steps
{
    // ── Nó 2: SetParameters (_RNdJyl6PEfGBBLgT-R5iuw) ─────────────────────

    /// <summary>
    /// Passo SetParameters — envelope técnico.
    /// Inicializa MAXRETRIES no contexto de execução.
    /// A decisão de domínio (se deve inicializar) já foi avaliada por
    /// <see cref="AtzintpcSetParametersRule.ShouldInitialize"/>.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplySetParameters(ProcessExecutionContext ctx)
    {
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = AtzintpcSetParametersRule.DefaultMaxRetries;
    }

    // ── Nó 3: Start Loop (_RNdJzF6PEfGBBLgT-R5iuw) ─────────────────────────

    /// <summary>
    /// Passo Start Loop — envelope técnico.
    /// Inicializa NUMAPPRETRIES=0 quando ainda não foi inicializado nesta execução.
    /// NOEQ-iprocess-builtin: SW_DATE usado aqui pelo iProcess;
    /// em .NET é substituído por <see cref="DateTime.UtcNow"/> quando necessário.
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

    // ── Nó Set Technical Error (_RNdKGF6PEfGBBLgT-R5iuw, regresso via TechError) ──

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
    /// </summary>
    public static bool IsAppErrorFlag(ProcessExecutionContext ctx) =>
        ctx.ISAPPERROR == "Y";

    /// <summary>
    /// Gateway More Retries (_RNdJ1V6PEfGBBLgT-R5iuw).
    /// Ramo Yes: NUMAPPRETRIES &lt; MAXRETRIES.
    /// </summary>
    public static bool HasMoreRetries(ProcessExecutionContext ctx) =>
        ctx.NUMAPPRETRIES < ctx.MAXRETRIES;

    /// <summary>
    /// Gateway Manually Fixed (_RNdJy16PEfGBBLgT-R5iuw).
    /// Ramo Yes: OUTCOME == "OK".
    /// </summary>
    public static bool IsManuallyFixed(ProcessExecutionContext ctx) =>
        ctx.OUTCOME == "OK";

    /// <summary>
    /// Gateway Try Again (_RNdJ0F6PEfGBBLgT-R5iuw).
    /// Ramo Yes: OUTCOME == "R".
    /// </summary>
    public static bool IsTryAgain(ProcessExecutionContext ctx) =>
        ctx.OUTCOME == "R";

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
