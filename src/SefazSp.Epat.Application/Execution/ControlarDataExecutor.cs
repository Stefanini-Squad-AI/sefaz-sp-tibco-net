#nullable enable

// AC6 — Controlar Data (scriptTask _lrer_lqhEfG5K7mY0I3I6w)
// Classificacao: logica tecnica (Application/Execution)
//
// DECISAO SENTINEL-DEAT0050-_lresFVqhE (glossario):
//   DATACONTROLE = PRAZODEFESA regista a memoria que impede o ciclo infinito.
//   Sem este registo, a gateway (_lrer_VqhEfG5K7mY0I3I6w) enviaria o fluxo de volta
//   a "Aguarda Defesa" indefinidamente.
//
// DECISAO NOEQ-iprocess-builtin = shim-tri-state (ratificado 2026-08-06):
//   DATACONTROLE e do tipo FieldValue<DateOnly> (tri-estado: HasValue / IsNotAvailable / Empty).
//   Apos "Controlar Data" o estado passa de IsNotAvailable/HasValue para HasValue(PRAZODEFESA),
//   o que fara a gateway decidir pelo ramo "Sim" (fim do troco) na proxima iteracao — ou
//   terminar o troco nesta execucao, dependendo da topologia.

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Execution;

/// <summary>
/// Executa o scriptTask <c>Controlar Data</c> (<c>_lrer_lqhEfG5K7mY0I3I6w</c>)
/// do processo DEAT0050.
///
/// Este passo e o ultimo do troco (passo 6 do cenario SC-DEAT0050-001).
/// Responsabilidade: registar <c>DATACONTROLE = PRAZODEFESA</c>, que funciona
/// como sentinela de saida do laco de prazo de defesa.
///
/// Decisao SENTINEL-DEAT0050-_lresFVqhE: sem este registo, a gateway de laco
/// continuaria a encaminhar para "Aguarda Defesa" indefinidamente.
/// </summary>
public static class ControlarDataExecutor
{
    /// <summary>
    /// Aplica o script Controlar Data sobre o <paramref name="caso"/>.
    ///
    /// Pos-condicao: <c>caso.DATACONTROLE</c> fica com o mesmo valor de
    /// <c>caso.PRAZODEFESA</c>, fazendo a gateway decidir pelo ramo "Sim"
    /// (DATACONTROLE == PRAZODEFESA) na proxima avaliacao.
    /// </summary>
    public static void Execute(AiimCase caso)
    {
        // DATACONTROLE := PRAZODEFESA
        // Sentinela que interrompe o laco de prazo (SENTINEL-DEAT0050-_lresFVqhE).
        caso.DATACONTROLE = FieldValue<DateOnly>.Of(caso.PRAZODEFESA);
    }
}
