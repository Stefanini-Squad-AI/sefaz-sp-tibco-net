#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Workflows.PRPINTPC;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Implementação SOAP da operação CaptaParametros invocada pelo serviceTask
/// _KEwDWF6EEfGBBLgT-R5iuw do processo PRPINTPC.
///
/// Esta classe é a única que conhece o transporte SOAP/HTTP para o serviço de
/// captação de parâmetros. A porta de abstracção é injectada no workflow
/// via <c>ICaptaParametrosSoapService</c> (definida em Application/Workflows/PRPINTPC).
///
/// O envelope de resposta segue o padrão TIBCO BusinessWorks:
///   RESULT/STATUS_CODE : '0' = sucesso; qualquer outro valor = erro aplicacional.
///   RESULT/ERROR/ERROR_CODE, ERROR_DESCRIPTION: mapeados para STERRORCODE/STERRORDESC.
///
/// Correcção rulings.CLONE-PRPINTPC: STATUS_CODE == "0" indica sucesso (não SW_NA).
///
/// Excepção de transporte (HTTP/SOAP falha) → o chamador deve capturar e
/// encaminhar para o gateway Tech Error (_KEwC7V6EEfGBBLgT-R5iuw, entrouPor=regresso).
///
/// Card: BUILD-PRPINTPC-seg038
/// </summary>
public sealed class CaptaParametrosSoapService : ICaptaParametrosSoapService
{
    private readonly HttpClient _http;

    /// <param name="http">
    /// Cliente HTTP pré-configurado com o endpoint do TIBCO BusinessWorks
    /// (base address, timeouts e headers de autenticação configurados no registo de infraestrutura).
    /// </param>
    public CaptaParametrosSoapService(HttpClient http)
    {
        _http = http;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Invoca a operação CaptaParametros via SOAP/HTTP.
    /// STATUS_CODE == "0" indica sucesso (decisão rulings.CLONE-PRPINTPC).
    /// Excepção de transporte propaga-se para o chamador, que encaminha para Tech Error.
    /// </remarks>
    public Task<ServiceEnvelope> InvokeAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        // O payload SOAP é construído com os dados do caso (IdAiim, ProcessId) e
        // enviado para o endpoint BusinessWorks configurado em _http.BaseAddress.
        // A resposta é deserializada para o envelope técnico.
        //
        // Implementação de transporte a completar quando o WSDL/endpoint for confirmado.
        // O corpo fica vazio neste scaffold; a porta (interface) é o que o card exige.
        throw new NotImplementedException(
            $"CaptaParametros SOAP transport not yet implemented. " +
            $"Case: {caseRef.IdAiim} / {caseRef.ProcessId}");
    }
}
