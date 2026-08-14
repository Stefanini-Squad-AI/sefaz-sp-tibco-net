#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.UseCases.ATZINTPC;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Workflows.ATZINTPC;

/// <summary>
/// Topologia completa do fluxo ATZINTPC, segmento 042:
/// de "Start Event" (_RNdJyV6PEfGBBLgT-R5iuw) a "Done - Fixed" (_RNdJz16PEfGBBLgT-R5iuw).
///
/// Nós percorridos (checklist, card BUILD-ATZINTPC-seg042):
///   ordem  1 — startEvent      _RNdJyV6PEfGBBLgT-R5iuw  Start Event
///   ordem  2 — scriptTask      _RNdJyl6PEfGBBLgT-R5iuw  SetParameters              entrouPor=fluxo
///   ordem  3 — scriptTask      _RNdJzF6PEfGBBLgT-R5iuw  Start Loop                 entrouPor=fluxo
///   ordem  4 — subProcessScope _RNdJ2l6PEfGBBLgT-R5iuw  Control System Task Call   entrouPor=fluxo
///   ordem  5 — startEvent      _RNdKFl6PEfGBBLgT-R5iuw  (inner startEvent)         entrouPor=DESCIDA (*)
///   ordem  6 — scriptTask      _RNdKFF6PEfGBBLgT-R5iuw  Start TX                   entrouPor=fluxo
///   ordem  7 — gateway         _RNdKFV6PEfGBBLgT-R5iuw  Check Retries SW_QRETRYCOUNT
///   ordem  8 — serviceTask     _RNdKHF6PEfGBBLgT-R5iuw  AtualizarIntimacao
///   ordem  9 — gateway         _RNdKGl6PEfGBBLgT-R5iuw  AppError: STATUS_CODE != "0"
///   ordem 10 — scriptTask      _RNdKGV6PEfGBBLgT-R5iuw  Set App Error
///   ordem 11 — gateway         _RNdKG16PEfGBBLgT-R5iuw  junção de fluxo
///   ordem 12 — endEvent        _RNdKF16PEfGBBLgT-R5iuw  (inner endEvent ActivitySet)
///   ordem 13 — gateway         _RNdJ2V6PEfGBBLgT-R5iuw  Tech Error                 entrouPor=REGRESSO (*)
///   ordem 14 — gateway         _RNdJ2F6PEfGBBLgT-R5iuw  App Error
///   ordem 15 — gateway         _RNdJ1V6PEfGBBLgT-R5iuw  More Retries
///   ordem 16 — gateway         _RNdJ216PEfGBBLgT-R5iuw  junção → Manipular Excecao
///   ordem 17 — userTask        _RNdJ0V6PEfGBBLgT-R5iuw  Manipular Excecao
///   ordem 18 — gateway         _RNdJy16PEfGBBLgT-R5iuw  Manually Fixed
///   ordem 19 — endEvent        _RNdJz16PEfGBBLgT-R5iuw  Done - Fixed
///
/// (*) As arestas DESCIDA (entrada no subProcessScope) e REGRESSO (saída para Tech Error)
///     NÃO existem no XPDL — são escritas explicitamente conforme derived.linkEdges.
///
/// Valores de retorno:
///   "_RNdJ2V6PEfGBBLgT-R5iuw:tech-error" — ramo de erro técnico (ISTECHERROR=="Y")
///   "_RNdJz16PEfGBBLgT-R5iuw"            — Done - Fixed (OUTCOME=="OK")
/// </summary>
public sealed class AtzintpcSeg042Workflow
{
    private readonly IEpatServices _services;
    private readonly ManipularExcecaoAtzintpcUseCase _manipularExcecao;

    public AtzintpcSeg042Workflow(
        IEpatServices services,
        ManipularExcecaoAtzintpcUseCase manipularExcecao)
    {
        _services         = services;
        _manipularExcecao = manipularExcecao;
    }

