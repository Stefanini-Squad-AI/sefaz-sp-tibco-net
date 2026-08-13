#nullable enable

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Application.Abstractions.Services;

/// <summary>
/// Porta do serviço CalcularPrazo (EPAT.wsdl).
/// Transcricao do WSDL — não tocar.
/// </summary>
public interface ICalcularPrazoService
{
    Task<ServiceEnvelope> CalcularPrazoAsync(AiimCaseRef caseRef, CancellationToken ct);
}
