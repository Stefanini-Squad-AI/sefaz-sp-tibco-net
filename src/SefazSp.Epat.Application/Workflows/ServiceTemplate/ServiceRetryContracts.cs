#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Workflows.ServiceTemplate;

/// <summary>
/// Desfecho da fase síncrona do molde de serviço (Start → SetParameters → Loop →
/// Call → gateways de erro → retry). Fronteira ANTES da tarefa humana.
/// </summary>
public enum ServiceCallOutcome
{
    /// <summary>STATUS_CODE == "0": chamada bem sucedida.</summary>
    Success,

    /// <summary>Erro técnico não retentável (ISAPPERROR != "Y").</summary>
    NonAppError,

    /// <summary>Retentativas esgotadas → suspende na userTask 'Manipular Excecao'.</summary>
    RequiresOperator,
}

/// <summary>Decisão do operador aplicada DEPOIS da suspensão 'Manipular Excecao'.</summary>
public enum OperatorDecisionOutcome
{
    /// <summary>OUTCOME == "OK": caso resolvido manualmente.</summary>
    ManuallyFixed,

    /// <summary>OUTCOME == "R": repetir → volta ao início do laço.</summary>
    TryAgain,

    /// <summary>OUTCOME diferente: encerra em 'Done - Bail'.</summary>
    Bail,
}

/// <summary>
/// Molde dos cinco subprocessos de serviço, partido no ÚNICO ponto de suspensão real
/// (a userTask 'Manipular Excecao'). O motor (Elsa) agenda as duas fases com um bookmark
/// entre elas; o método RunAsync original compõe as mesmas duas fases de forma síncrona
/// para os oráculos. Uma implementação, dois consumidores — sem duplicação de lógica.
/// </summary>
public interface IServiceRetryTemplate
{
    /// <summary>Chave do processo (ex.: "CRNOTPC"), usada pelo motor para escolher o molde.</summary>
    string ProcessKey { get; }

    /// <summary>Prólogo do subprocesso: SetParameters + Start Loop (+ Start TX). Idempotente.</summary>
    void InitializeContext(ProcessExecutionContext ctx, string? processId);

    /// <summary>
    /// Fase 1: corre o laço de retry até um desfecho terminal ou até precisar do operador.
    /// </summary>
    /// <param name="swQRetryCount">
    /// Contador de retentativas da fila do motor (IPESystemValues.SW_QRETRYCOUNT), lido pelo
    /// gateway Check Retries. Processos sem esse gateway (ex.: CRNOTPC) ignoram-no.
    /// </param>
    Task<ServiceCallOutcome> RunUntilOperatorAsync(
        AiimCaseRef caseRef, ProcessExecutionContext ctx, long swQRetryCount, CancellationToken ct);

    /// <summary>Fase 2: aplica a decisão do operador já gravada em ctx.OUTCOME.</summary>
    OperatorDecisionOutcome ApplyOperatorDecision(ProcessExecutionContext ctx);
}
