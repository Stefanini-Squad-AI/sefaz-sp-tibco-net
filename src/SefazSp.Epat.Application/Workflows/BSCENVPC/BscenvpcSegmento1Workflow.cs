#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.BSCENVPC;
using SefazSp.Epat.Application.UseCases.BSCENVPC;

namespace SefazSp.Epat.Application.Workflows.BSCENVPC;

/// <summary>
/// Resultado do segmento 1 do processo BSCENVPC (passos 8–19, cenario SC-BSCENVPC-008).
/// </summary>
public enum BscenvpcSegmento1Outcome
{
    /// <summary>
    /// Chamada bem sucedida (STATUS_CODE = '0'). O fluxo prossegue normalmente.
    /// </summary>
    Success,

    /// <summary>
    /// Erro de aplicacao sem retentativa disponivel — encerrou no endEvent _qIDu316BEfGBBLgT-R5iuw.
    /// </summary>
    AppErrorEnd,

    /// <summary>
    /// Caso resolvido manualmente — encerrou no endEvent Done - Fixed (_qIDuml6BEfGBBLgT-R5iuw).
    /// </summary>
    DoneFixed,
}

/// <summary>
/// Workflow do segmento 1 de BSCENVPC: de 'Busca Envolvidos Vista Por AIIM'
/// ate 'Done - Fixed'.
///
/// Topologia (12 nos, passos 8–19 do cenario SC-BSCENVPC-008):
///
///   [1] serviceTask  Busca Envolvidos Vista Por AIIM  _qIDu5F6BEfGBBLgT-R5iuw
///   [2] gateway      (A chamada foi bem sucedida?)    _qIDu4l6BEfGBBLgT-R5iuw
///   [3] scriptTask   Set App Error                    _qIDu4V6BEfGBBLgT-R5iuw  (ramo AppError)
///   [4] gateway      (anonimo)                        _qIDu416BEfGBBLgT-R5iuw
///   [5] endEvent     (fim precoce)                    _qIDu316BEfGBBLgT-R5iuw
///   [6] gateway      Tech Error  [regresso]           _qIDupF6BEfGBBLgT-R5iuw  ← sem transicao XPDL; ligacao explicita
///   [7] gateway      App Error                        _qIDuo16BEfGBBLgT-R5iuw
///   [8] gateway      More Retries                     _qIDuoF6BEfGBBLgT-R5iuw
///   [9] gateway      (anonimo)                        _qIDupl6BEfGBBLgT-R5iuw
///  [10] userTask     Manipular Excecao                _qIDunF6BEfGBBLgT-R5iuw
///  [11] gateway      Manually Fixed                   _qIDull6BEfGBBLgT-R5iuw
///  [12] endEvent     Done - Fixed                     _qIDuml6BEfGBBLgT-R5iuw
///
/// Nota AC4: o gateway Tech Error (_qIDupF6BEfGBBLgT-R5iuw) e alcancado por 'regresso'
/// — nao existe transicao XPDL. Em .NET, a aresta de ligacao e escrita explicitamente
/// via bloco catch na invocacao do servico.
/// </summary>
public sealed class BscenvpcSegmento1Workflow
{
    private readonly IEpatServices _services;
    private readonly ManipularExcecaoUseCase _manipularExcecao;

    public BscenvpcSegmento1Workflow(
        IEpatServices services,
        ManipularExcecaoUseCase manipularExcecao)
    {
        _services = services;
        _manipularExcecao = manipularExcecao;
    }

