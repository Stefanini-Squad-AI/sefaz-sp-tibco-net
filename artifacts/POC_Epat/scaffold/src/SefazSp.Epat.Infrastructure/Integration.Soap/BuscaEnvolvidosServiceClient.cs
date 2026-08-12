#nullable enable

using System.Collections.ObjectModel;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

public sealed class BuscaEnvolvidosServiceClient : IEpatServices
{
    private static readonly Func<AiimCaseRef, CancellationToken, Task<ServiceEnvelope>> DefaultBuscarHandler =
        static (_, _) => Task.FromResult(new ServiceEnvelope("0", null, null));

    private readonly HttpClient _httpClient;
    private readonly Func<AiimCaseRef, CancellationToken, Task<ServiceEnvelope>> _buscarHandler;
    private readonly List<SoapServiceCallTrace> _calls = [];

    public BuscaEnvolvidosServiceClient(
        HttpClient httpClient,
        Func<AiimCaseRef, CancellationToken, Task<ServiceEnvelope>>? buscarHandler = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _buscarHandler = buscarHandler ?? DefaultBuscarHandler;
    }

    public ReadOnlyCollection<SoapServiceCallTrace> Calls => _calls.AsReadOnly();

    public Task<ServiceEnvelope> PrepararintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        Task.FromException<ServiceEnvelope>(new NotSupportedException("Operation not implemented in this PoC client."));

    public Task<ServiceEnvelope> AtualizarintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        Task.FromException<ServiceEnvelope>(new NotSupportedException("Operation not implemented in this PoC client."));

    public async Task<ServiceEnvelope> BuscarvistasativasporaiimAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        _calls.Add(new SoapServiceCallTrace(
            OperationName: nameof(BuscarvistasativasporaiimAsync),
            CaseRef: caseRef,
            Endpoint: _httpClient.BaseAddress?.ToString()));

        return await _buscarHandler(caseRef, ct).ConfigureAwait(false);
    }

    public Task<ServiceEnvelope> CriarnotificacoesaiimAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        Task.FromException<ServiceEnvelope>(new NotSupportedException("Operation not implemented in this PoC client."));

    public Task<ServiceEnvelope> ObterprimeirodiautilaposperiododediascorridosdeatAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        Task.FromException<ServiceEnvelope>(new NotSupportedException("Operation not implemented in this PoC client."));
}

public sealed record SoapServiceCallTrace(string OperationName, AiimCaseRef CaseRef, string? Endpoint);
