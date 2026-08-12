#nullable enable

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Application.Execution;

/// <summary>
/// Passos de execucao do envelope tecnico para o processo BSCENVPC.
/// Corresponde ao scriptTask 'Set App Error' (_qIDu4V6BEfGBBLgT-R5iuw, ordem 3).
/// NAO usa IPEConversionUtil nem IPEStringUtil — sem dependencia do bloqueador BUILTIN-SEMANTICS.
/// </summary>
public static class BscenvpcExecutionSteps
{
    /// <summary>
    /// Ordem 3 — scriptTask 'Set App Error' (_qIDu4V6BEfGBBLgT-R5iuw).
    /// Propaga o erro de aplicacao do envelope tecnico para o contexto de execucao.
    /// STATUS_CODE != "0" activa este passo; ISAPPERROR fica como 'Y'.
    /// </summary>
    public static void SetAppError(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.ISAPPERROR = "Y";
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
    }
}
