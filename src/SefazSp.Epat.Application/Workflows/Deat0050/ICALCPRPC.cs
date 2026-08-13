#nullable enable

// Card: BUILD-DEAT0050-seg013
// Interface de processo CALCPRPC — ainda nao gerada pelo scaffold.
// NOTA: este ficheiro deve ser movido para
//   src/SefazSp.Epat.Application/Abstractions/Processes/ICALCPRPC.cs
// quando o scaffold for regenerado com CALCPRPC incluido.
// O namespace e mantido em Abstractions.Processes para alinhamento com IAGURETPC, ICTRINTPC, INOTFAIIM.

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Application.Abstractions.Processes;

/// <summary>
/// Interface de processo CALCPRPC (Calculo de Prazo por Processo).
/// Chamado estaticamente pelo callActivity CalculaPrazo (_lrer3lqhEfG5K7mY0I3I6w) do DEAT0050.
/// Recalcula o prazo de defesa do AIIM (PRAZODEFESA, PRAZODEFESAT) a cada volta do laco.
/// Implementacoes concretas pertencem ao pacote CALCPRPC.
/// </summary>
public interface ICALCPRPC
{
    Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct);
}
