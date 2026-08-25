#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Workflows.CALCPRPC;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble em memória de <see cref="ICalcularPrazoSoapService"/> para a PoC.
/// Por omissão devolve erro de aplicação (STATUS_CODE = "1"), conduzindo o laço de retry
/// de CALCPRPC até esgotar. Substituível pela implementação SOAP real.
/// </summary>
public sealed class CalcularPrazoServiceDouble : ICalcularPrazoSoapService
{
    private readonly ServiceEnvelope _envelope;

    public CalcularPrazoServiceDouble(ServiceEnvelope? envelope = null)
        => _envelope = envelope ?? new ServiceEnvelope("1", "APP", "erro de aplicação simulado");

    public Task<ServiceEnvelope> InvokeAsync(AiimCaseRef caseRef, CancellationToken ct)
        => Task.FromResult(_envelope);
}
