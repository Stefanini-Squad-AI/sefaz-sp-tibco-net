#nullable enable

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Application.Execution;

/// <summary>
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
