#nullable enable

// Card: BUILD-POCEPATPROCESS-seg052
// Checklist ordem 5: userTask _tbOD4FqPEfG5K7mY0I3I6w "Realizar Atividade Vista Mista"
// Processo: POC_EpatProcess · Etapa: 5

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.UseCases.PocEpatProcess;

/// <summary>
/// Caso de uso para a userTask 'Realizar Atividade Vista Mista'
/// (<c>_tbOD4FqPEfG5K7mY0I3I6w</c>) do processo POC_EpatProcess.
///
/// <para>
/// O formulário 'Realizar Atividade Vista Mista' é apresentado ao responsável do caso;
/// as regras do code-behind da tela que decidem o desfecho do processo residem aqui,
/// não na camada de apresentação.
/// </para>
/// </summary>
public sealed class RealizarAtividadeVistaMistaUseCase
{
    /// <summary>
    /// Aguarda a submissão do formulário 'Realizar Atividade Vista Mista'
    /// e aplica as regras de code-behind ao caso.
    /// </summary>
    /// <param name="caseRef">Referência do caso (para correlação com a UI).</param>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    /// <param name="waitForSubmit">
    /// Delegate de interacção humana: suspende o workflow até o responsável submeter
    /// o formulário 'Realizar Atividade Vista Mista'.
    /// Devolve o <see cref="RealizarAtividadeVistaMistaFormData"/> preenchido.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task<RealizarAtividadeVistaMistaFormData>> waitForSubmit,
        CancellationToken ct)
    {
        // ── userTask: aguarda submissão do formulário 'Realizar Atividade Vista Mista' ──
        await waitForSubmit(caseRef, ct).ConfigureAwait(false);
    }
}

/// <summary>
/// Dados submetidos pelo utilizador no formulário 'Realizar Atividade Vista Mista'.
/// </summary>
public sealed record RealizarAtividadeVistaMistaFormData();
