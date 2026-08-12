#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble conduzido por cenario para a interface de processo NOTFAIIM.
/// Implementacoes entregues no pacote: DEAT0050.
/// O conjunto de destinos e validado no arranque via <see cref="NOTFAIIMRegistry"/>;
/// um destino sem duble registado falha de forma visivel — NAO herda HaltOnBadSubProcess=false do legado TIBCO.
/// </summary>
public sealed class NOTFAIIMDouble : INOTFAIIM
{
    private readonly NOTFAIIMScenario _scenario;

    public NOTFAIIMDouble(NOTFAIIMScenario scenario)
    {
        _scenario = scenario;
    }

    /// <inheritdoc />
    public Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_scenario.Result);
    }
}

/// <summary>
/// Cenario que determina o comportamento do duble <see cref="NOTFAIIMDouble"/>.
/// Configure antes de executar o teste; nao escreva logica de producao aqui.
/// </summary>
public sealed class NOTFAIIMScenario
{
    public ProcessCallResult Result { get; set; } =
        new ProcessCallResult(Started: true, ChildInstanceId: "test-instance", Failure: null);
}
