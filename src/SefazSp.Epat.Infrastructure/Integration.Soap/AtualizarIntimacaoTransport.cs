#nullable enable

using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Implementação SOAP da operação 'atualizarIntimacao' do EPAT.wsdl.
///
/// Corresponde ao serviceTask 'AtualizarIntimacao'
/// (_RNdKHF6PEfGBBLgT-R5iuw) no processo ATZINTPC.
///
/// Fonte TIBCO: EPAT.wsdl
///   operacao: __sol_EPATInterfaceWrappers_sol_atualizarIntimacao.1
/// </summary>
internal sealed class AtualizarIntimacaoTransport
{
    private readonly HttpClient _httpClient;
    private readonly string _endpointUrl;

    public AtualizarIntimacaoTransport(HttpClient httpClient, string endpointUrl)
    {
        _httpClient  = httpClient;
        _endpointUrl = endpointUrl;
    }

    /// <summary>
    /// Envia o pedido SOAP e devolve o envelope técnico do BusinessWorks.
    /// STATUS_CODE = '0' indica sucesso; qualquer outro valor é erro.
    /// </summary>
    public async Task<ServiceEnvelope> InvokeAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        var soapBody = BuildSoapRequest(caseRef);

        using var request = new HttpRequestMessage(HttpMethod.Post, _endpointUrl)
        {
            Content = new StringContent(soapBody, Encoding.UTF8, "text/xml"),
        };
        request.Headers.Add("SOAPAction",
            "\"__sol_EPATInterfaceWrappers_sol_atualizarIntimacao.1\"");

        using var response = await _httpClient.SendAsync(request, ct);
        var responseBody   = await response.Content.ReadAsStringAsync(ct);

        return ParseSoapResponse(responseBody);
    }

    private static string BuildSoapRequest(AiimCaseRef caseRef) =>
        $"""
         <?xml version="1.0" encoding="utf-8"?>
         <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
                        xmlns:epat="urn:EPATInterfaceWrappers">
           <soap:Body>
             <epat:atualizarIntimacaoRequest>
               <epat:idAiim>{caseRef.IdAiim}</epat:idAiim>
               <epat:processId>{caseRef.ProcessId}</epat:processId>
             </epat:atualizarIntimacaoRequest>
           </soap:Body>
         </soap:Envelope>
         """;

    private static ServiceEnvelope ParseSoapResponse(string responseBody)
    {
        try
        {
            var doc  = XDocument.Parse(responseBody);
            var ns   = (XNamespace)"urn:EPATInterfaceWrappers";
            var body = doc.Descendants(ns + "atualizarIntimacaoResponse").FirstOrDefault();

            if (body is null)
                return new ServiceEnvelope(STATUS_CODE: "PARSE_ERROR",
                    STERRORCODE: "SOAP_PARSE",
                    STERRORDESC: "Resposta SOAP sem corpo esperado.");

            var statusCode = body.Element(ns + "STATUS_CODE")?.Value ?? "UNKNOWN";
            var errorCode  = body.Element(ns + "STERRORCODE")?.Value;
            var errorDesc  = body.Element(ns + "STERRORDESC")?.Value;

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
