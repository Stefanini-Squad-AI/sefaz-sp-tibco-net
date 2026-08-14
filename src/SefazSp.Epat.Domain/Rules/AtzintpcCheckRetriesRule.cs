#nullable enable

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT
/// Regra de transição do gateway "Check Retries SW_QRETRYCOUNT" no processo ATZINTPC.
///
/// Expressão legada: IPESystemValues.SW_QRETRYCOUNT &lt; MAXRETRIES
/// Ramo verdadeiro ("Stillgood"): prossegue para AtualizarIntimacao.
/// Ramo falso: retentativas do motor esgotadas — segue para Set Technical Error.
/// Ramo falso: retentativas do motor esgotadas.
///
/// SW_QRETRYCOUNT é o contador de falhas de entrega da fila, controlado pelo runtime
/// iProcess e lido, nunca escrito, pelo processo. Valor numérico simples; não usa SW_NA.
/// Decisão NOEQ-iprocess-builtin (shim-tri-state, ratificado).
///
/// Invariante: identificadores ATZINTPC, CheckRetriesSWQRETRYCOUNT, SW_QRETRYCOUNT,
/// MAXRETRIES não devem ser renomeados.
/// Card: BUILD-ATZINTPC-seg046 · AC3 · Nó _RNdKFV6PEfGBBLgT-R5iuw
/// Decisão NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06).
///
/// Invariante: identificador do nó _RNdKFV6PEfGBBLgT-R5iuw não deve ser renomeado.
/// Card: BUILD-ATZINTPC-seg043 · AC5
/// </summary>
public static class AtzintpcCheckRetriesRule
{
    /// <summary>
    /// Avalia se o número de tentativas do motor ainda está dentro do limite.
    /// </summary>
    /// <param name="swQRetryCount">
    ///   Valor de IPESystemValues.SW_QRETRYCOUNT fornecido pelo runtime — nunca escrito pelo processo.
    /// </param>
    /// <param name="maxRetries">Tecto de tentativas (MAXRETRIES), inicializado no SetParameters.</param>
    /// <returns>
    ///   <c>true</c>  → ramo "Stillgood" → AtualizarIntimacao<br/>
    ///   <c>false</c> → retentativas do motor esgotadas → Set Technical Error
    /// </returns>
    public static bool IsStillgood(long swQRetryCount, int maxRetries) =>
        swQRetryCount < maxRetries;
}
