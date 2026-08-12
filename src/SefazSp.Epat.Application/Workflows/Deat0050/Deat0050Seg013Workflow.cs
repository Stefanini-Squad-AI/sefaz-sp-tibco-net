#nullable enable

// Card: BUILD-DEAT0050-seg013
// Segmento: SC-DEAT0050-004 · passo 1 ao 5 · etapas 1, 2
// Herdado de: POC_EpatProcess/Aguardar evento de Notificacao do AIIM

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Application.Execution.Deat0050;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.Deat0050;

/// <summary>
/// Troco da jornada do DEAT0050 de 'Start Event' (_ppKXcFqjEfG5K7mY0I3I6w)
/// ate 'endEvent _lrer2FqhEfG5K7mY0I3I6w'.
///
/// Topologia (5 nos):
///   [startEvent _ppKXcFqjEfG5K7mY0I3I6w]
///     -> [callActivity _lrer3lqhEfG5K7mY0I3I6w] (CalculaPrazo → CALCPRPC)
///     -> [scriptTask  _lrer3VqhEfG5K7mY0I3I6w]  (HoraFimSC)
///     -> [gateway     _lrer_VqhEfG5K7mY0I3I6w]  (Ja se esperou pelo prazo em vigor?)
///         ramo OTHERWISE → [endEvent _lrer2FqhEfG5K7mY0I3I6w]
///         ramo verdadeiro (DATACONTROLE==SW_NA || DATACONTROLE!=PRAZODEFESA) → Aguarda Defesa (fora deste segmento)
///
/// Decisoes do glossario aplicadas:
///   gaps.iprocess-builtin = shim-tri-state (FieldValue&lt;T&gt;)
///   SENTINEL-DEAT0050-_lresFVqhE: SW_NA segue junto com 'prazo mudou' via ||
///   rulings.HARDCODED-VALUES: atalho `if (SW_HOSTNAME == des1)` REMOVIDO
/// </summary>
public sealed class Deat0050Seg013Workflow
{
    private readonly ICALCPRPC _calcPrpc;
    private readonly HoraFimScScript _horaFimSc;

    public Deat0050Seg013Workflow(ICALCPRPC calcPrpc, HoraFimScScript horaFimSc)
    {
        _calcPrpc = calcPrpc;
        _horaFimSc = horaFimSc;
    }

    /// <summary>
    /// Executa o troco a partir do startEvent (_ppKXcFqjEfG5K7mY0I3I6w).
    /// Devolve <see cref="Deat0050Seg013Result"/> a indicar se o segmento
    /// terminou em endEvent (prazo cumprido) ou deve aguardar defesa (fora do segmento).
    /// </summary>
    public async Task<Deat0050Seg013Result> RunAsync(
        AiimCase aiimCase,
        AiimCaseRef caseRef,
        CancellationToken ct)
    {
        // passo 2 — callActivity _lrer3lqhEfG5K7mY0I3I6w (CalculaPrazo → CALCPRPC)
        // entrouPor: fluxo (a partir do startEvent)
        await _calcPrpc.ExecuteAsync(caseRef, ct);

        // passo 3 — scriptTask _lrer3VqhEfG5K7mY0I3I6w (HoraFimSC)
        // entrouPor: fluxo (a partir de CalculaPrazo)
        // atalho `if (SW_HOSTNAME == des1)` REMOVIDO (rulings.HARDCODED-VALUES)
        _horaFimSc.Execute(aiimCase);

        // passo 4 — gateway _lrer_VqhEfG5K7mY0I3I6w
        // entrouPor: fluxo (a partir de HoraFimSC)
        // Regra: RI-transition-DEAT0050-gatewaylrerVqhEfG5K7mY0I3I6w
        // "Ja se esperou pelo prazo em vigor?"
        // DATACONTROLE e campo tri-estado (FieldValue<DateOnly>):
        //   SW_NA  → nunca esperou (primeira volta do laco) → ramo: volta a aguardar
        //   DATACONTROLE != PRAZODEFESA → prazo mudou       → ramo: volta a aguardar
        //   OTHERWISE (DATACONTROLE == PRAZODEFESA com valor) → prazo cumprido → endEvent
        var prazoJaEsperado = aiimCase.DATACONTROLE.Match(
            hasValue: dataControle => dataControle == aiimCase.PRAZODEFESA,
            notAvailable: () => false,   // SW_NA: primeira volta, nao esperou nada ainda
            empty: () => false           // Empty: equivalente a nao preenchido
        );

        if (!prazoJaEsperado)
        {
            // ramo verdadeiro da condicao original: DATACONTROLE == SW_NA || DATACONTROLE != PRAZODEFESA
            // → Aguarda Defesa (passo fora deste segmento)
            return Deat0050Seg013Result.AguardaDefesa;
        }

        // passo 5 — endEvent _lrer2FqhEfG5K7mY0I3I6w
        // entrouPor: fluxo (ramo OTHERWISE do gateway)
        return Deat0050Seg013Result.PrazoCumprido;
    }
}

/// <summary>Desfecho do segmento SC-DEAT0050-004.</summary>
public enum Deat0050Seg013Result
{
    /// <summary>
    /// O gateway decidiu que o prazo ja foi esperado.
    /// O subprocesso atingiu o endEvent _lrer2FqhEfG5K7mY0I3I6w e termina.
    /// </summary>
    PrazoCumprido,

    /// <summary>
    /// DATACONTROLE e SW_NA ou diferente de PRAZODEFESA.
    /// O fluxo deve seguir para 'Aguarda Defesa' (fora deste segmento).
    /// </summary>
    AguardaDefesa,
}
