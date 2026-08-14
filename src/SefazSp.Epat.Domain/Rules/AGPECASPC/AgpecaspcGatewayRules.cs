#nullable enable

// Card: BUILD-AGPECASPC-seg040
// Regra: RI-transition-AGPECASPC-gatewayEvOwVF6eEfGJqLUhfbpFcQ
// Fonte XPDL: linha 10684 — gateway _EvOwVF6eEfGJqLUhfbpFcQ
// Classificacao: eRegraDeNegocio = true → Domain/Rules
//
// Expressao XPDL: DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZORECEBIMENT;
// Consequencia: quando verdadeiro, o fluxo segue para SetPrazo.
//
// Decisao gaps.iprocess-builtin = shim-tri-state (ratificado 2026-08-06):
//   DATACONTROLE e FieldValue<DateOnly>; SW_NA e terceiro estado.
//
// SENTINEL-AGPECASPC-_EvOwZF6eE (glossario POC_Epat.yaml, ratificado 2026-08-07):
//   SW_NA significa PRIMEIRA VOLTA do laco de prazo: ainda nao se esperou por prazo nenhum.
//   Segue junto com 'prazo mudou', unido pelo ||, exactamente como no legado.
//   O ciclo e: [gateway] -> SetPrazo -> Aguardar Interposicoes -> Controla Datas -> [gateway].
//   Controla Datas escreve DATACONTROLE = PRAZORECEBIMENT; quando coincidem o ciclo termina.

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules.AGPECASPC;

/// <summary>
/// Regra de transição do gateway AGPECASPC <c>_EvOwVF6eEfGJqLUhfbpFcQ</c>.
///
/// Pergunta: "Já se esperou pelo prazo em vigor?"
/// <list type="bullet">
///   <item><description>
///     Resposta <see langword="true"/> → ainda não esperou (SW_NA) ou o prazo foi prorrogado →
///     fluxo segue para SetPrazo (<c>_EvOwUl6eEfGJqLUhfbpFcQ</c>).
///   </description></item>
///   <item><description>
///     Resposta <see langword="false"/> → já esperou pelo prazo actual → sai do ciclo.
///   </description></item>
/// </list>
///
/// Sentinel: DATACONTROLE inicia em SW_NA (primeira volta).
/// Controla Datas escreve DATACONTROLE = PRAZORECEBIMENT para interromper o ciclo
/// (SENTINEL-AGPECASPC-_EvOwZF6eE, ratificado 2026-08-07).
///
/// Decisão <c>gaps.iprocess-builtin = shim-tri-state</c>:
/// O tipo <see cref="FieldValue{T}"/> impõe a decisão para cada um dos três estados,
/// impedindo o colapso silencioso de SW_NA em null.
/// </summary>
public static class AgpecaspcGatewayRules
{
    /// <summary>
    /// RI-transition-AGPECASPC-gatewayEvOwVF6eEfGJqLUhfbpFcQ.
    /// Devolve <see langword="true"/> se o ciclo de espera por interposições deve continuar.
    /// Devolve <see langword="false"/> se o prazo já foi aguardado e o ciclo deve terminar.
    ///
    /// Tradução directa de:
    /// <c>DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZORECEBIMENT</c>
    /// </summary>
    /// <param name="caso">Estado actual do caso AIIM.</param>
    public static bool DeveAguardarInterposicoes(AiimCase caso)
        => caso.DATACONTROLE.Match(
            // HasValue: compara com PRAZORECEBIMENT; verdadeiro se o prazo mudou.
            hasValue:     dataControle => dataControle != caso.PRAZORECEBIMENT,
            // SW_NA: primeira volta do laço — ainda não esperou nada → vai aguardar.
            notAvailable: () => true,
            // Empty: equivalente a não preenchido — vai aguardar (defensivo).
            empty:        () => true);
}
