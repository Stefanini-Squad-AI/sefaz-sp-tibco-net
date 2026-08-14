#nullable enable

// Card: BUILD-POCEPATPROCESS-seg054
// AC3 + AC4 — Duble para a interface de processo CTRINTPC.
//
// Padrão: interface-registry-validated (NOEQ-dynamic-subprocess, ratificado 2026-08-06).
// Origem: callActivity _nQntZ16JEfGBBLgT-R5iuw 'Controlar Intimados', processo POC_EpatProcess.
// Destino entregue: CONTROPC (implementa ICTRINTPC, registado com chave "CONTROPC").
// O conjunto de destinos é validado no arranque via CTRINTPCRegistry;
// um destino sem duble registado falha de forma visível — NAO herda HaltOnBadSubProcess=false do TIBCO.

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble conduzido por cenário para a interface de processo CTRINTPC.
/// Implementações entregues no pacote: CONTROPC.
/// O conjunto de destinos é validado no arranque via <see cref="CTRINTPCRegistry"/>;
/// um destino sem duble registado falha de forma visível — NAO herda HaltOnBadSubProcess=false do legado TIBCO.
/// </summary>
public sealed class CONTROPCDouble : ICTRINTPC
{
    private readonly CONTROPCScenario _scenario;

    public CONTROPCDouble(CONTROPCScenario scenario)
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
/// Cenário que determina o comportamento do duble <see cref="CONTROPCDouble"/>.
/// Configure antes de executar o teste; não escreva lógica de produção aqui.
/// </summary>
public sealed class CONTROPCScenario
{
    public ProcessCallResult Result { get; set; } =
        new ProcessCallResult(Started: true, ChildInstanceId: "test-instance", Failure: null);
}
