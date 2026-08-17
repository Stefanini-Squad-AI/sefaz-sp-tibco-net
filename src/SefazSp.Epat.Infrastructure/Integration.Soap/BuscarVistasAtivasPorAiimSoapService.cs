#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Implementacao SOAP da operacao buscarVistasAtivasPorAiim.1 (EPAT.wsdl).
/// Faz parte da implementacao concreta de <see cref="IEpatServices"/> para o canal SOAP/HTTP.
///
/// Contrato WSDL:
///   - Operacao: __sol_EPATInterfaceWrappers_sol_buscarVistasAtivasPorAiim.1
///   - Input  element: ns218:request  (buscarVistasAtivasPorAiimRequest)
///     - HEADER/TRANSACTION_ID, HEADER/DATETIME, HEADER/APPLICATIONDATAS, BODY/nrAiim (xs:long)
///   - Output element: ns219:response (buscarVistasAtivasPorAiimResponse)
///     - BODY/ListaEmailsConcatenados, BODY/ListaProcessos/*, RESULT/STATUS_CODE,
///       RESULT/ERROR/SERVICE_NAME, RESULT/ERROR/ERROR_CODE, RESULT/ERROR/ERROR_DESCRIPTION,
///       RESULT/ERROR/ERROR_STACKTRACE, RESULT/ERROR/PROCESS_STACK, RESULT/ERROR/DUMP_ANALYSIS
///
/// A porta <see cref="IEpatServices"/> e transcricao do WSDL e nao se mexe (status final).
/// Esta classe e a unica que conhece o transporte SOAP/HTTP.
/// </summary>
public sealed class BuscarVistasAtivasPorAiimSoapService
{
    private readonly HttpClient _http;

    /// <param name="http">
    /// Cliente HTTP pre-configurado com o endpoint do TIBCO BusinessWorks (base address,
    /// timeouts e headers de autenticacao configurados no registo de infraestrutura).
    /// </param>
    public BuscarVistasAtivasPorAiimSoapService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Invoca a operacao buscarVistasAtivasPorAiim.1 via SOAP/HTTP.
    /// Devolve o envelope tecnico mapeado de <c>RESULT/STATUS_CODE</c>,
    /// <c>RESULT/ERROR/ERROR_CODE</c> e <c>RESULT/ERROR/ERROR_DESCRIPTION</c>.
    /// </summary>
    /// <param name="caseRef">Identidade do caso (NRAIIM = caseRef.IdAiim, PROCESS_ID para correlacao).</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// <see cref="ServiceEnvelope"/> com STATUS_CODE, STERRORCODE e STERRORDESC extraidos da resposta.
    /// STATUS_CODE == "0" indica sucesso; qualquer outro valor indica erro aplicacional.
    /// </returns>
    public async Task<ServiceEnvelope> InvokeAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        var soapBody = BuildSoapRequest(caseRef);

        using var request = new HttpRequestMessage(HttpMethod.Post, "buscarVistasAtivasPorAiim")
        {
            Content = new StringContent(soapBody, System.Text.Encoding.UTF8, "text/xml"),
        };
        request.Headers.Add("SOAPAction",
            "\"__sol_EPATInterfaceWrappers_sol_buscarVistasAtivasPorAiim.1\"");

        using var response = await _http.SendAsync(request, ct);
        var responseXml    = await response.Content.ReadAsStringAsync(ct);

        return ParseSoapResponse(responseXml);
    }

    // ── Construcao do envelope SOAP ─────────────────────────────────────────

    private static string BuildSoapRequest(AiimCaseRef caseRef)
    {
        // Campos obrigatorios: TRANSACTION_ID, DATETIME, APPLICATIONDATA, BODY/nrAiim
        var transactionId = Guid.NewGuid().ToString("N");
        var datetime      = DateTimeOffset.UtcNow.ToString("o");

        return $"""
            <soapenv:Envelope
                xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                xmlns:ns218="http://www.tibco.com/schemas/EPATInterfaceWrappers/buscarVistasAtivasPorAiim/request"
                xmlns:ns219="http://www.tibco.com/schemas/EPATInterfaceWrappers/buscarVistasAtivasPorAiim/response">
              <soapenv:Body>
                <ns218:request>
                  <HEADER>
                    <TRANSACTION_ID>{transactionId}</TRANSACTION_ID>
                    <DATETIME>{datetime}</DATETIME>
                    <APPLICATIONDATAS>
                      <APPLICATIONDATA>
                        <NAME>PROCESS_ID</NAME>
                        <VALUE>{System.Security.SecurityElement.Escape(caseRef.ProcessId)}</VALUE>
                      </APPLICATIONDATA>
                    </APPLICATIONDATAS>
                  </HEADER>
                  <BODY>
                    <nrAiim>{caseRef.IdAiim}</nrAiim>
                  </BODY>
                </ns218:request>
              </soapenv:Body>
            </soapenv:Envelope>
            """;
    }

    // ── Analise da resposta SOAP ────────────────────────────────────────────

    private static ServiceEnvelope ParseSoapResponse(string xml)
    {
        // Extraccao minima baseada em XPath para evitar dependencia de WCF.
        // A validacao completa do contrato esta em BuscarVistasAtivasPorAiimContractTests.
        var doc = new System.Xml.XmlDocument();
        doc.LoadXml(xml);

        var ns = new System.Xml.XmlNamespaceManager(doc.NameTable);
        ns.AddNamespace("ns219",
            "http://www.tibco.com/schemas/EPATInterfaceWrappers/buscarVistasAtivasPorAiim/response");

        var statusCode  = doc.SelectSingleNode("//ns219:response/RESULT/STATUS_CODE/text()",   ns)?.Value;
        var errorCode   = doc.SelectSingleNode("//ns219:response/RESULT/ERROR/ERROR_CODE/text()", ns)?.Value;
        var errorDesc   = doc.SelectSingleNode("//ns219:response/RESULT/ERROR/ERROR_DESCRIPTION/text()", ns)?.Value;

        return new ServiceEnvelope(
            STATUS_CODE:  statusCode,
            STERRORCODE:  errorCode,
            STERRORDESC:  errorDesc);
    }
}
