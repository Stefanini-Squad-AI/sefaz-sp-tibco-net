#nullable enable

// Card: BUILD-POCEPATPROCESS-seg006
// Checklist ordem 1: userTask _xWNLe1qSEfG5K7mY0I3I6w "Finalizar AIIM"
// Processo: POC_EpatProcess · Etapas: 1, 2

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.UseCases.PocEpatProcess;

/// <summary>
/// Caso de uso para a userTask 'Finalizar AIIM' (<c>_xWNLe1qSEfG5K7mY0I3I6w</c>).
///
/// Quando o AFR submete o formulário, a regra de code-behind
/// <c>RI-formScript-POC_EpatProcess-FinalizarAIIM</c> executa:
/// <list type="bullet">
///   <item><c>AFR = IPEStarterUtil.GETATTRIBUTE("Name");</c></item>
///   <item><c>CNTINSTANCIASUF = 0;</c></item>
/// </list>
///
/// O literal <c>"Name"</c> é portado sem interpretação: o pacote não declara o seu
/// significado — naoSabemos, conforme rule-catalogue.json.
/// </summary>
public sealed class FinalizarAiimUseCase
{
    /// <summary>
    /// Aguarda a submissão do formulário e aplica a regra
    /// <c>RI-formScript-POC_EpatProcess-FinalizarAIIM</c> ao caso.
    /// </summary>
    /// <param name="caseRef">Referência do caso (para correlação com a UI).</param>
    /// <param name="aiimCase">Estado de negócio mutável do caso — <c>AFR</c> e <c>CNTINSTANCIASUF</c> são actualizados aqui.</param>
    /// <param name="waitForSubmit">
    /// Delegate que representa a interacção humana: suspende o workflow até o AFR
    /// submeter o formulário "Finalizar AIIM". Devolve uma função
    /// <c>getAttribute(attributeName)</c> que encapsula a chamada literal a
    /// <c>IPEStarterUtil.GETATTRIBUTE</c> — o significado do argumento <c>"Name"</c>
    /// não está declarado no pacote; a chamada é reproduzida literalmente.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task<Func<string, string>>> waitForSubmit,
        CancellationToken ct)
    {
        // ── userTask: aguarda submissão do formulário "Finalizar AIIM" ──────────
        var getAttribute = await waitForSubmit(caseRef, ct).ConfigureAwait(false);

        // ── RI-formScript-POC_EpatProcess-FinalizarAIIM ──────────────────────────
        // Expressão original (POC_Epat.xpdl, linha 2124):
        //   AFR = IPEStarterUtil.GETATTRIBUTE("Name");
        //   CNTINSTANCIASUF = 0;
        // O significado de "Name" não está declarado — portado sem interpretação.
        aiimCase.AFR = getAttribute("Name");
        aiimCase.CNTINSTANCIASUF = 0;
    }
}
