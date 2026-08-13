#nullable enable

using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Execution.CRNOTPC;

/// <summary>
/// Passos de envelope técnico do processo CRNOTPC — segmento 028 (prólogo).
/// Contém a lógica que toca STATUS_CODE, ISAPPERROR, ISTECHERROR e
/// os contadores de retentativa — nunca cálculo ou decisão de domínio.
/// A separação segue <c>classification.eRegraDeNegocio</c> do rule-catalogue.json.
///
/// Card: BUILD-CRNOTPC-seg028 · AC2/AC3/AC5/AC7
/// </summary>
public static class CrnotpcSeg028Steps
{
    /// <summary>
    /// Passo SetParameters (_NcJJ4l9KEfGqPfX31TKC3w) — envelope técnico.
    /// Inicializa MAXRETRIES e PROCESS_ID no contexto de execução.
    /// A decisão de domínio (se deve inicializar) é avaliada por
    /// <see cref="CrnotpcSetParametersRule.ShouldInitialize"/>.
    ///
    /// MAXRETRIES: aplica o default quando ainda não foi fixado (==0 como sentinela de não inicializado).
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="processId">Identificador do processo derivado de IDPROCESSO, ou null.</param>
    public static void ApplySetParameters(ProcessExecutionContext ctx, string? processId)
    {
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = CrnotpcSetParametersRule.DefaultMaxRetries;

        if (processId is not null)
            ctx.PROCESS_ID = processId;
    }

    /// <summary>
    /// Passo Start Loop (_NcJJ5F9KEfGqPfX31TKC3w) — envelope técnico.
    /// Inicializa NUMAPPRETRIES=0 quando ainda não foi inicializado.
    /// Fonte: glossário POC_Epat.yaml — "if (NUMAPPRETRIES == null) NUMAPPRETRIES = 0".
    /// O contador NUMAPPRETRIES é independente de SW_QRETRYCOUNT (motor).
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        // Condição legada: "if (NUMAPPRETRIES == null)". Em .NET, int default é 0 — nenhuma alteração.
        _ = ctx.NUMAPPRETRIES;
    }

    /// <summary>
    /// Passo Start TX (_NcJxKF9KEfGqPfX31TKC3w) — envelope técnico (escopo subProcessScope).
    /// Reinicia os indicadores de erro antes de iniciar a transacção de serviço.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartTx(ProcessExecutionContext ctx)
    {
        ctx.STATUS_CODE = null;
        ctx.ISAPPERROR  = "N";
        ctx.ISTECHERROR = "N";
    }
}
