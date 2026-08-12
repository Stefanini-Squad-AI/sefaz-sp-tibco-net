#nullable enable

using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Application.UseCases;

/// <summary>
/// Caso de uso para a tarefa humana 'Manipular Excecao'
/// (_qIDunF6BEfGBBLgT-R5iuw, ordem 10, passo 17 do cenario SC-BSCENVPC-009).
///
/// O operador recebe o estado de erro do caso e decide:
///   • OUTCOME = 'OK' → caso resolvido manualmente (gateway Manually Fixed)
///   • OUTCOME = 'R'  → tentar novamente (gateway Try Again)
///
/// A implementacao concreta da UI (form MANEXC) vive na camada de apresentacao.
/// Esta interface define o contrato da camada de aplicacao.
///
/// Fonte TIBCO: POC_Epat.xpdl //xpdl2:Activity[@Id='_qIDunF6BEfGBBLgT-R5iuw']
/// </summary>
public interface IManipularExcecaoUseCase
{
    /// <summary>
    /// Apresenta o estado de erro ao operador e aguarda a sua decisao.
    /// Preenche <see cref="Execution.ProcessExecutionContext.OUTCOME"/> com o resultado.
    /// </summary>
    Task HandleAsync(
        AiimCaseRef caseRef,
        Execution.ProcessExecutionContext ctx,
        CancellationToken ct);
}

/// <summary>
/// Comando de entrada para IManipularExcecaoUseCase.
/// Carrega as informacoes de contexto de erro apresentadas ao operador.
/// </summary>
/// <param name="CaseRef">Referencia do caso AIIM.</param>
/// <param name="StatusCode">STATUS_CODE da ultima chamada ao servico.</param>
/// <param name="ErrorCode">STERRORCODE da ultima chamada ao servico; pode ser nulo.</param>
/// <param name="ErrorDesc">STERRORDESC da ultima chamada ao servico; pode ser nulo.</param>
/// <param name="RetriesUsed">Numero de tentativas ja realizadas (NUMAPPRETRIES).</param>
/// <param name="MaxRetries">Numero maximo de tentativas (MAXRETRIES).</param>
public sealed record ManipularExcecaoCommand(
    AiimCaseRef CaseRef,
    string? StatusCode,
    string? ErrorCode,
    string? ErrorDesc,
    int RetriesUsed,
    int MaxRetries);

/// <summary>
/// Dominio dos valores de OUTCOME permitidos apos 'Manipular Excecao'.
/// Confirmado em 2026-08-06 (glossario POC_Epat.yaml, campo OUTCOME).
/// </summary>
public static class ManipularExcecaoOutcome
{
    /// <summary>Caso resolvido manualmente; prossegue via gateway 'Manually Fixed'.</summary>
    public const string ManuallyFixed = "OK";

    /// <summary>Tentar novamente; prossegue via gateway 'Try Again'.</summary>
    public const string TryAgain = "R";
}
