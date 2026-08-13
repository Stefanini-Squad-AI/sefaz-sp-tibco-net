#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.BSCENVPC;

namespace SefazSp.Epat.Application.Workflows.BSCENVPC;

/// <summary>
/// Troco 4 da jornada BSCENVPC: de Busca Envolvidos Vista Por AIIM ate Done - Success.
/// Cobre os passos 8 a 15 do cenario SC-BSCENVPC-010.
///
/// Topologia (8 nos, 2 escopos):
///
/// [ActivitySet]
///   (8)  _qIDu5F6BEfGBBLgT-R5iuw  Busca Envolvidos Vista Por AIIM  serviceTask
///   (9)  _qIDu4l6BEfGBBLgT-R5iuw  gateway                          exclusiveGateway
///                                  ramo AppError: STATUS_CODE != "0" → (10)
///                                  ramo Good (default) → (11)
///  (10)  _qIDu4V6BEfGBBLgT-R5iuw  Set App Error                    scriptTask
///  (11)  _qIDu416BEfGBBLgT-R5iuw  Convergência                     exclusiveGateway
///  (12)  _qIDu316BEfGBBLgT-R5iuw  endEvent (ActivitySet)           endEvent
///
/// [MAIN — alcancado por regresso]
///  (13)  _qIDupF6BEfGBBLgT-R5iuw  Tech Error                       exclusiveGateway
///                                  ramo Yes: ISTECHERROR == "Y" → convergência/retry
///                                  ramo No (default) → (14)
///  (14)  _qIDuo16BEfGBBLgT-R5iuw  App Error                        exclusiveGateway
///                                  ramo Yes: ISAPPERROR == "Y" → retry
///                                  ramo No (default) → (15)
///  (15)  _qIDuol6BEfGBBLgT-R5iuw  Done - Success                   endEvent
///
/// ATENCAO: o no (13) e alcancado por REGRESSO — ligacao que NAO existe como
/// transicao no XPDL; esta aresta de regressao e escrita explicitamente aqui.
///
/// Condicoes reproduzidas do XPDL (BSCENVPC usa STATUS_CODE != "0", nao SW_NA
/// como o PRPINTPC — diferenca registada em rulings.CLONE-PRPINTPC):
///   gateway (9): STATUS_CODE != "0"  → AppError
///   Tech Error (13): ISTECHERROR == "Y" → retry
///   App Error (14): ISAPPERROR == "Y" → retry
///
/// H1 CONFIRMADA (2026-08-06): ISAPPERROR = "N" e ausencia de erro,
/// nao "ainda nao avaliado" — entrada ISAPPERROR do glossario POC_Epat.yaml.
/// </summary>
public sealed class Seg004BuscaEnvolvidosVistaPorAiimWorkflow
{
    private readonly IEpatServices _services;

    public Seg004BuscaEnvolvidosVistaPorAiimWorkflow(IEpatServices services)
    {
        _services = services;
    }

    /// <summary>
    /// Executa os passos 8-15 do troco seg004, partindo do contexto corrente do processo.
    /// Devolve o resultado do endEvent alvo:
    ///   <see cref="Seg004Outcome.DoneSuccess"/>      — passo 15, Done - Success
    ///   <see cref="Seg004Outcome.ActivitySetEnded"/> — passo 12, endEvent do ActivitySet (sem App/Tech Error)
    /// O chamador e responsavel por encaminhar para o retry se outcome != DoneSuccess.
    /// </summary>
    public async Task<Seg004Outcome> RunActivitySetSegmentAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        CancellationToken ct)
    {
        // (8) _qIDu5F6BEfGBBLgT-R5iuw — Busca Envolvidos Vista Por AIIM (serviceTask)
        // Chamada SOAP/JMS mediada pelo scaffold de Integration.Soap.
        // O contrato da porta (IEpatServices) nao e alterado.
        var envelope = await _services.BuscarvistasativasporaiimAsync(caseRef, ct);

        // Copiar o envelope tecnico para o contexto de execucao (passo de mapeamento explicito).
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;

        // (9) _qIDu4l6BEfGBBLgT-R5iuw — gateway
        // Condicao: STATUS_CODE != "0" → ramo AppError
        // Default (ramo Good): STATUS_CODE == "0" → convergencia directa
        if (ctx.STATUS_CODE != "0")
        {
            // (10) _qIDu4V6BEfGBBLgT-R5iuw — Set App Error (scriptTask)
            SetAppErrorStep.Execute(ctx);
        }

        // (11) _qIDu416BEfGBBLgT-R5iuw — gateway de convergencia
        // (12) _qIDu316BEfGBBLgT-R5iuw — endEvent do ActivitySet
        // O regresso ao MAIN e implicito na saida do ActivitySet (regresso).
        return Seg004Outcome.ActivitySetEnded;
    }

    /// <summary>
    /// Avalia os gateways do MAIN apos o regresso do ActivitySet (passos 13-15).
    /// Este metodo modela a aresta de REGRESSO que NAO existe no XPDL mas
    /// deve estar explicitamente no fluxo .NET (ver nota da topologia acima).
    /// </summary>
    /// <returns>
    ///   <see cref="Seg004Outcome.DoneSuccess"/>    — Done - Success (passo 15)
    ///   <see cref="Seg004Outcome.TechErrorRetry"/> — Tech Error branch: ISTECHERROR=="Y"
    ///   <see cref="Seg004Outcome.AppErrorRetry"/>  — App Error branch: ISAPPERROR=="Y"
    /// </returns>
    public static Seg004Outcome EvaluateMainGatewaysAfterRegresso(ProcessExecutionContext ctx)
    {
        // (13) _qIDupF6BEfGBBLgT-R5iuw — Tech Error (exclusiveGateway, MAIN)
        // Alcancado por REGRESSO: aresta de regressao escrita explicitamente aqui.
        // ramo Yes: ISTECHERROR == "Y" → convergencia (retry loop)
        // ramo No (default/OTHERWISE) → App Error
        if (ctx.ISTECHERROR == "Y")
        {
            return Seg004Outcome.TechErrorRetry;
        }

        // (14) _qIDuo16BEfGBBLgT-R5iuw — App Error (exclusiveGateway, MAIN)
        // ramo Yes: ISAPPERROR == "Y" → retry path
        // ramo No (default/OTHERWISE) → Done - Success
        if (ctx.ISAPPERROR == "Y")
        {
            return Seg004Outcome.AppErrorRetry;
        }

        // (15) _qIDuol6BEfGBBLgT-R5iuw — Done - Success (endEvent, MAIN)
        return Seg004Outcome.DoneSuccess;
    }
}

/// <summary>
/// Resultado possivel da execucao do troco seg004.
/// </summary>
public enum Seg004Outcome
{
    /// <summary>
    /// Passo 15: _qIDuol6BEfGBBLgT-R5iuw — Done - Success. Caminho de sucesso.
    /// </summary>
    DoneSuccess,

    /// <summary>
    /// Passo 12: _qIDu316BEfGBBLgT-R5iuw — endEvent do ActivitySet concluido.
    /// O chamador deve invocar EvaluateMainGatewaysAfterRegresso para continuar.
    /// </summary>
    ActivitySetEnded,

    /// <summary>
    /// Passo 13 ramo Yes: Tech Error — ISTECHERROR=="Y".
    /// Fluxo deve regressar ao laco de retry tecnico.
    /// </summary>
    TechErrorRetry,

    /// <summary>
    /// Passo 14 ramo Yes: App Error — ISAPPERROR=="Y".
    /// Fluxo deve regressar ao laco de retry de aplicacao.
    /// </summary>
    AppErrorRetry,
}
