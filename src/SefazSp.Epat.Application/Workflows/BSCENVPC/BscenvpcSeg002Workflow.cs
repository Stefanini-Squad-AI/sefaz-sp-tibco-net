#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.UseCases.BSCENVPC;

namespace SefazSp.Epat.Application.Workflows.BSCENVPC;

/// <summary>
/// Topologia do segmento 2 do processo BSCENVPC:
/// de 'Busca Envolvidos Vista Por AIIM' ate 'Done - Bail' (13 nos, passos 8-20 do SC-BSCENVPC-009).
///
/// AC5 — REGRESSO EXPLÍCITO:
///   O no 'Tech Error' (_qIDupF6BEfGBBLgT-R5iuw) e alcancado por REGRESSO.
///   Esta aresta NAO existe como transicao no XPDL; esta escrita explicitamente abaixo
///   como a transicao que liga o endEvent do ActivitySet (_qIDu316BEfGBBLgT-R5iuw)
///   de volta ao gateway Tech Error no scope MAIN.
///   Sem esta aresta o percurso de erro tecnico nunca seria activado.
/// </summary>
public sealed class BscenvpcSeg002Workflow
{
    // Node IDs — never rename these
    public const string NodeBuscaEnvolvidosVistaPorAiim = "_qIDu5F6BEfGBBLgT-R5iuw"; // ordem 1, serviceTask
    public const string NodeGatewayDecisaoServico = "_qIDu4l6BEfGBBLgT-R5iuw"; // ordem 2, gateway
    public const string NodeSetAppError = "_qIDu4V6BEfGBBLgT-R5iuw"; // ordem 3, scriptTask
    public const string NodeGatewayAposSetAppError = "_qIDu416BEfGBBLgT-R5iuw"; // ordem 4, gateway
    public const string NodeEndEventInterno = "_qIDu316BEfGBBLgT-R5iuw"; // ordem 5, endEvent (ActivitySet)
    public const string NodeTechError = "_qIDupF6BEfGBBLgT-R5iuw"; // ordem 6, gateway — entrouPor=REGRESSO
    public const string NodeAppError = "_qIDuo16BEfGBBLgT-R5iuw"; // ordem 7, gateway
    public const string NodeMoreRetries = "_qIDuoF6BEfGBBLgT-R5iuw"; // ordem 8, gateway
    public const string NodeGatewayAposMoreRetries = "_qIDupl6BEfGBBLgT-R5iuw"; // ordem 9, gateway
    public const string NodeManipularExcecao = "_qIDunF6BEfGBBLgT-R5iuw"; // ordem 10, userTask
    public const string NodeManuallyFixed = "_qIDull6BEfGBBLgT-R5iuw"; // ordem 11, gateway
    public const string NodeTryAgain = "_qIDum16BEfGBBLgT-R5iuw"; // ordem 12, gateway
    public const string NodeDoneBail = "_qIDumV6BEfGBBLgT-R5iuw"; // ordem 13, endEvent

    private readonly IEpatServices _services;
    private readonly ManipularExcecaoUseCase _manipularExcecao;

    public BscenvpcSeg002Workflow(IEpatServices services, ManipularExcecaoUseCase manipularExcecao)
    {
        _services = services;
        _manipularExcecao = manipularExcecao;
    }

