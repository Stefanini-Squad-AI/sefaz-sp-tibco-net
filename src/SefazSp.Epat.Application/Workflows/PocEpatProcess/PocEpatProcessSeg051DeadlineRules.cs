#nullable enable

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Abstractions;

// Card: BUILD-POCEPATPROCESS-seg051
// Timer "timerEvent _CtQ7A1qPEfG5K7mY0I3I6w" — evento de fronteira interruptivo em
// Pedido de Vistas (_CtQ68lqPEfG5K7mY0I3I6w).
// Regra: RI-deadline-POC_EpatProcess-passosemrotulo (linha 1733 do XPDL).
// Expressão XPDL: PRAZORETIRADAVI; HORAFINAL.Time;
// Decisão: absolute-instant (gaps.expression-deadline, ratificada em NOEQ-expression-deadline, 2026-08-06).
// RISCO RESIDUAL ASSUMIDO: o timer não acompanha prorrogação feita após o agendamento.
// FUSO HORÁRIO: America/Sao_Paulo (POR CONFIRMAR com a SEFAZ — assumido).

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Regra de prazo do timer POC_EpatProcess "<c>_CtQ7A1qPEfG5K7mY0I3I6w</c>"
/// (timerEvent de fronteira interruptivo em <em>Pedido de Vistas</em>).
///
/// Implementa <c>RI-deadline-POC_EpatProcess-passosemrotulo</c> (linha 1733 do XPDL):
/// combina o campo de data (<see cref="AiimCase.PRAZORETIRADAVI"/>)
/// com o componente de hora de <see cref="AiimCase.HORAFINAL"/>
/// num instante absoluto no momento do agendamento (decisão <c>absolute-instant</c>,
/// ratificada em <c>NOEQ-expression-deadline</c>).
///
/// Classificação: <c>fixa-prazo</c> (compromisso de tempo); portador: <c>deadline</c>.
/// </summary>
public static class PocEpatProcessSeg051DeadlineRules
{
    // POR CONFIRMAR com a SEFAZ: fuso horário actual do iProcess.
    private static readonly TimeZoneInfo SaoPauloTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>
    /// RI-deadline-POC_EpatProcess-passosemrotulo (linha 1733).
    /// Devolve o instante absoluto em que o timerEvent
    /// '<c>_CtQ7A1qPEfG5K7mY0I3I6w</c>' deve disparar sobre <em>Pedido de Vistas</em>.
    ///
    /// O par data+hora é fixado no momento do agendamento (<c>absolute-instant</c>).
    /// RISCO RESIDUAL: o timer não acompanha prorrogação posterior dos campos.
    /// </summary>
    /// <param name="caseData">Estado do caso no momento do agendamento.</param>
    /// <param name="clock">
    /// Relógio injectado — <b>nunca</b> <c>DateTime.Now</c> nem
    /// <c>DateTimeOffset.Now</c>: o teste de prazo exige relógio controlável.
    /// </param>
    public static DateTimeOffset ComputePedidoDeVistasDeadline(AiimCase caseData, IClock clock)
    {
        var date = caseData.PRAZORETIRADAVI;          // XPDL: PRAZORETIRADAVI; (DateOnly)
        var time = TimeOnly.FromDateTime(caseData.HORAFINAL); // XPDL: HORAFINAL.Time;

        // Combina data e hora num DateTime local sem deslocamento.
        var localDateTime = date.ToDateTime(time);

        // Converte para instante absoluto no fuso de São Paulo.
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, SaoPauloTimeZone);
    }
}
