#nullable enable

// Card: BUILD-POCEPATPROCESS-seg014
// Segmento: SC-POC_EpatProcess-001 · passos 5–6 · etapa 1
// Processo: POC_EpatProcess · Conceito: loops-retornos

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: de 'Corrigir Fechamento' (linkThrow)
/// ate 'Corrigir Fechamento' (linkCatch) — passos 5 a 6 do cenario
/// SC-POC_EpatProcess-001, segmento ordemNaJornada=2.
///
/// Topologia (2 nos):
///   1  linkThrow  _tN6q4lqTEfG5K7mY0I3I6w  Corrigir Fechamento  (entrouPor=fluxo)
///   2  linkCatch  _5E444FqTEfG5K7mY0I3I6w  Corrigir Fechamento  (entrouPor=link)
///
/// No sem transicao XPDL — escrito como aresta explicita (decisao NOEQ-link-goto, flatten-edge):
///   - Ordem 2 (_5E444FqTEfG5K7mY0I3I6w, entrouPor=link): o par linkThrow/linkCatch e
///     achatado numa aresta de fluxo directa; nao usar evento de sinal intermediario,
///     pois o TIBCO nao tem ponto de espera aqui (keep-as-signal foi recusado).
/// </summary>
public sealed class PocEpatProcessSeg014Workflow
{
    /// <summary>
    /// Executa o troco: recebe o controlo no linkThrow 'Corrigir Fechamento'
    /// e transfere-o imediatamente ao linkCatch 'Corrigir Fechamento' por
    /// aresta explicita de goto (decisao NOEQ-link-goto, flatten-edge).
    /// </summary>
    /// <returns>
    /// O terminal alcancado: sempre <see cref="PocEpatProcessSeg014Terminal.CorrigirFechamentoLinkCatch"/>
    /// porque o par linkThrow/linkCatch e incondicional — o TIBCO nao tem
    /// guarda nem decisao neste passo.
    /// </returns>
    public PocEpatProcessSeg014Terminal Execute()
    {
        // ── ordem 1: linkThrow 'Corrigir Fechamento' (_tN6q4lqTEfG5K7mY0I3I6w, entrouPor=fluxo) ─
        // Ponto de entrada do segmento — alvo da transicao de fluxo normal vinda de fora
        // (e.g. ramo 'Sim' do gateway 'Corrigir?' no segmento anterior).

        // ── ordem 2: linkCatch 'Corrigir Fechamento' (_5E444FqTEfG5K7mY0I3I6w, entrouPor=link) ──
        // Aresta explicita de goto: o linkThrow (_tN6q4lqTEfG5K7mY0I3I6w) lanca o controlo
        // para o linkCatch (_5E444FqTEfG5K7mY0I3I6w) directamente, sem ponto de espera.
        // NAO existe nenhuma transicao XPDL que alcance este no — a aresta e obrigatoriamente
        // escrita no codigo .NET (conforme content.checklist ordem 2 e gaps[0].decisionRef).
        goto CorrigirFechamentoLinkCatch;

        CorrigirFechamentoLinkCatch:
        return PocEpatProcessSeg014Terminal.CorrigirFechamentoLinkCatch;
    }
}

/// <summary>
/// Terminal alcancavel no segmento 014 do POC_EpatProcess.
/// </summary>
public enum PocEpatProcessSeg014Terminal
{
    /// <summary>
    /// linkCatch 'Corrigir Fechamento' (_5E444FqTEfG5K7mY0I3I6w) —
    /// unico terminal deste troco; controlo prossegue para o segmento seguinte.
    /// </summary>
    CorrigirFechamentoLinkCatch,
}