    /// <summary>
    /// Percorre o segmento 2 de 'Busca Envolvidos Vista Por AIIM' ate 'Done - Bail'.
    /// Retorna o trace dos node IDs visitados, em ordem.
    /// </summary>
    /// <param name="caseRef">Identidade do caso.</param>
    /// <param name="ctx">Contexto de execucao (mutado em linha).</param>
    /// <param name="operatorOutcome">
    ///   Decisao do operador quando ManipularExcecao e atingido: 'OK' ou 'R'.
    ///   Null se o caminho nao chega a ManipularExcecao.
    /// </param>
    public async Task<WorkflowTrace> RunSegmentAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        string? operatorOutcome = null,
        CancellationToken ct = default)
    {
        var visited = new List<string>();

        // ── Passo 8, ordem 1: serviceTask 'Busca Envolvidos Vista Por AIIM' ──────────
        visited.Add(NodeBuscaEnvolvidosVistaPorAiim);
        ServiceEnvelope envelope;
        try
        {
            envelope = await _services.BuscarvistasativasporaiimAsync(caseRef, ct);
            ctx.STATUS_CODE  = envelope.STATUS_CODE;
            ctx.STERRORCODE  = envelope.STERRORCODE;
            ctx.STERRORDESC  = envelope.STERRORDESC;
            ctx.ISTECHERROR  = "N";
        }
        catch
        {
            // Falha tecnica (infraestrutura): o envelope sintetico marca erro tecnico.
            ctx.ISTECHERROR = "Y";
            ctx.STATUS_CODE = "ERR";
            envelope = new ServiceEnvelope("ERR", null, null);
        }

        // ── Passo 9, ordem 2: gateway — 'A chamada foi bem sucedida?' ────────────────
        visited.Add(NodeGatewayDecisaoServico);

        if (ctx.STATUS_CODE != "0") // ramo AppError: STATUS_CODE != "0"
        {
            // ── Passo 10, ordem 3: scriptTask 'Set App Error' ────────────────────────
            visited.Add(NodeSetAppError);
            BscenvpcExecutionSteps.SetAppError(ctx, envelope);

            // ── Passo 11, ordem 4: gateway apos Set App Error ────────────────────────
            visited.Add(NodeGatewayAposSetAppError);

            // ── Passo 12, ordem 5: endEvent interno do ActivitySet ───────────────────
            // O subprocesso 'Control System Task Call' conclui; fluxo regressa ao MAIN.
            visited.Add(NodeEndEventInterno);

            // ── AC5 — REGRESSO EXPLÍCITO (aresta nao existente no XPDL) ─────────────
            // Passo 13, ordem 6: gateway 'Tech Error' — entrouPor=regresso
            visited.Add(NodeTechError);

            // Tech Error gateway: ramo 'No' (OTHERWISE) → App Error
            // ── Passo 14, ordem 7: gateway 'App Error' ───────────────────────────────
            visited.Add(NodeAppError);

            if (ctx.ISAPPERROR == "Y") // ISAPPERROR set by SetAppError
            {
                // ── Passo 15, ordem 8: gateway 'More Retries' ────────────────────────
                visited.Add(NodeMoreRetries);

                bool moreRetries = ctx.NUMAPPRETRIES < ctx.MAXRETRIES;
                if (!moreRetries) // ramo 'No' → ManipularExcecao
                {
                    // ── Passo 16, ordem 9: gateway routing ───────────────────────────
                    visited.Add(NodeGatewayAposMoreRetries);

                    // ── Passo 17, ordem 10: userTask 'Manipular Excecao' ─────────────
                    visited.Add(NodeManipularExcecao);
                    if (operatorOutcome is not null)
                        _manipularExcecao.RecordOutcome(ctx, operatorOutcome);

                    // ── Passo 18, ordem 11: gateway 'Manually Fixed' ─────────────────
                    visited.Add(NodeManuallyFixed);

                    if (_manipularExcecao.IsManuallyFixed(ctx))
                    {
                        // Operador corrigiu: processo encerra (nao por Done-Bail)
                        return new WorkflowTrace(visited);
                    }

                    // ramo 'No' (OTHERWISE) → Try Again
                    // ── Passo 19, ordem 12: gateway 'Try Again' ──────────────────────
                    visited.Add(NodeTryAgain);

                    if (_manipularExcecao.IsTryAgain(ctx))
                    {
                        // Operador quer repetir: retorna sem Done-Bail (loop externo)
                        return new WorkflowTrace(visited);
                    }

                    // ramo 'No' (OTHERWISE) → Done - Bail
                    // ── Passo 20, ordem 13: endEvent 'Done - Bail' ───────────────────
                    visited.Add(NodeDoneBail);
                }
                // ramo 'Yes' de More Retries: loop de retry (passo nao cobertoaqui)
            }
            // App Error gateway ramo 'No': outro desfecho (nao AppError)
        }
        // Gateway ordem 2 ramo sucesso: fluxo continua normalmente (fora deste segmento)

        return new WorkflowTrace(visited);
    }
}

/// <summary>Trace ordenado dos node IDs visitados durante o percurso.</summary>
public sealed record WorkflowTrace(IReadOnlyList<string> VisitedNodes);
