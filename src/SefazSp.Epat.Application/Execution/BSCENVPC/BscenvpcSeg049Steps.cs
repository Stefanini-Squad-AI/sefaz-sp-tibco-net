#nullable enable

using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Execution.BSCENVPC;

/// <summary>
/// Passos de envelope técnico do segmento 049 do processo BSCENVPC.
/// Cobre os nós adicionais ao prólogo (nodes 8-12) do percurso de referência
/// SC-BSCENVPC-016: de 'Set Technical Error' a 'Done - Success'.
///
/// Separação de responsabilidades (rule-catalogue.json · classification.eRegraDeNegocio):
///   • O que calcula ou decide sobre o caso → Domain/Rules (função pura).
///   • O que mexe no envelope técnico (STATUS_CODE, contadores) → aqui.
///
/// Invariantes de identificador (card BUILD-BSCENVPC-seg049):
///   _qIDu4F6BEfGBBLgT-R5iuw — Set Technical Error (scriptTask, ActivitySet, node 8)
///   _qIDu316BEfGBBLgT-R5iuw — endEvent interno   (ActivitySet, node 9)
///   _qIDupF6BEfGBBLgT-R5iuw — Tech Error gateway  (MAIN, node 10, regresso)
///   _qIDuo16BEfGBBLgT-R5iuw — App Error gateway   (MAIN, node 11)
///   _qIDuol6BEfGBBLgT-R5iuw — Done - Success      (MAIN, node 12)
/// </summary>
public static class BscenvpcSeg049Steps
{
    /// <summary>
    /// Nó 8 — Set Technical Error (_qIDu4F6BEfGBBLgT-R5iuw, scriptTask, ActivitySet).
    /// Executado quando o gateway Check Retries toma o ramo OTHERWISE ("Maxretriesexceeded"):
    /// IPESystemValues.SW_QRETRYCOUNT >= MAXRETRIES.
    ///
    /// Documenta o motivo da saída do ActivitySet (retentativas de motor esgotadas).
    /// NÃO altera ISTECHERROR nem ISAPPERROR: o gateway Tech Error (nó 10) avalia
    /// ISTECHERROR, que foi reiniciado para "N" pelo Start TX (nó 6). O ramo
    /// OTHERWISE ("No") do Tech Error é alcançado precisamente porque ISTECHERROR != "Y".
    /// Fontes: oracle SC-BSCENVPC-016 (decisions[1].ramo = "No", tipo = "OTHERWISE"),
    ///         glossário POC_Epat.yaml — ISTECHERROR: "N" = sem erro técnico.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="swQRetryCount">Valor corrente de SW_QRETRYCOUNT (motor).</param>
    public static void ApplySetTechnicalError(ProcessExecutionContext ctx, long swQRetryCount)
    {
        // Documenta o esgotamento das retentativas de motor.
        // Não escreve ISTECHERROR = "Y": o gateway Tech Error (nó 10) tomará o ramo "No".
        ctx.STERRORCODE = $"SW_QRETRYCOUNT={swQRetryCount}";
        ctx.STERRORDESC = $"Motor: retentativas esgotadas (SW_QRETRYCOUNT={swQRetryCount} >= MAXRETRIES={ctx.MAXRETRIES})";
    }

    // ── Condições de gateway (topologia como dado) ────────────────────────────

    /// <summary>
    /// Nó 10 — gateway Tech Error (_qIDupF6BEfGBBLgT-R5iuw).
    /// Alcançado por REGRESSO (aresta explícita — não existe no XPDL).
    /// Ramo "Yes":  ISTECHERROR == "Y"  → caminho de erro técnico (fora deste segmento).
    /// Ramo "No" (OTHERWISE): ISTECHERROR != "Y" → App Error (nó 11).
    /// </summary>
    public static bool IsTechError(ProcessExecutionContext ctx)
        => ctx.ISTECHERROR == "Y";

    /// <summary>
    /// Nó 11 — gateway App Error (_qIDuo16BEfGBBLgT-R5iuw).
    /// Ramo "Yes":  ISAPPERROR == "Y"  → caminho de erro de aplicação (fora deste segmento).
    /// Ramo "No" (OTHERWISE): ISAPPERROR != "Y" → Done - Success (nó 12).
    /// </summary>
    public static bool IsAppError(ProcessExecutionContext ctx)
        => ctx.ISAPPERROR == "Y";
}
