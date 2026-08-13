#nullable enable

using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Implementacao SOAP/JMS da operacao 'buscarVistasAtivasPorAiim' do EPAT.wsdl.
///
/// Corresponde ao serviceTask 'Busca Envolvidos Vista Por AIIM'
/// (_qIDu5F6BEfGBBLgT-R5iuw) no processo BSCENVPC.
///
/// A porta IEpatServices (status: final — transcricao do WSDL) e injectada em
/// EpatSoapServices, que agrega as 5 operacoes declaradas no IEpatServices.
/// Esta classe contem apenas a logica de transporte da operacao buscarVistasAtivasPorAiim.
///
/// Fonte TIBCO: EPAT.wsdl
///   operacao: __sol_EPATInterfaceWrappers_sol_buscarVistasAtivasPorAiim.1
/// </summary>
internal sealed class BuscaEnvolvidosVistaTransport
{
    private readonly HttpClient _httpClient;
    private readonly string _endpointUrl;

    public BuscaEnvolvidosVistaTransport(HttpClient httpClient, string endpointUrl)
    {
        _httpClient = httpClient;
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
            "\"__sol_EPATInterfaceWrappers_sol_buscarVistasAtivasPorAiim.1\"");

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
             <epat:buscarVistasAtivasPorAiimRequest>
               <epat:idAiim>{caseRef.IdAiim}</epat:idAiim>
               <epat:processId>{caseRef.ProcessId}</epat:processId>
             </epat:buscarVistasAtivasPorAiimRequest>
           </soap:Body>
         </soap:Envelope>
         """;

    private static ServiceEnvelope ParseSoapResponse(string responseBody)
    {
        try
        {
            var doc = XDocument.Parse(responseBody);
            var ns = (XNamespace)"urn:EPATInterfaceWrappers";
            var body = doc.Descendants(ns + "buscarVistasAtivasPorAiimResponse").FirstOrDefault();

            if (body is null)
                return new ServiceEnvelope(STATUS_CODE: "PARSE_ERROR",
                    STERRORCODE: "SOAP_PARSE",
                    STERRORDESC: "Resposta SOAP sem corpo esperado.");

            var statusCode = body.Element(ns + "STATUS_CODE")?.Value ?? "UNKNOWN";
            var errorCode = body.Element(ns + "STERRORCODE")?.Value;
            var errorDesc = body.Element(ns + "STERRORDESC")?.Value;

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

/// <summary>
/// Implementacao de <see cref="IEpatServices"/> via SOAP/JMS para o ambiente de producao.
/// Agrega os 5 transportes declarados na porta (IEpatServices, status: final).
///
/// Injectado como Scoped no DI; cada HttpClient tem o timeout e headers configurados
/// no registo da infraestrutura.
/// </summary>
public sealed class EpatSoapServices : IEpatServices
{
    private readonly HttpClient _httpClient;
    private readonly EpatSoapOptions _options;

    public EpatSoapServices(HttpClient httpClient, EpatSoapOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    /// <inheritdoc />
    public Task<ServiceEnvelope> BuscarvistasativasporaiimAsync(
        AiimCaseRef caseRef, CancellationToken ct)
    {
        var transport = new BuscaEnvolvidosVistaTransport(
            _httpClient, _options.BuscarVistasAtivasPorAiimEndpoint);
        return transport.InvokeAsync(caseRef, ct);
    }

    /// <inheritdoc />
    public Task<ServiceEnvelope> PrepararintimacaoAsync(
        AiimCaseRef caseRef, CancellationToken ct)
        => throw new NotImplementedException(
            "PrepararintimacaoAsync: implementacao SOAP pendente (outro card).");

    /// <inheritdoc />
    public Task<ServiceEnvelope> AtualizarintimacaoAsync(
        AiimCaseRef caseRef, CancellationToken ct)
        => throw new NotImplementedException(
            "AtualizarintimacaoAsync: implementacao SOAP pendente (outro card).");

    /// <inheritdoc />
    public Task<ServiceEnvelope> CriarnotificacoesaiimAsync(
        AiimCaseRef caseRef, CancellationToken ct)
    {
        var transport = new CriarNotificacoesAiimTransport(
            _httpClient, _options.CriarNotificacoesAiimEndpoint);
        return transport.InvokeAsync(caseRef, ct);
    }

    /// <inheritdoc />
    public Task<ServiceEnvelope> ObterprimeirodiautilaposperiododediascorridosdeatAsync(
        AiimCaseRef caseRef, CancellationToken ct)
        => throw new NotImplementedException(
            "ObterprimeirodiautilaposperiododediascorridosdeatAsync: implementacao SOAP pendente (outro card).");
}

/// <summary>
/// Opcoes de configuracao para EpatSoapServices.
/// </summary>
public sealed class EpatSoapOptions
{
    /// <summary>URL do endpoint SOAP buscarVistasAtivasPorAiim.</summary>
    public string BuscarVistasAtivasPorAiimEndpoint { get; init; } = string.Empty;

    /// <summary>URL do endpoint SOAP criarNotificacoesAIIM.</summary>
    public string CriarNotificacoesAiimEndpoint { get; init; } = string.Empty;
}
