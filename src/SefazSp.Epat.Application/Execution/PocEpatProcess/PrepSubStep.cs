#nullable enable

// Card: BUILD-POCEPATPROCESS-seg053
// scriptTask 14 — 'prepSub' (_zE3XeV6JEfGBBLgT-R5iuw)
//
// Envelope técnico para prepSub. Invoca PrepSubRule.Apply() e trata STATUS_CODE/retry counters.
// Divisão eRegraDeNegocio: lógica de domínio em PrepSubRule, envelope técnico aqui.
//
// naoSabemos: STATUS_CODE e contadores de retry não estão declarados no pacote.

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Rules.PocEpatProcess;

namespace SefazSp.Epat.Application.Execution.PocEpatProcess;

/// <summary>
/// Envelope técnico do scriptTask 'prepSub'
/// (<c>_zE3XeV6JEfGBBLgT-R5iuw</c>) do processo POC_EpatProcess.
///
/// Invoca a regra de domínio <see cref="PrepSubRule"/> e trata aspectos técnicos
/// como STATUS_CODE e contadores de retry.
///
/// <para>
/// <b>Divisão de responsabilidades:</b>
/// <list type="bullet">
///   <item>Lógica de domínio (colecção de peças, contagem de intimados) → <see cref="PrepSubRule"/>.</item>
///   <item>Envelope técnico (STATUS_CODE, retry) → <see cref="PrepSubStep"/>.</item>
/// </list>
/// </para>
/// </summary>
public sealed class PrepSubStep
{
    /// <summary>
    /// Executa o passo 'prepSub': invoca a regra de domínio e trata aspectos técnicos.
    ///
    /// <para>
    /// RI-script-POC_EpatProcess-prepSub.
    /// STATUSSUBPROC é definido por <see cref="PrepSubRule.Apply"/>.
    /// </para>
    /// </summary>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    public static void Apply(AiimCase aiimCase)
    {
        // Technical envelope: invoke domain rule, then handle STATUS_CODE/retry counters
        PrepSubRule.Apply(aiimCase);
        // STATUS_CODE and retry counters: naoSabemos — exact values not declared in package
        // STATUSSUBPROC is set by PrepSubRule.Apply
    }
}
