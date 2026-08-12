#nullable enable

namespace SefazSp.Epat.Application.Execution.BSCENVPC;

/// <summary>
/// Passo _qIDu4V6BEfGBBLgT-R5iuw — Set App Error (scriptTask, ActivitySet escopo).
/// Reflecte o script do iProcess: ISAPPERROR='Y'; OUTCOME='R';
/// Altera o envelope tecnico (ProcessExecutionContext) — nao o estado de negocio do caso.
/// Separacao registada em rule-catalogue.json, campo classification.eRegraDeNegocio.
/// </summary>
public static class SetAppErrorStep
{
    /// <summary>
    /// Aplica os efeitos do passo Set App Error no contexto de execucao.
    /// ISAPPERROR e OUTCOME sao variaveis do envelope tecnico, nao do modelo de dominio.
    /// </summary>
    public static void Execute(ProcessExecutionContext ctx)
    {
        ctx.ISAPPERROR = "Y";
        ctx.OUTCOME = "R";
    }
}
