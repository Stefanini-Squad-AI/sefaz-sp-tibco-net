#nullable enable

// Card: BUILD-POCEPATPROCESS-seg020
// Segmento: SC-POC_EpatProcess-014 · passo 9 · etapa 2
// Processo: POC_EpatProcess · ordemNaJornada: 4

using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: gateway "Existe Notificação?" —
/// passo 9 do cenário SC-POC_EpatProcess-014, segmento ordemNaJornada=4.
///
/// Topologia (1 nó — aresta vem de transição real do XPDL):
/// <code>
///   1  gateway  _IxqJMlqTEfG5K7mY0I3I6w  Existe Notificação?  (entrouPor=fluxo)
///      ├─ aresta _J_OvkFqTEfG5K7mY0I3I6w (CONDITION: EXISTENOTIFICAC == true)
///      │    → _Faq_Q1qTEfG5K7mY0I3I6w  endEvent
///      └─ aresta _JNqIsFqTEfG5K7mY0I3I6w (OTHERWISE, isDefault=true)
///           → _Faq_RVqTEfG5K7mY0I3I6w  Inicia Graft Step (linkThrow)
/// </code>
///
/// A condição de desvio é implementada integralmente pela regra de negócio
/// <c>RI-transition-POC_EpatProcess-ExisteNotificao</c>; nenhuma lógica de
/// decisão foi adicionada fora dessa regra.
///
/// O campo <c>EXISTENOTIFICAC</c> (nome completo no domínio: <c>existeNotificacao</c>;
/// o iProcess trunca em 15 caracteres) é lido directamente de <see cref="AiimCase"/>
/// sem conversão nem renomeação, e comparado com <c>== true</c> conforme o legado.
/// </summary>
public sealed class PocEpatProcessSeg020Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg020) ──

    /// <summary>Nó 1 — gateway "Existe Notificação?" (Exclusive / XOR).</summary>
    public const string NodeExisteNotificacao = "_IxqJMlqTEfG5K7mY0I3I6w";

    /// <summary>Terminal "Sim" — endEvent <c>_Faq_Q1qTEfG5K7mY0I3I6w</c>.</summary>
    public const string NodeEndEvent = "_Faq_Q1qTEfG5K7mY0I3I6w";

    /// <summary>Terminal "Não" — linkThrow "Inicia Graft Step" <c>_Faq_RVqTEfG5K7mY0I3I6w</c>.</summary>
    public const string NodeIniciaGraftStep = "_Faq_RVqTEfG5K7mY0I3I6w";

    /// <summary>
    /// Executa o troco: avalia o gateway "Existe Notificação?" pela regra
    /// <c>RI-transition-POC_EpatProcess-ExisteNotificao</c> e devolve o terminal alcançado.
    /// </summary>
    /// <param name="aiimCase">
    /// Estado do caso — fornece o campo <c>EXISTENOTIFICAC</c> (<see cref="AiimCase.EXISTENOTIFICAC"/>)
    /// que determina o ramo de saída. Comparado literalmente com <c>== true</c>, conforme o legado.
    /// </param>
    /// <returns>
    /// <see cref="PocEpatProcessSeg020Terminal.EndEvent"/> quando <c>EXISTENOTIFICAC == true</c>
    /// (ramo "Sim", aresta XPDL <c>_J_OvkFqTEfG5K7mY0I3I6w</c>); caso contrário
    /// <see cref="PocEpatProcessSeg020Terminal.IniciaGraftStep"/> (ramo OTHERWISE/default,
    /// aresta XPDL <c>_JNqIsFqTEfG5K7mY0I3I6w</c>).
    /// </returns>
    public PocEpatProcessSeg020Terminal Execute(AiimCase aiimCase)
    {
        // ── ordem 1: gateway "Existe Notificação?" (_IxqJMlqTEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // Regra de negócio: RI-transition-POC_EpatProcess-ExisteNotificao
        // Condição extraída do XPDL (aresta _J_OvkFqTEfG5K7mY0I3I6w): EXISTENOTIFICAC == true
        // O campo é lido sem conversão nem renomeação — o nome iProcess truncado é o identificador
        // de domínio (glossário POC_Epat.yaml, entrada EXISTENOTIFICAC).
        if (aiimCase.EXISTENOTIFICAC == true)
        {
            // Ramo "Sim" — aresta _J_OvkFqTEfG5K7mY0I3I6w → endEvent _Faq_Q1qTEfG5K7mY0I3I6w
            return PocEpatProcessSeg020Terminal.EndEvent;
        }

        // Ramo "Não" (OTHERWISE, isDefault=true) — aresta _JNqIsFqTEfG5K7mY0I3I6w
        // → linkThrow "Inicia Graft Step" _Faq_RVqTEfG5K7mY0I3I6w
        return PocEpatProcessSeg020Terminal.IniciaGraftStep;
    }
}

/// <summary>
/// Terminais alcançáveis no segmento 020 do POC_EpatProcess.
/// O gateway <c>_IxqJMlqTEfG5K7mY0I3I6w</c> é Exclusive (XOR): exactamente um terminal
/// é devolvido por execução.
/// </summary>
public enum PocEpatProcessSeg020Terminal
{
    /// <summary>
    /// Ramo "Sim" — endEvent <c>_Faq_Q1qTEfG5K7mY0I3I6w</c>.
    /// Condição XPDL (aresta <c>_J_OvkFqTEfG5K7mY0I3I6w</c>): <c>EXISTENOTIFICAC == true</c>.
    /// </summary>
    EndEvent,

    /// <summary>
    /// Ramo "Não" (OTHERWISE, default) — linkThrow "Inicia Graft Step" <c>_Faq_RVqTEfG5K7mY0I3I6w</c>.
    /// Aresta XPDL: <c>_JNqIsFqTEfG5K7mY0I3I6w</c> (conditionType=OTHERWISE, isDefault=true).
    /// </summary>
    IniciaGraftStep,
}
