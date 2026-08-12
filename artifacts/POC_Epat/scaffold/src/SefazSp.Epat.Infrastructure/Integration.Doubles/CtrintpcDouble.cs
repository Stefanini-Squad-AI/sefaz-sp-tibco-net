#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble conduzido por cenario para a interface de processo CTRINTPC (Controle de Intimacoes por Processo).
/// Implementacoes concretas sao entregues pelo pacote CONTROPC em producao.
///
/// O duble aceita cenarios injectados externamente e devolve respostas pre-configuradas,
/// sem qualquer logica de negocio propria. Qualquer caseRef sem cenario configurado
/// produz uma excecao explicita, tornando visivel a ausencia de setup de teste.
/// </summary>
public sealed class CtrintpcDouble : ICTRINTPC
{
    private readonly Dictionary<AiimCaseRef, ProcessCallResult> _scenarios = new();

    /// <summary>
    /// Configura o cenario para um dado <paramref name="caseRef"/>.
    /// Deve ser chamado antes da execucao do teste.
    /// </summary>
    public CtrintpcDouble WithScenario(AiimCaseRef caseRef, ProcessCallResult result)
    {
        _scenarios[caseRef] = result;
        return this;
    }

    /// <inheritdoc />
    public Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        if (!_scenarios.TryGetValue(caseRef, out var result))
        {
            throw new InvalidOperationException(
                $"[CtrintpcDouble] Nenhum cenario configurado para caseRef={caseRef}. " +
                $"Registe o cenario com WithScenario() antes de executar o teste.");
        }

        return Task.FromResult(result);
    }
}
