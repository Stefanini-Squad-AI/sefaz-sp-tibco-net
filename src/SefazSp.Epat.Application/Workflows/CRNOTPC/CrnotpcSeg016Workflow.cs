#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.CRNOTPC;
using SefazSp.Epat.Application.UseCases.CRNOTPC;
using SefazSp.Epat.Application.Workflows.ServiceTemplate;

namespace SefazSp.Epat.Application.Workflows.CRNOTPC;

/// <summary>
/// Topologia do segmento 016 do processo CRNOTPC: de 'CriaNotificacao'
/// a 'Done - Bail' (passos 8–20 do cenário SC-CRNOTPC-009, segmento de ordem 1).
///
/// Trata 13 nos:
///   1  serviceTask  _NcJxMF9KEfGqPfX31TKC3w  CriaNotificacao                  (entrouPor=fluxo)
///   2  gateway      _NcJxLl9KEfGqPfX31TKC3w  gateway _NcJxLl9KEfGqPfX31TKC3w (entrouPor=fluxo)
///   3  scriptTask   _NcJxLV9KEfGqPfX31TKC3w  Set App Error                    (entrouPor=fluxo)
///   4  gateway      _NcJxL19KEfGqPfX31TKC3w  gateway _NcJxL19KEfGqPfX31TKC3w (entrouPor=fluxo)
///   5  endEvent     _NcJxK19KEfGqPfX31TKC3w  endEvent _NcJxK19KEfGqPfX31TKC3w(entrouPor=fluxo)
///   6  gateway      _NcJJ8V9KEfGqPfX31TKC3w  Tech Error                       (entrouPor=regresso)
///   7  gateway      _NcJJ8F9KEfGqPfX31TKC3w  App Error                        (entrouPor=fluxo)
///   8  gateway      _NcJJ7V9KEfGqPfX31TKC3w  More Retries                     (entrouPor=fluxo)
///   9  gateway      _NcJw8V9KEfGqPfX31TKC3w  gateway _NcJw8V9KEfGqPfX31TKC3w (entrouPor=fluxo)
///  10  userTask     _NcJJ6V9KEfGqPfX31TKC3w  Manipular Excecao                (entrouPor=fluxo)
///  11  gateway      _NcJJ419KEfGqPfX31TKC3w  Manually Fixed                   (entrouPor=fluxo)
///  12  gateway      _NcJJ6F9KEfGqPfX31TKC3w  Try Again                        (entrouPor=fluxo)
///  13  endEvent     _NcJJ5l9KEfGqPfX31TKC3w  Done - Bail                      (entrouPor=fluxo)
///
/// Nos sem transicao XPDL — escritos como arestas explicitas (decisao NOEQ-link-goto, flatten-edge):
///   - Ordem 6  (_NcJJ8V9KEfGqPfX31TKC3w, regresso): aresta de retorno explicita desde o fim
///     do ActivitySet (endEvent _NcJxK19KEfGqPfX31TKC3w) de volta ao escopo MAIN. Escrita
///     explicitamente como queda implicita no fluxo .NET — nao existe como transicao no XPDL.
/// </summary>
public sealed class CrnotpcSeg016Workflow : IServiceRetryTemplate
{
    private readonly IEpatServices _services;
    private readonly ManipularExcecaoUseCase _manipularExcecao;

    public CrnotpcSeg016Workflow(IEpatServices services, ManipularExcecaoUseCase manipularExcecao)
    {
        _services         = services;
        _manipularExcecao = manipularExcecao;
    }

    // ── Molde de serviço (IServiceRetryTemplate) ────────────────────────────
    // As duas fases abaixo são a MESMA lógica dos nós 1–13; RunAsync compõe-nas.

    /// <inheritdoc />
    public string ProcessKey => "CRNOTPC";

    /// <inheritdoc />
    public void InitializeContext(ProcessExecutionContext ctx, string? processId)
    {
        // Prólogo do subprocesso (segmento 028): SetParameters + Start Loop + Start TX.
        CrnotpcSeg028Steps.ApplySetParameters(ctx, processId);
        CrnotpcSeg028Steps.ApplyStartLoop(ctx);
        CrnotpcSeg028Steps.ApplyStartTx(ctx);
    }

