#nullable enable

using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.UseCases.BSCENVPC;

/// <summary>
/// Caso de uso da tarefa humana 'Manipular Excecao' (_qIDunF6BEfGBBLgT-R5iuw, ordem 10).
/// As regras do code-behind das telas decidem o desfecho do processo, nao a apresentacao.
/// Dominio do OUTCOME: 'OK' (Manually Fixed) ou 'R' (Try Again). Fechado — confirmado 2026-08-06.
/// </summary>
public sealed class ManipularExcecaoUseCase
{
    /// <summary>
    /// Regista a decisao do operador no contexto de execucao.
    /// </summary>
    public void RecordOutcome(ProcessExecutionContext ctx, string outcome)
    {
        if (outcome != "OK" && outcome != "R")
            throw new ArgumentException(
                $"OUTCOME invalido: '{outcome}'. Dominio fechado: 'OK' (correcao manual) ou 'R' (repetir).",
                nameof(outcome));
        ctx.OUTCOME = outcome;
    }

    /// <summary>
    /// Passo 'Manually Fixed' (_qIDull6BEfGBBLgT-R5iuw, ordem 11).
    /// True se o operador declarou correcao manual (OUTCOME='OK').
    /// </summary>
    public bool IsManuallyFixed(ProcessExecutionContext ctx) => ctx.OUTCOME == "OK";

    /// <summary>
    /// Passo 'Try Again' (_qIDum16BEfGBBLgT-R5iuw, ordem 12).
    /// True se o operador quer repetir a chamada (OUTCOME='R').
    /// </summary>
    public bool IsTryAgain(ProcessExecutionContext ctx) => ctx.OUTCOME == "R";
}
