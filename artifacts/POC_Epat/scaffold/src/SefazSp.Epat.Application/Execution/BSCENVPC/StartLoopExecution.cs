#nullable enable

using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Execution.BSCENVPC;

/// <summary>
/// Execucao do passo 'Start Loop' (_qIDul16BEfGBBLgT-R5iuw, scriptTask, MAIN).
/// Inicializa os campos do envelope tecnico no inicio de cada iteracao do laco de retry.
///
/// Script original (XPDL linha 5894):
///   if (NUMAPPRETRIES==null) { NUMAPPRETRIES=0; } else { NUMAPPRETRIES=NUMAPPRETRIES+1; }
///   ISAPPERROR='N'; ISTECHERROR='N'; OUTCOME='OK';
///   DATETIME = IPEConversionUtil.DATESTR(IPESystemValues.SW_DATE);
///
/// GAP NOEQ-iprocess-builtin (gate humano necessario — BUILTIN-SEMANTICS):
///   IPEConversionUtil.DATESTR e IPESystemValues.SW_DATE nao tem equivalente .NET confirmado.
///   DATETIME e deixado sem valor nesta portagem.
/// </summary>
public static class StartLoopExecution
{
    /// <summary>
    /// Aplica o script ao contexto.
    /// </summary>
    public static void Apply(ProcessExecutionContext ctx)
    {
        if (ctx.NUMAPPRETRIES == 0)
        {
            ctx.NUMAPPRETRIES = 0;
        }
        else
        {
            ctx.NUMAPPRETRIES = ctx.NUMAPPRETRIES + 1;
        }

        ctx.ISAPPERROR = "N";
        ctx.ISTECHERROR = "N";
        ctx.OUTCOME = "OK";
        // NOEQ-iprocess-builtin: IPEConversionUtil.DATESTR(SW_DATE) — gate humano necessario.
        // ctx.DATETIME nao e preenchido nesta portagem.
    }
}
