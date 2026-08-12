#nullable enable

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

// BUILD-DEAT0050-seg012 — Gateway "Já se esperou pelo prazo em vigor?"
// Regra: RI-transition-DEAT0050-gatewaylrerVqhEfG5K7mY0I3I6w
// Expressão XPDL (linha 4005): DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZODEFESA;
// Decisão: shim-tri-state (gaps.iprocess-builtin) — SW_NA é terceiro estado, distinto de null e de Empty.
// Comportamento: quando verdadeiro → segue para Aguarda Defesa; quando falso → sai do ciclo.

namespace SefazSp.Epat.Application.Workflows.Deat0050;

/// <summary>
/// Regra de transição do gateway DEAT0050 "_lrer_VqhEfG5K7mY0I3I6w".
/// Pergunta: "Já se esperou pelo prazo em vigor?"
/// Resposta verdadeira → ainda não esperou (ou o prazo foi prorrogado) → vai aguardar.
/// Resposta falsa → já esperou pelo prazo actual → sai do ciclo.
///
/// SENTINEL: DATACONTROLE inicia em SW_NA (nunca esperou).
/// O ciclo só termina quando DATACONTROLE == PRAZODEFESA após a espera.
/// </summary>
public static class Deat0050GatewayRules
{
    /// <summary>
    /// RI-transition-DEAT0050-gatewaylrerVqhEfG5K7mY0I3I6w.
    /// Devolve <see langword="true"/> se o timer Aguarda Defesa deve ser (re)armado.
    /// Devolve <see langword="false"/> se o ciclo de prorrogação está concluído.
    /// </summary>
    /// <remarks>
    /// Tradução directa de: <c>DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZODEFESA</c>
    /// O tipo <see cref="FieldValue{T}"/> impõe a decisão para cada um dos três estados:
    ///   HasValue  — comparar com PRAZODEFESA; verdadeiro se diferente.
    ///   NotAvailable (SW_NA) — nunca esperou; verdadeiro sempre.
    ///   Empty     — não preenchido; verdadeiro (equivalente a nunca esperou).
    /// </remarks>
    public static bool DeveAguardarDefesa(AiimCase caseData)
        => caseData.DATACONTROLE.Match(
            hasValue:      dataControle => dataControle != caseData.PRAZODEFESA,
            notAvailable:  () => true,  // SW_NA: nunca esperou — vai aguardar
            empty:         () => true   // não preenchido — vai aguardar
        );
}
