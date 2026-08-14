#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Execution.PRPINTPC;

/// <summary>
/// Passos de envelope técnico do processo PRPINTPC — segmento 036
/// (passos 1–19 do cenário SC-PRPINTPC-008).
///
/// Contém apenas lógica que toca STATUS_CODE, ISAPPERROR, ISTECHERROR e
/// contadores de retentativa — nunca cálculo ou decisão de domínio.
/// A separação segue classification.eRegraDeNegocio do rule-catalogue.json.
///
/// Invariantes (glossário POC_Epat.yaml, confirmados 2026-08-06):
///   STATUS_CODE  : '0' = sucesso; != '0' = erro.
///   ISAPPERROR   : 'N' = sem erro de aplicação; 'Y' = erro de aplicação.
///   ISTECHERROR  : 'N' = sem erro técnico;      'Y' = erro técnico.
///   MAXRETRIES   : 5 por omissão.
///   NUMAPPRETRIES: começa em 0, incrementa a cada falha de aplicação.
///   SW_QRETRYCOUNT: lido, nunca escrito.
/// </summary>
public static class PrpintpcExecutionSteps
{
    // ── Nó 2: SetParameters (_KEwC3l6EEfGBBLgT-R5iuw) ─────────────────────

    /// <summary>
    /// Passo SetParameters (RI-script-PRPINTPC-SetParameters) — envelope técnico.
    /// Inicializa MAXRETRIES com o valor por omissão quando ainda nulo.
    /// Não altera campos de domínio do caso.
    /// Fonte: glossário POC_Epat.yaml — "if (MAXRETRIES == null) MAXRETRIES = 5".
    /// </summary>
    public static void ApplySetParameters(ProcessExecutionContext ctx)
    {
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = PrpintpcSetParametersRule.DefaultMaxRetries;
    }

    // ── Nó 3: Start Loop (_KEwC4F6EEfGBBLgT-R5iuw) ──────────────────────────

    /// <summary>
    /// Passo Start Loop (RI-script-PRPINTPC-StartLoop) — envelope técnico.
    /// Inicializa NUMAPPRETRIES=0 quando ainda não foi inicializado.
    /// SW_DATE é um valor de ambiente do iProcess; não é escrito no contexto técnico .NET.
    /// Fonte: glossário POC_Epat.yaml — "if (NUMAPPRETRIES == null) NUMAPPRETRIES = 0".
    /// </summary>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        // NUMAPPRETRIES já é int (não anulável); o valor 0 representa o estado
        // "ainda não inicializado" na primeira passagem do laço.
        // Sem alteração em chamadas subsequentes (regresso do retry manual).
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
    /// Comportamento CORRIGIDO face ao legado: STATUS_CODE != "0" (não != SW_NA).
    /// Fonte: rulings.CLONE-PRPINTPC, glossário POC_Epat.yaml.
    /// </summary>
    public static void SetAppError(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
        ctx.ISAPPERROR  = "Y";
        ctx.NUMAPPRETRIES++;
    }

    // ── Nó Tech Error (regresso explícito, _KEwC7V6EEfGBBLgT-R5iuw) ─────────

    /// <summary>
    /// Marca ISTECHERROR="Y" no contexto.
    /// Chamado tanto pelo ramo Maxretriesexceeded (quando SW_QRETRYCOUNT &gt;= MAXRETRIES)
    /// quanto pela captura de excepção de transporte (regresso explícito).
    /// </summary>
    public static void SetTechError(ProcessExecutionContext ctx, string reason)
    {
        ctx.ISTECHERROR = "Y";
        ctx.STERRORDESC ??= reason;
    }

    // ── Helpers de gateway ────────────────────────────────────────────────────

    /// <summary>
    /// Gateway _KEwDVl6EEfGBBLgT-R5iuw: "A chamada a CaptaParametros foi bem sucedida?"
    /// Ramo AppError: STATUS_CODE != "0".
    /// Comportamento CORRIGIDO: compara com "0", não com SW_NA (rulings.CLONE-PRPINTPC).
    /// </summary>
    public static bool IsAppError(ProcessExecutionContext ctx) =>
        ctx.STATUS_CODE != "0";

    /// <summary>
    /// Gateway Tech Error (_KEwC7V6EEfGBBLgT-R5iuw): ramo "Yes" (erro técnico).
    /// Alcançado por REGRESSO — aresta explícita no fluxo .NET, não existe no XPDL.
    /// </summary>
    public static bool IsTechError(ProcessExecutionContext ctx) =>
        ctx.ISTECHERROR == "Y";

    /// <summary>
    /// Gateway App Error (_KEwC7F6EEfGBBLgT-R5iuw): ISAPPERROR == "Y".
    /// </summary>
    public static bool IsAppErrorFlag(ProcessExecutionContext ctx) =>
        ctx.ISAPPERROR == "Y";

    /// <summary>
    /// Gateway More Retries (_KEwC6V6EEfGBBLgT-R5iuw): NUMAPPRETRIES &lt; MAXRETRIES.
    /// Ramo "Yes": há retentativas disponíveis → incrementa e regressa ao início do laço.
    /// </summary>
    public static bool HasMoreRetries(ProcessExecutionContext ctx) =>
        ctx.NUMAPPRETRIES < ctx.MAXRETRIES;

    /// <summary>
    /// Mapeia o envelope de serviço para o contexto após chamada bem-sucedida a CaptaParametros.
    /// </summary>
    public static void MapServiceEnvelope(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
    }
}
