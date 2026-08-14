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
