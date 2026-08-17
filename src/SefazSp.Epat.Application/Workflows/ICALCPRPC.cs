#nullable enable

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Application.Abstractions.Processes;

/// <summary>
/// Interface de processo CALCPRPC (Calculo de Prazo por Processo).
/// Exposta em caminho compilado pelo scaffold ate a camada canonica de
/// Abstractions/Processes ser entregue.
/// </summary>
public interface ICALCPRPC
{
    Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct);
}
