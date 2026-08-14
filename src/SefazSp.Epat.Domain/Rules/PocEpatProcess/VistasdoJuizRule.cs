#nullable enable

// Card: BUILD-POCEPATPROCESS-seg053
// Gateway 3 — 'Vistas do Juiz?' (_CtQ7BFqPEfG5K7mY0I3I6w)
//
// Regra: RI-transition-POC_EpatProcess-VistasdoJuiz
// Expressão XPDL: TIPOVISTAS == 'JUIZ' || TIPOVISTAS == IPESystemValues.SW_NA;
//
// Decisão NOEQ-iprocess-builtin → shim-tri-state (2026-08-06):
//   SW_NA é um terceiro estado distinto de null e de vazio.
//   TIPOVISTAS == SW_NA vai deliberadamente para o caminho do juiz.
//
// Hipótese 3 (DEVE SER PRESERVADA):
//   SW_NA NÃO deve colapsar para null — o fluxo do juiz é intencional.

using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Domain.Rules.PocEpatProcess;

/// <summary>
/// Regra de transição do gateway 'Vistas do Juiz?'
/// (<c>_CtQ7BFqPEfG5K7mY0I3I6w</c>) do processo POC_EpatProcess.
///
/// Avalia se o caso deve seguir pelo caminho de vistas do juiz.
/// Função pura: não depende de relógio, I/O nem estado externo.
///
/// <para>
/// <b>Hipótese 3 (PRESERVADA):</b> <c>TIPOVISTAS == SW_NA</c> vai deliberadamente
/// para o caminho do juiz. O SW_NA <b>NÃO</b> deve colapsar para null —
/// isso mudaria o ramo sem erro de compilação nem teste vermelho.
/// </para>
/// </summary>
public static class VistasdoJuizRule
{
    /// <summary>
    /// Identificador da regra de instância — invariante: não renomear.
    /// </summary>
    public const string RuleId = "RI-transition-POC_EpatProcess-VistasdoJuiz";

    /// <summary>
    /// Avalia se o caso segue para o caminho de vistas do juiz.
    ///
    /// <para>
    /// Expressão legado (XPDL):
    /// <code>TIPOVISTAS == 'JUIZ' || TIPOVISTAS == IPESystemValues.SW_NA</code>
    /// </para>
    ///
    /// <para>
    /// Comportamento tri-state:
    /// <list type="bullet">
    ///   <item>HasValue "JUIZ" → <see langword="true"/> (caminho juiz)</item>
    ///   <item>IsNotAvailable (SW_NA) → <see langword="true"/> (caminho juiz — hipótese 3)</item>
    ///   <item>HasValue outro valor ou Empty → <see langword="false"/> (caminho alternativo)</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="aiimCase">Estado de negócio do caso.</param>
    /// <returns>
    /// <see langword="true"/> se o fluxo deve seguir pelo caminho de vistas do juiz;
    /// <see langword="false"/> caso contrário.
    /// </returns>
    public static bool Evaluate(AiimCase aiimCase)
    {
        // RI-transition-POC_EpatProcess-VistasdoJuiz
        // Decisão shim-tri-state: SW_NA é terceiro estado distinto de null/vazio.
        // Hipótese 3: SW_NA vai deliberadamente para o caminho do juiz.
        return aiimCase.TIPOVISTAS.Match(
            hasValue:      v => string.Equals(v, "JUIZ", StringComparison.Ordinal),
            notAvailable:  () => true,   // SW_NA → caminho juiz (hipótese 3)
            empty:         () => false);
    }
}
