#nullable enable

using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Legacy;
using SefazSp.Epat.Application.Abstractions.Rules;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Application.Execution.PocEpatProcess;
using SefazSp.Epat.Application.Execution.POCEpatProcess;
using SefazSp.Epat.Application.Workflows;
using SefazSp.Epat.Application.Workflows.CONTROPC;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Rules.PocEpatProcess;
using SefazSp.Epat.Infrastructure.Integration.Doubles;
using SefazSp.Epat.Infrastructure.Runtime;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.PocEpat;

/// <summary>
/// Orquestrador do fluxo principal POC_EpatProcess — Fase 1, percurso de referência SC-001
/// (30 nós, Etapas 1→7). Máquina de estados sobre 5 bookmarks (eventos externos), reutilizando
/// as unidades finas já testadas (steps/rules/use-cases). Os 3 subprocessos (DEAT0050, PRPINTPC,
/// CONTROPC) são STUBS nesta fase. O caminho percorrido é registado em
/// <see cref="PocEpatMainSnapshot.Path"/> e comparado ao oráculo SC-001 no fim.
/// </summary>
[Activity("Epat", "POCEPAT", "Fluxo principal POC_EpatProcess (percurso SC-001, Fase 1).")]
public class PocEpatMainActivity : Activity
{
    public const string BkIniciarNovoGraft   = "pocepat-iniciar-novo-graft";
    public const string BkPrepararNotificacao = "pocepat-preparar-notificacao";
    public const string BkFinalizarAiim       = "pocepat-finalizar-aiim";
    public const string BkDeatInicalc         = "pocepat-deat-inicalc";
    public const string BkGraftProceed        = "pocepat-graft-proceed";
    public const string BkOperatorDecision    = "pocepat-operator-decision";
    public const string BkVerificarRetorno    = "pocepat-verificar-retorno";
    public const string BkVistasDoJuiz        = "pocepat-vistas-do-juiz";
    public const string BkRealizarVistaMista  = "pocepat-realizar-vista-mista";
    public const string BkPedidoDeVistas      = "pocepat-pedido-de-vistas";

    /// <summary>Sequência canónica de nós do percurso SC-001 (fonte única — comparada ao fixture e ao runtime).</summary>
    public static readonly string[] Sc001NodePath =
    {
        "_OAgPol9UEfG6Lfb98zsREQ", // 01 Iniciar Novo Graft (receiveTask)
        "_XWivF1qTEfG5K7mY0I3I6w", // 02 Set Nome Etapa 2
        "_sfwu-VqUEfG5K7mY0I3I6w", // 03 Preparar Notificacao (userTask)
        "_sJqYklqTEfG5K7mY0I3I6w", // 04 Corrigir?
        "_tN6q4lqTEfG5K7mY0I3I6w", // 05 Corrigir Fechamento (linkThrow)
        "_5E444FqTEfG5K7mY0I3I6w", // 06 Corrigir Fechamento (linkCatch)
        "_xWNLe1qSEfG5K7mY0I3I6w", // 07 Finalizar AIIM (userTask)
        "_Faq_RFqTEfG5K7mY0I3I6w", // 08 gateway AND-split
        "_IxqJMlqTEfG5K7mY0I3I6w", // 09 Existe Notificação?
        "_Faq_RVqTEfG5K7mY0I3I6w", // 10 Inicia Graft Step (linkThrow)
        "_0XWagFqNEfG5K7mY0I3I6w", // 11 Inicia Graft Step (linkCatch)
        "_0XWahVqNEfG5K7mY0I3I6w", // 12 Flag Retirati True GS
        "_0XWagVqNEfG5K7mY0I3I6w", // 13 Aguardar Notificacao → DEAT0050 (stub)
        "_0XWahFqNEfG5K7mY0I3I6w", // 14 Trocar Notificação?
        "_LeuhgFqVEfG5K7mY0I3I6w", // 15 Iniciar Decisions (linkThrow)
        "_CI6l0VqREfG5K7mY0I3I6w", // 16 Iniciar Decisions (linkCatch)
        "_CI6lx1qREfG5K7mY0I3I6w", // 17 Verificar Anulacao
        "_CI6lyFqREfG5K7mY0I3I6w", // 18 Prepara Intimação → PRPINTPC (stub)
        "_G4hU81qhEfG5K7mY0I3I6w", // 19 Define Destinatarios
        "_6WNq-lqgEfG5K7mY0I3I6w", // 20 Email Limite Rel 1
        "_30jAcFqVEfG5K7mY0I3I6w", // 21 Verificar Retorno Decisions (userTask)
        "_89MVQlqVEfG5K7mY0I3I6w", // 22 Validar Paralelos (linkThrow)
        "_Ei94AFqPEfG5K7mY0I3I6w", // 23 Validação Paralelos (linkCatch)
        "_CtQ7BFqPEfG5K7mY0I3I6w", // 24 Vistas do Juiz ?
        "_CtQ6-1qPEfG5K7mY0I3I6w", // 25 Vistas do Juiz (receiveTask)
        "_CtQ6_VqPEfG5K7mY0I3I6w", // 26 Vistas Mista ?
        "_CtQ6-lqPEfG5K7mY0I3I6w", // 27 gateway _CtQ6-lqP
        "_zE3XeV6JEfGBBLgT-R5iuw", // 28 prepSub
        "_nQntZ16JEfGBBLgT-R5iuw", // 29 Controlar Intimados → CONTROPC (stub)
        "_H22mclqWEfG5K7mY0I3I6w", // 30 Fim (endEvent)
    };

