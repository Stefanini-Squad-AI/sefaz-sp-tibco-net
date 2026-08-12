#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Stub SOAP adapter for the POC. The production HTTP/SOAP call is intentionally out of scope;
/// tests can inject the desired result for the Buscar Vistas Ativas por AIIM operation.
/// </summary>
public sealed class BuscarVistasAtivasPorAiimSoapService : IEpatServices
{
    private readonly Func<AiimCaseRef, CancellationToken, Task<ServiceEnvelope>> _buscarVistasHandler;

    public BuscarVistasAtivasPorAiimSoapService()
        : this((_, _) => throw new NotImplementedException(
            "SOAP transport is out of scope for the POC. Inject a handler or a fixed ServiceEnvelope."))
    {
    }

    public BuscarVistasAtivasPorAiimSoapService(ServiceEnvelope envelope)
        : this((_, ct) => Task.FromResult(envelope).WaitAsync(ct))
    {
    }

    public BuscarVistasAtivasPorAiimSoapService(
        Func<AiimCaseRef, CancellationToken, Task<ServiceEnvelope>> buscarVistasHandler)
    {
        _buscarVistasHandler = buscarVistasHandler ?? throw new ArgumentNullException(nameof(buscarVistasHandler));
    }

    public Task<ServiceEnvelope> BuscarvistasativasporaiimAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        _buscarVistasHandler(caseRef, ct);

    public Task<ServiceEnvelope> PrepararintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        throw CreateOutOfScopeException(nameof(PrepararintimacaoAsync));

    public Task<ServiceEnvelope> AtualizarintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        throw CreateOutOfScopeException(nameof(AtualizarintimacaoAsync));

    public Task<ServiceEnvelope> CriarnotificacoesaiimAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        throw CreateOutOfScopeException(nameof(CriarnotificacoesaiimAsync));

    public Task<ServiceEnvelope> ObterprimeirodiautilaposperiododediascorridosdeatAsync(
        AiimCaseRef caseRef,
        CancellationToken ct) =>
        throw CreateOutOfScopeException(nameof(ObterprimeirodiautilaposperiododediascorridosdeatAsync));

    private static NotSupportedException CreateOutOfScopeException(string operationName) =>
        new($"Operation '{operationName}' is outside the scope of BUILD-BSCENVPC-seg002.");
}
