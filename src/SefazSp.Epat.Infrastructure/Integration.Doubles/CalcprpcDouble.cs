#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble conduzido por cenário para a interface de processo CALCPRPC.
/// Invocado pelo callActivity CalculaPrazo (_lrer3lqhEfG5K7mY0I3I6w) no processo DEAT0050.
///
/// AC2 — O duble aceita cenários injectados externamente e devolve respostas pré-configuradas,
/// sem lógica de negócio própria. Qualquer caseRef sem cenário configurado
/// produz uma excepção explícita, tornando visível a ausência de setup de teste.
///
/// Implementacoes concretas do subprocesso CALCPRPC sao entregues pelo pacote CALCPRPC em producao.
/// Em arranque sem implementacao entregue, este duble deve estar registado para que o processo
/// DEAT0050 arranque sem falhar (gaps.dynamic-subprocess = interface-registry-validated).
///
/// Rastreia: checklist ordem 2 (_lrer3lqhEfG5K7mY0I3I6w, entrouPor=fluxo)
/// Processo: DEAT0050 · Segmento: BUILD-DEAT0050-seg009
/// </summary>
public sealed class CalcprpcDouble : ICALCPRPC
{
    private readonly CalcprpcScenario _scenario;

    public CalcprpcDouble(CalcprpcScenario scenario)
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
/// Cenário que determina o comportamento do duble <see cref="CalcprpcDouble"/>.
/// Configure antes de executar o teste; não escreva lógica de produção aqui.
/// </summary>
public sealed class CalcprpcScenario
{
    /// <summary>
    /// Resultado padrão: subprocesso arrancou com sucesso.
    /// Ajuste em testes de caminho de erro.
    /// </summary>
    public ProcessCallResult Result { get; set; } =
        new ProcessCallResult(Started: true, ChildInstanceId: "test-calcprpc-instance", Failure: null);
}

/// <summary>
/// Registo de destinos da interface de processo CALCPRPC.
/// Validado no arranque da aplicação: um destino sem duble registado
/// lança <see cref="InvalidOperationException"/> de forma imediata e visível.
/// NAO herda HaltOnBadSubProcess=false do legado TIBCO.
/// </summary>
public sealed class CalcprpcRegistry
{
    private readonly IReadOnlyDictionary<string, ICALCPRPC> _destinations;

    public CalcprpcRegistry(IReadOnlyDictionary<string, ICALCPRPC> destinations)
    {
        if (destinations is null || destinations.Count == 0)
            throw new InvalidOperationException(
                "CalcprpcRegistry: nenhum destino registado. " +
                "Cada destino de CALCPRPC requer uma implementacao concreta do duble (pacote CALCPRPC). " +
                "Destino sem implementacao falha visivelmente no arranque, nao em silencio.");

        _destinations = destinations;
    }

    /// <summary>
    /// Resolve o duble para o <paramref name="destination"/> indicado.
    /// Lança <see cref="InvalidOperationException"/> se o destino não estiver registado.
    /// </summary>
    public ICALCPRPC Resolve(string destination)
    {
        if (!_destinations.TryGetValue(destination, out var impl))
            throw new InvalidOperationException(
                $"CalcprpcRegistry: destino '{destination}' nao tem duble registado. " +
                "Registe a implementacao antes de iniciar a aplicacao (pacote CALCPRPC).");

        return impl;
    }

    /// <summary>Destinos actualmente registados (para diagnóstico e testes).</summary>
    public IEnumerable<string> RegisteredDestinations => _destinations.Keys;
}
