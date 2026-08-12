#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Workflows;

/// <summary>
/// Topologia do fluxo BSCENVPC, segmento 4 (passos 8–15 do cenário SC-BSCENVPC-010):
/// de "Busca Envolvidos Vista Por AIIM" (_qIDu5F6BEfGBBLgT-R5iuw)
/// a "Done - Success" (_qIDuol6BEfGBBLgT-R5iuw).
///
/// Nós percorridos (checklist, card BUILD-BSCENVPC-seg004):
///   ordem 1 — serviceTask  _qIDu5F6BEfGBBLgT-R5iuw  Busca Envolvidos Vista Por AIIM  entrouPor=fluxo
///   ordem 2 — gateway      _qIDu4l6BEfGBBLgT-R5iuw  decide sucesso/falha              entrouPor=fluxo
///   ordem 3 — scriptTask   _qIDu4V6BEfGBBLgT-R5iuw  Set App Error                    entrouPor=fluxo
///   ordem 4 — gateway      _qIDu416BEfGBBLgT-R5iuw  junção de fluxo                  entrouPor=fluxo
///   ordem 5 — endEvent     _qIDu316BEfGBBLgT-R5iuw  (erro, no ActivitySet)           entrouPor=fluxo
///   ordem 6 — gateway      _qIDupF6BEfGBBLgT-R5iuw  Tech Error                       entrouPor=REGRESSO (*)
///   ordem 7 — gateway      _qIDuo16BEfGBBLgT-R5iuw  App Error                        entrouPor=fluxo
///   ordem 8 — endEvent     _qIDuol6BEfGBBLgT-R5iuw  Done - Success                   entrouPor=fluxo
///
/// (*) A aresta de REGRESSO para Tech Error NÃO existe no XPDL — é escrita
///     explicitamente aqui como aresta de fluxo derivada de derived.linkEdges.
///     Quando o ActivitySet termina pelo endEvent de erro (_qIDu316BEfGBBLgT-R5iuw),
///     o controlo regressa ao escopo MAIN em _qIDupF6BEfGBBLgT-R5iuw.
///
/// A condição de cada gateway é dado de topologia (extraída do XPDL);
/// não é lógica espalhada pelo código.
/// </summary>
public sealed class BscenvpcWorkflowSeg004
{
    private readonly IEpatServices _services;

    public BscenvpcWorkflowSeg004(IEpatServices services)
    {
        _services = services;
    }

    /// <summary>
    /// Executa o segmento 4 do processo BSCENVPC a partir do passo 8.
    /// Devolve o identificador do nó terminal alcançado:
    ///   "_qIDu316BEfGBBLgT-R5iuw" — endEvent de erro (ramo AppError sem regresso)
    ///   "_qIDuol6BEfGBBLgT-R5iuw" — Done - Success (caminho de sucesso)
    /// </summary>
    public async Task<string> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        CancellationToken ct)
    {
        // ordem 1: serviceTask _qIDu5F6BEfGBBLgT-R5iuw — Busca Envolvidos Vista Por AIIM
        var envelope = await _services.BuscarvistasativasporaiimAsync(caseRef, ct);
        BscenvpcExecutionSteps.MapServiceEnvelope(ctx, envelope);

        // ordem 2: gateway _qIDu4l6BEfGBBLgT-R5iuw — decide sucesso/falha
        //   condição AppError: STATUS_CODE != "0"  (dado de topologia XPDL)
        //   condição Good (otherwise): STATUS_CODE == "0"
        bool isAppError = BscenvpcExecutionSteps.IsAppError(ctx);

        if (isAppError)
        {
            // ordem 3: scriptTask _qIDu4V6BEfGBBLgT-R5iuw — Set App Error
            BscenvpcExecutionSteps.SetAppError(ctx);

            // ordem 4: gateway _qIDu416BEfGBBLgT-R5iuw — junção de fluxo
            // (converge ramos; no cenário SC-BSCENVPC-010 só o ramo AppError chega aqui)

            // ordem 5: endEvent _qIDu316BEfGBBLgT-R5iuw — evento terminal de erro no ActivitySet
            // O endEvent encerra o ActivitySet (escopo interno) e devolve controlo ao MAIN.
            // *** REGRESSO EXPLÍCITO *** (aresta não existe no XPDL — escrita aqui: derived.linkEdges)
            return await ResumeAtTechErrorGatewayAsync(caseRef, ctx);
        }

        // Ramo Good (STATUS_CODE == "0"):
        // A topologia do XPDL prevê saída directa para o escopo MAIN — ver SC-BSCENVPC-010 path.
        // O fluxo salta directamente para o gateway de junção MAIN (Tech Error, ordem 6).
        return await ResumeAtTechErrorGatewayAsync(caseRef, ctx);
    }

    /// <summary>
    /// Aresta de REGRESSO explícita (derived.linkEdges):
    /// retoma o fluxo MAIN no gateway Tech Error (_qIDupF6BEfGBBLgT-R5iuw, ordem 6).
    /// Esta aresta NÃO existe no XPDL e é escrita explicitamente conforme AC6.
    /// </summary>
    private static Task<string> ResumeAtTechErrorGatewayAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx)
    {
        // ordem 6: gateway _qIDupF6BEfGBBLgT-R5iuw — Tech Error  (entrouPor=regresso)
        //   condição "Yes": ISTECHERROR == "Y"  → ramo de erro técnico (fora deste segmento)
        //   condição "No"  (otherwise)          → continua para App Error
        if (BscenvpcExecutionSteps.IsTechError(ctx))
        {
            // Ramo de erro técnico: não encerra com Done-Success.
            // O destino concreto (retentativa ou tratamento manual) é tratado
            // nos segmentos anteriores do processo BSCENVPC. Sinaliza saída.
            return Task.FromResult("_qIDupF6BEfGBBLgT-R5iuw:tech-error");
        }

        // ordem 7: gateway _qIDuo16BEfGBBLgT-R5iuw — App Error  (entrouPor=fluxo)
        //   condição "Yes": ISAPPERROR == "Y"  → ramo de erro de aplicação (fora deste segmento)
        //   condição "No"  (otherwise)         → continua para Done - Success
        if (BscenvpcExecutionSteps.IsStillAppError(ctx))
        {
            // Ramo de erro de aplicação: não encerra com Done-Success.
            return Task.FromResult("_qIDuo16BEfGBBLgT-R5iuw:app-error");
        }

        // ordem 8: endEvent _qIDuol6BEfGBBLgT-R5iuw — Done - Success  (entrouPor=fluxo)
        return Task.FromResult("_qIDuol6BEfGBBLgT-R5iuw");
    }
}
