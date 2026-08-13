#nullable enable

// Card: BUILD-POCEPATPROCESS-seg025
// Checklist ordem 1: userTask _sfwu-VqUEfG5K7mY0I3I6w "Preparar Notificacao"
// Processo: POC_EpatProcess · Etapa: 2

// NOEQ-non-interrupting-boundary (medium, DEFERIDO):
//   O nó _sfwu-VqUEfG5K7mY0I3I6w tem um boundary event não-interrompente (deadline DTFIMCQ/HRFIMCQ)
//   que dispara um ramo lateral enquanto a tarefa hospedeira continua executando.
//   Em .NET não há equivalente directo sem execução concorrente explícita.
//   Opção sugerida: ramo lateral paralelo dentro do mesmo escopo (parallel-branch).
//   DECISÃO DO GATE HUMANO — diferido, não ignorado.
//   O valor esperado do oráculo não é afectado por este adiamento.

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.UseCases.PocEpatProcess;

/// <summary>
/// Caso de uso para a userTask 'Preparar Notificacao' (<c>_sfwu-VqUEfG5K7mY0I3I6w</c>).
///
/// A tarefa aguarda a submissão do formulário pelo fiscal antes de avançar para o
/// gateway <c>Corrigir?</c>. Não existe regra de code-behind (formScript) declarada
/// no pacote para este nó — a submissão produz o campo <c>CORRECAO</c> cujo valor
/// é avaliado pela regra de transição <c>RI-transition-POC_EpatProcess-Corrigir</c>.
/// </summary>
public sealed class PrepararNotificacaoUseCase
{
    /// <summary>
    /// Aguarda a submissão do formulário 'Preparar Notificacao'.
    /// </summary>
    /// <param name="caseRef">Referência do caso (para correlação com a UI).</param>
    /// <param name="waitForSubmit">
    /// Delegate que representa a interacção humana: suspende o workflow até o fiscal
    /// submeter o formulário. Ao submeter, o campo <c>CORRECAO</c> em
    /// <paramref name="aiimCase"/> reflecte a escolha do utilizador.
    /// </param>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, AiimCase, CancellationToken, Task> waitForSubmit,
        CancellationToken ct)
    {
        // ── userTask: aguarda submissão do formulário "Preparar Notificacao" ──────
        // O campo CORRECAO é preenchido pelo utilizador no formulário.
        // A regra RI-transition-POC_EpatProcess-Corrigir (CORRECAO == true;) é avaliada
        // no gateway seguinte, não aqui.
        await waitForSubmit(caseRef, aiimCase, ct).ConfigureAwait(false);
    }
}
