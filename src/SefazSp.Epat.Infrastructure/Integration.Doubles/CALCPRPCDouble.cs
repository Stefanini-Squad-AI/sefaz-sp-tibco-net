#nullable enable

// BUILD-DEAT0050-seg012 — duble para o callActivity CalculaPrazo → CALCPRPC
// CALCPRPC é uma chamada de subprocesso estática (dinamica=false).
// A interface ICALCPRPC deve ser declarada em
//   src/SefazSp.Epat.Application/Abstractions/Processes
// quando o molde-subprocesso-de-servico entregar o scaffold de CALCPRPC.
//
// Este duble segue o mesmo padrão de AGURETCPDouble: conduzido por cenário,
// sem lógica de negócio própria.

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble conduzido por cenário para a interface de processo CALCPRPC.
/// Chamado pelo callActivity CalculaPrazo do DEAT0050.
/// Não contém lógica de negócio própria: devolve o resultado configurado pelo cenário activo.
///
/// DEPENDÊNCIA: requer ICALCPRPC de src/SefazSp.Epat.Application/Abstractions/Processes.
/// O scaffold de CALCPRPC é responsabilidade do molde-subprocesso-de-servico.
/// </summary>
public sealed class CALCPRPCDouble : ICALCPRPC
{
    private readonly Dictionary<string, ProcessCallResult> _scenarios = new();
    private string? _activeScenario;

    /// <summary>Regista um resultado para o cenário identificado por <paramref name="scenarioKey"/>.</summary>
    public void ConfigureScenario(string scenarioKey, ProcessCallResult result)
        => _scenarios[scenarioKey] = result;

    /// <summary>Activa o cenário a devolver na próxima chamada a <see cref="ExecuteAsync"/>.</summary>
    public void SetActiveScenario(string scenarioKey)
        => _activeScenario = scenarioKey;

    /// <inheritdoc/>
    public Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_activeScenario is null || !_scenarios.TryGetValue(_activeScenario, out var result))
            throw new InvalidOperationException(
                $"CALCPRPCDouble: nenhum cenário activo configurado. " +
                $"Chame {nameof(ConfigureScenario)} e {nameof(SetActiveScenario)} antes de invocar.");

        return Task.FromResult(result);
    }
}
