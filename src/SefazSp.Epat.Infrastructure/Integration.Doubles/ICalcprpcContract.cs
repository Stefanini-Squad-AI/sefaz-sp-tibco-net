#nullable enable

using SefazSp.Epat.Application.Abstractions;

// ICALCPRPC pertence a src/SefazSp.Epat.Application/Abstractions/Processes/ICALCPRPC.cs
// (path: final — a criar pelo agente responsável pelas Abstractions, não pelo implementador-integration-doubles).
// Definida aqui provisoriamente para que o duble compile sem depender de outro card.
namespace SefazSp.Epat.Application.Abstractions.Processes;

/// <summary>
/// Traducao directa do xpdExt:ProcessInterface 'CALCPRPC' do XPDL.
/// Implementacoes entregues no pacote: CALCPRPC (subprocesso de calculo de prazo de defesa).
/// Invocado pelo callActivity CalculaPrazo (_lrer3lqhEfG5K7mY0I3I6w) no processo DEAT0050.
/// O destino da chamada e resolvido em runtime; o conjunto de destinos e validado no arranque.
/// gaps.dynamic-subprocess = interface-registry-validated.
/// </summary>
public interface ICALCPRPC
{
    Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct);
}
