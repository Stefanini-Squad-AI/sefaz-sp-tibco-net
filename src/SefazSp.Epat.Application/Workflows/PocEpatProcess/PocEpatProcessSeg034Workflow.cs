#nullable enable

// Card: BUILD-POCEPATPROCESS-seg034
// Segmento: SC-POC_EpatProcess-001 · passos 15–21 · etapas 3, 4
// Processo: POC_EpatProcess · ordemNaJornada: 7
//
// Gap link-goto (decided, NOEQ-link-goto, 2026-08-06):
//   flatten-edge: o par linkThrow/linkCatch é implementado como aresta explícita.
//   manter como sinal introduziria pontos de espera inexistentes no TIBCO.
//
// Gap iprocess-builtin (decided, NOEQ-iprocess-builtin, 2026-08-06):
//   shim-tri-state: SW_NA é terceiro estado distinto de null/vazio.

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution.PocEpatProcess;
using SefazSp.Epat.Application.UseCases.PocEpatProcess;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Porta de saída para o emailTask 'Email Limite Rel 1'
/// (<c>_6WNq-lqgEfG5K7mY0I3I6w</c>) do processo POC_EpatProcess.
///
/// Declarada em Application/Workflows/PocEpatProcess para respeitar a fronteira de
/// Clean Architecture (Application não referencia Infrastructure).
/// A implementação concreta vive em
/// <c>SefazSp.Epat.Infrastructure.Integration.Soap.EmailLimiteRel1SmtpTask</c>.
/// </summary>
public interface IEmailLimiteRel1Task
{
    /// <summary>Envia a notificação de limite (Relação 1) aos destinatários calculados.</summary>
    Task SendAsync(EmailLimiteRel1Parameters parameters, CancellationToken ct);
}

/// <summary>
/// Parâmetros extraídos do caso para o emailTask 'Email Limite Rel 1'.
/// Os destinatários CC/BCC foram calculados pelo scriptTask 'Define Destinatarios'.
/// </summary>
/// <param name="CcRelatorio">CC do e-mail; campo <c>CCRELATORIO</c> do caso.</param>
/// <param name="BccRelatorio">BCC do e-mail; campo <c>BCCRELATORIO</c> do caso.</param>
/// <param name="SwCaseDesc">Descrição do caso (<c>SW_CASEDESC</c>).</param>
/// <param name="LinkIpe">URL de acesso à tarefa no ePAT (<c>LINKIPE</c>).</param>
public sealed record EmailLimiteRel1Parameters(
    string? CcRelatorio,
    string? BccRelatorio,
    string SwCaseDesc,
    string LinkIpe);

