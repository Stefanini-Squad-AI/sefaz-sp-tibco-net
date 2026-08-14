#nullable enable

// Card: BUILD-POCEPATPROCESS-seg021
// Segmento: SC-POC_EpatProcess-001 · passos 9–12 · etapa 2
// Processo: POC_EpatProcess · ordemNaJornada: 4
//
// NOEQ-link-goto  (decided: flatten-edge) — 2 nós neste segmento.
//   Os nós _Faq_RVqTEfG5K7mY0I3I6w (linkThrow) e _0XWagFqNEfG5K7mY0I3I6w (linkCatch)
//   são aplanados para uma aresta directa: sem evento intermediário, sem ponto de espera.
//   O linkCatch é escrito explicitamente porque não existe transição XPDL de origem
//   para esse nó (AC3 — ausência silenciosa sem escrita explícita).
//
// NOEQ-graft-step (decided: correlation-join) — 2 nós neste segmento.
//   Os mesmos nós _Faq_RVqTEfG5K7mY0I3I6w e _0XWagFqNEfG5K7mY0I3I6w marcam a entrada
//   do graft step (correlação pai/filho). O mecanismo de correlation-join completo é
//   gerido pelo segmento seguinte (callActivity 'Aguardar evento de Notificacao do AIIM',
//   _0XWagVqNEfG5K7mY0I3I6w), fora do escopo deste card.

using SefazSp.Epat.Application.Execution.POCEpatProcess;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: de 'Existe Notificação?' (gateway)
/// até 'Flag Retirati True GS' (scriptTask) —
/// passos 9 a 12 do cenário SC-POC_EpatProcess-001, segmento ordemNaJornada=4.
///
/// Topologia (4 nós):
/// <code>
///   1  gateway     _IxqJMlqTEfG5K7mY0I3I6w  Existe Notificação?      (entrouPor=fluxo)
///      │  regra: RI-transition-POC_EpatProcess-ExisteNotificao
///      │    Ramo "Sim" (aresta _J_OvkFqTEfG5K7mY0I3I6w, CONDITION: EXISTENOTIFICAC == true)
///      │      → endEvent _Faq_Q1qTEfG5K7mY0I3I6w
///      └─ Ramo "Não" (aresta _JNqIsFqTEfG5K7mY0I3I6w, OTHERWISE isDefault=true):
///   2      linkThrow   _Faq_RVqTEfG5K7mY0I3I6w  Inicia Graft Step    (entrouPor=fluxo)
///             │  [flatten-edge per NOEQ-link-goto: sem evento intermediário]
///   3      linkCatch   _0XWagFqNEfG5K7mY0I3I6w  Inicia Graft Step    (entrouPor=link)
///             │  [escrito explicitamente: NÃO existe transição XPDL para este nó — AC3]
///             ↓ aresta _0XWahFqMEfG5K7mY0I3I6w (UNCONDITIONAL)
///   4      scriptTask  _0XWahVqNEfG5K7mY0I3I6w  Flag Retirati True GS (entrouPor=fluxo)
///             │  regra: RI-script-POC_EpatProcess-FlagRetiratiTrueGS (eRegraDeNegocio=false)
///             │    FLAGRETIRATE    = true
///             │    EXISTENOTIFICAC = true
/// </code>
///
/// O par linkThrow/linkCatch é aplanado para uma aresta directa (NOEQ-link-goto:
/// flatten-edge) — não há evento, não há ponto de espera.
/// O linkCatch está escrito explicitamente no fluxo .NET porque não existe transição
/// de origem no XPDL (seria omitido silenciosamente sem esta escrita).
/// </summary>
public sealed class PocEpatProcessSeg021Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg021) ──

    /// <summary>Nó 1 — gateway 'Existe Notificação?' (Exclusive / XOR).</summary>
    public const string NodeExisteNotificacao = "_IxqJMlqTEfG5K7mY0I3I6w";

    /// <summary>Terminal "Sim" — endEvent <c>_Faq_Q1qTEfG5K7mY0I3I6w</c>.</summary>
    public const string NodeEndEvent = "_Faq_Q1qTEfG5K7mY0I3I6w";

    /// <summary>
    /// Nó 2 — linkThrow 'Inicia Graft Step' (<c>_Faq_RVqTEfG5K7mY0I3I6w</c>).
    /// Aplanado para aresta directa per NOEQ-link-goto (flatten-edge).
    /// </summary>
    public const string NodeLinkThrowIniciaGraftStep = "_Faq_RVqTEfG5K7mY0I3I6w";

    /// <summary>
    /// Nó 3 — linkCatch 'Inicia Graft Step' (<c>_0XWagFqNEfG5K7mY0I3I6w</c>).
    /// Escrito explicitamente: NÃO existe transição XPDL de origem para este nó (AC3).
    /// Aplanado para aresta directa per NOEQ-link-goto (flatten-edge).
    /// </summary>
    public const string NodeLinkCatchIniciaGraftStep = "_0XWagFqNEfG5K7mY0I3I6w";

    /// <summary>Nó 4 — scriptTask 'Flag Retirati True GS' (<c>_0XWahVqNEfG5K7mY0I3I6w</c>).</summary>
    public const string NodeFlagRetiratiTrueGs = "_0XWahVqNEfG5K7mY0I3I6w";

    /// <summary>
    /// Executa o troco: avalia o gateway 'Existe Notificação?' pela regra
    /// <c>RI-transition-POC_EpatProcess-ExisteNotificao</c> e, quando o ramo
    /// OTHERWISE é seguido, atravessa o par linkThrow/linkCatch aplanado e executa
    /// o script técnico 'Flag Retirati True GS'.
    /// </summary>
    /// <param name="aiimCase">
    /// Estado de negócio mutável do caso. O campo <c>EXISTENOTIFICAC</c> determina o ramo
    /// do gateway; após o script, <c>FLAGRETIRATE</c> e <c>EXISTENOTIFICAC</c> ficam ambos
    /// em <see langword="true"/>.
    /// </param>
    /// <returns>
    /// <see cref="PocEpatProcessSeg021Terminal.EndEvent"/> quando <c>EXISTENOTIFICAC == true</c>
    /// (ramo "Sim", aresta <c>_J_OvkFqTEfG5K7mY0I3I6w</c>); caso contrário
    /// <see cref="PocEpatProcessSeg021Terminal.FlagRetiratiTrueGs"/> após execução do script
    /// (ramo OTHERWISE/default, aresta <c>_JNqIsFqTEfG5K7mY0I3I6w</c> + flatten-edge).
    /// </returns>
    public PocEpatProcessSeg021Terminal Execute(AiimCase aiimCase)
    {
        // ── ordem 1: gateway 'Existe Notificação?' (_IxqJMlqTEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // Regra: RI-transition-POC_EpatProcess-ExisteNotificao
        // Expressão XPDL: EXISTENOTIFICAC == true
        // Ramo "Sim" (aresta _J_OvkFqTEfG5K7mY0I3I6w): condição verdadeira → endEvent
        if (aiimCase.EXISTENOTIFICAC == true)
        {
            return PocEpatProcessSeg021Terminal.EndEvent;
        }

        // Ramo "Não" (OTHERWISE, isDefault=true) — aresta _JNqIsFqTEfG5K7mY0I3I6w
        // → linkThrow 'Inicia Graft Step' _Faq_RVqTEfG5K7mY0I3I6w

        // ── ordem 2: linkThrow 'Inicia Graft Step' (_Faq_RVqTEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // NOEQ-link-goto (flatten-edge): aplanado para aresta directa — sem evento, sem espera.
        // NOEQ-graft-step (correlation-join): marca a entrada do graft; correlation-join
        //   gerido pelo segmento seguinte (_0XWagVqNEfG5K7mY0I3I6w).

        // ── ordem 3: linkCatch 'Inicia Graft Step' (_0XWagFqNEfG5K7mY0I3I6w, entrouPor=link) ──
        // AC3: escrito explicitamente — NÃO existe transição XPDL de origem para este nó.
        // NOEQ-link-goto (flatten-edge): aplanado, continuação imediata.

        // ── aresta _0XWahFqMEfG5K7mY0I3I6w: UNCONDITIONAL → scriptTask ──────────

        // ── ordem 4: scriptTask 'Flag Retirati True GS' (_0XWahVqNEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // Regra: RI-script-POC_EpatProcess-FlagRetiratiTrueGS (eRegraDeNegocio=false, efeito=tecnico)
        // Atribui: FLAGRETIRATE = true; EXISTENOTIFICAC = true
        // Não é regra de domínio — nenhum corpo em Domain/Rules (AC4).
        FlagRetiratiTrueGsStep.Execute(aiimCase);

        return PocEpatProcessSeg021Terminal.FlagRetiratiTrueGs;
    }
}

