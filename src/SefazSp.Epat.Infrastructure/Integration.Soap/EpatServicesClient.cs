#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Implementacao SOAP/JMS das 5 operacoes catalogadas em IEpatServices.
/// Cada metodo traduz a chamada para o endpoint BusinessWorks correspondente
/// e devolve o envelope tecnico STATUS_CODE/STERRORCODE/STERRORDESC.
///
/// Operacoes cobertas por este segmento (BUILD-BSCENVPC-seg003):
///   - BuscarvistasativasporaiimAsync  (_qIDu5F6BEfGBBLgT-R5iuw)
///
/// Fonte: EPAT.wsdl, operacao buscarVistasAtivasPorAiim.1
/// </summary>
public sealed class EpatServicesClient : IEpatServices
{
    private readonly HttpClient _http;

    public EpatServicesClient(HttpClient http)
    {
        _http = http;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Operacao TIBCO: __sol_EPATInterfaceWrappers_sol_buscarVistasAtivasPorAiim.1
    /// Declarada em EPAT.wsdl.
    /// Invocada pelo passo _qIDu5F6BEfGBBLgT-R5iuw (Busca Envolvidos Vista Por AIIM).
    /// STATUS_CODE='0' indica sucesso; qualquer outro valor activa o ramo AppError.
    /// Excepcao de transporte (HTTP/SOAP falha) sinaliza TechError — o chamador
    /// deve capturar e encaminhar para o gateway Tech Error (_qIDupF6BEfGBBLgT-R5iuw).
    /// </remarks>
    public async Task<ServiceEnvelope> BuscarvistasativasporaiimAsync(
        AiimCaseRef caseRef,
        CancellationToken ct)
    {
        // SOAP envelope minimo para a operacao buscarVistasAtivasPorAiim.
        // O endpoint e configurado via HttpClient base address (composicao externa).
        var soapBody = BuildSoapEnvelope("buscarVistasAtivasPorAiim", caseRef);

        using var request = new HttpRequestMessage(HttpMethod.Post, (Uri?)null)
        {
            Content = new StringContent(soapBody, System.Text.Encoding.UTF8, "text/xml"),
        };
        request.Headers.Add("SOAPAction", "buscarVistasAtivasPorAiim");

        using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return ParseEnvelope(xml);
    }

    /// <inheritdoc />
    public Task<ServiceEnvelope> PrepararintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct)
        => throw new NotImplementedException(
            "PrepararintimacaoAsync nao pertence a este segmento (BUILD-BSCENVPC-seg003).");

    /// <inheritdoc />
    public Task<ServiceEnvelope> AtualizarintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct)
        => throw new NotImplementedException(
            "AtualizarintimacaoAsync nao pertence a este segmento (BUILD-BSCENVPC-seg003).");

    /// <inheritdoc />
    public Task<ServiceEnvelope> CriarnotificacoesaiimAsync(AiimCaseRef caseRef, CancellationToken ct)
        => throw new NotImplementedException(
            "CriarnotificacoesaiimAsync nao pertence a este segmento (BUILD-BSCENVPC-seg003).");

    /// <inheritdoc />
    public Task<ServiceEnvelope> ObterprimeirodiautilaposperiododediascorridosdeatAsync(
        AiimCaseRef caseRef,
        CancellationToken ct)
        => throw new NotImplementedException(
            "ObterprimeirodiautilaposperiododediascorridosdeatAsync nao pertence a este segmento.");

    // -------------------------------------------------------------------------

    private static string BuildSoapEnvelope(string operation, AiimCaseRef caseRef)
    {
        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
                           xmlns:epat="http://EPATInterfaceWrappers">
              <soap:Body>
                <epat:{operation}>
                  <idAiim>{caseRef.IdAiim}</idAiim>
                  <processId>{System.Security.SecurityElement.Escape(caseRef.ProcessId)}</processId>
                </epat:{operation}>
              </soap:Body>
            </soap:Envelope>
            """;
    }

    private static ServiceEnvelope ParseEnvelope(string xml)
    {
        // Leitura minima do envelope tecnico: STATUS_CODE, STERRORCODE, STERRORDESC.
        // Parsing via XmlDocument para manter zero dependencias externas neste modulo.
        var doc = new System.Xml.XmlDocument();
        doc.LoadXml(xml);

        var statusCode = doc.SelectSingleNode("//*[local-name()='STATUS_CODE']")?.InnerText;
        var errorCode = doc.SelectSingleNode("//*[local-name()='STERRORCODE']")?.InnerText;
        var errorDesc = doc.SelectSingleNode("//*[local-name()='STERRORDESC']")?.InnerText;

        return new ServiceEnvelope(statusCode, errorCode, errorDesc);
    }
}
