#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Implementacao SOAP da operacao CalcularPrazo.
/// Traduz a invocacao do passo _AsZCkVqkEfG5K7mY0I3I6w para SOAP/HTTP.
/// </summary>
public sealed class CalcularPrazoSoapService : ICalcularPrazoService
{
    private readonly HttpClient _http;

    public CalcularPrazoSoapService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ServiceEnvelope> CalcularPrazoAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        var soapBody = BuildSoapRequest(caseRef);

        using var request = new HttpRequestMessage(HttpMethod.Post, "calcularPrazo")
        {
            Content = new StringContent(soapBody, System.Text.Encoding.UTF8, "text/xml"),
        };
        request.Headers.Add("SOAPAction",
            "\"__sol_EPATInterfaceWrappers_sol_obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEAT.1\"");

        using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        var responseXml = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        return ParseSoapResponse(responseXml);
    }

    private static string BuildSoapRequest(AiimCaseRef caseRef)
    {
        var transactionId = Guid.NewGuid().ToString("N");
        var datetime = DateTimeOffset.UtcNow.ToString("o");

        return $"""
            <soapenv:Envelope
                xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                xmlns:req="http://www.tibco.com/schemas/EPATInterfaceWrappers/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEAT/request">
              <soapenv:Body>
                <req:request>
                  <HEADER>
                    <TRANSACTION_ID>{transactionId}</TRANSACTION_ID>
                    <PROCESS_ID>{System.Security.SecurityElement.Escape(caseRef.ProcessId)}</PROCESS_ID>
                    <DATETIME>{datetime}</DATETIME>
                  </HEADER>
                  <BODY>
                    <dataInicioPeriodo />
                    <periodoEmDias>0</periodoEmDias>
                    <codigoMunicipio>0</codigoMunicipio>
                  </BODY>
                </req:request>
              </soapenv:Body>
            </soapenv:Envelope>
            """;
    }

    private static ServiceEnvelope ParseSoapResponse(string xml)
    {
        var doc = new System.Xml.XmlDocument();
        doc.LoadXml(xml);

        var statusCode = doc.SelectSingleNode("//*[local-name()='STATUS_CODE']/text()")?.Value;
        var errorCode = doc.SelectSingleNode("//*[local-name()='ERROR_CODE']/text()")?.Value
            ?? doc.SelectSingleNode("//*[local-name()='STERRORCODE']/text()")?.Value;
        var errorDesc = doc.SelectSingleNode("//*[local-name()='ERROR_DESCRIPTION']/text()")?.Value
            ?? doc.SelectSingleNode("//*[local-name()='STERRORDESC']/text()")?.Value;

        return new ServiceEnvelope(
            STATUS_CODE: statusCode,
            STERRORCODE: errorCode,
            STERRORDESC: errorDesc);
    }
}
