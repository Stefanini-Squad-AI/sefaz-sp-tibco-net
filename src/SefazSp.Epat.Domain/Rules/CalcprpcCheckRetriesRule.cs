#nullable enable

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-transition-CALCPRPC-CheckRetriesSWQRETRYCOUNT
/// Regra de transição do gateway "Check Retries SW_QRETRYCOUNT" no processo CALCPRPC.
///
/// Expressão legada: IPESystemValues.SW_QRETRYCOUNT &lt; MAXRETRIES
/// Ramo verdadeiro ("Stillgood"): prossegue para CalcularPrazo.
/// Ramo falso: retentativas do motor esgotadas.
///
/// SW_QRETRYCOUNT é o contador de falhas de entrega da fila, controlado pelo runtime
/// iProcess e lido, nunca escrito, pelo processo. Valor numérico simples; não usa SW_NA.
/// Decisão NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06).
/// </summary>
public static class CalcprpcCheckRetriesRule
{
    /// <summary>
    /// Avalia se o número de tentativas do motor ainda está dentro do limite.
    /// </summary>
    /// <param name="swQRetryCount">
    ///   Valor de IPESystemValues.SW_QRETRYCOUNT fornecido pelo runtime — nunca escrito pelo processo.
    /// </param>
    /// <param name="maxRetries">Tecto de tentativas (MAXRETRIES), inicializado no SetParameters.</param>
    /// <returns>
    ///   <c>true</c>  → ramo "Stillgood" → CalcularPrazo<br/>
    ///   <c>false</c> → retentativas do motor esgotadas
    /// </returns>
    public static bool IsStillgood(long swQRetryCount, int maxRetries) =>
        swQRetryCount < maxRetries;
}
