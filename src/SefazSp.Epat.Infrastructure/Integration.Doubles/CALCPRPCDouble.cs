#nullable enable

// Card: BUILD-DEAT0050-seg013
// Duble de processo CALCPRPC para testes de DEAT0050

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble conduzido por cenario para a interface de processo CALCPRPC
/// (Calculo de Prazo por Processo).
/// Invocado pelo callActivity CalculaPrazo (_lrer3lqhEfG5K7mY0I3I6w) do DEAT0050.
///
/// O duble nao contem logica de negocio propria: devolve o resultado pre-configurado
/// pelo cenario activo. Qualquer invocacao sem cenario configurado produz uma
/// excecao explicita, tornando visivel a ausencia de setup de teste.
///
/// O double espelha o padrao de AGURETCPDouble / CtrintpcDouble.
/// </summary>
public sealed class CALCPRPCDouble : ICALCPRPC
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
        ct.ThrowIfCancellationRequested();

        if (_activeScenario is null || !_scenarios.TryGetValue(_activeScenario, out var result))
            throw new InvalidOperationException(
                $"CALCPRPCDouble: nenhum cenario activo configurado. " +
                $"Chame {nameof(ConfigureScenario)} e {nameof(SetActiveScenario)} antes de invocar (pacote DEAT0050).");

        return Task.FromResult(result);
    }
}
