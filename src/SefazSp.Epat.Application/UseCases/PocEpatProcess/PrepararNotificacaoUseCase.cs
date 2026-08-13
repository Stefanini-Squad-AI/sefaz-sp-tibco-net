#nullable enable

// Card: BUILD-POCEPATPROCESS-seg026
// Checklist ordem 1: userTask _sfwu-VqUEfG5K7mY0I3I6w "Preparar Notificacao"
// Processo: POC_EpatProcess · Etapa: 2

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.UseCases.PocEpatProcess;

/// <summary>
/// Caso de uso para a userTask 'Preparar Notificacao' (<c>_sfwu-VqUEfG5K7mY0I3I6w</c>).
///
/// O pacote não declara regras de code-behind para esta tarefa — a tarefa humana
/// aguarda a submissão do formulário pelo auditor/fiscal sem side-effects de campo.
/// </summary>
public sealed class PrepararNotificacaoUseCase
{
    /// <summary>
    /// Aguarda a submissão do formulário 'Preparar Notificacao' pelo auditor/fiscal.
    /// </summary>
    /// <param name="caseRef">Referência do caso (para correlação com a UI).</param>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    /// <param name="waitForSubmit">
    /// Delegate que representa a interacção humana: suspende o workflow até o formulário
    /// ser submetido. Devolve <see langword="Task"/> após a submissão.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task> waitForSubmit,
        CancellationToken ct)
    {
        // ── userTask: aguarda submissão do formulário "Preparar Notificacao" ──────
        // Nenhuma regra de code-behind declarada no pacote para esta tarefa —
        // o caso não é mutado; aguarda apenas a acção humana.
        await waitForSubmit(caseRef, ct).ConfigureAwait(false);
    }
}
