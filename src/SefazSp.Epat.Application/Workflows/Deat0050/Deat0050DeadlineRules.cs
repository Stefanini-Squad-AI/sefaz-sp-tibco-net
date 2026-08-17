#nullable enable

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Abstractions;

// BUILD-DEAT0050-seg012 — Timer "Aguarda Defesa"
// Regra: RI-deadline-DEAT0050-AguardaDefesa
// Expressão XPDL (linha 3872): PRAZODEFESA; PRAZODEFESAT;
// Decisão: absolute-instant (gaps.expression-deadline).
// RISCO RESIDUAL ASSUMIDO: o timer não acompanha prorrogação feita após o agendamento.
// MITIGAÇÃO (transcrita do legado): gateway rearranca o fluxo se DATACONTROLE != PRAZODEFESA,
//   criando um novo timer — não é reinvenção, é o comportamento observado no TIBCO.
// FUSO HORÁRIO: America/Sao_Paulo (POR CONFIRMAR — assumido).

namespace SefazSp.Epat.Application.Workflows.Deat0050;

/// <summary>
/// Regra de prazo do timer DEAT0050 "_lrer2lqhEfG5K7mY0I3I6w" (Aguarda Defesa).
/// Combina o campo de data (<see cref="SefazSp.Epat.Domain.Cases.AiimCase.PRAZODEFESA"/>)
/// e o campo de hora (<see cref="SefazSp.Epat.Domain.Cases.AiimCase.PRAZODEFESAT"/>)
/// num instante absoluto no momento do agendamento.
/// </summary>
public static class Deat0050DeadlineRules
{
    // POR CONFIRMAR com a SEFAZ: fuso horário actual do iProcess.
    private static readonly TimeZoneInfo SaoPauloTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>
    /// RI-deadline-DEAT0050-AguardaDefesa.
    /// Devolve o instante absoluto em que o timer Aguarda Defesa deve disparar.
    /// O par data+hora é fixado no momento do agendamento (absolute-instant).
    /// </summary>
    /// <param name="caseData">Estado do caso no momento do agendamento.</param>
    /// <param name="clock">Relógio injectado — nunca DateTime.Now.</param>
    public static DateTimeOffset ComputeAguardaDefesaDeadline(AiimCase caseData, IClock clock)
    {
        var date = caseData.PRAZODEFESA;
        var time = caseData.PRAZODEFESAT;

        // Combina data e hora num DateTime local sem deslocamento.
        var localDateTime = date.ToDateTime(time);

        // Converte para instante absoluto no fuso de São Paulo.
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, SaoPauloTimeZone);
    }
}
