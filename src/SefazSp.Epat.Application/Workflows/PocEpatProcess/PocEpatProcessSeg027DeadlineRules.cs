#nullable enable

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Abstractions;

// Card: BUILD-POCEPATPROCESS-seg027 — Timer "Fim de Prazo Mantendo Atividade"
// Regra: RI-deadline-POC_EpatProcess-FimdePrazoMantendoAtividade
// Expressão XPDL (linha 2269): DTFIMCQ; HRFIMCQ;
// Decisão: absolute-instant (gaps.expression-deadline, ratificada em review-dossier.json).
// RISCO RESIDUAL ASSUMIDO: o timer não acompanha prorrogação feita após o agendamento.
// FUSO HORÁRIO: America/Sao_Paulo (POR CONFIRMAR com a SEFAZ — assumido).

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Regra de prazo do timer POC_EpatProcess "<c>_XWivFlqTEfG5K7mY0I3I6w</c>"
/// (Fim de Prazo Mantendo Atividade).
///
/// Implementa <c>RI-deadline-POC_EpatProcess-FimdePrazoMantendoAtividade</c>:
/// combina o campo de data (<see cref="AiimCase.DTFIMCQ"/>)
/// e o campo de hora (<see cref="AiimCase.HRFIMCQ"/>)
/// num instante absoluto no momento do agendamento (decisão <c>absolute-instant</c>,
/// ratificada em <c>NOEQ-expression-deadline</c>).
///
/// Classificação: <c>fixa-prazo</c> (compromisso de tempo); portador: <c>deadline</c>.
/// </summary>
public static class PocEpatProcessSeg027DeadlineRules
{
    // POR CONFIRMAR com a SEFAZ: fuso horário actual do iProcess.
    private static readonly TimeZoneInfo SaoPauloTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>
    /// RI-deadline-POC_EpatProcess-FimdePrazoMantendoAtividade.
    /// Devolve o instante absoluto em que o timer 'Fim de Prazo Mantendo Atividade'
    /// deve disparar.
    ///
    /// O par data+hora é fixado no momento do agendamento (<c>absolute-instant</c>).
    /// RISCO RESIDUAL: o timer não acompanha prorrogação posterior dos campos.
    /// </summary>
    /// <param name="caseData">Estado do caso no momento do agendamento.</param>
    /// <param name="clock">
    /// Relógio injectado — <b>nunca</b> <c>DateTime.Now</c> nem
    /// <c>DateTimeOffset.Now</c>: o teste de prazo exige relógio controlável.
    /// </param>
    public static DateTimeOffset ComputeFimDePrazoDeadline(AiimCase caseData, IClock clock)
    {
        var date = caseData.DTFIMCQ;   // XPDL: DTFIMCQ; (DateOnly)
        var time = caseData.HRFIMCQ;   // XPDL: HRFIMCQ; (TimeOnly)

        // Combina data e hora num DateTime local sem deslocamento.
        var localDateTime = date.ToDateTime(time);

        // Converte para instante absoluto no fuso de São Paulo.
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, SaoPauloTimeZone);
    }
}
