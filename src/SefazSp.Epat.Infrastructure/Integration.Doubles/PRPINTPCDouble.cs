#nullable enable

// Card: BUILD-POCEPATPROCESS-seg034
// AC3 — callActivity 'Prepara Intimação' (_CI6lyFqREfG5K7mY0I3I6w, entrouPor=fluxo)
//       continuaEm: PRPINTPC · resolvidaPor: process · dinamica: false
//
// PRPINTPC não declara xpdExt:ProcessInterface no XPDL (resolvedVia=process, dynamic=false).
// O double é uma implementação estática injectada como delegate no workflow.
// Destino sem double configurado falha visivelmente — não herda HaltOnBadSubProcess=false do TIBCO.

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble conduzido por cenário para o callActivity 'Prepara Intimação'
/// (<c>_CI6lyFqREfG5K7mY0I3I6w</c>) do processo POC_EpatProcess.
///
/// O callee é o processo <c>PRPINTPC</c> (PrepararIntimacao), invocado de forma estática
/// (dynamic=false, resolvedVia=process). Não existe xpdExt:ProcessInterface para este processo
/// — o double expõe o mesmo contrato de assinatura (<see cref="ProcessCallResult"/>) que os
/// restantes doubles de subprocesso.
///
/// Configure o cenário antes de executar o teste; uma chamada sem cenário configurado
/// lança excepção imediata e identificável.
/// </summary>
public sealed class PRPINTPCDouble
{
    private ProcessCallResult? _result;

    /// <summary>
    /// Configura o resultado a devolver na próxima chamada a <see cref="ExecuteAsync"/>.
    /// </summary>
    /// <param name="result">Resultado a simular.</param>
    /// <returns><see langword="this"/> para encadeamento fluente.</returns>
    public PRPINTPCDouble WithResult(ProcessCallResult result)
    {
        _result = result;
        return this;
    }

    /// <summary>
    /// Simula a invocação do subprocesso PRPINTPC e devolve o resultado configurado.
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
                $"[PRPINTPCDouble] Nenhum resultado configurado para caseRef={caseRef}. " +
                $"Chame {nameof(WithResult)} antes de invocar. " +
                "O double falha visivelmente — não herda HaltOnBadSubProcess=false do legado TIBCO.");

        return Task.FromResult(_result.Value);
    }

    /// <summary>
    /// Devolve o delegate compatível com a assinatura esperada pelo workflow
    /// <c>PocEpatProcessSeg034Workflow</c>.
    /// </summary>
    public Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> AsDelegate() =>
        (caseRef, ct) => ExecuteAsync(caseRef, ct);
}
