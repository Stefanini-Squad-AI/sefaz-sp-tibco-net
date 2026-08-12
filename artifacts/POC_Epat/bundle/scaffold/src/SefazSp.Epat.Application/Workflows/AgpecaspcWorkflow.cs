#nullable enable

using SefazSp.Epat.Application.Abstractions.Runtime;
using SefazSp.Epat.Application.Execution.Agpecaspc;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows;

/// <summary>
/// Topologia do processo AGPECASPC: Start Event → Set Values → [loop gateway] →
/// [decision gateway] → SetPrazo → Aguardar Interposicoes (receive task with boundary timer) →
/// [timer fires] → Set Flag Decurso → Controla Datas.
///
/// O timerEvent _EvOwRF6eEfGJqLUhfbpFcQ é ligado explicitamente como boundary event
/// sobre Aguardar Interposicoes — não existe como transição XPDL.
/// </summary>
public sealed class AgpecaspcWorkflow
{
    public const string NodeStartEvent = "_i4UpgF9IEfGqPfX31TKC3w";
    public const string NodeSetValues = "_EvOwTF6eEfGJqLUhfbpFcQ";
    public const string NodeLoopGateway = "_vshgkF6fEfGJqLUhfbpFcQ";
    public const string NodeDecisionGateway = "_EvOwVF6eEfGJqLUhfbpFcQ";
    public const string NodeSetPrazo = "_EvOwUl6eEfGJqLUhfbpFcQ";
    public const string NodeAguardarInterpos = "_EvOwQl6eEfGJqLUhfbpFcQ";
    public const string NodeTimerBoundary = "_EvOwRF6eEfGJqLUhfbpFcQ";
    public const string NodeSetFlagDecurso = "_EvOwWV6eEfGJqLUhfbpFcQ";
    public const string NodeControlaDatas = "_EvOwU16eEfGJqLUhfbpFcQ";

    private readonly SetPrazoStep _setPrazo;
    private readonly SetFlagDecursoStep _setFlagDecurso;
    private readonly ControlaDatasStep _controlaDatas;
    private readonly ICorrelationStore _correlationStore;
    private readonly IClock _clock;

    public AgpecaspcWorkflow(
        SetPrazoStep setPrazo,
        SetFlagDecursoStep setFlagDecurso,
        ControlaDatasStep controlaDatas,
        ICorrelationStore correlationStore,
        IClock clock)
    {
        _setPrazo = setPrazo;
        _setFlagDecurso = setFlagDecurso;
        _controlaDatas = controlaDatas;
        _correlationStore = correlationStore;
        _clock = clock;
    }

    /// <summary>
    /// Executa o troco completo de 9 passos do AGPECASPC.
    /// correlationKey: chave para resumo por evento externo.
    /// instanceId: identidade desta instância de workflow.
    /// timerDeadline: quando o boundary timer dispara.
    /// </summary>
    public async Task<AgpecaspcResult> ExecuteAsync(
        AiimCase aiimCase,
        string correlationKey,
        string instanceId,
        DateTimeOffset timerDeadline,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(aiimCase);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);

        AgpecaspcSetValuesRule.Apply(aiimCase);

        var timerFired = false;

        while (AgpecaspcGatewayDecisionRule.ShouldEnterWaitBranch(aiimCase))
        {
            _setPrazo.Execute(aiimCase);

            await _correlationStore.RegisterAsync(correlationKey, instanceId, ct);
            try
            {
                var delay = timerDeadline - _clock.Now;

                using var timerCts = new CancellationTokenSource(delay > TimeSpan.Zero ? delay : TimeSpan.Zero);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timerCts.Token);

                var timerTask = Task.Delay(Timeout.Infinite, linkedCts.Token);
                await timerTask.ContinueWith(_ => { }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

                ct.ThrowIfCancellationRequested();
                timerFired = timerCts.IsCancellationRequested;
            }
            finally
            {
                await _correlationStore.UnregisterAsync(correlationKey, ct);
            }

            if (timerFired)
            {
                break;
            }
        }

        if (timerFired)
        {
            _setFlagDecurso.Execute(aiimCase);
        }

        _controlaDatas.Execute(aiimCase);

        return new AgpecaspcResult(TimerFired: timerFired, DataControle: aiimCase.DATACONTROLE);
    }
}

/// <summary>Resultado do troco AGPECASPC.</summary>
public sealed record AgpecaspcResult(
    bool TimerFired,
    FieldValue<DateOnly> DataControle);
