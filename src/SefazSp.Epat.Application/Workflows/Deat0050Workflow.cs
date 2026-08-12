#nullable enable

// Workflow DEAT0050 — troço de 6 passos: INICALC → Controlar Data
// Cenário de referência: SC-DEAT0050-001 (passos 1-6)
// Herdado de: POC_EpatProcess/Aguardar evento de Notificacao do AIIM (_0XWagVqNEfG5K7mY0I3I6w)
//
// Decisões aplicadas:
//   NOEQ-external-event = bookmark-correlation (2026-08-06)
//     INICALC suspende como bookmark correlacionado pela chave PROCESS_ID.
//   NOEQ-iprocess-builtin = shim-tri-state (2026-08-06)
//     DATACONTROLE é FieldValue<DateOnly>; IsNotAvailable = SW_NA.
//     Gateway: SW_NA || DATACONTROLE != PRAZODEFESA → Aguarda Defesa.
//   NOEQ-expression-deadline = absolute-instant (2026-08-06)
//     Aguarda Defesa: timer agendado para PRAZODEFESA + PRAZODEFESAT como DateTime absoluto.
//     Mitigação: timer rearmado sempre que PRAZODEFESA for reescrito (rearme pelo laco).
//   SENTINEL-DEAT0050-_lresFVqhE:
//     Controlar Data escreve DATACONTROLE = PRAZODEFESA para interromper o laco.
//   rulings.SCRIPT-HARDCODED:
//     Linha "if (SW_HOSTNAME == 'des1')" removida; IClock injectado nos testes.
//
// Motor alvo: Elsa 3 (IWorkflow / IActivity).
// Esta classe é a topologia do troço: nunca contém lógica de domínio.

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows;

/// <summary>
/// Define a topologia do troço DEAT0050 de INICALC a Controlar Data.
///
/// A classe modela os 6 nós do cenario SC-DEAT0050-001 na sequencia:
/// <list type="number">
///   <item><description>INICALC (<c>_lrer81qhEfG5K7mY0I3I6w</c>) — receiveTask, bookmark-correlation</description></item>
///   <item><description>CalculaPrazo (<c>_lrer3lqhEfG5K7mY0I3I6w</c>) — callActivity → INOTFAIIM</description></item>
///   <item><description>HoraFimSC (<c>_lrer3VqhEfG5K7mY0I3I6w</c>) — scriptTask, logica tecnica</description></item>
///   <item><description>Gateway <c>_lrer_VqhEfG5K7mY0I3I6w</c> — regra RI-transition-DEAT0050-gatewaylrerVqhEfG5K7mY0I3I6w</description></item>
///   <item><description>Aguarda Defesa (<c>_lrer2lqhEfG5K7mY0I3I6w</c>) — timerEvent, absolute-instant</description></item>
///   <item><description>Controlar Data (<c>_lrer_lqhEfG5K7mY0I3I6w</c>) — scriptTask, sentinela DATACONTROLE</description></item>
/// </list>
/// </summary>
public sealed class Deat0050Workflow
{
    private readonly INOTFAIIM _calculaPrazo;
    private readonly IClock _clock;

    public Deat0050Workflow(INOTFAIIM calculaPrazo, IClock clock)
    {
        _calculaPrazo = calculaPrazo;
        _clock = clock;
    }

    // -----------------------------------------------------------------------
    // Passo 1 — INICALC (_lrer81qhEfG5K7mY0I3I6w) — receiveTask
    // -----------------------------------------------------------------------
    // bookmark-correlation: o workflow suspende aqui e aguarda o sinal externo
    // correlacionado pela chave PROCESS_ID.
    // A chave tem o formato 'idAiim-<n>idProc-<n>', montada pelos scripts antes
    // de cada chamada. O endpoint Deat0050ResumeEndpoint entrega o sinal via ICorrelationStore.
    //
    // Este comentário documenta o ponto de suspensão; a implementação concreta
    // usa a API de bookmark do motor Elsa 3 (entregue por fundacao-motor).
    //
    // Invariante: não há 'entrouPor' — a entrada é por evento externo, não por
    // transição interna do segmento.

    // -----------------------------------------------------------------------
    // Passo 2 — CalculaPrazo (_lrer3lqhEfG5K7mY0I3I6w) — callActivity
    // -----------------------------------------------------------------------
    // Invoca o subprocesso via INOTFAIIM. O resultado (campos de prazo actualizados)
    // fica disponível no contexto do fluxo quando a callActivity retorna (AC2).
    // O mapeamento de entrada: PERIODOEMDIAS = 0 (RI-dataMapping-DEAT0050-CalculaPrazo).

