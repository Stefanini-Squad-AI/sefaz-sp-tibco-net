#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Workflows.CALCPRPC;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Implementação SOAP da operação CalcularPrazo invocada pelo serviceTask
/// _AsZCkVqkEfG5K7mY0I3I6w do processo CALCPRPC.
///
/// Esta classe é a única que conhece o transporte SOAP/HTTP para o serviço de
/// cálculo de prazo do AIIM. A porta de abstracção é injectada no workflow
/// via <c>ICalcularPrazoSoapService</c> (definida em Application/Workflows/CALCPRPC).
///
/// O envelope de resposta segue o padrão TIBCO BusinessWorks:
///   RESULT/STATUS_CODE : '0' = sucesso; qualquer outro valor = erro aplicacional.
///   RESULT/ERROR/ERROR_CODE, ERROR_DESCRIPTION: mapeados para STERRORCODE/STERRORDESC.
///
/// Excepção de transporte (HTTP/SOAP falha) → o chamador deve capturar e
/// encaminhar para o gateway Tech Error (_zJIHZVqiEfG5K7mY0I3I6w, entrouPor=regresso).
/// </summary>
public sealed class CalcularPrazoSoapService : ICalcularPrazoSoapService
{
    private readonly HttpClient _http;

    /// <param name="http">
    /// Cliente HTTP pré-configurado com o endpoint do TIBCO BusinessWorks
    /// (base address, timeouts e headers de autenticação configurados no registo de infraestrutura).
    /// </param>
    public CalcularPrazoSoapService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Invoca a operação CalcularPrazo via SOAP/HTTP.
    /// Devolve o envelope técnico mapeado de STATUS_CODE, STERRORCODE e STERRORDESC.
    /// </summary>
    /// <param name="caseRef">Identidade do caso (IdAiim e ProcessId para correlação).</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// <see cref="ServiceEnvelope"/> com STATUS_CODE, STERRORCODE e STERRORDESC.
    /// STATUS_CODE == "0" indica sucesso; qualquer outro valor indica erro aplicacional.
    /// </returns>
    public async Task<ServiceEnvelope> InvokeAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        var soapBody = BuildSoapRequest(caseRef);

        using var request = new HttpRequestMessage(HttpMethod.Post, (Uri?)null)
        {
            Content = new StringContent(soapBody, System.Text.Encoding.UTF8, "text/xml"),
        };
        request.Headers.Add("SOAPAction", "\"CalcularPrazo\"");

        using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        var responseXml    = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        return ParseSoapResponse(responseXml);
    }

    // ── Construção do envelope SOAP ──────────────────────────────────────────

    private static string BuildSoapRequest(AiimCaseRef caseRef)
    {
        var transactionId = Guid.NewGuid().ToString("N");
        var datetime      = DateTimeOffset.UtcNow.ToString("o");

        return $"""
            <soapenv:Envelope
                xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                xmlns:epat="urn:EPATInterfaceWrappers">
              <soapenv:Body>
                <epat:CalcularPrazoRequest>
                  <HEADER>
                    <TRANSACTION_ID>{System.Security.SecurityElement.Escape(transactionId)}</TRANSACTION_ID>
                    <DATETIME>{System.Security.SecurityElement.Escape(datetime)}</DATETIME>
                  </HEADER>
                  <BODY>
                    <nrAiim>{caseRef.IdAiim}</nrAiim>
                    <processId>{System.Security.SecurityElement.Escape(caseRef.ProcessId)}</processId>
                  </BODY>
                </epat:CalcularPrazoRequest>
              </soapenv:Body>
            </soapenv:Envelope>
            """;
    }

    // ── Análise da resposta SOAP ─────────────────────────────────────────────

    private static ServiceEnvelope ParseSoapResponse(string responseXml)
    {
        // Extrai STATUS_CODE, ERROR_CODE e ERROR_DESCRIPTION do envelope TIBCO.
        // Procura na secção RESULT/STATUS_CODE e RESULT/ERROR/*.
        var statusCode  = ExtractElement(responseXml, "STATUS_CODE");
        var errorCode   = ExtractElement(responseXml, "ERROR_CODE");
        var errorDesc   = ExtractElement(responseXml, "ERROR_DESCRIPTION");

        return new ServiceEnvelope(statusCode, errorCode, errorDesc);
    }

    private static string? ExtractElement(string xml, string elementName)
    {
        var open  = $"<{elementName}>";
        var close = $"</{elementName}>";
        var start = xml.IndexOf(open, StringComparison.Ordinal);
        if (start < 0) return null;
        start += open.Length;
        var end = xml.IndexOf(close, start, StringComparison.Ordinal);
        if (end < 0) return null;
        return xml[start..end];
    }
}
