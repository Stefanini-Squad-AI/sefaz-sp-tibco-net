#nullable enable

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

// BUILD-DEAT0050-seg012 — scriptTask Controlar Data (_lrer_lqhEfG5K7mY0I3I6w)
// Fonte XPDL: linha 4025
// Classificação: eRegraDeNegocio=false, efeito=tecnico
// Regra: RI-script-DEAT0050-ControlarData
//
// O script faz: DATACONTROLE = PRAZODEFESA
// Após o timer Aguarda Defesa disparar, o processo memoriza o prazo pelo qual já esperou.
// DATACONTROLE é FieldValue<DateOnly> tri-estado (SW_NA = nunca esperou).
// A escrita usa FieldValue<DateOnly>.Of(...) — o estado passa de SW_NA para HasValue.

namespace SefazSp.Epat.Application.Execution.Deat0050;

/// <summary>
/// Passo Controlar Data do DEAT0050 — escreve
/// <c>DATACONTROLE = PRAZODEFESA</c> após o timer Aguarda Defesa disparar.
///
/// Esta escrita transita DATACONTROLE de <see cref="FieldState.IsNotAvailable"/> (SW_NA)
/// para <see cref="FieldState.HasValue"/>, registando o prazo pelo qual o processo já esperou.
/// O gateway seguinte compara DATACONTROLE com PRAZODEFESA: se iguais, o ciclo termina.
///
/// Adaptador da camada anticorrupção: a atribuição usa <see cref="FieldValue{T}.Of"/>
/// conforme a decisão ratificada em gaps.iprocess-builtin (shim-tri-state).
/// </summary>
public static class ControlarDataStep
{
    /// <summary>
    /// Executa o script Controlar Data: <c>DATACONTROLE = PRAZODEFESA</c>.
    /// </summary>
    public static void Execute(AiimCase caseData)
    {
        caseData.DATACONTROLE = FieldValue<DateOnly>.Of(caseData.PRAZODEFESA);
    }
}
