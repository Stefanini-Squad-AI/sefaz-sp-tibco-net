#nullable enable

// Card: BUILD-AGPECASPC-seg040
// Cenário de referência: SC-AGPECASPC-002 · segmento 1 · passos 1-9 · etapa 4
// Herdado de: CONTROPC/Aguardar Retorno (_-bkw-V6JEfGBBLgT-R5iuw)
//
// Topologia (9 nós):
//   [startEvent   _i4UpgF9IEfGqPfX31TKC3w]  Start Event
//     → [scriptTask  _EvOwTF6eEfGJqLUhfbpFcQ]  Set Values       (eRegraDeNegocio=true → Domain/Rules)
//     → [gateway     _vshgkF6fEfGJqLUhfbpFcQ]  gateway pass-through / merge do laco
//     → [gateway     _EvOwVF6eEfGJqLUhfbpFcQ]  "Ja se esperou pelo prazo em vigor?"
//                                                (RI-transition-AGPECASPC-gatewayEvOwVF6eEfGJqLUhfbpFcQ)
//         ramo true  → [scriptTask  _EvOwUl6eEfGJqLUhfbpFcQ]  SetPrazo
//                    → [receiveTask _EvOwQl6eEfGJqLUhfbpFcQ]  Aguardar Interposicoes
//                          boundary timer _EvOwRF6eEfGJqLUhfbpFcQ (Hours=1, fronteira)
//                          → [scriptTask _EvOwWV6eEfGJqLUhfbpFcQ]  Set Flag Decurso
//                    → [scriptTask _EvOwU16eEfGJqLUhfbpFcQ]  Controla Datas    (fim do segmento)
//         ramo false → (saida do ciclo, fora deste segmento)
//
// ATENCAO: _EvOwRF6eEfGJqLUhfbpFcQ NAO existe como transicao no XPDL.
//   E um evento de FRONTEIRA sobre o receiveTask Aguardar Interposicoes.
//   Declarado explicitamente neste ficheiro conforme instrucao do card.
//
// Decisoes aplicadas:
//   NOEQ-external-event = bookmark-correlation (2026-08-06):
//     Aguardar Interposicoes suspende como bookmark correlacionado pela chave PROCESS_ID.
//     O endpoint AgpecaspcResumeEndpoint entrega o sinal via ICorrelationStore.
//   NOEQ-iprocess-builtin = shim-tri-state (2026-08-06):
//     DATACONTROLE e FieldValue<DateOnly>; SW_NA = primeira volta.
//     Gateway: SW_NA || DATACONTROLE != PRAZORECEBIMENT → aguarda.
//   RI-deadline-AGPECASPC-passosemrotulo = Hours=1:
//     Timer boundary agendado para clock.Now + 1h.
//     O rearme e feito pelo motor quando o laco reinicia (laco de prorrogacao).
//   SENTINEL-AGPECASPC-_EvOwZF6eE (ratificado 2026-08-07):
//     Controla Datas escreve DATACONTROLE = PRAZORECEBIMENT para interromper o ciclo.
//   IClock injectado — nunca DateTime.Now.

using SefazSp.Epat.Application.Execution.AGPECASPC;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Rules.AGPECASPC;

namespace SefazSp.Epat.Application.Workflows.AGPECASPC;

