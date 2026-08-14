#nullable enable

using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-script-PRPINTPC-StartLoop
/// Regra de domínio pura do passo Start Loop do processo PRPINTPC.
///
/// Expressão legada (ficheiro POC_Epat.xpdl, linha 7429):
///   NUMAPPRETRIES==null | INSTANCIA == 2 | STSADMTITCNT == IPESystemValues.SW_NA | STSADMTITDRF == IPESystemValues.SW_NA
/// Consequência: inicializa NUMAPPRETRIES, ISAPPERROR, ISTECHERROR, OUTCOME, DATETIME, STSADMTITCNT, STSADMTITDRF.
///
/// STSADMTITCNT e STSADMTITDRF são comparados com SW_NA — TERCEIRO estado distinto de null e de vazio.
/// Usa <see cref="FieldValue{T}"/> (shim-tri-state, decisão NOEQ-iprocess-builtin, ratificada 2026-08-06).
/// SW_NA NUNCA é mapeado para null.
///
/// NUMAPPRETRIES=="null" em iProcess: em .NET, NUMAPPRETRIES é int; "null" é representado
/// como argumento <c>int? numAppRetries = null</c>, passado pelo chamador quando o campo
/// ainda não foi inicializado nesta execução.
///
/// Card: BUILD-PRPINTPC-seg037 · AC2 (nó _KEwC4F6EEfGBBLgT-R5iuw)
/// Invariante: identificadores de nó não devem ser renomeados.
/// </summary>
public static class PrpintpcStartLoopRule
{
    /// <summary>
    /// Avalia a condição legada:
    ///   NUMAPPRETRIES==null  OU  INSTANCIA == 2
    ///   OU  STSADMTITCNT == SW_NA  OU  STSADMTITDRF == SW_NA
    ///
    /// Retorna <c>true</c> quando o passo Start Loop deve inicializar os campos de estado
    /// para a iteração em curso.
    /// </summary>
    /// <param name="numAppRetries">
    ///   Null quando NUMAPPRETRIES ainda não foi inicializado nesta execução.
    ///   Distinto de zero: zero significa "nenhuma falha registada", null significa
    ///   "loop ainda não arrancou".
    /// </param>
    /// <param name="instancia">Valor do campo INSTANCIA do caso (inteiro).</param>
    /// <param name="stsadmTitCnt">
    ///   Campo tri-estado STSADMTITCNT (StatusAdmissaoTITCNT).
    ///   HasValue = preenchido; IsNotAvailable = SW_NA; Empty = não declarado.
    ///   SW_NA NUNCA é mapeado para null.
    /// </param>
    /// <param name="stsadmTitDrf">
    ///   Campo tri-estado STSADMTITDRF (StatusAdmissaoTITDRF).
    ///   HasValue = preenchido; IsNotAvailable = SW_NA; Empty = não declarado.
    ///   SW_NA NUNCA é mapeado para null.
    /// </param>
    /// <returns>
    ///   <c>true</c>  → inicializar os campos de estado para esta iteração do loop.<br/>
    ///   <c>false</c> → nenhuma inicialização necessária.
    /// </returns>
    public static bool ShouldInitialize(
        int?          numAppRetries,
        int           instancia,
        FieldValue<int> stsadmTitCnt,
        FieldValue<int> stsadmTitDrf) =>
        numAppRetries is null
        || instancia == 2
        || stsadmTitCnt.IsNotAvailable
        || stsadmTitDrf.IsNotAvailable;
}
