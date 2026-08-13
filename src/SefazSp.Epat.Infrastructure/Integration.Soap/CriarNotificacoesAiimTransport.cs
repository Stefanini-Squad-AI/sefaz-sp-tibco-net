#nullable enable

using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Implementacao SOAP/JMS da operacao 'criarNotificacoesAIIM' do EPAT.wsdl.
///
/// Corresponde ao serviceTask 'CriaNotificacao'
/// (_NcJxMF9KEfGqPfX31TKC3w) no processo CRNOTPC.
///
/// A porta IEpatServices (status: final — transcricao do WSDL) e injectada em
/// EpatSoapServices, que agrega as 5 operacoes declaradas no IEpatServices.
/// Esta classe contem apenas a logica de transporte da operacao criarNotificacoesAIIM.
///
/// Campos de dominio adicionais (LOGINAFR, NOTIFICADOS, etc.) sao fornecidos pelo
/// agente de servico TIBCO a partir do caso — o envelope minimo usa IdAiim e ProcessId
/// como correlacao, conforme o padrao dos outros transportes do pacote.
///
/// Fonte TIBCO: EPAT.wsdl
///   operacao: __sol_EPATInterfaceWrappers_sol_criarNotificacoesAIIM.1
/// </summary>
internal sealed class CriarNotificacoesAiimTransport
{
    private readonly HttpClient _httpClient;
    private readonly string _endpointUrl;

    public CriarNotificacoesAiimTransport(HttpClient httpClient, string endpointUrl)
    {
        _httpClient  = httpClient;
        _endpointUrl = endpointUrl;
    }

    /// <summary>
    /// Envia o pedido SOAP e devolve o envelope tecnico do BusinessWorks.
    /// STATUS_CODE = '0' indica sucesso; qualquer outro valor e erro.
    /// </summary>
    public async Task<ServiceEnvelope> InvokeAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        var soapBody = BuildSoapRequest(caseRef);

        using var request = new HttpRequestMessage(HttpMethod.Post, _endpointUrl)
        {
            Content = new StringContent(soapBody, Encoding.UTF8, "text/xml"),
        };
        request.Headers.Add("SOAPAction",
            "\"__sol_EPATInterfaceWrappers_sol_criarNotificacoesAIIM.1\"");

        using var response = await _httpClient.SendAsync(request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        return ParseSoapResponse(responseBody);
    }

    private static string BuildSoapRequest(AiimCaseRef caseRef) =>
        $"""
         <?xml version="1.0" encoding="utf-8"?>
         <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
                        xmlns:epat="urn:EPATInterfaceWrappers">
           <soap:Body>
             <epat:criarNotificacoesAIIMRequest>
               <epat:BODY>
                 <epat:idAiim>{caseRef.IdAiim}</epat:idAiim>
                 <epat:caseNumber>{caseRef.ProcessId}</epat:caseNumber>
               </epat:BODY>
             </epat:criarNotificacoesAIIMRequest>
           </soap:Body>
         </soap:Envelope>
         """;

    private static ServiceEnvelope ParseSoapResponse(string responseBody)
    {
        try
        {
            var doc  = XDocument.Parse(responseBody);
            var ns   = (XNamespace)"urn:EPATInterfaceWrappers";
            var body = doc.Descendants(ns + "criarNotificacoesAIIMResponse").FirstOrDefault();

            if (body is null)
                return new ServiceEnvelope(STATUS_CODE: "PARSE_ERROR",
                    STERRORCODE: "SOAP_PARSE",
                    STERRORDESC: "Resposta SOAP sem corpo esperado (criarNotificacoesAIIMResponse).");

            var resultEl   = body.Element(ns + "RESULT");
            var statusCode = resultEl?.Element(ns + "STATUS_CODE")?.Value ?? "UNKNOWN";
            var errorEl    = resultEl?.Element(ns + "ERROR");
            var errorCode  = errorEl?.Element(ns + "ERROR_CODE")?.Value;
            var errorDesc  = errorEl?.Element(ns + "ERROR_DESCRIPTION")?.Value;

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
