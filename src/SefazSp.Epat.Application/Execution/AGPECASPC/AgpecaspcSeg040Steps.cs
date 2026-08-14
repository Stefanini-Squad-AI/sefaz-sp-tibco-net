#nullable enable

// Card: BUILD-AGPECASPC-seg040
// Passos técnicos (eRegraDeNegocio = false) do segmento SC-AGPECASPC-002 · etapa 4.
// Ficheiro XPDL: POC_Epat.xpdl
//
// SetPrazo        (_EvOwUl6eEfGJqLUhfbpFcQ) — ordem 5 — RI-script-AGPECASPC-SetPrazo
// SetFlagDecurso  (_EvOwWV6eEfGJqLUhfbpFcQ) — ordem 8 — RI-script-AGPECASPC-SetFlagDecurso
// ControlaDatas   (_EvOwU16eEfGJqLUhfbpFcQ) — ordem 9 — RI-script-AGPECASPC-ControlaDatas
//
// SENTINEL-AGPECASPC-_EvOwZF6eE (glossario, ratificado 2026-08-07):
//   ControlaDatas escreve DATACONTROLE = PRAZORECEBIMENT — sentinela que interrompe o ciclo.
// NOEQ-iprocess-builtin = shim-tri-state (ratificado 2026-08-06):
//   DATACONTROLE e FieldValue<DateOnly>; a escrita usa FieldValue<DateOnly>.Of(...).

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Execution.AGPECASPC;

/// <summary>
/// Passos técnicos de execução do segmento AGPECASPC-seg040.
/// Estes passos manipulam o envelope técnico e o estado de controlo do caso;
/// a lógica de domínio reside em <c>Domain/Rules/AGPECASPC</c>.
/// </summary>
public static class AgpecaspcSeg040Steps
{
    // -----------------------------------------------------------------------
    // Passo 5 — SetPrazo (_EvOwUl6eEfGJqLUhfbpFcQ) — scriptTask
    // -----------------------------------------------------------------------
    // RI-script-AGPECASPC-SetPrazo
    // Fonte XPDL: linha 10619
    // Classificacao: eRegraDeNegocio = false; efeito = tecnico
    //
    // INCERTEZA DOCUMENTADA: a expressao XPDL esta vazia no rule-catalogue.json
    // e nenhum campo de atribuicao esta declarado. O corpo deste passo e opaco.
    // Hipotese: SetPrazo prepara campos auxiliares de prazo que a topologia
    // (e o timer boundary 'Hours=1') consomem. A implementacao concreta
    // sera fornecida quando o XPDL de SetPrazo for desambiguado.

    /// <summary>
    /// Executa o scriptTask <c>SetPrazo</c> (<c>_EvOwUl6eEfGJqLUhfbpFcQ</c>).
    ///
    /// <para>
    /// O corpo deste script é opaco no pacote POC_Epat: a expressão XPDL está vazia
    /// e nenhum campo de atribuição está declarado em <c>rule-catalogue.json</c>
    /// (RI-script-AGPECASPC-SetPrazo). O método preserva o contrato da topologia
    /// sem alterar o estado até que a origem seja desambiguada.
    /// </para>
    /// </summary>
    /// <param name="caso">Estado mutável do caso AIIM.</param>
    public static void ExecuteSetPrazo(AiimCase caso)
    {
        // INCERTEZA: corpo opaco. Sem alteracao de estado ate desambiguacao.
        // O timer boundary '_EvOwRF6eEfGJqLUhfbpFcQ' usa 'Hours=1' (RI-deadline-AGPECASPC-passosemrotulo).
        _ = caso; // referencia explícita para garantir o contrato de assinatura.
    }

    // -----------------------------------------------------------------------
    // Passo 8 — Set Flag Decurso (_EvOwWV6eEfGJqLUhfbpFcQ) — scriptTask
    // -----------------------------------------------------------------------
    // RI-script-AGPECASPC-SetFlagDecurso
    // Fonte XPDL: linha 10704
    // Classificacao: eRegraDeNegocio = false; efeito = tecnico
    // Atribui: FLGTERMODEC
    //
    // Activado pelo evento de fronteira do timer (_EvOwRF6eEfGJqLUhfbpFcQ, entrouPor=fronteira).
    // "Decurso" = decurso de prazo (timer expirou). FLGTERMODEC sinaliza ao processo pai
    // que a espera terminou por expiração e nao por recepcao de evento.

    /// <summary>
    /// Executa o scriptTask <c>Set Flag Decurso</c> (<c>_EvOwWV6eEfGJqLUhfbpFcQ</c>).
    ///
    /// Activa o flag <see cref="AiimCase.FLGTERMODEC"/> para indicar que a espera por
    /// interposições terminou por decurso de prazo (timer boundary expirou),
    /// e não por recepção de evento externo.
    /// </summary>
    /// <param name="caso">Estado mutável do caso AIIM.</param>
    public static void ExecuteSetFlagDecurso(AiimCase caso)
    {
        // FLGTERMODEC := true — "decurso do prazo": o timer boundary disparou.
        // Sinaliza ao processo pai que o prazo expirou sem recepcao de interposicao.
        caso.FLGTERMODEC = true;
    }

    // -----------------------------------------------------------------------
    // Passo 9 — Controla Datas (_EvOwU16eEfGJqLUhfbpFcQ) — scriptTask
    // -----------------------------------------------------------------------
    // RI-script-AGPECASPC-ControlaDatas
    // Fonte XPDL: linha 10650
    // Classificacao: eRegraDeNegocio = false; efeito = tecnico
    // Atribui: DATACONTROLE
    //
    // SENTINEL-AGPECASPC-_EvOwZF6eE (glossario, ratificado 2026-08-07):
    //   DATACONTROLE = PRAZORECEBIMENT regista a memoria que impede o ciclo infinito.
    //   Sem este registo, a gateway (_EvOwVF6eEfGJqLUhfbpFcQ) enviaria o fluxo
    //   de volta a SetPrazo indefinidamente.
    //
    // NOEQ-iprocess-builtin = shim-tri-state:
    //   DATACONTROLE e FieldValue<DateOnly>.
    //   Apos ControlaDatas, o estado transita de IsNotAvailable/HasValue para HasValue(PRAZORECEBIMENT).
    //   Na proxima iteracao, a gateway decide pelo ramo falso (fim do ciclo).

    /// <summary>
    /// Executa o scriptTask <c>Controla Datas</c> (<c>_EvOwU16eEfGJqLUhfbpFcQ</c>).
    ///
    /// Escreve <c>DATACONTROLE = PRAZORECEBIMENT</c>, registando o prazo pelo qual
    /// o processo já esperou. O gateway seguinte compara os dois valores:
    /// quando iguais, o ciclo de espera por interposições termina.
    ///
    /// Sentinela SENTINEL-AGPECASPC-_EvOwZF6eE (ratificado 2026-08-07):
    /// sem esta escrita o ciclo seria infinito.
    /// </summary>
    /// <param name="caso">Estado mutável do caso AIIM.</param>
    public static void ExecuteControlaDatas(AiimCase caso)
    {
        // DATACONTROLE := PRAZORECEBIMENT
        // Sentinela que interrompe o laco de prazo (SENTINEL-AGPECASPC-_EvOwZF6eE).
        // DATACONTROLE transita de IsNotAvailable (SW_NA) para HasValue(PRAZORECEBIMENT).
        caso.DATACONTROLE = FieldValue<DateOnly>.Of(caso.PRAZORECEBIMENT);
    }
}