/// <summary>
/// Define a topologia do segmento AGPECASPC-seg040:
/// de Start Event (<c>_i4UpgF9IEfGqPfX31TKC3w</c>) a Controla Datas (<c>_EvOwU16eEfGJqLUhfbpFcQ</c>).
///
/// <para>
/// O segmento modela um ciclo de espera por interposições com prorrogação de prazo.
/// O ciclo repete até que <c>DATACONTROLE == PRAZORECEBIMENT</c> (sentinela do gateway).
/// </para>
///
/// <para>
/// A suspensão real (receiveTask, timer boundary) é gerida pelo motor Elsa 3;
/// esta classe contém apenas a topologia e os métodos de negócio que o motor invoca.
/// </para>
///
/// <list type="number">
///   <item><description>Start Event (<c>_i4UpgF9IEfGqPfX31TKC3w</c>)</description></item>
///   <item><description>Set Values (<c>_EvOwTF6eEfGJqLUhfbpFcQ</c>) — scriptTask, lógica de domínio</description></item>
///   <item><description>Gateway pass-through (<c>_vshgkF6fEfGJqLUhfbpFcQ</c>) — merge do laço</description></item>
///   <item><description>Gateway "Já se esperou?" (<c>_EvOwVF6eEfGJqLUhfbpFcQ</c>) — decisão de laço</description></item>
///   <item><description>SetPrazo (<c>_EvOwUl6eEfGJqLUhfbpFcQ</c>) — scriptTask, técnico</description></item>
///   <item><description>Aguardar Interposições (<c>_EvOwQl6eEfGJqLUhfbpFcQ</c>) — receiveTask, bookmark-correlation</description></item>
///   <item><description>Timer boundary (<c>_EvOwRF6eEfGJqLUhfbpFcQ</c>) — fronteira, Hours=1</description></item>
///   <item><description>Set Flag Decurso (<c>_EvOwWV6eEfGJqLUhfbpFcQ</c>) — scriptTask, técnico</description></item>
///   <item><description>Controla Datas (<c>_EvOwU16eEfGJqLUhfbpFcQ</c>) — scriptTask, sentinela DATACONTROLE</description></item>
/// </list>
/// </summary>
public sealed class AgpecaspcSeg040Workflow
{
    private readonly IClock _clock;

    public AgpecaspcSeg040Workflow(IClock clock)
    {
        _clock = clock;
    }

    // -----------------------------------------------------------------------
    // Passo 1 — Start Event (_i4UpgF9IEfGqPfX31TKC3w)
    // -----------------------------------------------------------------------
    // Arranca o fluxo sem configuração adicional (AC1).
    // O startEvent não tem 'entrouPor' — é a origem do segmento.
    // O segmento é invocado a partir de CONTROPC/Aguardar Retorno (_-bkw-V6JEfGBBLgT-R5iuw).

    // -----------------------------------------------------------------------
    // Passo 2 — Set Values (_EvOwTF6eEfGJqLUhfbpFcQ) — scriptTask
    // -----------------------------------------------------------------------
    // Regra: RI-script-AGPECASPC-SetValues (eRegraDeNegocio = true → Domain/Rules)
    // A lógica de domínio (condição sobre CNTPECA1-4) reside em AgpecaspcSetValuesRule.
    // A manipulação do envelope técnico (FIELDSNAMES, FIELDSTYPES, FIELDSVALUES, IDPECAS,
    // PERIODOEMDIAS) seria feita aqui — conteúdo opaco no pacote (naoSabemos).
    //
    // AC2: separa lógica de domínio (Domain/Rules) de envelope técnico (Application/Execution).

    /// <summary>
    /// Passo 2 — scriptTask Set Values (<c>_EvOwTF6eEfGJqLUhfbpFcQ</c>).
    ///
    /// <para>
    /// A condição de domínio (<c>RI-script-AGPECASPC-SetValues</c>) é avaliada por
    /// <see cref="AgpecaspcSetValuesRule.ExistePecaDisponivel"/>.
    /// </para>
    ///
    /// <para>
    /// Os campos de envelope técnico (FIELDSNAMES, FIELDSTYPES, FIELDSVALUES, IDPECAS,
    /// PERIODOEMDIAS) têm corpo opaco no pacote POC_Epat: a expressão XPDL não declara
    /// os valores concretos (naoSabemos em rule-catalogue.json).
    /// </para>
    /// </summary>
    public static bool ExecuteSetValues(AiimCase caso)
        // RI-script-AGPECASPC-SetValues — lógica de domínio (eRegraDeNegocio=true)
        => AgpecaspcSetValuesRule.ExistePecaDisponivel(caso);

