#nullable enable

// Card: BUILD-POCEPATPROCESS-seg033
// Segmento: SC-POC_EpatProcess-001 · passo 14 · etapa 2
// Processo: POC_EpatProcess · ordemNaJornada: 6

using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: gateway "Trocar Notificação?" —
/// passo 14 do cenário SC-POC_EpatProcess-001, segmento ordemNaJornada=6.
///
/// Topologia (1 nó — aresta vem de transição real do XPDL):
/// <code>
///   1  gateway  _0XWahFqNEfG5K7mY0I3I6w  Trocar Notificação?  (entrouPor=fluxo)
///      ├─ aresta CONDITION (TROCATPNOTIFICA == 1)
///      │    → _0XWahVqNEfG5K7mY0I3I6w  Flag Retirati True GS  (Sim)
///      └─ aresta OTHERWISE (ramo negativo)
///           → _LeuhgFqVEfG5K7mY0I3I6w  Iniciar Decisions  (linkThrow)
/// </code>
///
/// A condição de desvio é implementada integralmente pela regra de negócio
/// <c>RI-transition-POC_EpatProcess-TrocarNotificao</c>; nenhuma lógica de
/// decisão foi adicionada fora dessa regra.
///
/// O campo <c>TROCATPNOTIFICA</c> (Trocar Tipo Notificacao; int 1 dígito)
/// é lido directamente de <see cref="AiimCase"/> sem conversão nem renomeação,
/// e comparado com <c>== 1</c> conforme o XPDL (linha 1392).
/// </summary>
public sealed class PocEpatProcessSeg033Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg033) ──

    /// <summary>Nó 1 — gateway "Trocar Notificação?" (Exclusive / XOR).</summary>
    public const string NodeTrocarNotificacao = "_0XWahFqNEfG5K7mY0I3I6w";

    /// <summary>Terminal "Sim" — scriptTask "Flag Retirati True GS" <c>_0XWahVqNEfG5K7mY0I3I6w</c>.</summary>
    public const string NodeFlagRetiratiTrueGs = "_0XWahVqNEfG5K7mY0I3I6w";

    /// <summary>Terminal ramo negativo — linkThrow "Iniciar Decisions" <c>_LeuhgFqVEfG5K7mY0I3I6w</c>.</summary>
    public const string NodeIniciarDecisions = "_LeuhgFqVEfG5K7mY0I3I6w";

    /// <summary>
    /// Executa o troco: avalia o gateway "Trocar Notificação?" pela regra
    /// <c>RI-transition-POC_EpatProcess-TrocarNotificao</c> e devolve o terminal alcançado.
    /// </summary>
    /// <param name="aiimCase">
    /// Estado do caso — fornece o campo <c>TROCATPNOTIFICA</c> (<see cref="AiimCase.TROCATPNOTIFICA"/>)
    /// que determina o ramo de saída. Comparado literalmente com <c>== 1</c>, conforme o XPDL (linha 1392).
    /// </param>
    /// <returns>
    /// <see cref="PocEpatProcessSeg033Terminal.FlagRetiratiTrueGs"/> quando <c>TROCATPNOTIFICA == 1</c>
    /// (ramo "Sim"); caso contrário
    /// <see cref="PocEpatProcessSeg033Terminal.IniciarDecisions"/> (ramo negativo/OTHERWISE).
    /// </returns>
    public PocEpatProcessSeg033Terminal Execute(AiimCase aiimCase)
    {
        // ── ordem 1: gateway "Trocar Notificação?" (_0XWahFqNEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // Regra de negócio: RI-transition-POC_EpatProcess-TrocarNotificao
        // Condição extraída do XPDL (linha 1392): TROCATPNOTIFICA == 1
        if (aiimCase.TROCATPNOTIFICA == 1)
        {
            // Ramo "Sim" → scriptTask "Flag Retirati True GS" (_0XWahVqNEfG5K7mY0I3I6w)
            return PocEpatProcessSeg033Terminal.FlagRetiratiTrueGs;
        }

        // Ramo negativo (OTHERWISE) → linkThrow "Iniciar Decisions" (_LeuhgFqVEfG5K7mY0I3I6w)
        return PocEpatProcessSeg033Terminal.IniciarDecisions;
    }
}

/// <summary>
/// Terminais alcançáveis no segmento 033 do POC_EpatProcess.
/// O gateway <c>_0XWahFqNEfG5K7mY0I3I6w</c> é Exclusive (XOR): exactamente um terminal
/// é devolvido por execução.
/// </summary>
public enum PocEpatProcessSeg033Terminal
{
    /// <summary>
    /// Ramo "Sim" — scriptTask "Flag Retirati True GS" (<c>_0XWahVqNEfG5K7mY0I3I6w</c>).
    /// Activado quando <c>TROCATPNOTIFICA == 1</c> (regra <c>RI-transition-POC_EpatProcess-TrocarNotificao</c>).
    /// </summary>
    FlagRetiratiTrueGs,

    /// <summary>
    /// Ramo negativo (OTHERWISE) — linkThrow "Iniciar Decisions" (<c>_LeuhgFqVEfG5K7mY0I3I6w</c>).
    /// Activado quando <c>TROCATPNOTIFICA != 1</c>.
    /// </summary>
    IniciarDecisions,
}
