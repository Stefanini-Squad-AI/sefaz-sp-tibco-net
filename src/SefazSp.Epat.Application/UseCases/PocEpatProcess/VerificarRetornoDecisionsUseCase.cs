#nullable enable

// Card: BUILD-POCEPATPROCESS-seg034
// AC6 — userTask 'Verificar Retorno Decisions' (_30jAcFqVEfG5K7mY0I3I6w, entrouPor=fluxo)
//
// A tarefa humana é um caso de uso em Application/UseCases.
// As regras do code-behind das telas decidem o desfecho do processo, não a apresentação.
//
// Campo DESCREGRA: vocabulário resolvido em card content.vocab.
//   DESCREGRA — DescricaoRegra; preenchido no formulário VerificarRetornoDecisions.

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.UseCases.PocEpatProcess;

/// <summary>
/// Caso de uso para a userTask 'Verificar Retorno Decisions'
/// (<c>_30jAcFqVEfG5K7mY0I3I6w</c>) do processo POC_EpatProcess.
///
/// <para>
/// O formulário 'Verificar Retorno Decisions' apresenta o retorno do Decisions ao
/// responsável do caso; este confirma o resultado e preenche <c>DESCREGRA</c>
/// (DescricaoRegra). O desfecho (qual ramo do fluxo seguinte é tomado) é determinado
/// pelas regras do code-behind da tela — não pela apresentação.
/// </para>
/// </summary>
public sealed class VerificarRetornoDecisionsUseCase
{
    /// <summary>
    /// Aguarda a submissão do formulário 'Verificar Retorno Decisions'
    /// e aplica a regra de code-behind ao caso.
    ///
    /// <para>
    /// A regra de code-behind escreve <c>DESCREGRA</c> com a descrição da regra
    /// confirmada pelo utilizador (campo vocabularizado como DescricaoRegra).
    /// </para>
    /// </summary>
    /// <param name="caseRef">Referência do caso (para correlação com a UI).</param>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    /// <param name="waitForSubmit">
    /// Delegate de interacção humana: suspende o workflow até o responsável submeter
    /// o formulário 'Verificar Retorno Decisions'.
    /// Devolve o <see cref="VerificarRetornoDecisionsFormData"/> preenchido.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task<VerificarRetornoDecisionsFormData>> waitForSubmit,
        CancellationToken ct)
    {
        // ── userTask: aguarda submissão do formulário 'Verificar Retorno Decisions' ─
        var formData = await waitForSubmit(caseRef, ct).ConfigureAwait(false);

        // ── code-behind: DESCREGRA = <valor submetido pelo utilizador> ────────
        // Campo vocabularizado: DESCREGRA = DescricaoRegra.
        // O valor vem do formulário — nunca um literal no código.
        aiimCase.DESCREGRA = formData.DescricaoRegra;
    }
}

/// <summary>
/// Dados submetidos pelo utilizador no formulário 'Verificar Retorno Decisions'.
/// </summary>
/// <param name="DescricaoRegra">
/// Descrição da regra aplicada pelo Decisions — campo <c>DESCREGRA</c>.
/// Vocabularizado em card content.vocab como DescricaoRegra.
/// </param>
public sealed record VerificarRetornoDecisionsFormData(
    string DescricaoRegra);
