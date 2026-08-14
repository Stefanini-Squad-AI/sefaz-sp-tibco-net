#nullable enable

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
}
