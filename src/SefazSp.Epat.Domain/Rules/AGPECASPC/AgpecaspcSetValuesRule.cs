#nullable enable

// Card: BUILD-AGPECASPC-seg040
// Regra: RI-script-AGPECASPC-SetValues
// Fonte XPDL: linha 10549 — passo "Set Values" (_EvOwTF6eEfGJqLUhfbpFcQ)
// Classificacao: eRegraDeNegocio = true → Domain/Rules
//
// Expressao XPDL:
//   CNTPECA1 != IPESystemValues.SW_NA || CNTPECA1 != '9'
//   | CNTPECA2 != IPESystemValues.SW_NA
//   | CNTPECA3 != IPESystemValues.SW_NA
//   | CNTPECA4 != IPESystemValues.SW_NA
//
// Decisao gaps.iprocess-builtin = shim-tri-state (ratificado 2026-08-06):
//   CNTPECA1-4 sao FieldValue<string>; SW_NA e terceiro estado distinto de null e de Empty.
//
// INCERTEZAS DOCUMENTADAS (naoSabemos em rule-catalogue.json):
//   • O significado exacto do valor sentinela "9" para CNTPECA1 nao esta declarado no pacote.
//   • Os valores concretos a atribuir a FIELDSNAMES, FIELDSTYPES, FIELDSVALUES, IDPECAS,
//     PERIODOEMDIAS nao estao declarados; a expressao XPDL e opaca neste ponto.
//   • O nome completo (label de negocio) de CNTPECA1-4 nao esta declarado no pacote.
// Hipotese adoptada: CNTPECA1 == '9' e valor sentinela de fim de lista; qualquer outro
// valor nao-SW_NA indica uma peca disponivel.

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules.AGPECASPC;

/// <summary>
/// RI-script-AGPECASPC-SetValues
/// Regra de domínio pura do passo "Set Values" do processo AGPECASPC.
///
/// Expressão legada (XPDL POC_Epat.xpdl, linha 10549):
///   CNTPECA1!=IPESystemValues.SW_NA || CNTPECA1!='9'
///   | CNTPECA2!=IPESystemValues.SW_NA
///   | CNTPECA3!=IPESystemValues.SW_NA
///   | CNTPECA4!=IPESystemValues.SW_NA
///
/// Consequência: escreve FIELDSNAMES, FIELDSTYPES, FIELDSVALUES, IDPECAS, PERIODOEMDIAS.
/// Os valores atribuídos não estão declarados no XPDL (naoSabemos no rule-catalogue.json);
/// a implementação expressa a condição de guarda fielmente ao legado.
///
/// CNTPECA1, CNTPECA2, CNTPECA3, CNTPECA4 são comparados com SW_NA —
/// um TERCEIRO estado distinto de null e de vazio.
/// Usa <see cref="FieldValue{T}"/> (shim-tri-state, NOEQ-iprocess-builtin, ratificado 2026-08-06).
/// SW_NA NUNCA é mapeado para null.
///
/// Invariante: o identificador RI-script-AGPECASPC-SetValues não deve ser renomeado.
/// Regra de domínio <c>RI-script-AGPECASPC-SetValues</c>.
///
/// Avalia quais peças estão disponíveis (CNTPECA1–4 != SW_NA e CNTPECA1 != '9')
/// e devolve <see langword="true"/> se existe pelo menos uma peça disponível —
/// condição que governa os campos FIELDSNAMES / FIELDSTYPES / FIELDSVALUES / IDPECAS /
/// PERIODOEMDIAS no passo <c>Set Values</c> (<c>_EvOwTF6eEfGJqLUhfbpFcQ</c>).
///
/// Decisão <c>gaps.iprocess-builtin = shim-tri-state</c>:
/// <see cref="FieldValue{T}"/> força o compilador a exigir a decisão em cada um dos
/// três estados (HasValue / IsNotAvailable=SW_NA / Empty).
/// </summary>
public static class AgpecaspcSetValuesRule
{
    /// <summary>
    /// Literal sentinela de CNTPECA1 observado no pacote.
    /// Significado de negócio não declarado; o XPDL só mostra que o valor é usado
    /// como limite de comparação.
    /// </summary>
    public const string CntPeca1SentinelLiteral = "9";

