#nullable enable

// Card: BUILD-POCEPATPROCESS-seg025
// Checklist ordem 3: callActivity _BQIgAF9KEfGqPfX31TKC3w "Criar Notificacao" → CRNOTPC
// Processo: POC_EpatProcess · Etapa: 2

// CRNOTPC não declara xpdExt:ProcessInterface no XPDL (interfaceName=null, resolvedVia=process,
// dynamic=false). O double é uma implementação estática injectada como delegate no workflow.
// Destino sem double configurado falha visivelmente — não herda HaltOnBadSubProcess=false do TIBCO.

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble conduzido por cenário para o callActivity <c>Criar Notificacao</c>
/// (<c>_BQIgAF9KEfGqPfX31TKC3w</c>) do processo POC_EpatProcess.
///
/// O callee é o processo <c>CRNOTPC</c> (CriaNotificacaoAiim), invocado de forma estática
/// (dynamic=false, resolvedVia=process). Não existe xpdExt:ProcessInterface para este processo
/// — o double expõe o mesmo contrato de assinatura (<see cref="ProcessCallResult"/>) que os
/// restantes doubles de subprocesso.
///
/// Configure o cenário antes de executar o teste; uma chamada sem cenário configurado
/// lança excepção imediata e identificável.
/// </summary>
public sealed class CRNOTPCDouble
{
    private ProcessCallResult? _result;

    /// <summary>
    /// Configura o resultado a devolver na próxima chamada a <see cref="ExecuteAsync"/>.
    /// </summary>
    /// <param name="result">Resultado a simular.</param>
    /// <returns><see langword="this"/> para encadeamento fluente.</returns>
    public CRNOTPCDouble WithResult(ProcessCallResult result)
    {
        _result = result;
        return this;
    }

    /// <summary>
    /// Simula a invocação do subprocesso CRNOTPC e devolve o resultado configurado.
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
                $"[CRNOTPCDouble] Nenhum resultado configurado para caseRef={caseRef}. " +
                $"Chame {nameof(WithResult)} antes de invocar. " +
                "O double falha visivelmente — não herda HaltOnBadSubProcess=false do legado TIBCO.");

        return Task.FromResult(_result.Value);
    }

    /// <summary>
    /// Devolve o delegate compatível com a assinatura esperada pelo workflow
    /// <c>PocEpatProcessSeg025Workflow</c>.
    /// </summary>
    public Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> AsDelegate() =>
        (caseRef, ct) => ExecuteAsync(caseRef, ct);
}