    // Prefixo partilhado pelas 3 vias TIPOVISTAS: nós 1–24 (até 'Vistas do Juiz ?').
    private static readonly string[] SharedPrefix = Sc001NodePath[..24];

    /// <summary>Percurso SC-012 (MISTA): prefixo partilhado + cauda 'Realizar Atividade Vista Mista'.</summary>
    public static readonly string[] Sc012MistaPath = SharedPrefix.Concat(new[]
    {
        "_CtQ7BVqPEfG5K7mY0I3I6w", "_tbOD4FqPEfG5K7mY0I3I6w", "_InbWgFqQEfG5K7mY0I3I6w",
        "_CtQ67FqPEfG5K7mY0I3I6w", "_CtQ66lqPEfG5K7mY0I3I6w",
    }).ToArray();

    /// <summary>Percurso SC-010 (DRF, timer de fronteira vence): prefixo + cauda FimDRF/Fim de Prazo.</summary>
    public static readonly string[] Sc010DrfPath = SharedPrefix.Concat(new[]
    {
        "_CtQ7BVqPEfG5K7mY0I3I6w", "_CtQ68lqPEfG5K7mY0I3I6w", "_CtQ7A1qPEfG5K7mY0I3I6w",
        "_CtQ66FqPEfG5K7mY0I3I6w", "_WvTQIFqQEfG5K7mY0I3I6w", "_Xw86YlqQEfG5K7mY0I3I6w",
    }).ToArray();

    /// <summary>Percurso SC-014 (Existe Notificação?=Sim): nós 1–9 + endEvent (curto-circuito).</summary>
    public static readonly string[] Sc014NodePath = Sc001NodePath[..9]
        .Concat(new[] { "_Faq_Q1qTEfG5K7mY0I3I6w" }).ToArray();

    /// <summary>Percurso SC-015 (Corrigir?=No): nós 1–4 + Criar Notificacao (CRNOTPC) + endEvent.</summary>
    public static readonly string[] Sc015NodePath = Sc001NodePath[..4]
        .Concat(new[] { "_BQIgAF9KEfGqPfX31TKC3w", "_O7K3MF9LEfGqPfX31TKC3w" }).ToArray();