    /// <summary>
    /// Executa o segmento 1 (passos 8–19) do processo BSCENVPC.
    /// </summary>
    /// <param name="caseRef">Referencia do caso.</param>
    /// <param name="ctx">Contexto de execucao mutavel partilhado com o loop externo.</param>
    /// <param name="decideManipularExcecao">
    /// Delegate de interacao humana para a userTask 'Manipular Excecao'.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O desfecho do segmento.</returns>
    public async Task<BscenvpcSegmento1Outcome> ExecuteAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoResult>> decideManipularExcecao,
        CancellationToken ct)
    {
        // ── passo 1: serviceTask _qIDu5F6BEfGBBLgT-R5iuw ────────────────────
        // Busca Envolvidos Vista Por AIIM
        // Uma excepcao de transporte/infraestrutura activa o gateway Tech Error
        // (passo 6, entrouPor=regresso). Nao existe transicao XPDL: a aresta
        // e escrita explicitamente aqui (AC4).
        ServiceEnvelope envelope;
        bool isTechError = false;
        try
        {
            envelope = await _services
                .BuscarvistasativasporaiimAsync(caseRef, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Aresta de regresso para o gateway Tech Error (_qIDupF6BEfGBBLgT-R5iuw).
            // Registada explicitamente por nao existir como transicao no XPDL.
            ctx.ISTECHERROR = "Y";
            ctx.STATUS_CODE = ex.Message;
            isTechError = true;
            envelope = new ServiceEnvelope(null, null, ex.Message);
        }

        if (!isTechError)
        {
            // Mapear envelope tecnico para o contexto de execucao.
            ctx.STATUS_CODE  = envelope.STATUS_CODE;
            ctx.STERRORCODE  = envelope.STERRORCODE;
            ctx.STERRORDESC  = envelope.STERRORDESC;
        }

        // ── passo 2: gateway _qIDu4l6BEfGBBLgT-R5iuw ────────────────────────
        // "A chamada a Busca Envolvidos Vista Por AIIM foi bem sucedida?"
        // Condicao AppError: STATUS_CODE != "0"
        if (!isTechError && ctx.STATUS_CODE == "0")
        {
            // Ramo de sucesso: o segmento termina e o controlo regressa ao chamador.
            return BscenvpcSegmento1Outcome.Success;
        }

        if (!isTechError)
        {
            // ── passo 3: scriptTask _qIDu4V6BEfGBBLgT-R5iuw ─────────────────
            // Set App Error: define ISAPPERROR = "Y"
            SetAppError.Apply(ctx);

            // ── passo 4: gateway _qIDu416BEfGBBLgT-R5iuw ────────────────────
            // (anonimo, encaminha para o endEvent de erro de aplicacao)

            // ── passo 5: endEvent _qIDu316BEfGBBLgT-R5iuw ───────────────────
            // Fim precoce no ramo de App Error.
            return BscenvpcSegmento1Outcome.AppErrorEnd;
        }

        // ── passo 6: gateway _qIDupF6BEfGBBLgT-R5iuw — Tech Error ───────────
        // Alcancado por regresso (sem transicao XPDL). Ligacao escrita acima.
        // Ramo OTHERWISE → encaminha para gateway App Error.

        // ── passo 7: gateway _qIDuo16BEfGBBLgT-R5iuw — App Error ────────────
        // Condicao Yes: ISAPPERROR == 'Y'
        if (ctx.ISAPPERROR != "Y")
        {
            // Ramo de negacao (nao e erro de aplicacao): o fluxo nao tem destino
            // declarado neste segmento — o chamador e responsavel por re-encaminhar.
            return BscenvpcSegmento1Outcome.AppErrorEnd;
        }

        // ── passo 8: gateway _qIDuoF6BEfGBBLgT-R5iuw — More Retries ─────────
        // Condicao Otherwise → sem retentativa disponivel, vai para tratamento manual.
        // (A logica de contagem de tentativas — NUMAPPRETRIES vs MAXRETRIES — e
        //  gerida pelo loop externo do processo BSCENVPC; este segmento recebe
        //  o controlo apenas quando ja nao ha retentativas.)

        // ── passo 9: gateway _qIDupl6BEfGBBLgT-R5iuw ─────────────────────────
        // (anonimo, encaminha para userTask Manipular Excecao)

        // ── passo 10: userTask _qIDunF6BEfGBBLgT-R5iuw — Manipular Excecao ──
        await _manipularExcecao
            .ExecuteAsync(caseRef, ctx, decideManipularExcecao, ct)
            .ConfigureAwait(false);

        // ── passo 11: gateway _qIDull6BEfGBBLgT-R5iuw — Manually Fixed ───────
        // Condicao Yes: OUTCOME == 'OK'
        if (ctx.OUTCOME == "OK")
        {
            // ── passo 12: endEvent _qIDuml6BEfGBBLgT-R5iuw — Done - Fixed ───
            return BscenvpcSegmento1Outcome.DoneFixed;
        }

        // OUTCOME == 'R': operador quer repetir; o loop externo retoma o controlo.
        return BscenvpcSegmento1Outcome.Success;
    }
}