/// <summary>
/// Terminais alcançáveis no segmento 021 do POC_EpatProcess.
/// O gateway <c>_IxqJMlqTEfG5K7mY0I3I6w</c> é Exclusive (XOR): exactamente um terminal
/// é devolvido por execução.
/// </summary>
public enum PocEpatProcessSeg021Terminal
{
    /// <summary>
    /// Ramo "Sim" — endEvent <c>_Faq_Q1qTEfG5K7mY0I3I6w</c>.
    /// Condição XPDL (aresta <c>_J_OvkFqTEfG5K7mY0I3I6w</c>): <c>EXISTENOTIFICAC == true</c>.
    /// Regra: <c>RI-transition-POC_EpatProcess-ExisteNotificao</c>.
    /// </summary>
    EndEvent,

    /// <summary>
    /// Ramo "Não" (OTHERWISE, default) — após traversal do par linkThrow/linkCatch aplanado
    /// e execução de 'Flag Retirati True GS' (<c>_0XWahVqNEfG5K7mY0I3I6w</c>).
    /// Aresta de saída do gateway: <c>_JNqIsFqTEfG5K7mY0I3I6w</c> (conditionType=OTHERWISE).
    /// Após este terminal: o fluxo continua para <c>_0XWagVqNEfG5K7mY0I3I6w</c>
    /// (callActivity 'Aguardar evento de Notificacao do AIIM') — correlation-join do graft step.
    /// </summary>
    FlagRetiratiTrueGs,
}