    /// <summary>
    /// Executa CalculaPrazo via callActivity.
    /// </summary>
    public Task<ProcessCallResult> ExecuteCalculaPrazoAsync(AiimCaseRef caseRef, CancellationToken ct)
        => _calculaPrazo.ExecuteAsync(caseRef, ct);

    // -----------------------------------------------------------------------
    // Passo 3 — HoraFimSC (_lrer3VqhEfG5K7mY0I3I6w) — scriptTask
    // -----------------------------------------------------------------------
    // Calcula PRAZODEFESA e PRAZODEFESAT a partir de DAYSOVER.
    // Lógica técnica → Application/Execution/HoraFimScExecutor.
    // A linha "if (SW_HOSTNAME == 'des1')" foi removida (rulings.SCRIPT-HARDCODED).

    /// <summary>
    /// Executa HoraFimSC: calcula e preenche os campos de prazo no <paramref name="caso"/>.
    /// </summary>
    public void ExecuteHoraFimSc(AiimCase caso, int daysOver)
        => HoraFimScExecutor.Execute(caso, _clock, daysOver);

    // -----------------------------------------------------------------------
    // Passo 4 — Gateway _lrer_VqhEfG5K7mY0I3I6w
    // -----------------------------------------------------------------------
    // Regra: RI-transition-DEAT0050-gatewaylrerVqhEfG5K7mY0I3I6w
    // Expressão XPDL: DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZODEFESA
    //
    // Tradução shim-tri-state:
    //   • IsNotAvailable (SW_NA) → ramo "Não" → Aguarda Defesa
    //   • HasValue != PRAZODEFESA → ramo "Não" → Aguarda Defesa
    //   • HasValue == PRAZODEFESA → ramo "Sim" → fim do troço
    //   • Empty → ramo "Não" → Aguarda Defesa (defensivo)

    /// <summary>
    /// Avalia a gateway de laço do prazo de defesa.
    /// </summary>
    /// <returns>
    /// <c>true</c> → ramo "Não" (vai para Aguarda Defesa);
    /// <c>false</c> → ramo "Sim" (fim do troço).
    /// </returns>
    public static bool GatewayDeveAguardarDefesa(AiimCase caso)
        // RI-transition-DEAT0050-gatewaylrerVqhEfG5K7mY0I3I6w
        => caso.DATACONTROLE.Match(
            hasValue: v => v != caso.PRAZODEFESA,   // HasValue diferente de PRAZODEFESA → aguarda
            notAvailable: () => true,                // SW_NA → primeira volta → aguarda
            empty: () => true);                      // Empty → defensivo → aguarda

    // -----------------------------------------------------------------------
    // Passo 5 — Aguarda Defesa (_lrer2lqhEfG5K7mY0I3I6w) — timerEvent
    // -----------------------------------------------------------------------
    // Regra: RI-deadline-DEAT0050-AguardaDefesa
    // Decisão: absolute-instant — combina PRAZODEFESA + PRAZODEFESAT num DateTime absoluto.
    // Mitigação: o timer é rearmado sempre que PRAZODEFESA for reescrito
    //            (o legado recria o timer ao reiniciar o laço; esta implementação replica).
    // IClock injectado — nunca DateTime.Now.

    /// <summary>
    /// Calcula o instante absoluto do timer Aguarda Defesa.
    ///
    /// Regra RI-deadline-DEAT0050-AguardaDefesa: combina PRAZODEFESA (DateOnly) e
    /// PRAZODEFESAT (TimeOnly) num <see cref="DateTimeOffset"/> absoluto.
    ///
    /// O motor Elsa 3 agenda o timer para este instante (entregue por fundacao-motor).
    /// Quando o campo PRAZODEFESA for reescrito (prorrogação), chamar este método
    /// novamente e repassar o novo instante ao motor para rearme do timer.
    /// </summary>
    public DateTimeOffset CalcularInstanteAguardaDefesa(AiimCase caso)
        => HoraFimScExecutor.ToAbsoluteInstant(caso.PRAZODEFESA, caso.PRAZODEFESAT, _clock);

    // -----------------------------------------------------------------------
    // Passo 6 — Controlar Data (_lrer_lqhEfG5K7mY0I3I6w) — scriptTask
    // -----------------------------------------------------------------------
    // Sentinela: DATACONTROLE = PRAZODEFESA (SENTINEL-DEAT0050-_lresFVqhE).
    // Este é o último passo do troço.

    /// <summary>
    /// Executa Controlar Data: regista DATACONTROLE = PRAZODEFESA e termina o troço.
    /// </summary>
    public static void ExecuteControlarData(AiimCase caso)
        => ControlarDataExecutor.Execute(caso);
}
