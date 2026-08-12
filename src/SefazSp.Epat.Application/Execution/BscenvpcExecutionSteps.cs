#nullable enable

using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Execution;

/// <summary>
/// Passos de envelope técnico do processo BSCENVPC.
/// Contém a lógica que toca STATUS_CODE, ISAPPERROR, ISTECHERROR e
/// os contadores de retentativa — nunca cálculo ou decisão de domínio.
/// A separação segue <c>classification.eRegraDeNegocio</c> do rule-catalogue.json.
/// </summary>
public static class BscenvpcExecutionSteps
{
    /// <summary>
    /// Passo SetParameters (_qIDulV6BEfGBBLgT-R5iuw) — envelope técnico.
    /// Inicializa MAXRETRIES e PROCESS_ID no contexto de execução.
    /// A decisão de domínio (se deve inicializar) já foi avaliada por
    /// <see cref="BscenvpcSetParametersRule.ShouldInitialize"/>.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="processId">Identificador do processo derivado de IDPROCESSO, ou null.</param>
    public static void ApplySetParameters(ProcessExecutionContext ctx, string? processId)
    {
        // MAXRETRIES: aplica o default quando ainda não foi fixado (==0 como sentinela de não inicializado).
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = BscenvpcSetParametersRule.DefaultMaxRetries;

        if (processId is not null)
            ctx.PROCESS_ID = processId;
    }

    /// <summary>
    /// Passo Start Loop (_qIDul16BEfGBBLgT-R5iuw) — envelope técnico.
    /// Inicializa NUMAPPRETRIES=0 quando ainda não foi inicializado.
    /// Fonte: glossário POC_Epat.yaml — "if (NUMAPPRETRIES == null) NUMAPPRETRIES = 0",
    /// confirmado em 2026-08-06.
    /// O contador NUMAPPRETRIES é independente de SW_QRETRYCOUNT (motor).
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        // A condição legada é "if (NUMAPPRETRIES == null)", i.e., apenas na primeira entrada no loop.
        // Em .NET, 0 é o valor padrão de int; o campo já vem a 0 na primeira entrada.
        // Sem alteração necessária — o valor já é 0.
        _ = ctx.NUMAPPRETRIES; // leitura explícita para rastreabilidade do contador
    }

    /// <summary>
    /// Passo Start TX (_qIDu3F6BEfGBBLgT-R5iuw) — envelope técnico (escopo ActivitySet).
    /// Reinicia os indicadores de erro antes de iniciar a transacção de serviço.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartTx(ProcessExecutionContext ctx)
    {
        ctx.STATUS_CODE  = null;
        ctx.ISAPPERROR   = "N";
        ctx.ISTECHERROR  = "N";
    }
}
