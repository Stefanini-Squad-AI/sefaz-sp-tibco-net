#nullable enable

namespace SefazSp.Epat.Domain.Rules.BSCENVPC;

/// <summary>
/// RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT (XPDL linha 5522)
/// Guarda de transicao no gateway 'Check Retries SW_QRETRYCOUNT'
/// (_qIDu3V6BEfGBBLgT-R5iuw) dentro do ActivitySet de BSCENVPC.
///
/// Expressao original: IPESystemValues.SW_QRETRYCOUNT &lt; MAXRETRIES
/// Quando verdadeiro: ramo 'Stillgood' (continuar para chamada de servico).
/// Quando falso (OTHERWISE): ramo 'Maxretriesexceeded' (ir para Set Technical Error).
///
/// GAP NOEQ-iprocess-builtin (gate humano necessario — BUILTIN-SEMANTICS):
///   SW_QRETRYCOUNT e fornecido pelo motor iProcess como valor de runtime da fila.
///   Em .NET e passado explicitamente como parametro de entrada do workflow.
/// </summary>
public static class CheckRetriesSWQRETRYCOUNTRule
{
    /// <summary>
    /// Avalia a guarda. Devolve <c>true</c> quando ainda ha tentativas disponeis
    /// (ramo 'Stillgood'); <c>false</c> quando o limite foi atingido
    /// (ramo 'Maxretriesexceeded' — OTHERWISE).
    /// </summary>
    public static bool Evaluate(long swQRetryCount, int maxRetries)
    {
        return swQRetryCount < maxRetries;
    }
}
