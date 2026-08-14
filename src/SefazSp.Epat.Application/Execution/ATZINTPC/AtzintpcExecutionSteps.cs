#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Execution;

/// <summary>
/// Passos de envelope técnico do processo ATZINTPC.
/// Contém a lógica que toca STATUS_CODE, ISAPPERROR, ISTECHERROR e
/// os contadores de retentativa — nunca cálculo ou decisão de domínio.
/// A separação segue <c>classification.eRegraDeNegocio</c> do rule-catalogue.json.
///
/// Invariantes (glossário POC_Epat.yaml, confirmados 2026-08-06):
///   STATUS_CODE  : '0' = sucesso; != '0' = erro.
///   ISAPPERROR   : 'N' = sem erro de aplicação; 'Y' = erro de aplicação.
///   ISTECHERROR  : 'N' = sem erro técnico;      'Y' = erro técnico.
///   MAXRETRIES   : 5 por omissão.
/// </summary>
public static class AtzintpcExecutionSteps
{
    /// <summary>
    /// Passo SetParameters (_RNdJyl6PEfGBBLgT-R5iuw) — envelope técnico.
    /// Inicializa MAXRETRIES e PROCESS_ID no contexto de execução.
    /// A decisão de domínio (se deve inicializar) já foi avaliada por
    /// <see cref="AtzintpcSetParametersRule.ShouldInitialize"/>.
    /// </summary>
    public static void ApplySetParameters(ProcessExecutionContext ctx, string? processId)
    {
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = AtzintpcSetParametersRule.DefaultMaxRetries;

        if (processId is not null)
            ctx.PROCESS_ID = processId;
    }

    /// <summary>
    /// Passo Start Loop (_RNdJzF6PEfGBBLgT-R5iuw) — envelope técnico.
    /// NUMAPPRETRIES permanece 0 (valor padrão de int na primeira entrada).
    /// </summary>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        _ = ctx.NUMAPPRETRIES; // leitura explícita para rastreabilidade do contador
    }

    /// <summary>
    /// Passo Start TX (_RNdKFF6PEfGBBLgT-R5iuw) — envelope técnico (escopo ActivitySet).
    /// Reinicia os indicadores de erro antes de iniciar a transacção de serviço.
    /// </summary>
    public static void ApplyStartTx(ProcessExecutionContext ctx)
    {
        ctx.STATUS_CODE = null;
        ctx.ISAPPERROR  = "N";
        ctx.ISTECHERROR = "N";
    }

    /// <summary>
    /// Passo AtualizarIntimacao (_RNdKHF6PEfGBBLgT-R5iuw):
    /// copia os valores do envelope de serviço para o contexto de execução.
    /// </summary>
    public static void MapServiceEnvelope(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
    }

    /// <summary>
    /// Passo "Set App Error" (_RNdKGV6PEfGBBLgT-R5iuw):
    /// marca ISAPPERROR="Y" e incrementa NUMAPPRETRIES.
    /// </summary>
    public static void SetAppError(ProcessExecutionContext ctx)
    {
        ctx.ISAPPERROR = "Y";
        ctx.NUMAPPRETRIES++;
    }

    /// <summary>
    /// Condição do gateway _RNdKGl6PEfGBBLgT-R5iuw.
    /// Ramo AppError: STATUS_CODE != "0".
    /// </summary>
    public static bool IsAppError(ProcessExecutionContext ctx)
        => ctx.STATUS_CODE != "0";

    /// <summary>
    /// Condição do gateway _RNdJ2V6PEfGBBLgT-R5iuw ("Tech Error").
    /// Ramo "Yes": ISTECHERROR == "Y".
    /// </summary>
    public static bool IsTechError(ProcessExecutionContext ctx)
        => ctx.ISTECHERROR == "Y";

    /// <summary>
    /// Condição do gateway _RNdJ2F6PEfGBBLgT-R5iuw ("App Error").
    /// Ramo "Yes": ISAPPERROR == "Y".
    /// </summary>
    public static bool IsStillAppError(ProcessExecutionContext ctx)
        => ctx.ISAPPERROR == "Y";

    /// <summary>
    /// Condição do gateway _RNdJ1V6PEfGBBLgT-R5iuw ("More Retries").
    /// Ramo "Yes": NUMAPPRETRIES &lt; MAXRETRIES.
    /// </summary>
    public static bool HasMoreRetries(ProcessExecutionContext ctx)
        => ctx.NUMAPPRETRIES < ctx.MAXRETRIES;
}