    /// <summary>
    /// Executa o segmento 042 do processo ATZINTPC desde o Start Event.
    /// </summary>
    /// <param name="caseRef">Referência do caso.</param>
    /// <param name="ctx">Contexto de execução mutável.</param>
    /// <param name="swQRetryCount">
    ///   Valor de IPESystemValues.SW_QRETRYCOUNT fornecido pelo runtime — lido, nunca escrito.
    /// </param>
    /// <param name="decideOutcome">
    ///   Delegate que representa a interação humana na userTask Manipular Excecao.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task<string> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoAtzintpcResult>> decideOutcome,
        CancellationToken ct)
    {
        // ordem 1: startEvent _RNdJyV6PEfGBBLgT-R5iuw — entry point

        // ordem 2: scriptTask _RNdJyl6PEfGBBLgT-R5iuw — SetParameters
        var processId = caseRef.ProcessId;
        AtzintpcExecutionSteps.ApplySetParameters(ctx, processId);

        // ordem 3: scriptTask _RNdJzF6PEfGBBLgT-R5iuw — Start Loop
        AtzintpcExecutionSteps.ApplyStartLoop(ctx);

        // Loop: Control System Task Call → Tech Error → retentativa ou Manipular Excecao
        while (true)
        {
            // ordem 4: subProcessScope _RNdJ2l6PEfGBBLgT-R5iuw — Control System Task Call
            // ordem 5: startEvent _RNdKFl6PEfGBBLgT-R5iuw — DESCIDA (aresta explícita)
            // ordem 6: scriptTask _RNdKFF6PEfGBBLgT-R5iuw — Start TX
            AtzintpcExecutionSteps.ApplyStartTx(ctx);

            // ordem 7: gateway _RNdKFV6PEfGBBLgT-R5iuw — Check Retries SW_QRETRYCOUNT
            //   condição Stillgood: SW_QRETRYCOUNT < MAXRETRIES → AtualizarIntimacao
            if (AtzintpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
            {
                // ordem 8: serviceTask _RNdKHF6PEfGBBLgT-R5iuw — AtualizarIntimacao
                var envelope = await _services.AtualizarintimacaoAsync(caseRef, ct);
                AtzintpcExecutionSteps.MapServiceEnvelope(ctx, envelope);

                // ordem 9: gateway _RNdKGl6PEfGBBLgT-R5iuw — AppError: STATUS_CODE != "0"
                if (AtzintpcExecutionSteps.IsAppError(ctx))
                {
                    // ordem 10: scriptTask _RNdKGV6PEfGBBLgT-R5iuw — Set App Error
                    AtzintpcExecutionSteps.SetAppError(ctx);
                }
                // ordem 11: gateway _RNdKG16PEfGBBLgT-R5iuw — junção (ambos os ramos convergem)
            }
            // ordem 12: endEvent _RNdKF16PEfGBBLgT-R5iuw — inner endEvent (encerra ActivitySet)

            // ordem 13: gateway _RNdJ2V6PEfGBBLgT-R5iuw — Tech Error  (entrouPor=REGRESSO)
            //   REGRESSO: aresta explícita (derived.linkEdges) — não existe no XPDL
            if (AtzintpcExecutionSteps.IsTechError(ctx))
            {
                return "_RNdJ2V6PEfGBBLgT-R5iuw:tech-error";
            }

            // ordem 14: gateway _RNdJ2F6PEfGBBLgT-R5iuw — App Error
            //   condição "Yes": ISAPPERROR == "Y" → More Retries
            //   condição "No"  (otherwise): caminho de sucesso (sem erro de aplicação)
            if (AtzintpcExecutionSteps.IsStillAppError(ctx))
            {
                // ordem 15: gateway _RNdJ1V6PEfGBBLgT-R5iuw — More Retries
                //   condição "Yes": NUMAPPRETRIES < MAXRETRIES → loop de volta ao Control System Task Call
                //   condição "No"  (otherwise) → Manipular Excecao
                if (AtzintpcExecutionSteps.HasMoreRetries(ctx))
                {
                    // "Yes": retorna ao início do subProcessScope (Control System Task Call)
                    continue;
                }

                // "No": segue para Manipular Excecao
                // ordem 16: gateway _RNdJ216PEfGBBLgT-R5iuw — junção → Manipular Excecao
            }
            else
            {
                // Sem erro de aplicação: segue directamente para Done - Fixed
                return "_RNdJz16PEfGBBLgT-R5iuw";
            }

            // ordem 17: userTask _RNdJ0V6PEfGBBLgT-R5iuw — Manipular Excecao
            await _manipularExcecao.ExecuteAsync(caseRef, ctx, decideOutcome, ct);

            // ordem 18: gateway _RNdJy16PEfGBBLgT-R5iuw — Manually Fixed
            //   condição "Yes": OUTCOME == "OK" → Done - Fixed
            //   condição "No"  (otherwise / OUTCOME == "R"): loop de volta ao Control System Task Call
            if (ctx.OUTCOME == "OK")
            {
                // ordem 19: endEvent _RNdJz16PEfGBBLgT-R5iuw — Done - Fixed
                return "_RNdJz16PEfGBBLgT-R5iuw";
            }

            // OUTCOME == "R": repete desde o Control System Task Call
        }
    }
}
