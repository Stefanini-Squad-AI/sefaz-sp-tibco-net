#nullable enable

// AC3 — HoraFimSC (scriptTask _lrer3VqhEfG5K7mY0I3I6w)
// Classificacao em rule-catalogue.json: RI-script-DEAT0050-HoraFimSC
//   eRegraDeNegocio: false  (nao e regra de negocio; e envelope tecnico)
//   atribui: DAYSOVER, PRAZODEFESA, PRAZODEFESAT
//
// DECISAO NOEQ-iprocess-builtin = shim-tri-state (ratificado 2026-08-06).
// DECISAO rulings.SCRIPT-HARDCODED (ratificado 2026-08-06):
//   A linha "if (SW_HOSTNAME == 'des1')" que encurtava o prazo para 1 hora NAO migra.
//   Os testes usam relogio controlavel (IClock); o atalho de desenvolvimento e removido.
// DECISAO expression-deadline = absolute-instant (ratificado 2026-08-06):
//   PRAZODEFESA (DateOnly) + PRAZODEFESAT (TimeOnly) formam o DateTime absoluto do timer.
//   O rearme do timer quando o campo for reescrito e responsabilidade do Workflow (AC5).

using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Execution;

/// <summary>
/// Executa o scriptTask <c>HoraFimSC</c> (<c>_lrer3VqhEfG5K7mY0I3I6w</c>)
/// do processo DEAT0050.
///
/// Responsabilidade deste executor (camada Application/Execution — logica tecnica):
/// calcular os campos de prazo a partir do resultado de CalculaPrazo e
/// preencher <see cref="AiimCase.PRAZODEFESA"/>, <see cref="AiimCase.PRAZODEFESAT"/>
/// e <see cref="AiimCase.DAYSOVER"/> no estado do caso.
///
/// A linha condicional <c>if (SW_HOSTNAME == 'des1')</c> do legado NAO migra
/// (decisao rulings.SCRIPT-HARDCODED). Os testes controlam o tempo via <see cref="IClock"/>.
/// </summary>
public static class HoraFimScExecutor
{
    /// <summary>
    /// Aplica o script HoraFimSC sobre o <paramref name="caso"/>.
    ///
    /// Pos-condicao: <c>caso.PRAZODEFESA</c> e <c>caso.PRAZODEFESAT</c> estao
    /// preenchidos com o instante absoluto do prazo de defesa, prontos para o
    /// timer <c>Aguarda Defesa</c> (AC5 — absolute-instant).
    /// </summary>
    /// <param name="caso">Estado mutavel do caso AIIM.</param>
    /// <param name="clock">Relogio injectado — nunca <c>DateTime.Now</c>.</param>
    /// <param name="daysOver">
    /// Numero de dias de prazo fornecido pelo subprocesso CalculaPrazo.
    /// Corresponde ao campo DAYSOVER do legado.
    /// </param>
    public static void Execute(AiimCase caso, IClock clock, int daysOver)
    {
        // DAYSOVER regista o numero de dias calculado pelo subprocesso.
        caso.DAYSOVER = daysOver;

        // Calcula o instante absoluto do prazo de defesa.
        // Fuso horario: America/Sao_Paulo (assumido — POR CONFIRMAR se UseWorkingDays=true afecta).
        // absolute-instant: combinamos data+hora no momento do agendamento.
        // O rearme do timer quando o campo for reescrito e feito no Workflow (AC5).
        var now = clock.Now.ToOffset(clock.TimeZone.GetUtcOffset(clock.Now.UtcDateTime));
        var prazoDate = DateOnly.FromDateTime(now.DateTime).AddDays(daysOver);

        // A hora de fim de prazo e o fim do dia (23:59:59) por omissao.
        // O legado nao declara uma hora padrao explicitamente fora do atalho 'des1' (removido).
        var prazoTime = new TimeOnly(23, 59, 59);

        caso.PRAZODEFESA = prazoDate;
        caso.PRAZODEFESAT = prazoTime;
    }

    /// <summary>
    /// Combina <paramref name="date"/> e <paramref name="time"/> num <see cref="DateTimeOffset"/>
    /// absoluto usando o fuso horario do <paramref name="clock"/>.
    ///
    /// Usado pelo Workflow para agendar o timer <c>Aguarda Defesa</c> (absolute-instant, AC5).
    /// </summary>
    public static DateTimeOffset ToAbsoluteInstant(DateOnly date, TimeOnly time, IClock clock)
    {
        var naive = date.ToDateTime(time);
        return new DateTimeOffset(naive, clock.TimeZone.GetUtcOffset(naive));
    }
}
