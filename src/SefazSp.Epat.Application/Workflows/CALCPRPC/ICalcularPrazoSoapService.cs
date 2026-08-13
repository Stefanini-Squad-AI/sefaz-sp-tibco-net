#nullable enable

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Application.Workflows.CALCPRPC;

/// <summary>
/// Porta do serviço CalcularPrazo invocado pelo serviceTask _AsZCkVqkEfG5K7mY0I3I6w
/// do processo CALCPRPC.
///
/// A implementação concreta vive em
/// <c>SefazSp.Epat.Infrastructure.Integration.Soap.CalcularPrazoSoapService</c>.
/// O duble de teste pode ser qualquer implementação desta interface.
///
/// Excepção de transporte (HttpRequestException, SocketException, …) sinaliza erro
/// técnico; o workflow captura e encaminha para o gateway Tech Error
/// (_zJIHZVqiEfG5K7mY0I3I6w, entrouPor=regresso).
/// </summary>
public interface ICalcularPrazoSoapService
{
    /// <summary>
    /// Invoca o serviço CalcularPrazo via SOAP/HTTP.
    /// </summary>
    /// <param name="caseRef">Identidade do caso.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// Envelope técnico com STATUS_CODE, STERRORCODE e STERRORDESC.
    /// STATUS_CODE == "0" indica sucesso.
    /// </returns>
    Task<ServiceEnvelope> InvokeAsync(AiimCaseRef caseRef, CancellationToken ct);
}