    // -----------------------------------------------------------------------
    // Passo 3 — Gateway _vshgkF6fEfGJqLUhfbpFcQ
    // -----------------------------------------------------------------------
    // Gateway de merge/pass-through: recebe o fluxo do startEvent e, após o ciclo,
    // também recebe o fluxo de retorno de Controla Datas.
    // Não tem condição lógica: a topologia é determinística (AC3).
    // A decisão de continuar ou sair fica no gateway seguinte (_EvOwVF6eEfGJqLUhfbpFcQ).

    // -----------------------------------------------------------------------
    // Passo 4 — Gateway _EvOwVF6eEfGJqLUhfbpFcQ
    // -----------------------------------------------------------------------
    // Regra: RI-transition-AGPECASPC-gatewayEvOwVF6eEfGJqLUhfbpFcQ (eRegraDeNegocio=true → Domain/Rules)
    // "Já se esperou pelo prazo em vigor?"
    // Condição: DATACONTROLE == SW_NA || DATACONTROLE != PRAZORECEBIMENT
    //   → true  (ainda não esperou ou prazo mudou) → SetPrazo
    //   → false (DATACONTROLE == PRAZORECEBIMENT)  → sai do ciclo (fora deste segmento)
    //
    // SENTINEL-AGPECASPC-_EvOwZF6eE: SW_NA segue junto com 'prazo mudou' via ||.

    /// <summary>
    /// Passo 4 — gateway <c>_EvOwVF6eEfGJqLUhfbpFcQ</c>.
    ///
    /// Devolve <see langword="true"/> quando o ciclo deve continuar (vai para SetPrazo).
    /// Devolve <see langword="false"/> quando o ciclo termina (DATACONTROLE == PRAZORECEBIMENT).
    /// </summary>
    public static bool GatewayDeveAguardarInterposicoes(AiimCase caso)
        // RI-transition-AGPECASPC-gatewayEvOwVF6eEfGJqLUhfbpFcQ
        => AgpecaspcGatewayRules.DeveAguardarInterposicoes(caso);

    // -----------------------------------------------------------------------
    // Passo 5 — SetPrazo (_EvOwUl6eEfGJqLUhfbpFcQ) — scriptTask
    // -----------------------------------------------------------------------
    // RI-script-AGPECASPC-SetPrazo (eRegraDeNegocio = false → Application/Execution)
    // Corpo opaco no pacote; veja AgpecaspcSeg040Steps.ExecuteSetPrazo.

    /// <summary>
    /// Passo 5 — scriptTask SetPrazo (<c>_EvOwUl6eEfGJqLUhfbpFcQ</c>).
    /// </summary>
    public static void ExecuteSetPrazo(AiimCase caso)
        => AgpecaspcSeg040Steps.ExecuteSetPrazo(caso);

    // -----------------------------------------------------------------------
    // Passo 6 — Aguardar Interposições (_EvOwQl6eEfGJqLUhfbpFcQ) — receiveTask
    // -----------------------------------------------------------------------
    // NOEQ-external-event = bookmark-correlation (ratificado 2026-08-06).
    // O workflow suspende aqui como bookmark correlacionado pela chave PROCESS_ID.
    // Formato PROCESS_ID: 'idAiim-<n>idProc-<n>' — montado pelos scripts, não inventado.
    // O endpoint AgpecaspcResumeEndpoint entrega o sinal via ICorrelationStore.
    //
    // Dois caminhos para sair desta suspensão:
    //   A) Evento externo chega → ICorrelationStore.ResumeAsync → bookmark retomado →
    //      fluxo continua para Controla Datas (caminho normal).
    //   B) Timer boundary dispara após 1h → passo 7 (_EvOwRF6eEfGJqLUhfbpFcQ) →
    //      Set Flag Decurso → Controla Datas (caminho de decurso).
    //
    // A suspensão real é gerida pelo motor Elsa 3 (fundacao-motor).
    // Esta nota documenta o ponto de suspensão; não há código executável aqui.