/// <summary>
/// Troco do workflow POC_EpatProcess: de 'Iniciar Decisions' (linkThrow)
/// até 'Verificar Retorno Decisions' (userTask) —
/// passos 15 a 21 do cenário SC-POC_EpatProcess-001, segmento ordemNaJornada=7.
///
/// Topologia (7 nós):
/// <code>
///   1  linkThrow    _LeuhgFqVEfG5K7mY0I3I6w  Iniciar Decisions          (entrouPor=fluxo)
///      │  [flatten-edge: GOTO — aresta explícita, não sinal; NOEQ-link-goto]
///      ↓ aresta explícita .NET
///   2  linkCatch    _CI6l0VqREfG5K7mY0I3I6w  Iniciar Decisions          (entrouPor=link)
///      │  [este nó NÃO existe como transição no XPDL — escrito explicitamente]
///      ↓ aresta fluxo
///   3  scriptTask   _CI6lx1qREfG5K7mY0I3I6w  Verificar Anulacao         (entrouPor=fluxo)
///      │  regra: RI-script-POC_EpatProcess-VerificarAnulacao
///      ↓ aresta fluxo
///   4  callActivity _CI6lyFqREfG5K7mY0I3I6w  Prepara Intimação          (entrouPor=fluxo)
///      │  continuaEm: PRPINTPC · resolvidaPor: process · dinamica: false
///      │  regras: RI-dataMapping-POC_EpatProcess-PreparaIntimao (×2)
///      ↓ aresta fluxo
///   5  scriptTask   _G4hU81qhEfG5K7mY0I3I6w  Define Destinatarios       (entrouPor=fluxo)
///      │  regra: RI-script-POC_EpatProcess-DefineDestinatarios (eRegraDeNegocio=false)
///      ↓ aresta fluxo
///   6  emailTask    _6WNq-lqgEfG5K7mY0I3I6w  Email Limite Rel 1         (entrouPor=fluxo)
///      ↓ aresta fluxo
///   7  userTask     _30jAcFqVEfG5K7mY0I3I6w  Verificar Retorno Decisions (entrouPor=fluxo)
/// </code>
///
/// O par linkThrow/linkCatch (_LeuhgFqVEfG5K7mY0I3I6w / _CI6l0VqREfG5K7mY0I3I6w) NÃO
/// existe como transição no XPDL. A aresta entre eles é escrita explicitamente como
/// sequência de chamadas neste método (flatten-edge, NOEQ-link-goto).
/// </summary>
public sealed class PocEpatProcessSeg034Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg034) ──

    /// <summary>Nó 1 — linkThrow 'Iniciar Decisions'.</summary>
    public const string NodeIniciarDecisionsThrow = "_LeuhgFqVEfG5K7mY0I3I6w";

    /// <summary>Nó 2 — linkCatch 'Iniciar Decisions' (aresta explícita — não existe no XPDL).</summary>
    public const string NodeIniciarDecisionsCatch = "_CI6l0VqREfG5K7mY0I3I6w";

    /// <summary>Nó 3 — scriptTask 'Verificar Anulacao'.</summary>
    public const string NodeVerificarAnulacao = "_CI6lx1qREfG5K7mY0I3I6w";

    /// <summary>Nó 4 — callActivity 'Prepara Intimação' (callee: PRPINTPC).</summary>
    public const string NodePreparaIntimacao = "_CI6lyFqREfG5K7mY0I3I6w";

    /// <summary>Nó 5 — scriptTask 'Define Destinatarios'.</summary>
    public const string NodeDefineDestinatarios = "_G4hU81qhEfG5K7mY0I3I6w";

    /// <summary>Nó 6 — emailTask 'Email Limite Rel 1'.</summary>
    public const string NodeEmailLimiteRel1 = "_6WNq-lqgEfG5K7mY0I3I6w";

    /// <summary>Nó 7 — userTask 'Verificar Retorno Decisions'.</summary>
    public const string NodeVerificarRetornoDecisions = "_30jAcFqVEfG5K7mY0I3I6w";

    private readonly VerificarAnulacaoStep _verificarAnulacao;
    private readonly Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> _prpintpc;
    private readonly DefineDestinatariosStep _defineDestinatarios;
    private readonly IEmailLimiteRel1Task _emailLimiteRel1;
    private readonly VerificarRetornoDecisionsUseCase _verificarRetornoDecisions;

    /// <param name="verificarAnulacao">
    /// Passo de execução do scriptTask 'Verificar Anulacao'.
    /// </param>
    /// <param name="prpintpc">
    /// Delegate que invoca o subprocesso PRPINTPC ('Prepara Intimação').
    /// Em testes, substituir pelo double <c>PRPINTPCDouble</c>; em produção, pelo adaptador real.
    /// </param>
    /// <param name="defineDestinatarios">
    /// Passo de execução do scriptTask 'Define Destinatarios'.
    /// </param>
    /// <param name="emailLimiteRel1">
    /// Porta de saída para o emailTask 'Email Limite Rel 1'.
    /// </param>
    /// <param name="verificarRetornoDecisions">
    /// Caso de uso para a userTask 'Verificar Retorno Decisions'.
    /// </param>
    public PocEpatProcessSeg034Workflow(
        VerificarAnulacaoStep verificarAnulacao,
        Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> prpintpc,
        DefineDestinatariosStep defineDestinatarios,
        IEmailLimiteRel1Task emailLimiteRel1,
        VerificarRetornoDecisionsUseCase verificarRetornoDecisions)
    {
        _verificarAnulacao = verificarAnulacao;
        _prpintpc = prpintpc;
        _defineDestinatarios = defineDestinatarios;
        _emailLimiteRel1 = emailLimiteRel1;
        _verificarRetornoDecisions = verificarRetornoDecisions;
    }

    /// <summary>
    /// Executa o troco do segmento 034: de 'Iniciar Decisions' a
    /// 'Verificar Retorno Decisions'.
    ///
    /// <para>
    /// <b>AC1 — flatten-edge (NOEQ-link-goto):</b>
    /// O linkThrow <c>_LeuhgFqVEfG5K7mY0I3I6w</c> é alcançado pelo chamador
    /// (segmento anterior) e este método representa o linkCatch
    /// <c>_CI6l0VqREfG5K7mY0I3I6w</c> como primeiro passo da execução —
    /// aresta explícita, sem sinal intermédio.
    /// </para>
    /// </summary>
    /// <param name="caseRef">Referência do caso.</param>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    /// <param name="waitForVerificarRetornoDecisions">
    /// Delegate de interacção humana para a userTask 'Verificar Retorno Decisions'.
    /// Suspende o workflow até o responsável submeter o formulário.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task<VerificarRetornoDecisionsFormData>> waitForVerificarRetornoDecisions,
        CancellationToken ct)
    {
        // ── ordem 1: linkThrow 'Iniciar Decisions' (_LeuhgFqVEfG5K7mY0I3I6w) ─────
        // O chamador (PocEpatProcessSeg033Workflow) chegou ao terminal IniciarDecisions.
        // Esta execução começa imediatamente a seguir ao throw — sem ponto de espera.

        // ── flatten-edge: linkCatch implícito ─────────────────────────────────────
        // ordem 2: linkCatch 'Iniciar Decisions' (_CI6l0VqREfG5K7mY0I3I6w, entrouPor=link)
        // Este nó NÃO existe como transição no XPDL — é aresta explícita no .NET
        // (decisão flatten-edge ratificada em NOEQ-link-goto).
        // A execução continua directamente para o nó seguinte sem ponto de espera.

        // ── ordem 3: scriptTask 'Verificar Anulacao' (_CI6lx1qREfG5K7mY0I3I6w) ───
        // Regra: RI-script-POC_EpatProcess-VerificarAnulacao (eRegraDeNegocio=true)
        _verificarAnulacao.Execute(aiimCase);

        // ── ordem 4: callActivity 'Prepara Intimação' (_CI6lyFqREfG5K7mY0I3I6w) ──
        // continuaEm: PRPINTPC · resolvidaPor: process · dinamica: false
        // Regras de data mapping: RI-dataMapping-POC_EpatProcess-PreparaIntimao (×2)
        // A chamada é estática; o destino PRPINTPC é fixo no XPDL (dynamic=false).
        await _prpintpc(caseRef, ct).ConfigureAwait(false);

        // ── ordem 5: scriptTask 'Define Destinatarios' (_G4hU81qhEfG5K7mY0I3I6w) ─
        // Regra: RI-script-POC_EpatProcess-DefineDestinatarios (eRegraDeNegocio=false)
        // Escreve CCRELATORIO e BCCRELATORIO no caso para consumo pelo emailTask.
        _defineDestinatarios.Execute(aiimCase);

        // ── ordem 6: emailTask 'Email Limite Rel 1' (_6WNq-lqgEfG5K7mY0I3I6w) ────
        // Os destinatários CC/BCC vêm de CCRELATORIO/BCCRELATORIO calculados no passo anterior.
        // Nenhum endereço é literal no código (rulings.HARDCODED-VALUES).
        var emailParams = new EmailLimiteRel1Parameters(
            CcRelatorio: aiimCase.CCRELATORIO.Match(
                hasValue:     v => (string?)v,
                notAvailable: () => (string?)null,
                empty:        () => (string?)null),
            BccRelatorio: aiimCase.BCCRELATORIO.Match(
                hasValue:     v => (string?)v,
                notAvailable: () => (string?)null,
                empty:        () => (string?)null),
            SwCaseDesc: aiimCase.SW_CASEDESC ?? string.Empty,
            LinkIpe: aiimCase.LINKIPE ?? string.Empty);

        await _emailLimiteRel1.SendAsync(emailParams, ct).ConfigureAwait(false);

        // ── ordem 7: userTask 'Verificar Retorno Decisions' (_30jAcFqVEfG5K7mY0I3I6w) ─
        await _verificarRetornoDecisions.ExecuteAsync(
            caseRef, aiimCase, waitForVerificarRetornoDecisions, ct)
            .ConfigureAwait(false);
    }
}
