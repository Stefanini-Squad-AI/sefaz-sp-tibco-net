#nullable enable

// Duble de integração para o processo DEAT0050 (INOTFAIIM).
// Preenche o papel "CalculaPrazo" (callActivity _lrer3lqhEfG5K7mY0I3I6w).
//
// Decisao gaps.dynamic-subprocess = interface-registry-validated:
//   Destino sem duble registado quebra o teste de registo no arranque, de forma visivel.
//   NAO herda HaltOnBadSubProcess=false do legado TIBCO.

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble conduzido por cenario para a callActivity <c>CalculaPrazo</c> do processo DEAT0050.
///
/// DEAT0050 usa <see cref="INOTFAIIM"/> para chamar o subprocesso de calculo de prazo.
/// Este duble devolve o resultado configurado pelo cenario activo, sem logica propria.
///
/// Configure o cenario antes de cada teste; uma chamada sem cenario activo lanca
/// excecao imediata e identificavel.
/// </summary>
public sealed class Deat0050CalculaPrazoDouble : INOTFAIIM
{
    private readonly Dictionary<string, ProcessCallResult> _scenarios = new();
    private string? _activeScenario;

    /// <summary>
    /// Regista um resultado pre-configurado para o cenario identificado por <paramref name="scenarioKey"/>.
    /// </summary>
    public Deat0050CalculaPrazoDouble WithScenario(string scenarioKey, ProcessCallResult result)
    {
        _scenarios[scenarioKey] = result;
        return this;
    }

    /// <summary>
    /// Activa o cenario a devolver na proxima chamada a <see cref="ExecuteAsync"/>.
    /// </summary>
    public void SetActiveScenario(string scenarioKey)
        => _activeScenario = scenarioKey;

    /// <inheritdoc/>
    public Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_activeScenario is null || !_scenarios.TryGetValue(_activeScenario, out var result))
            throw new InvalidOperationException(
                $"[Deat0050CalculaPrazoDouble] Nenhum cenario activo configurado para caseRef={caseRef}. " +
                $"Chame {nameof(WithScenario)} e {nameof(SetActiveScenario)} antes de invocar. " +
                "Destino sem duble falha visivelmente (gaps.dynamic-subprocess = interface-registry-validated).");

        return Task.FromResult(result);
    }
}
