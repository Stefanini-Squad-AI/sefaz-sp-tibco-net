#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

public sealed class EpatSoapServices : IEpatServices
{
    private readonly Func<AiimCaseRef, ServiceEnvelope>? _atualizarIntimacaoResponse;

    public EpatSoapServices(Func<AiimCaseRef, ServiceEnvelope>? atualizarIntimacaoResponse = null)
    {
        _atualizarIntimacaoResponse = atualizarIntimacaoResponse;
    }

    public Task<ServiceEnvelope> PrepararintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        Task.FromResult(new ServiceEnvelope("0", null, null));

    public Task<ServiceEnvelope> AtualizarintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        var envelope = _atualizarIntimacaoResponse?.Invoke(caseRef) ?? new ServiceEnvelope("0", null, null);
        return Task.FromResult(envelope);
    }

    public Task<ServiceEnvelope> BuscarvistasativasporaiimAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        Task.FromResult(new ServiceEnvelope("0", null, null));

    public Task<ServiceEnvelope> CriarnotificacoesaiimAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        Task.FromResult(new ServiceEnvelope("0", null, null));

    public Task<ServiceEnvelope> ObterprimeirodiautilaposperiododediascorridosdeatAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        Task.FromResult(new ServiceEnvelope("0", null, null));
}
