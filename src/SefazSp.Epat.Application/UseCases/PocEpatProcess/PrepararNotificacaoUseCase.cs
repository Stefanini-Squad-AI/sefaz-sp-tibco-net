#nullable enable

// Card: BUILD-POCEPATPROCESS-seg027
// Checklist ordem 1: userTask _sfwu-VqUEfG5K7mY0I3I6w "Preparar Notificacao"
// Processo: POC_EpatProcess · Etapa: 2

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.UseCases.PocEpatProcess;

/// <summary>
/// Caso de uso para a userTask 'Preparar Notificacao' (<c>_sfwu-VqUEfG5K7mY0I3I6w</c>).
///
/// O fiscal de renda preenche e submete o formulário de notificação do AIIM.
/// O desfecho do processo é decidido aqui, na camada de aplicação, não na apresentação.
///
/// Não há regra de code-behind (<c>RI-formScript-*</c>) declarada no
/// <c>rule-catalogue.json</c> para esta tarefa: a submissão do formulário é o
/// único efeito observável.
///
/// Segmento: SC-POC_EpatProcess-017 · passo 3 · ordemNaJornada=1 · etapa 2.
/// </summary>
public sealed class PrepararNotificacaoUseCase
{
    /// <summary>
    /// Aguarda a submissão do formulário 'Preparar Notificacao' pelo fiscal de renda.
    /// </summary>
    /// <param name="caseRef">Referência do caso (para correlação com a UI).</param>
    /// <param name="aiimCase">Estado de negócio do caso — reservado para extensão futura caso
    /// seja identificada uma regra de code-behind nesta tarefa.</param>
    /// <param name="waitForSubmit">
    /// Delegate que representa a interacção humana: suspende o workflow até o
    /// fiscal submeter o formulário 'Preparar Notificacao'.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task> waitForSubmit,
        CancellationToken ct)
    {
        // ── userTask: aguarda submissão do formulário "Preparar Notificacao" ──
        // nodeId: _sfwu-VqUEfG5K7mY0I3I6w · entrouPor: fluxo
        // Nenhuma regra RI-formScript-* identificada no rule-catalogue para esta tarefa.
        await waitForSubmit(caseRef, ct).ConfigureAwait(false);
    }
}
