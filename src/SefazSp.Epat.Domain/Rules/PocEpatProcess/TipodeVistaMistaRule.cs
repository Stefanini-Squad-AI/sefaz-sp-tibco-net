#nullable enable

// Card: BUILD-POCEPATPROCESS-seg053
// Gateway 10 — 'Tipo de Vista Mista?' (_CtQ7AlqPEfG5K7mY0I3I6w)
//
// Regra: RI-transition-POC_EpatProcess-TipodeVistaMista
// Expressão XPDL: TIPOVISTAS == 'MISTA';
//
// Decisão NOEQ-iprocess-builtin → shim-tri-state:
//   TIPOVISTAS usa FieldValue<string> — SW_NA é estado distinto.

using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Domain.Rules.PocEpatProcess;

/// <summary>
/// Regra de transição do gateway 'Tipo de Vista Mista?'
/// (<c>_CtQ7AlqPEfG5K7mY0I3I6w</c>) do processo POC_EpatProcess.
///
/// Avalia se o caso deve seguir pelo caminho de vista mista.
/// Função pura: não depende de relógio, I/O nem estado externo.
/// </summary>
public static class TipodeVistaMistaRule
{
    /// <summary>
    /// Identificador da regra de instância — invariante: não renomear.
    /// </summary>
    public const string RuleId = "RI-transition-POC_EpatProcess-TipodeVistaMista";

    /// <summary>
    /// Avalia se o caso segue para o caminho de vista mista.
    ///
    /// <para>
    /// Expressão legado (XPDL):
    /// <code>TIPOVISTAS == 'MISTA'</code>
    /// </para>
    ///
    /// <para>
    /// Comportamento tri-state:
    /// <list type="bullet">
    ///   <item>HasValue "MISTA" → <see langword="true"/> (caminho mista)</item>
    ///   <item>IsNotAvailable (SW_NA) ou Empty → <see langword="false"/> (outro caminho)</item>
    ///   <item>HasValue outro valor → <see langword="false"/></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="aiimCase">Estado de negócio do caso.</param>
    /// <returns>
    /// <see langword="true"/> se o fluxo deve seguir pelo caminho de vista mista;
    /// <see langword="false"/> caso contrário.
    /// </returns>
    public static bool Evaluate(AiimCase aiimCase)
    {
        // RI-transition-POC_EpatProcess-TipodeVistaMista
        // Decisão shim-tri-state: só HasValue "MISTA" dispara true.
        return aiimCase.TIPOVISTAS.Match(
            hasValue:      v => string.Equals(v, "MISTA", StringComparison.Ordinal),
            notAvailable:  () => false,
            empty:         () => false);
    }
}
