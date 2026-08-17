#nullable enable

namespace SefazSp.Epat.Application.Execution;

/// <summary>
/// Execution step para o scriptTask 'Set App Error' (_qIDu4V6BEfGBBLgT-R5iuw),
/// passo 3 do segmento 002 do processo BSCENVPC.
///
/// O que calcula ou decide sobre o caso e regra de dominio e vive em Domain/Rules.
/// Este passo so actualiza o envelope tecnico (STATUS_CODE, contadores de retentativa)
/// no ProcessExecutionContext — conforme nota de scaffold de Application/Execution.
///
/// Fonte TIBCO: POC_Epat.xpdl //xpdl2:Activity[@Id='_qIDu4V6BEfGBBLgT-R5iuw']
/// </summary>
public static class BscenvpcSetAppErrorStep
{
    /// <summary>
    /// Aplica o resultado do envelope de servico ao contexto de execucao.
    /// STATUS_CODE != "0" e erro de aplicacao (rulings.CLONE-PRPINTPC confirmado:
    /// a condicao correcta e STATUS_CODE != "0", nao != SW_NA).
    /// </summary>
    public static void Apply(ProcessExecutionContext ctx, ServiceEnvelopeResult envelope)
    {
        ctx.STATUS_CODE = envelope.StatusCode;
        ctx.STERRORCODE = envelope.ErrorCode;
        ctx.STERRORDESC = envelope.ErrorDesc;
        ctx.ISTECHERROR = envelope.IsTechError ? "Y" : "N";
        ctx.ISAPPERROR = envelope.IsAppError ? "Y" : "N";

        if (envelope.IsAppError)
            ctx.NUMAPPRETRIES += 1;
    }
}

/// <summary>
/// Resultado normalizado de uma chamada ao envelope de servico TIBCO BusinessWorks.
/// Produzido pelo passo de mapeamento apos cada chamada de servico;
/// consumido por BscenvpcSetAppErrorStep.
/// </summary>
/// <param name="StatusCode">STATUS_CODE devolvido pelo BusinessWorks ('0' = sucesso).</param>
/// <param name="ErrorCode">STERRORCODE; null quando STATUS_CODE = '0'.</param>
/// <param name="ErrorDesc">STERRORDESC; null quando STATUS_CODE = '0'.</param>
/// <param name="IsTechError">true quando a falha e de infraestrutura (fila/rede/indisponibilidade).</param>
/// <param name="IsAppError">true quando a falha e de regra de negocio (STATUS_CODE != '0' e nao tecnico).</param>
public sealed record ServiceEnvelopeResult(
    string? StatusCode,
    string? ErrorCode,
    string? ErrorDesc,
    bool IsTechError,
    bool IsAppError);
