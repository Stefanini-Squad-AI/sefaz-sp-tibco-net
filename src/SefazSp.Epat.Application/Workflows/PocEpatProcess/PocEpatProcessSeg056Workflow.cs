#nullable enable

// Card: BUILD-POCEPATPROCESS-seg056
// Segmento: SC-POC_EpatProcess-007 · passo 28 · etapa 7
// Processo: POC_EpatProcess · ordemNaJornada: 9

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: endEvent <c>_CtQ66lqPEfG5K7mY0I3I6w</c> —
/// passo 28 do cenário SC-POC_EpatProcess-007, segmento ordemNaJornada=9.
///
/// Topologia (1 nó — aresta vem de transição real do XPDL):
/// <code>
///   1  endEvent  _CtQ66lqPEfG5K7mY0I3I6w  (entrouPor=fluxo)
///      └─ encerramento da instância do processo raiz POC_EpatProcess
/// </code>
///
/// <para>
/// <b>Semântica:</b> Este é o endEvent do processo <b>principal</b> POC_EpatProcess.
/// Representa o encerramento definitivo da instância — não um regresso a um chamador.
/// Contrasta com o endEvent de subprocesso (que é sempre um regresso ao chamador).
/// Fonte: card BUILD-POCEPATPROCESS-seg056, ressalva de topologia.
/// </para>
/// </summary>
public sealed class PocEpatProcessSeg056Workflow
{
    // ── identificador de nó — invariante: não renomear (card BUILD-POCEPATPROCESS-seg056) ──

    /// <summary>
    /// Nó 1 — endEvent <c>_CtQ66lqPEfG5K7mY0I3I6w</c> (encerramento da instância raiz).
    /// </summary>
    public const string NodeEndEvent = "_CtQ66lqPEfG5K7mY0I3I6w";

    /// <summary>
    /// Executa o troco: sinaliza o encerramento da instância do processo POC_EpatProcess.
    /// </summary>
    /// <returns>
    /// Identificador do nó terminal: <c>"_CtQ66lqPEfG5K7mY0I3I6w"</c> —
    /// endEvent de encerramento da instância raiz.
    /// </returns>
    public string Execute()
    {
        // ── ordem 1: endEvent _CtQ66lqPEfG5K7mY0I3I6w (entrouPor=fluxo) ──
        // Encerramento da instância raiz do processo POC_EpatProcess.
        // Nenhuma acção adicional: a instância termina ao atingir este nó.
        return NodeEndEvent;
    }
}