    // -----------------------------------------------------------------------
    // Passo 7 — Timer boundary _EvOwRF6eEfGJqLUhfbpFcQ — timerEvent (fronteira)
    // -----------------------------------------------------------------------
    // ATENÇÃO: este nó NÃO existe como transição no XPDL; é um evento de FRONTEIRA
    // sobre o receiveTask Aguardar Interposições (_EvOwQl6eEfGJqLUhfbpFcQ).
    // Declarado explicitamente aqui conforme instrução do card (entrouPor=fronteira).
    //
    // Regra: RI-deadline-AGPECASPC-passosemrotulo (Hours=1).
    // Cálculo do instante absoluto: AgpecaspcDeadlineRules.ComputeAguardarInterposicoesDeadline.
    // Quando dispara, o receiveTask é interrompido e o fluxo segue para Set Flag Decurso.

    /// <summary>
    /// Calcula o instante absoluto do timer boundary Aguardar Interposições
    /// (<c>_EvOwRF6eEfGJqLUhfbpFcQ</c>, entrouPor=fronteira).
    ///
    /// Regra <c>RI-deadline-AGPECASPC-passosemrotulo</c>: <c>Hours=1</c>.
    /// O motor Elsa 3 agenda o timer para este instante ao suspender o receiveTask.
    /// </summary>
    public DateTimeOffset CalcularInstanteTimerBoundary()
        => AgpecaspcDeadlineRules.ComputeAguardarInterposicoesDeadline(_clock);

    // -----------------------------------------------------------------------
    // Passo 8 — Set Flag Decurso (_EvOwWV6eEfGJqLUhfbpFcQ) — scriptTask
    // -----------------------------------------------------------------------
    // RI-script-AGPECASPC-SetFlagDecurso (eRegraDeNegocio = false → Application/Execution)
    // Activado APENAS pelo timer boundary (caminho de decurso).
    // Escreve FLGTERMODEC = true.

    /// <summary>
    /// Passo 8 — scriptTask Set Flag Decurso (<c>_EvOwWV6eEfGJqLUhfbpFcQ</c>).
    ///
    /// Activado pelo timer boundary (<c>_EvOwRF6eEfGJqLUhfbpFcQ</c>).
    /// Escreve <c>FLGTERMODEC = true</c> para sinalizar decurso de prazo.
    /// </summary>
    public static void ExecuteSetFlagDecurso(AiimCase caso)
        => AgpecaspcSeg040Steps.ExecuteSetFlagDecurso(caso);

    // -----------------------------------------------------------------------
    // Passo 9 — Controla Datas (_EvOwU16eEfGJqLUhfbpFcQ) — scriptTask
    // -----------------------------------------------------------------------
    // RI-script-AGPECASPC-ControlaDatas (eRegraDeNegocio = false → Application/Execution)
    // Escreve DATACONTROLE = PRAZORECEBIMENT.
    // SENTINEL-AGPECASPC-_EvOwZF6eE: este registo interrompe o ciclo na próxima iteração.
    // Último passo do segmento; o fluxo regressa ao gateway _vshgkF6fEfGJqLUhfbpFcQ.

    /// <summary>
    /// Passo 9 — scriptTask Controla Datas (<c>_EvOwU16eEfGJqLUhfbpFcQ</c>).
    ///
    /// Escreve <c>DATACONTROLE = PRAZORECEBIMENT</c> (sentinela de saída do ciclo).
    /// Último passo do segmento — após este passo o fluxo regressa ao gateway
    /// (<c>_vshgkF6fEfGJqLUhfbpFcQ</c>) onde a condição será avaliada de novo.
    /// </summary>
    public static void ExecuteControlaDatas(AiimCase caso)
        => AgpecaspcSeg040Steps.ExecuteControlaDatas(caso);
}
