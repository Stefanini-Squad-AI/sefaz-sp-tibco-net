#nullable enable

using System.Net.Http;
using System.Security;
using System.Text;
using System.Xml.Linq;
using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Implementação SOAP/JMS da operação 'obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEAT'
/// do EPAT.wsdl.
///
/// Corresponde ao serviceTask 'CalcularPrazo' (_AsZCkVqkEfG5K7mY0I3I6w) no processo CALCPRPC.
/// Esta classe contém apenas a lógica de transporte SOAP/HTTP.
/// </summary>
internal sealed class CalcularPrazoTransport
{
    private const string SoapAction = "\"__sol_EPATInterfaceWrappers_sol_obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEAT.1\"";

    private readonly HttpClient _httpClient;
    private readonly string _endpointUrl;

    public CalcularPrazoTransport(HttpClient httpClient, string endpointUrl)
    {
        _httpClient = httpClient;
        _endpointUrl = endpointUrl;
    }

    public async Task<ServiceEnvelope> InvokeAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        var soapBody = BuildSoapRequest(caseRef);

        using var request = new HttpRequestMessage(HttpMethod.Post, _endpointUrl)
        {
            Content = new StringContent(soapBody, Encoding.UTF8, "text/xml"),
        };
        request.Headers.Add("SOAPAction", SoapAction);

        using var response = await _httpClient.SendAsync(request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        return ParseSoapResponse(responseBody);
    }

    private static string BuildSoapRequest(AiimCaseRef caseRef)
    {
        var processId = SecurityElement.Escape(caseRef.ProcessId) ?? string.Empty;
        var dateTime = DateTimeOffset.UtcNow.ToString("O");

        return $"""
                 <?xml version="1.0" encoding="utf-8"?>
                 <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
                                xmlns:epat="urn:EPATInterfaceWrappers">
                   <soap:Body>
                     <epat:obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATRequest>
                       <epat:HEADER>
                         <epat:TRANSACTION_ID>{caseRef.IdAiim}</epat:TRANSACTION_ID>
                         <epat:PROCESS_ID>{processId}</epat:PROCESS_ID>
                         <epat:DATETIME>{dateTime}</epat:DATETIME>
                       </epat:HEADER>
                       <epat:BODY>
                         <epat:dataInicioPeriodo />
                         <epat:periodoEmDias>0</epat:periodoEmDias>
                         <epat:codigoMunicipio>0</epat:codigoMunicipio>
                       </epat:BODY>
                     </epat:obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATRequest>
                   </soap:Body>
                 </soap:Envelope>
                 """;
    }

    private static ServiceEnvelope ParseSoapResponse(string responseBody)
    {
        try
        {
            var doc = XDocument.Parse(responseBody);
            var body = doc.Descendants().FirstOrDefault(static element =>
                element.Name.LocalName == "obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse");

            if (body is null)
                return new ServiceEnvelope(
                    STATUS_CODE: "PARSE_ERROR",
                    STERRORCODE: "SOAP_PARSE",
                    STERRORDESC: "Resposta SOAP sem corpo esperado.");

            var statusCode = body.Descendants().FirstOrDefault(static element => element.Name.LocalName == "STATUS_CODE")?.Value ?? "UNKNOWN";
            var errorCode = body.Descendants().FirstOrDefault(static element => element.Name.LocalName == "ERROR_CODE" || element.Name.LocalName == "STERRORCODE")?.Value;
            var errorDesc = body.Descendants().FirstOrDefault(static element => element.Name.LocalName == "ERROR_DESCRIPTION" || element.Name.LocalName == "STERRORDESC")?.Value;

            return new ServiceEnvelope(statusCode, errorCode, errorDesc);
        }
        catch (Exception ex)
        {
            return new ServiceEnvelope(
                STATUS_CODE: "TRANSPORT_ERROR",
                STERRORCODE: ex.GetType().Name,
                STERRORDESC: ex.Message);
        }
    }
}
