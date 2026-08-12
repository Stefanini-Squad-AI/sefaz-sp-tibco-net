#nullable enable

using Xunit;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Infrastructure.Integration.Doubles;

namespace SefazSp.Epat.Oracles.Tests;

/// <summary>
/// Oraculo de contrato para CtrintpcDouble — verifica que o duble satisfaz o
/// contrato da fixture ICTRINTPC.cs (immutable=true, caseCount=1).
///
/// REGRA: nenhum valor esperado e escrito pelo agente; os resultados sao
/// determinados pela configuracao do cenario injectado, nao por logica do duble.
/// </summary>
public sealed class CtrintpcDoubleContractTests
{
    /// <summary>
    /// AC1+AC2: O duble implementa ICTRINTPC e devolve o resultado pre-configurado
    /// para o cenario injectado (conduzido por cenario, sem logica de negocio).
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithConfiguredScenario_ReturnsPreConfiguredResult()
    {
        // Arrange — cenario injectado externamente; nenhum valor e inventado pelo duble
        var caseRef = new AiimCaseRef(IdAiim: 1001L, ProcessId: "idAiim-1001-idProc-42");
        var expected = new ProcessCallResult(Started: true, ChildInstanceId: "child-instance-1", Failure: null);

        ICTRINTPC sut = new CtrintpcDouble()
            .WithScenario(caseRef, expected);

        // Act
        var actual = await sut.ExecuteAsync(caseRef, CancellationToken.None);

        // Assert — o duble devolve exactamente o cenario configurado
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// AC3+AC4: Ausencia de cenario produz excecao explicita e identificavel,
    /// nao falha silenciosa.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithoutConfiguredScenario_ThrowsExplicitly()
    {
        var caseRef = new AiimCaseRef(IdAiim: 9999L, ProcessId: "idAiim-9999-idProc-1");
        ICTRINTPC sut = new CtrintpcDouble(); // sem cenario configurado

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ExecuteAsync(caseRef, CancellationToken.None));
    }

    /// <summary>
    /// AC3: O registo de destinos e validado no arranque — ausencia de implementacao
    /// de ICTRINTPC no registo produz ProcessInterfaceRegistryException.
    /// </summary>
    [Fact]
    public void Registry_WithNoCtrintpcImplementation_ThrowsRegistryException()
    {
        var emptyRegistry = Array.Empty<Type>();

        Assert.Throws<ProcessInterfaceRegistryException>(
            () => ProcessInterfaceRegistry.ValidateCtrintpcDoubles(emptyRegistry));
    }

    /// <summary>
    /// AC3: Registo com CtrintpcDouble registado passa validacao sem excecao.
    /// </summary>
    [Fact]
    public void Registry_WithCtrintpcDoubleRegistered_PassesValidation()
    {
        var registry = new[] { typeof(CtrintpcDouble) };

        // Nao lanca excecao
        ProcessInterfaceRegistry.ValidateCtrintpcDoubles(registry);
    }
}
