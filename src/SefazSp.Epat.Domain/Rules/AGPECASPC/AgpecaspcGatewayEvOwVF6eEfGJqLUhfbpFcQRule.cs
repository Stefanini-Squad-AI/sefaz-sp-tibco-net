#nullable enable

using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules.AGPECASPC;

/// <summary>
/// RI-transition-AGPECASPC-gatewayEvOwVF6eEfGJqLUhfbpFcQ
/// Regra de domínio pura da transição do gateway <c>_EvOwVF6eEfGJqLUhfbpFcQ</c>
/// do processo AGPECASPC.
///
/// Decisão: "Já se esperou pelo prazo em vigor?"
///
/// Expressão legada (XPDL POC_Epat.xpdl, linha 10684):
///   DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZORECEBIMENT
///
/// Quando verdadeiro, o fluxo segue por este ramo para SetPrazo (ramo explícito).
/// Quando falso (OTHERWISE), o fluxo segue para End Event — percurso do cenário SC-AGPECASPC-003.
///
/// DATACONTROLE é comparado com SW_NA — um TERCEIRO estado distinto de null e de vazio.
/// Usa <see cref="FieldValue{T}"/> (shim-tri-state, NOEQ-iprocess-builtin, ratificado 2026-08-06).
/// SW_NA NUNCA é mapeado para null.
///
/// Invariante: o identificador RI-transition-AGPECASPC-gatewayEvOwVF6eEfGJqLUhfbpFcQ
/// não deve ser renomeado.
/// </summary>
public static class AgpecaspcGatewayEvOwVF6eEfGJqLUhfbpFcQRule
{
    /// <summary>
    /// Avalia a condição legada:
    ///   DATACONTROLE == SW_NA  OU  DATACONTROLE != PRAZORECEBIMENT
    ///
    /// Retorna verdadeiro quando o ramo explícito (SetPrazo) deve ser tomado.
    /// Retorna falso (OTHERWISE) quando o fluxo deve seguir para End Event.
    /// </summary>
    /// <param name="dataControle">
    ///   Campo tri-estado: HasValue = data preenchida; IsNotAvailable = SW_NA; Empty = não declarado.
    ///   SW_NA significa "não preenchido" — terceiro estado, nunca null.
    /// </param>
    /// <param name="prazoRecebiment">Data de prazo de recebimento do caso.</param>
    /// <returns>
    ///   <c>true</c> → tomar o ramo explícito (SetPrazo).
    ///   <c>false</c> → tomar o ramo OTHERWISE (End Event).
    /// </returns>
    public static bool ShouldTakeExplicitBranch(
        FieldValue<DateOnly> dataControle,
        DateOnly prazoRecebiment)
    {
        // DATACONTROLE == SW_NA → verdadeiro quando o campo é SW_NA
        if (dataControle.IsNotAvailable)
            return true;

        // DATACONTROLE != PRAZORECEBIMENT → verdadeiro quando as datas diferem
        return dataControle.Match(
            hasValue:     v => v != prazoRecebiment,
            notAvailable: () => true,  // coberto acima
            empty:        () => true); // Empty é distinto de qualquer data
    }
}
