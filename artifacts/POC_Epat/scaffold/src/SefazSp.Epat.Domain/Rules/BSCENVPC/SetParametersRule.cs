#nullable enable

namespace SefazSp.Epat.Domain.Rules.BSCENVPC;

/// <summary>
/// RI-script-BSCENVPC-SetParameters (XPDL linha 5832)
/// Inicializa PROCESS_ID e MAXRETRIES no arranque do processo BSCENVPC.
///
/// GAP NOEQ-iprocess-builtin (gate humano necessario — BUILTIN-SEMANTICS):
///   A construcao de PROCESS_ID usa IPESystemValues.SW_NA (sentinela sem equivalente .NET
///   confirmado) e IPEConversionUtil.STR (semantica de base 0 vs. base 1 por confirmar).
///   Esses builtins nao foram portados: PROCESS_ID e preenchido pelo chamador ou deixado nulo.
///
/// O que e portado neste metodo:
///   if (MAXRETRIES == null) MAXRETRIES = 5;
/// </summary>
public static class SetParametersRule
{
    /// <summary>
    /// Aplica a regra. Devolve o novo valor de MAXRETRIES (5 se ainda nao inicializado).
    /// PROCESS_ID fica fora do escopo da POC — ver GAP NOEQ-iprocess-builtin.
    /// </summary>
    public static int Apply(int maxRetries)
    {
        return maxRetries == 0 ? 5 : maxRetries;
    }
}