    /// <summary>
    /// Fase 1 — nós 1–8: CriaNotificacao → gateways de erro → More Retries.
    /// Devolve Success / NonAppError, ou RequiresOperator quando as retentativas esgotam.
    /// </summary>
    public async Task<ServiceCallOutcome> RunUntilOperatorAsync(
        AiimCaseRef caseRef, ProcessExecutionContext ctx, long swQRetryCount, CancellationToken ct)
    {
        _ = swQRetryCount; // CRNOTPC não tem gateway Check Retries SW_QRETRYCOUNT.

        // ── Ponto de reingresso do laço de retry (nó 1) ──────────────────────
        CriaNotificacaoEntry:

        // ── Nó 1: serviceTask 'CriaNotificacao' (_NcJxMF9KEfGqPfX31TKC3w) ─────
        var envelope = await _services.CriarnotificacoesaiimAsync(caseRef, ct);
        CrnotpcSeg016Steps.MapServiceEnvelopeToContext(ctx, envelope);

        // ── Nó 2: gateway — "A chamada a CriaNotificacao foi bem sucedida?" ───
        if (!CrnotpcSeg016Steps.IsAppError(ctx))
            return ServiceCallOutcome.Success;

        // ── Nó 3: scriptTask 'Set App Error' ─────────────────────────────────
        CrnotpcSeg016Steps.SetAppError(ctx, envelope);

        // ── Nós 6/7: gateways Tech Error / App Error ─────────────────────────
        if (!CrnotpcSeg016Steps.IsAppErrorFlag(ctx))
            return ServiceCallOutcome.NonAppError;

        // ── Nó 8: gateway 'More Retries' — NUMAPPRETRIES < MAXRETRIES ─────────
        if (CrnotpcSeg016Steps.HasMoreRetries(ctx))
            goto CriaNotificacaoEntry;

        // Retentativas esgotadas → tarefa humana 'Manipular Excecao'.
        return ServiceCallOutcome.RequiresOperator;
    }

    /// <summary>
    /// Fase 2 — nós 11–13: aplica a decisão já gravada em ctx.OUTCOME.
    /// </summary>
    public OperatorDecisionOutcome ApplyOperatorDecision(ProcessExecutionContext ctx)
    {
        // ── Nó 11: gateway 'Manually Fixed' — OUTCOME == 'OK'? ───────────────
        if (CrnotpcSeg016Steps.IsManuallyFixed(ctx))
            return OperatorDecisionOutcome.ManuallyFixed;

        // ── Nó 12: gateway 'Try Again' — OUTCOME == 'R'? ─────────────────────
        if (CrnotpcSeg016Steps.IsTryAgain(ctx))
            return OperatorDecisionOutcome.TryAgain;

        // ── Nó 13: endEvent 'Done - Bail' ────────────────────────────────────
        return OperatorDecisionOutcome.Bail;
    }

    /// <summary>
    /// Executa o segmento completo, incluindo retentativas e tratamento de excecao.
    /// Retorna o resultado final do segmento.
    /// </summary>
    /// <param name="caseRef">Identidade do caso (correlacao com o legado).</param>
    /// <param name="ctx">Contexto de execucao mutavel partilhado com o resto do subprocesso.</param>
    /// <param name="decideOutcome">
    /// Delegate que representa a interacao humana em 'Manipular Excecao'.
    /// Em producao, suspende o workflow ate o operador submeter o formulario MANEXC.
    /// Em testes, substituido por um valor configurado no cenario.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Resultado do percurso: Sucesso, ErroAplicacaoSemRetentativa, EsgotouRetentativas ou DoneBail.</returns>
    public async Task<CrnotpcSeg016Result> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoResult>> decideOutcome,
        CancellationToken ct)
    {
        // Composição das duas fases do molde — mesmo percurso dos nós 1–13.
        while (true)
        {
            var call = await RunUntilOperatorAsync(caseRef, ctx, swQRetryCount: 0, ct);
            if (call == ServiceCallOutcome.Success)     return CrnotpcSeg016Result.Sucesso;
            if (call == ServiceCallOutcome.NonAppError) return CrnotpcSeg016Result.ErroNaoAplicacao;

            // RequiresOperator — nó 9 (pass-through) + nó 10 (userTask 'Manipular Excecao').
            await _manipularExcecao.ExecuteAsync(caseRef, ctx, decideOutcome, ct);

            switch (ApplyOperatorDecision(ctx))
            {
                case OperatorDecisionOutcome.ManuallyFixed: return CrnotpcSeg016Result.ManuallyFixed;
                case OperatorDecisionOutcome.TryAgain:      continue; // regressa ao início do laço
                default:                                    return CrnotpcSeg016Result.DoneBail;
            }
        }
    }
}

/// <summary>
/// Resultado possivel do percurso do segmento 016 do CRNOTPC.
/// </summary>
public enum CrnotpcSeg016Result
{
    /// <summary>STATUS_CODE == "0": a chamada CriaNotificacao foi bem sucedida.</summary>
    Sucesso,

    /// <summary>ISAPPERROR != "Y" apos Set App Error: erro tecnico nao retentavel.</summary>
    ErroNaoAplicacao,

    /// <summary>OUTCOME == "OK": caso resolvido manualmente pelo operador.</summary>
    ManuallyFixed,

    /// <summary>
    /// OUTCOME != "R" e OUTCOME != "OK" apos Manipular Excecao (OTHERWISE):
    /// encerra em Done - Bail (_NcJJ5l9KEfGqPfX31TKC3w).
    /// </summary>
    DoneBail,
}
