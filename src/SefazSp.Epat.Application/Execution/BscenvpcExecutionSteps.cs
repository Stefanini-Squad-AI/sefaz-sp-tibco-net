#nullable enable

using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Application.Execution;

/// <summary>
/// Passos de envelope técnico do processo BSCENVPC.
/// Contém a lógica que toca STATUS_CODE, ISAPPERROR, ISTECHERROR e
/// os contadores de retentativa — nunca cálculo ou decisão de domínio.
/// A separação segue <c>classification.eRegraDeNegocio</c> do rule-catalogue.json.
/// Passos de envelope técnico para o processo BSCENVPC, segmento 4:
/// de "Busca Envolvidos Vista Por AIIM" a "Done - Success".
///
/// Separação de responsabilidades (rule-catalogue.json · classification.eRegraDeNegocio):
///   • O que calcula ou decide sobre o caso → Domain/Rules (função pura).
///   • O que mexe no envelope técnico (STATUS_CODE, contadores) → aqui.
///
/// Invariantes (glossário POC_Epat.yaml, confirmados 2026-08-06):
///   STATUS_CODE  : '0' = sucesso; != '0' = erro.
///   ISAPPERROR   : 'N' = sem erro de aplicação; 'Y' = erro de aplicação.
///   ISTECHERROR  : 'N' = sem erro técnico;      'Y' = erro técnico.
///   MAXRETRIES   : 5 por omissão.
/// </summary>
public static class BscenvpcExecutionSteps
{
    /// <summary>
    /// Passo SetParameters (_qIDulV6BEfGBBLgT-R5iuw) — envelope técnico.
    /// Inicializa MAXRETRIES e PROCESS_ID no contexto de execução.
    /// A decisão de domínio (se deve inicializar) já foi avaliada por
    /// <see cref="BscenvpcSetParametersRule.ShouldInitialize"/>.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="processId">Identificador do processo derivado de IDPROCESSO, ou null.</param>
    public static void ApplySetParameters(ProcessExecutionContext ctx, string? processId)
    {
        // MAXRETRIES: aplica o default quando ainda não foi fixado (==0 como sentinela de não inicializado).
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = BscenvpcSetParametersRule.DefaultMaxRetries;

        if (processId is not null)
            ctx.PROCESS_ID = processId;
    }

    /// <summary>
    /// Passo Start Loop (_qIDul16BEfGBBLgT-R5iuw) — envelope técnico.
    /// Inicializa NUMAPPRETRIES=0 quando ainda não foi inicializado.
    /// Fonte: glossário POC_Epat.yaml — "if (NUMAPPRETRIES == null) NUMAPPRETRIES = 0",
    /// confirmado em 2026-08-06.
    /// O contador NUMAPPRETRIES é independente de SW_QRETRYCOUNT (motor).
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        // A condição legada é "if (NUMAPPRETRIES == null)", i.e., apenas na primeira entrada no loop.
        // Em .NET, 0 é o valor padrão de int; o campo já vem a 0 na primeira entrada.
        // Sem alteração necessária — o valor já é 0.
        _ = ctx.NUMAPPRETRIES; // leitura explícita para rastreabilidade do contador
    }

    /// <summary>
    /// Passo Start TX (_qIDu3F6BEfGBBLgT-R5iuw) — envelope técnico (escopo ActivitySet).
    /// Reinicia os indicadores de erro antes de iniciar a transacção de serviço.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartTx(ProcessExecutionContext ctx)
    {
        ctx.STATUS_CODE  = null;
        ctx.ISAPPERROR   = "N";
        ctx.ISTECHERROR  = "N";
    }
    /// Passo "Busca Envolvidos Vista Por AIIM" (_qIDu5F6BEfGBBLgT-R5iuw):
    /// copia os valores do envelope de serviço para o contexto de execução.
    /// O valor nasce no envelope técnico (EPAT.wsdl) e tem de ser copiado explicitamente
    /// — não aparece no contexto por si próprio.
    /// </summary>
    public static void MapServiceEnvelope(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE   = envelope.STATUS_CODE;
        ctx.STERRORCODE   = envelope.STERRORCODE;
        ctx.STERRORDESC   = envelope.STERRORDESC;
    }

    /// <summary>
    /// Passo "Set App Error" (_qIDu4V6BEfGBBLgT-R5iuw):
    /// marca o indicador de erro de aplicação no contexto de execução.
    /// Só o envelope técnico é alterado aqui; a lógica de negócio que
    /// classifica a falha fica em Domain/Rules.
    /// </summary>
    public static void SetAppError(ProcessExecutionContext ctx)
    {
        ctx.ISAPPERROR = "Y";
        ctx.NUMAPPRETRIES++;
    }

    /// <summary>
    /// Condição do gateway _qIDu4l6BEfGBBLgT-R5iuw
    /// ("A chamada a Busca Envolvidos Vista Por AIIM foi bem sucedida?").
    /// Ramo AppError: STATUS_CODE != "0".
    /// Dado de topologia extraído do XPDL — não duplicar como código espalhado.
    /// </summary>
    public static bool IsAppError(ProcessExecutionContext ctx)
        => ctx.STATUS_CODE != "0";

    /// <summary>
    /// Condição do gateway _qIDupF6BEfGBBLgT-R5iuw ("Tech Error").
    /// Ramo "No" (otherwise): ISTECHERROR != 'Y'.
    /// Alcançado por REGRESSO (aresta explícita no fluxo .NET, não existe no XPDL).
    /// </summary>
    public static bool IsTechError(ProcessExecutionContext ctx)
        => ctx.ISTECHERROR == "Y";

    /// <summary>
    /// Condição do gateway _qIDuo16BEfGBBLgT-R5iuw ("App Error").
    /// Ramo "No" (otherwise): ISAPPERROR != 'Y'.
    /// </summary>
    public static bool IsStillAppError(ProcessExecutionContext ctx)
        => ctx.ISAPPERROR == "Y";
}
