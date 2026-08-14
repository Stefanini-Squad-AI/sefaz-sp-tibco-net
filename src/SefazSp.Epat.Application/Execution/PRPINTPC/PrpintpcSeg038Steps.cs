#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Execution.PRPINTPC;

/// <summary>
/// Passos de envelope técnico do processo PRPINTPC — segmento 038
/// (passos 1–18 do cenário SC-PRPINTPC-007).
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
/// Card: BUILD-PRPINTPC-seg038
/// </summary>
public static class PrpintpcSeg038Steps
{
    // ── Nó 2: SetParameters (_KEwC3l6EEfGBBLgT-R5iuw) ─────────────────────

    /// <summary>
    /// Passo SetParameters — envelope técnico.
    /// Inicializa MAXRETRIES no contexto de execução.
    /// A decisão de domínio (se deve inicializar) já foi avaliada por
    /// <see cref="PrpintpcSetParametersRule.ShouldInitialize"/>.
    /// </summary>
    public static void ApplySetParameters(ProcessExecutionContext ctx)
    {
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = PrpintpcSetParametersRule.DefaultMaxRetries;
    }

    // ── Nó 3: Start Loop (_KEwC4F6EEfGBBLgT-R5iuw) ─────────────────────────

    /// <summary>
    /// Passo Start Loop — envelope técnico.
    /// Regra RI-script-PRPINTPC-StartLoop.
    /// Inicializa NUMAPPRETRIES=0 quando ainda não foi inicializado.
    /// Fonte: glossário POC_Epat.yaml — "if (NUMAPPRETRIES == null) NUMAPPRETRIES = 0".
    /// </summary>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        if (ctx.NUMAPPRETRIES == 0)
            ctx.NUMAPPRETRIES = 0; // inicialização explícita para rastreabilidade
        ctx.ISAPPERROR  = "N";
        ctx.ISTECHERROR = "N";
        ctx.OUTCOME     = null;
    }

    // ── Nó 6: Start TX (_KEwDUF6EEfGBBLgT-R5iuw) ────────────────────────────

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

    // ── Nó 10: Set App Error (_KEwDVV6EEfGBBLgT-R5iuw) ──────────────────────

    /// <summary>
    /// Passo Set App Error — envelope técnico.
    /// Marca ISAPPERROR="Y" e incrementa o contador de retentativas de aplicação.
    /// Chamado quando STATUS_CODE != "0" após a chamada a CaptaParametros.
    /// Correcção aplicada: STATUS_CODE != "0" (rulings.CLONE-PRPINTPC, decisão de alinhar
    /// PRPINTPC com os processos irmãos em vez de comparar com SW_NA).
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

    // ── Condições de gateway ──────────────────────────────────────────────────

    /// <summary>
    /// Gateway _KEwDVl6EEfGBBLgT-R5iuw (nó 9, ActivitySet).
    /// "A chamada a CaptaParametros foi bem sucedida?"
    /// Ramo AppError: STATUS_CODE != "0".
    ///
    /// ATENÇÃO — correcção de defeito (rulings.CLONE-PRPINTPC):
    /// O XPDL legado compara com SW_NA; a decisão racionada alinha PRPINTPC com os
    /// quatro processos irmãos e usa STATUS_CODE != "0".
    /// Esta correcção muda comportamento observado no legado.
    /// </summary>
    public static bool IsAppError(ProcessExecutionContext ctx)
        => ctx.STATUS_CODE != "0";

    /// <summary>
    /// Gateway Tech Error (_KEwC7V6EEfGBBLgT-R5iuw, nó 13, MAIN).
    /// Alcançado por REGRESSO (aresta explícita no fluxo .NET, não existe no XPDL).
    /// Ramo "Yes": ISTECHERROR == "Y".
    /// </summary>
    public static bool IsTechError(ProcessExecutionContext ctx)
        => ctx.ISTECHERROR == "Y";

    /// <summary>
    /// Gateway App Error (_KEwC7F6EEfGBBLgT-R5iuw, nó 14, MAIN).
    /// Ramo "Yes": ISAPPERROR == "Y" → encaminha para More Retries.
    /// </summary>
    public static bool IsAppErrorFlag(ProcessExecutionContext ctx)
        => ctx.ISAPPERROR == "Y";

    /// <summary>
    /// Gateway More Retries (_KEwC6V6EEfGBBLgT-R5iuw, nó 15, MAIN).
    /// Ramo "Yes": NUMAPPRETRIES &lt; MAXRETRIES → encaminha para Pause.
    /// </summary>
    public static bool HasMoreRetries(ProcessExecutionContext ctx)
        => ctx.NUMAPPRETRIES < ctx.MAXRETRIES;

    /// <summary>
    /// Mapeia o envelope de serviço para o contexto de execução após chamada bem-sucedida.
    /// </summary>
    public static void MapServiceEnvelope(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
        ctx.ISAPPERROR  = "N";
        ctx.ISTECHERROR = "N";
    }
}
