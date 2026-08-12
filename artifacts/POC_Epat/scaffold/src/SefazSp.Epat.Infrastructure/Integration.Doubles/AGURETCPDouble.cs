#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble conduzido por cenario para IAGURETPC.
/// Nao contem logica de negocio propria: devolve o resultado configurado pelo cenario activo.
/// Destino sem registo falha de forma visivel no arranque (Keyed DI com chave "AGURETPC").
/// </summary>
public sealed class AGURETCPDouble : IAGURETPC
{
    private readonly Dictionary<string, ProcessCallResult> _scenarios = new();
    private string? _activeScenario;

    /// <summary>Regista um resultado para o cenario identificado por <paramref name="scenarioKey"/>.</summary>
    public void ConfigureScenario(string scenarioKey, ProcessCallResult result)
        => _scenarios[scenarioKey] = result;

    /// <summary>Activa o cenario a devolver na proxima chamada a <see cref="ExecuteAsync"/>.</summary>
    public void SetActiveScenario(string scenarioKey)
        => _activeScenario = scenarioKey;

    /// <inheritdoc/>
    public Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        if (_activeScenario is null || !_scenarios.TryGetValue(_activeScenario, out var result))
            throw new InvalidOperationException(
                $"AGURETCPDouble: nenhum cenario activo configurado. " +
                $"Chame {nameof(ConfigureScenario)} e {nameof(SetActiveScenario)} antes de invocar.");

        return Task.FromResult(result);
    }
}