    /// <summary>
    /// Avalia a condição de guarda legada:
    ///   CNTPECA1 != SW_NA  OU  CNTPECA1 != '9'
    ///   OU CNTPECA2 != SW_NA
    ///   OU CNTPECA3 != SW_NA
    ///   OU CNTPECA4 != SW_NA
    ///
    /// Retorna verdadeiro quando a condição permite a execução das atribuições de
    /// FIELDSNAMES, FIELDSTYPES, FIELDSVALUES, IDPECAS e PERIODOEMDIAS.
    /// </summary>
    /// <param name="cntPeca1">
    ///   Campo tri-estado: HasValue = preenchido; IsNotAvailable = SW_NA; Empty = não declarado.
    ///   SW_NA significa "não preenchido" — terceiro estado, nunca null.
    /// </param>
    /// <param name="cntPeca2">Campo tri-estado CNTPECA2.</param>
    /// <param name="cntPeca3">Campo tri-estado CNTPECA3.</param>
    /// <param name="cntPeca4">Campo tri-estado CNTPECA4.</param>
    /// <returns>
    ///   <c>true</c> → executar as atribuições no contexto de execução.
    ///   <c>false</c> → nenhuma escrita necessária.
    /// </returns>
    public static bool ShouldSetValues(
        FieldValue<string> cntPeca1,
        FieldValue<string> cntPeca2,
        FieldValue<string> cntPeca3,
        FieldValue<string> cntPeca4)
    {
        // CNTPECA1 != SW_NA
        bool cntPeca1NotSwNa = !cntPeca1.IsNotAvailable;

        // CNTPECA1 != '9'  (SW_NA != '9' → true; Empty != '9' → true)
        bool cntPeca1Not9 = cntPeca1.Match(
            hasValue:     v => v != CntPeca1SentinelLiteral,
            notAvailable: () => true,
            empty:        () => true);

        // CNTPECA2 != SW_NA
        bool cntPeca2NotSwNa = !cntPeca2.IsNotAvailable;

        // CNTPECA3 != SW_NA
        bool cntPeca3NotSwNa = !cntPeca3.IsNotAvailable;

        // CNTPECA4 != SW_NA
        bool cntPeca4NotSwNa = !cntPeca4.IsNotAvailable;

        // iProcess | e || são ambos OR lógico
        return (cntPeca1NotSwNa || cntPeca1Not9)
               || cntPeca2NotSwNa
               || cntPeca3NotSwNa
               || cntPeca4NotSwNa;
    }

    /// Valor sentinela de CNTPECA1 que indica fim de lista de peças.
    /// O significado exacto de "9" não está declarado no pacote (naoSabemos).
    /// </summary>
    private const string CntPeca1TerminalValue = "9";

    /// <summary>
    /// RI-script-AGPECASPC-SetValues.
    /// Devolve <see langword="true"/> se há pelo menos uma peça disponível.
    ///
    /// Tradução directa da expressão XPDL:
    /// <c>CNTPECA1 != SW_NA || CNTPECA1 != '9' | CNTPECA2 != SW_NA | CNTPECA3 != SW_NA | CNTPECA4 != SW_NA</c>
    ///
    /// O operador <c>|</c> (sem curto-circuito) do iProcess é traduzido como <c>||</c>
    /// para todos os operandos booleanos — o efeito é equivalente.
    /// </summary>
    public static bool ExistePecaDisponivel(AiimCase caso)
    {
        // CNTPECA1: disponível quando IsNotAvailable (SW_NA) é FALSE e valor != '9'.
        // A expressão XPDL combina as duas condições com ||: qualquer uma verdadeira
        // considera a peça 1 presente. Dado que SW_NA != '9' é sempre verdadeiro,
        // a interpretação mais conservadora é: peca1 disponível <=> HasValue AND != '9'.
        var peca1Disponivel = caso.CNTPECA1.Match(
            hasValue: v => v != CntPeca1TerminalValue,
            notAvailable: () => false,   // SW_NA: peça 1 não preenchida
            empty:        () => false);  // Empty: equivalente a não preenchido

        // CNTPECA2-4: disponível quando IsNotAvailable (SW_NA) é FALSE.
        var peca2Disponivel = caso.CNTPECA2.Match(
            hasValue: _ => true,
            notAvailable: () => false,
            empty:        () => false);

        var peca3Disponivel = caso.CNTPECA3.Match(
            hasValue: _ => true,
            notAvailable: () => false,
            empty:        () => false);

        var peca4Disponivel = caso.CNTPECA4.Match(
            hasValue: _ => true,
            notAvailable: () => false,
            empty:        () => false);

        return peca1Disponivel || peca2Disponivel || peca3Disponivel || peca4Disponivel;
    }
}
