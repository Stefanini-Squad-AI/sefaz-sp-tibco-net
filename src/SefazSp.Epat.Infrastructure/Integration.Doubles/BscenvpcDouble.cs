#nullable enable

// Card: BUILD-POCEPATPROCESS-seg053
// Double para o processo BSCENVPC invocado pelo callActivity 'Busca Emails' (_CtQ691qPEfG5K7mY0I3I6w).
//
// Este double implementa o comportamento Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>>.
// Como não existe interface para BSCENVPC em Abstractions/Processes (o card não pode criá-la lá),
// o double é implementado como classe com delegate configurável.
//
// Padrão: semelhante a CALCPRPCDouble.cs

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Double conduzido por cenário para o processo BSCENVPC (Busca Emails).
/// Invocado pelo callActivity 'Busca Emails' (<c>_CtQ691qPEfG5K7mY0I3I6w</c>) do POC_EpatProcess.
///
/// O double não contém lógica de negócio própria: devolve o resultado pré-configurado
/// pelo handler activo. Qualquer invocação sem handler configurado produz uma
/// excepção explícita, tornando visível a ausência de setup de teste.
/// </summary>
public sealed class BscenvpcDouble
{
    private Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>>? _handler;

    /// <summary>
    /// Configura o handler que será invocado em <see cref="ExecuteAsync"/>.
    /// </summary>
    /// <param name="handler">
    /// Função que recebe a referência do caso e token de cancelamento,
    /// devolvendo o resultado da chamada ao processo BSCENVPC.
    /// </param>
    public void ConfigureHandler(Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> handler)
        => _handler = handler;

    /// <summary>
    /// Executa a chamada simulada ao processo BSCENVPC.
    /// </summary>
    /// <param name="caseRef">Referência do caso (idAiim + processId).</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// O resultado configurado pelo handler.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Se nenhum handler foi configurado via <see cref="ConfigureHandler"/>.
    /// </exception>
    public Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_handler is null)
            throw new InvalidOperationException(
                $"BscenvpcDouble: nenhum handler configurado. " +
                $"Chame {nameof(ConfigureHandler)} antes de invocar (processo BSCENVPC/Busca Emails).");

        return _handler(caseRef, ct);
    }

    /// <summary>
    /// Cria um handler de sucesso para cenários de teste.
    /// </summary>
    /// <param name="childInstanceId">ID da instância filha (opcional).</param>
    /// <returns>
    /// Um handler que devolve <see cref="ProcessCallResult"/> com <c>Started = true</c>.
    /// </returns>
    public static Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> CreateSuccessHandler(
        string? childInstanceId = "BSCENVPC-mock-instance")
        => (_, _) => Task.FromResult(new ProcessCallResult(Started: true, ChildInstanceId: childInstanceId, Failure: null));

    /// <summary>
    /// Cria um handler de falha para cenários de teste.
    /// </summary>
    /// <param name="failureReason">Razão da falha.</param>
    /// <returns>
    /// Um handler que devolve <see cref="ProcessCallResult"/> com <c>Started = false</c>.
    /// </returns>
    public static Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> CreateFailureHandler(
        string failureReason = "BSCENVPC mock failure")
        => (_, _) => Task.FromResult(new ProcessCallResult(Started: false, ChildInstanceId: null, Failure: failureReason));
}
