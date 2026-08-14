#nullable enable

// Card: BUILD-CONTROPC-seg045
// AC2 — callActivity 'AtualizaIntimacao' (_-bkw_l6JEfGBBLgT-R5iuw, entrouPor=fluxo)
//       continuaEm: ATZINTPC · resolvidaPor: process · dinamica: false
//
// ATZINTPC não declara xpdExt:ProcessInterface no XPDL (resolvedVia=process, dynamic=false).
// O double é uma implementação estática injectada como delegate no workflow.
// Destino sem double configurado falha visivelmente — não herda HaltOnBadSubProcess=false do TIBCO.

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble conduzido por cenário para o callActivity 'AtualizaIntimacao'
/// (<c>_-bkw_l6JEfGBBLgT-R5iuw</c>) do processo CONTROPC.
///
/// O callee é o processo <c>ATZINTPC</c> (Atualiza Intimação), invocado de forma estática
/// (dynamic=false, resolvedVia=process). Não existe xpdExt:ProcessInterface para este processo
/// — o double expõe o mesmo contrato de assinatura (<see cref="ProcessCallResult"/>) que os
/// restantes doubles de subprocesso estáticos.
///
/// Configure o resultado antes de executar o teste; uma chamada sem resultado configurado
/// lança excepção imediata e identificável.
/// </summary>
public sealed class ATZINTPCDouble
{
    private ProcessCallResult? _result;

    /// <summary>
    /// Configura o resultado a devolver na próxima chamada a <see cref="ExecuteAsync"/>.
    /// </summary>
    /// <param name="result">Resultado a simular.</param>
    /// <returns><see langword="this"/> para encadeamento fluente.</returns>
    public ATZINTPCDouble WithResult(ProcessCallResult result)
    {
        _result = result;
        return this;
    }

    /// <summary>
    /// Simula a invocação do subprocesso ATZINTPC e devolve o resultado configurado.
    /// Lança <see cref="InvalidOperationException"/> se nenhum resultado tiver sido configurado.
    /// </summary>
    /// <param name="caseRef">Referência do caso.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O <see cref="ProcessCallResult"/> pré-configurado.</returns>
    public Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_result is null)
            throw new InvalidOperationException(
                $"[ATZINTPCDouble] Nenhum resultado configurado para caseRef={caseRef}. " +
                $"Chame {nameof(WithResult)} antes de invocar. " +
                "O double falha visivelmente — não herda HaltOnBadSubProcess=false do legado TIBCO.");

        return Task.FromResult(_result.Value);
    }

    /// <summary>
    /// Devolve o delegate compatível com a assinatura esperada pelo workflow CONTROPC
    /// que invoca o callActivity 'AtualizaIntimacao' (<c>_-bkw_l6JEfGBBLgT-R5iuw</c>).
    /// </summary>
    public Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> AsDelegate() =>
        (caseRef, ct) => ExecuteAsync(caseRef, ct);
}