    protected override ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var processId = context.GetWorkflowInput<string>("ProcessId");
        var idAiim    = context.GetWorkflowInput<long>("IdAiim");
        var existeNotif = context.GetWorkflowInput<bool>("ExisteNotificacao");
        var graftMode   = context.GetWorkflowInput<bool>("GraftMode");
        var prpintpcFails = context.GetWorkflowInput<bool>("PrpintpcFails");

        var snap = new PocEpatMainSnapshot(idAiim, processId);
        snap.Case.EXISTENOTIFICAC = existeNotif; // input do gateway node 9 'Existe Notificação?'
        snap.GraftMode = graftMode;              // node 13: graft-real vs descida única DEAT0050
        snap.PrpintpcFails = prpintpcFails;       // node 18: erro de app na 1ª tentativa → operador

        // node 18 (CaptaParametros): sementes dos atributos de entrada do motor Decisions.
        var seedJson = context.GetWorkflowInput<string>("DecisionsSeed");
        if (!string.IsNullOrWhiteSpace(seedJson))
        {
            var seed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string?>>(seedJson);
            if (seed is not null)
                foreach (var kv in seed) snap.DecisionsSeed[kv.Key] = kv.Value;
        }
        context.GetRequiredService<PocEpatProcessState>().Save(processId, snap);
        Console.WriteLine($"[POCEPAT] instância iniciada (PROCESS_ID={processId}) — suspensa em 'Iniciar Novo Graft'.");
        Suspend(context, snap, BkIniciarNovoGraft, OnIniciarNovoGraft);
        return default;
    }

    // ── chunk A→B: entra por 'Iniciar Novo Graft', corre 'Set Nome Etapa 2', suspende em 'Preparar Notificacao' ──
    private ValueTask OnIniciarNovoGraft(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        Log(snap, 0);  // Iniciar Novo Graft

        var clock    = context.GetRequiredService<IClock>();
        var builtins = context.GetRequiredService<IProcessBuiltins>();
        new SetNomeEtapa2Step(clock, builtins).Execute(snap.Case);
        Log(snap, 1);  // Set Nome Etapa 2

        Console.WriteLine("[POCEPAT] suspensa em 'Preparar Notificacao'.");
        Suspend(context, snap, BkPrepararNotificacao, OnPrepararNotificacao);
        return default;
    }

    // ── chunk C: 'Corrigir?' (Sim=Corrigir Fechamento→Finalizar AIIM / No=Criar Notificacao→endEvent) ──
    private async ValueTask OnPrepararNotificacao(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        Log(snap, 2);  // Preparar Notificacao
        Log(snap, 3);  // Corrigir?

        // node 4 XOR (RI-transition-Corrigir): CORRECAO == true → Sim → Corrigir Fechamento → Finalizar AIIM.
        if (snap.Case.CORRECAO)
        {
            Log(snap, 4);  // Corrigir Fechamento (linkThrow)
            Log(snap, 5);  // Corrigir Fechamento (linkCatch)
            Console.WriteLine("[POCEPAT] suspensa em 'Finalizar AIIM'.");
            Suspend(context, snap, BkFinalizarAiim, OnFinalizarAiim);
            return;
        }

        // No (OTHERWISE) → Criar Notificacao (CRNOTPC, callActivity resolvedVia=process) → endEvent (SC-015).
        snap.CorrigirNo = true;
        var crnotpc = new CRNOTPCDouble().WithResult(
            new ProcessCallResult(Started: true, ChildInstanceId: $"CRNOTPC-{snap.ProcessId}", Failure: null));
        var crnResult = await crnotpc.ExecuteAsync(
            new AiimCaseRef(snap.IdAiim, snap.ProcessId), context.CancellationToken);
        Console.WriteLine($"[POCEPAT] CORRECAO=false → No → CRNOTPC concluído (started={crnResult.Started}) → endEvent (SC-015).");
        Log(snap, "_BQIgAF9KEfGqPfX31TKC3w"); // Criar Notificacao → CRNOTPC
        Log(snap, "_O7K3MF9LEfGqPfX31TKC3w"); // endEvent
        await Finish(context, snap);
    }

    // ── chunk D: AND-split → 'Existe Notificação?' (Sim=SC-014 curto-circuito / No=graft → DEAT0050) ──
    private async ValueTask OnFinalizarAiim(ActivityExecutionContext context)
    {
        var snap = Resolve(context);

        // RI-formScript-FinalizarAIIM: AFR = GETATTRIBUTE("Name"); CNTINSTANCIASUF = 0.
        snap.Case.AFR = snap.PendingAfrName ?? "AFR-DEMO";
        snap.Case.CNTINSTANCIASUF = 0;
        Log(snap, 6);  // Finalizar AIIM
        Log(snap, 7);  // gateway AND-split
        Log(snap, 8);  // Existe Notificação?

        // node 9 XOR (RI-transition-ExisteNotificao): EXISTENOTIFICAC == true → Sim → endEvent (SC-014).
        if (snap.Case.EXISTENOTIFICAC)
        {
            snap.ExisteNotificacaoSim = true;
            Console.WriteLine("[POCEPAT] EXISTENOTIFICAC=true → Sim → endEvent (SC-014, curto-circuito).");
            Log(snap, "_Faq_Q1qTEfG5K7mY0I3I6w"); // endEvent (ramo Sim)
            await Finish(context, snap);
            return;
        }

        // ramo No (OTHERWISE) → Inicia Graft Step
        Log(snap, 9);  // Inicia Graft Step (linkThrow)
        Log(snap, 10); // Inicia Graft Step (linkCatch)

        FlagRetiratiTrueGsStep.Execute(snap.Case);
        Log(snap, 11); // Flag Retirati True GS

        // node 13: Aguardar evento de Notificacao do AIIM — pai do graft step.
        Log(snap, 12); // Aguardar Notificacao → DEAT0050

        if (snap.GraftMode)
        {
            // graft-real (correlation-join): o pai estaciona; filhos DEAT0050 anexam/concluem em
            // momentos diferentes; prossegue quando a janela fecha e todos concluem (ou timeout).
            var graft = context.GetRequiredService<InMemoryGraftJoin>();
            await graft.ParkAsync(snap.ProcessId, context.CancellationToken);
            Console.WriteLine("[POCEPAT]   ↳ GRAFT-REAL: pai estacionado — aguarda filhos DEAT0050 (attach/complete/close).");
            Suspend(context, snap, BkGraftProceed, OnGraftProceed);
            context.DelayFor(TimeSpan.FromSeconds(30), OnGraftTimeout); // safety net
            return;
        }

        // descida única (SC-001): DEAT0050 suspende primeiro em INICALC (evento externo) — subprocesso REAL.
        Console.WriteLine("[POCEPAT]   ↳ DEAT0050: suspensa em INICALC (evento externo).");
        Suspend(context, snap, BkDeatInicalc, OnDeatInicalc);
    }

    // graft-real: retomado quando a janela fecha e todos os filhos concluem.
    private async ValueTask OnGraftProceed(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        var graft = context.GetRequiredService<InMemoryGraftJoin>();
        if (!graft.TryResolve(snap.ProcessId)) return; // timeout já resolveu
        var (att, comp) = graft.Snapshot(snap.ProcessId);
        Console.WriteLine($"[POCEPAT]   ↳ GRAFT-REAL: fecho + {comp}/{att} filhos concluídos → pai prossegue.");
        graft.Clear(snap.ProcessId);
        await RunChunkD2(context, snap);
    }

    // graft-real: safety net — um filho que nunca termina não prende o pai.
    private async ValueTask OnGraftTimeout(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        var graft = context.GetRequiredService<InMemoryGraftJoin>();
        if (!graft.TryResolve(snap.ProcessId)) return; // prosseguimento normal já resolveu
        var (att, comp) = graft.Snapshot(snap.ProcessId);
        Console.WriteLine($"[POCEPAT]   ↳ GRAFT-REAL: TIMEOUT ({comp}/{att} concluídos) → pai prossegue (safety net).");
        graft.Clear(snap.ProcessId);
        await RunChunkD2(context, snap);
    }

    // ── DEAT0050 (node 13): INICALC retomado → CalculaPrazo + HoraFimSC → gateway 'Aguarda Defesa' ──
    private async ValueTask OnDeatInicalc(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        var wf = ResolveDeat(context);
        var caseRef = new AiimCaseRef(snap.IdAiim, snap.ProcessId);

        await wf.ExecuteCalculaPrazoAsync(caseRef, context.CancellationToken); // CalculaPrazo → CALCPRPC
        wf.ExecuteHoraFimSc(snap.Case, snap.Case.DAYSOVER);                     // HoraFimSC
        Console.WriteLine("[POCEPAT]   ↳ DEAT0050: INICALC → CalculaPrazo + HoraFimSC concluídos.");
        await EvaluateDeatGateway(context, snap);
    }

    private async ValueTask EvaluateDeatGateway(ActivityExecutionContext context, PocEpatMainSnapshot snap)
    {
        if (!Deat0050Workflow.GatewayDeveAguardarDefesa(snap.Case))
        {
            Console.WriteLine("[POCEPAT]   ↳ DEAT0050 concluído (não aguarda) — regressa ao fluxo principal.");
            await RunChunkD2(context, snap);
            return;
        }
        var wf = ResolveDeat(context);
        var instant = wf.CalcularInstanteAguardaDefesa(snap.Case);
        var demo = TimeSpan.FromSeconds(2);
        var opts = context.GetRequiredService<DeadlineDemoOptions>();
        var clock = context.GetRequiredService<IClock>();
        var delay = opts.DelayTo(instant, clock, demo);
        Console.WriteLine($"[POCEPAT]   ↳ DEAT0050: Aguarda Defesa — instante={instant:o}; demo={opts.Enabled}; delay={delay}.");
        // Ponto de gravação durável: persiste antes do timer (o callback recarrega o snapshot).
        context.GetRequiredService<PocEpatProcessState>().Save(snap.ProcessId, snap);
        context.DelayFor(delay, OnDeatTimer);
    }

    private async ValueTask OnDeatTimer(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        Deat0050Workflow.ExecuteControlarData(snap.Case); // Controlar Data (sentinela fecha o laço)
        Console.WriteLine("[POCEPAT]   ↳ DEAT0050: timer disparou → Controlar Data.");
        await EvaluateDeatGateway(context, snap);
    }

    // ── chunk D2: Trocar Notificação? → Decisions → PRPINTPC → Email, suspende em 'Verificar Retorno' ──
    private async ValueTask RunChunkD2(ActivityExecutionContext context, PocEpatMainSnapshot snap)
    {
        Log(snap, 13); // Trocar Notificação? (→ No → Iniciar Decisions)
        Log(snap, 14); // Iniciar Decisions (linkThrow)
        Log(snap, 15); // Iniciar Decisions (linkCatch)

        new VerificarAnulacaoStep().Execute(snap.Case);
        Log(snap, 16); // Verificar Anulacao

        await TryPrpintpcAsync(context, snap, attempt: 1);
    }

    // node 18: Prepara Intimação → PRPINTPC. Em erro de aplicação suspende para decisão do operador (retry template).
    private async ValueTask TryPrpintpcAsync(ActivityExecutionContext context, PocEpatMainSnapshot snap, int attempt)
    {
        var appError = snap.PrpintpcFails && attempt == 1;
        var prpintpc = new PRPINTPCDouble().WithResult(appError
            ? new ProcessCallResult(Started: false, ChildInstanceId: null, Failure: "STATUS_CODE != 0")
            : new ProcessCallResult(Started: true, ChildInstanceId: $"PRPINTPC-{snap.ProcessId}", Failure: null));
        var r = await prpintpc.ExecuteAsync(new AiimCaseRef(snap.IdAiim, snap.ProcessId), context.CancellationToken);

        if (!r.Started)
        {
            snap.PrpintpcAttempt = attempt;
            Console.WriteLine($"[POCEPAT]   ↳ PRPINTPC ERRO DE APLICAÇÃO (tentativa {attempt}, {r.Failure}) → suspende para decisão do operador.");
            Suspend(context, snap, BkOperatorDecision, OnOperatorDecision);
            return;
        }

        Console.WriteLine($"[POCEPAT]   ↳ PRPINTPC concluído (started=True, tentativa {attempt}) — subprocesso REAL (double).");
        Log(snap, 17); // Prepara Intimação → PRPINTPC

        // node 18 (CaptaParametros): motor de regras Decisions (fold override Corticon) — Etapa 3.
        var decisions = context.GetRequiredService<IIntimacoesDecision>();
        var resp = decisions.Evaluate(IntimacoesRequest.From(snap.DecisionsSeed));
        var setParams = resp.Attributes.Where(kv => kv.Value is not null).ToList();
        Console.WriteLine(setParams.Count == 0
            ? "[POCEPAT]   ↳ Decisions (CaptaParametros): fold → 0 parâmetros (todas SW_NA)."
            : $"[POCEPAT]   ↳ Decisions (CaptaParametros): fold override → {setParams.Count} parâmetro(s): " +
              string.Join(", ", setParams.Select(kv => $"{kv.Key}={kv.Value}")));

        new DefineDestinatariosStep(new DefineDestinatariosOptions { IsProducao = false }).Execute(snap.Case);
        Log(snap, 18); // Define Destinatarios

        Console.WriteLine("[POCEPAT]   ↳ emailTask 'Email Limite Rel 1' — double (Fase 1).");
        Log(snap, 19); // Email Limite Rel 1

        Console.WriteLine("[POCEPAT] suspensa em 'Verificar Retorno Decisions'.");
        Suspend(context, snap, BkVerificarRetorno, OnVerificarRetorno);
    }

    // operador (MANEXC): OUTCOME='R' (tentar novamente) → nova tentativa de PRPINTPC (agora sucesso).
    private async ValueTask OnOperatorDecision(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        Console.WriteLine("[POCEPAT]   ↳ operador: TENTAR NOVAMENTE (OUTCOME=R).");
        await TryPrpintpcAsync(context, snap, snap.PrpintpcAttempt + 1);
    }

    private static Deat0050Workflow ResolveDeat(ActivityExecutionContext context)
        => new(context.GetRequiredService<INOTFAIIM>(), context.GetRequiredService<IClock>());

    // ── chunk E: Validar Paralelos → 'Vistas do Juiz?' — desvio 3-vias por TIPOVISTAS ──
    private ValueTask OnVerificarRetorno(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        Log(snap, 20); // Verificar Retorno Decisions (TIPOVISTAS já aplicado pelo endpoint)
        Log(snap, 21); // Validar Paralelos (linkThrow)
        Log(snap, 22); // Validação Paralelos (linkCatch)
        Log(snap, 23); // Vistas do Juiz ?

        // via JUIZ / SW_NA → 'Vistas do Juiz' (SC-001).
        if (VistasdoJuizRule.Evaluate(snap.Case))
        {
            Console.WriteLine("[POCEPAT] TIPOVISTAS=JUIZ/SW_NA → suspensa em 'Vistas do Juiz'.");
            Suspend(context, snap, BkVistasDoJuiz, OnVistasDoJuiz);
            return default;
        }

        // ramo não-JUIZ → gateway _CtQ7BVqP
        Log(snap, "_CtQ7BVqPEfG5K7mY0I3I6w");

        // via MISTA → 'Realizar Atividade Vista Mista' (SC-012).
        if (TipodeVistaMistaRule.Evaluate(snap.Case))
        {
            Console.WriteLine("[POCEPAT] TIPOVISTAS=MISTA → suspensa em 'Realizar Atividade Vista Mista'.");
            Suspend(context, snap, BkRealizarVistaMista, OnRealizarVistaMista);
            return default;
        }

        // via DRF (TIPOVISTAS != JUIZ e != MISTA) → 'Pedido de Vistas' em corrida com o timer de fronteira (SC-010).
        Log(snap, "_CtQ68lqPEfG5K7mY0I3I6w"); // Pedido de Vistas (receiveTask)
        Console.WriteLine("[POCEPAT] TIPOVISTAS=DRF → 'Pedido de Vistas' [RACE: evento ⇄ timer de fronteira].");
        Suspend(context, snap, BkPedidoDeVistas, OnPedidoDeVistas);
        context.DelayFor(TimeSpan.FromSeconds(2), OnPedidoTimer);
        return default;
    }

    // via MISTA (SC-012): 'Fim Vista Mista' aplanado (NOEQ-link-goto) → endEvent.
    private async ValueTask OnRealizarVistaMista(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        Log(snap, "_tbOD4FqPEfG5K7mY0I3I6w"); // Realizar Atividade Vista Mista
        Log(snap, "_InbWgFqQEfG5K7mY0I3I6w"); // Fim Vista Mista (signalThrow, aplanado)
        Log(snap, "_CtQ67FqPEfG5K7mY0I3I6w"); // Fim Vista Mista (signalCatch, aplanado)
        Log(snap, "_CtQ66lqPEfG5K7mY0I3I6w"); // endEvent
        await Finish(context, snap);
    }

    // via DRF (SC-010): timer de fronteira vence → FimDRF aplanado → Fim de Prazo.
    private async ValueTask OnPedidoTimer(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        if (Interlocked.CompareExchange(ref snap.RaceResolved, 1, 0) != 0) return; // evento já resolveu
        Console.WriteLine("[POCEPAT]   ↳ Pedido de Vistas: TIMER de fronteira venceu (prazo).");
        Log(snap, "_CtQ7A1qPEfG5K7mY0I3I6w"); // timerEvent (fronteira)
        Log(snap, "_CtQ66FqPEfG5K7mY0I3I6w"); // FimDRF (signalThrow, aplanado)
        Log(snap, "_WvTQIFqQEfG5K7mY0I3I6w"); // Catch Signal Event (signalCatch, aplanado)
        Log(snap, "_Xw86YlqQEfG5K7mY0I3I6w"); // Fim de Prazo (endEvent)
        await Finish(context, snap);
    }

    // via DRF: evento externo vence — percurso distinto de SC-010 (não asserido nesta fase).
    private async ValueTask OnPedidoDeVistas(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        if (Interlocked.CompareExchange(ref snap.RaceResolved, 1, 0) != 0) return; // timer já resolveu
        Console.WriteLine("[POCEPAT]   ↳ Pedido de Vistas: EVENTO externo venceu (concluído, fora de SC-010).");
        await Finish(context, snap);
    }

    // ── chunk F (via JUIZ): 'Vistas Mista?' (OTHERWISE) → prepSub → CONTROPC → Fim ──
    private async ValueTask OnVistasDoJuiz(ActivityExecutionContext context)
    {
        var snap = Resolve(context);
        Log(snap, 24); // Vistas do Juiz
        Log(snap, 25); // Vistas Mista ? (TIPOVISTAS != MISTA → OTHERWISE)
        Log(snap, 26); // gateway _CtQ6-lqP
        Log(snap, 27); // prepSub

        // node 29: Controlar Intimados → CONTROPC (dynamic subprocess, interface-registry-validated).
        // Destino resolvido em runtime por AGUARDAR[IDX] — happy path SC-001 = "AgPecas" (AGPECASPC).
        var registry = context.GetRequiredService<AGUARDARRegistry>();
        snap.Case.AGUARDAR = new[] { "AgPecas" };
        snap.Case.IDX_AGUARDAR = 0;
        var controlopc = new ControlopcSeg039Workflow(registry.Resolve);
        var endEvent = await controlopc.ExecuteAsync(
            snap.Case, new AiimCaseRef(snap.IdAiim, snap.ProcessId), context.CancellationToken);
        Console.WriteLine($"[POCEPAT]   ↳ CONTROPC concluído (endEvent {endEvent}) — dynamic-subprocess REAL.");
        Log(snap, 28); // Controlar Intimados → CONTROPC
        Log(snap, 29); // Fim

        await Finish(context, snap);
    }

    private static async ValueTask Finish(ActivityExecutionContext context, PocEpatMainSnapshot snap)
    {
        // Ponto de gravação durável: persiste o percurso final antes de concluir a instância.
        context.GetRequiredService<PocEpatProcessState>().Save(snap.ProcessId, snap);
        var (label, expected) = SelectExpectedPath(snap);
        var match = snap.Path.SequenceEqual(expected);
        Console.WriteLine(
            $"[POCEPAT] fluxo concluído — {snap.Path.Count} nós. " +
            $"Comparação com oráculo {label}: {(match ? "IDÊNTICO ✅" : "DIVERGENTE ❌")}.");
        if (!match)
            Console.WriteLine("[POCEPAT] percurso: " + string.Join(" → ", snap.Path));
        await context.CompleteActivityAsync();
    }

    // Selecciona o oráculo esperado pela via efectivamente tomada.
    private static (string Label, string[] Path) SelectExpectedPath(PocEpatMainSnapshot snap)
    {
        if (snap.CorrigirNo) return ("SC-015 (Corrigir?=No)", Sc015NodePath);
        if (snap.ExisteNotificacaoSim) return ("SC-014 (Existe Notificação=Sim)", Sc014NodePath);
        var c = snap.Case;
        if (VistasdoJuizRule.Evaluate(c)) return ("SC-001 (JUIZ)", Sc001NodePath);
        if (TipodeVistaMistaRule.Evaluate(c)) return ("SC-012 (MISTA)", Sc012MistaPath);
        return ("SC-010 (DRF)", Sc010DrfPath);
    }

    private static PocEpatMainSnapshot Resolve(ActivityExecutionContext context)
    {
        var key = context.WorkflowExecutionContext.CorrelationId ?? string.Empty;
        return context.GetRequiredService<PocEpatProcessState>().Load(key)!;
    }

    private void Suspend(ActivityExecutionContext context, PocEpatMainSnapshot snap, string bookmark, ExecuteActivityDelegate callback)
    {
        // Ponto de gravação durável: persiste o percurso + caso antes de criar o bookmark de suspensão.
        context.GetRequiredService<PocEpatProcessState>().Save(snap.ProcessId, snap);
        var pid = context.WorkflowExecutionContext.CorrelationId ?? string.Empty;
        context.CreateBookmark(new CreateBookmarkArgs
        {
            BookmarkName = bookmark,
            Stimulus = new PocEpatStimulus(pid),
            Callback = callback,
            IncludeActivityInstanceId = false,
        });
    }

    private static void Log(PocEpatMainSnapshot snap, int index)
    {
        var id = Sc001NodePath[index];
        snap.Path.Add(id);
        Console.WriteLine($"[POCEPAT]   → [{snap.Path.Count:D2}] {id}");
    }

    private static void Log(PocEpatMainSnapshot snap, string id)
    {
        snap.Path.Add(id);
        Console.WriteLine($"[POCEPAT]   → [{snap.Path.Count:D2}] {id}");
    }
}
