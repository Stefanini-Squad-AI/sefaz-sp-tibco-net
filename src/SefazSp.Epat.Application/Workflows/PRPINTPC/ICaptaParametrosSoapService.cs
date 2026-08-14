#nullable enable

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Application.Workflows.PRPINTPC;

/// <summary>
/// Porta do serviço CaptaParametros invocado pelo serviceTask _KEwDWF6EEfGBBLgT-R5iuw
/// do processo PRPINTPC.
///
/// A implementação concreta vive em
/// <c>SefazSp.Epat.Infrastructure.Integration.Soap.CaptaParametrosSoapService</c>.
/// O duble de teste pode ser qualquer implementação desta interface.
///
/// Excepção de transporte (HttpRequestException, SocketException, …) sinaliza erro
/// técnico; o workflow captura e encaminha para o gateway Tech Error
/// (_KEwC7V6EEfGBBLgT-R5iuw, entrouPor=regresso).
///
/// Card: BUILD-PRPINTPC-seg038
/// </summary>
public interface ICaptaParametrosSoapService
{
    /// <summary>
    /// Invoca o serviço CaptaParametros via SOAP/HTTP.
    /// </summary>
    /// <param name="caseRef">Identidade do caso.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// Envelope técnico com STATUS_CODE, STERRORCODE e STERRORDESC.
    /// STATUS_CODE == "0" indica sucesso (decisão rulings.CLONE-PRPINTPC).
    /// </returns>
    Task<ServiceEnvelope> InvokeAsync(AiimCaseRef caseRef, CancellationToken ct);
}
