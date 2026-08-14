#nullable enable

// Card: BUILD-AGPECASPC-seg040
// Regra: RI-deadline-AGPECASPC-passosemrotulo
// Fonte XPDL: linha 10527 — timerEvent boundary _EvOwRF6eEfGJqLUhfbpFcQ
// Classificacao: efeito = fixa-prazo; eRegraDeNegocio = true
//
// Expressao XPDL: Hours=1
// O timer e um evento de fronteira (entrouPor=fronteira) sobre o receiveTask
// "Aguardar Interposicoes" (_EvOwQl6eEfGJqLUhfbpFcQ).
// Quando o timer dispara, o fluxo segue para Set Flag Decurso (_EvOwWV6eEfGJqLUhfbpFcQ).
//
// NOEQ-iprocess-builtin = shim-tri-state (ratificado 2026-08-06): NAO se aplica aqui;
//   o prazo e uma duracao fixa (1 hora) sem sentinela SW_NA.
// IClock injectado — nunca DateTime.Now.

using SefazSp.Epat.Domain.Abstractions;

namespace SefazSp.Epat.Application.Workflows.AGPECASPC;

/// <summary>
/// Regra de prazo do timer boundary AGPECASPC <c>_EvOwRF6eEfGJqLUhfbpFcQ</c>.
///
/// Expressão XPDL: <c>Hours=1</c> — o timer dispara 1 hora após o agendamento.
/// É um evento de fronteira sobre o receiveTask Aguardar Interposições
/// (<c>_EvOwQl6eEfGJqLUhfbpFcQ</c>). Quando dispara, o fluxo abandona a espera
/// e segue para Set Flag Decurso (<c>_EvOwWV6eEfGJqLUhfbpFcQ</c>).
///
/// <c>IClock</c> é injectado — nunca <c>DateTime.Now</c> —
/// para que os testes de prazo sejam reproduzíveis com relógio controlável.
/// </summary>
public static class AgpecaspcDeadlineRules
{
    /// <summary>
    /// RI-deadline-AGPECASPC-passosemrotulo.
    /// Devolve o instante absoluto em que o timer boundary Aguardar Interposições
    /// deve disparar: <paramref name="clock"/>.Now + 1 hora.
    ///
    /// O motor Elsa 3 agenda o timer para este instante (entregue por fundacao-motor).
    /// O timer é um evento de fronteira não-interruptivo sobre o receiveTask:
    /// se o evento externo chegar antes, o timer é cancelado.
    /// </summary>
    /// <param name="clock">Relógio injectado — nunca <c>DateTime.Now</c>.</param>
    /// <returns>
    /// Instante absoluto (<see cref="DateTimeOffset"/>) calculado a partir do relógio injectado.
    /// </returns>
    public static DateTimeOffset ComputeAguardarInterposicoesDeadline(IClock clock)
    {
        // Hours=1 (RI-deadline-AGPECASPC-passosemrotulo)
        // Duracao fixa: 1 hora apos o agendamento, sem dependencia de campos do caso.
        return clock.Now.AddHours(1);
    }
}
