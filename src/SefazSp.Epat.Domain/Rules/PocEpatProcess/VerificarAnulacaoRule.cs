#nullable enable

// Card: BUILD-POCEPATPROCESS-seg034
// AC2 — scriptTask 'Verificar Anulacao' (_CI6lx1qREfG5K7mY0I3I6w, entrouPor=fluxo)
//
// Classificação (rule-catalogue.json · RI-script-POC_EpatProcess-VerificarAnulacao):
//   eRegraDeNegocio=true · efeito=calcula-valor
//   "calcula ou atribui o valor de um campo do caso"
//   → lógica de domínio fica em Domain/Rules como função pura.
//
// Expressão legado (POC_Epat.xpdl, linha 1895):
//   ANULACAODTJ == true && INSTANCIA == 2 | INDNAORECORRER == true | INDNAORECORRER == false | ORIGEM == IPESystemValues.SW_NA
//
// Campos lidos : ANULACAODTJ, INSTANCIA, INDNAORECORRER, ORIGEM
// Campos escritos: FLAGCRZ, INDNAORECORRER, ORIGEM, STATUSRECURSOS
//
// naoSabemos:
//   - o corpo completo de atribuição não está declarado no pacote (confidence=medium, translation=lossy)
//   - ORIGEM é comparada com SW_NA → usa FieldValue<string>.IsNotAvailable (shim-tri-state)
//
// Decisão NOEQ-iprocess-builtin → shim-tri-state (2026-08-06):
//   SW_NA é um terceiro estado distinto de null e de vazio; ORIGEM usa FieldValue<string>.

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules.PocEpatProcess;

/// <summary>
/// Regra de domínio para o scriptTask 'Verificar Anulacao'
/// (<c>_CI6lx1qREfG5K7mY0I3I6w</c>) do processo POC_EpatProcess.
///
/// Avalia a condição de anulação DTJ e actualiza os campos de controlo de recursos.
/// Função pura: não depende de relógio, I/O nem estado externo.
///
/// <para>
/// <b>Nota de tradução (confidence=medium, translation=lossy):</b>
/// O corpo completo das atribuições a <c>FLAGCRZ</c> e <c>STATUSRECURSOS</c> não está
/// declarado no pacote. A lógica abaixo é derivada da expressão XPDL observada e das
/// condições inferidas; deve ser confirmada contra o legado antes de produção.
/// </para>
/// </summary>
public static class VerificarAnulacaoRule
{
    /// <summary>
    /// Identificador da regra de instância — invariante: não renomear.
    /// </summary>
    public const string RuleId = "RI-script-POC_EpatProcess-VerificarAnulacao";

    /// <summary>
    /// Aplica a regra 'Verificar Anulacao' ao caso.
    ///
    /// <para>
    /// Comportamento derivado do script legado (linha 1895 do POC_Epat.xpdl):
    /// <list type="bullet">
    ///   <item>
    ///     Se <c>ANULACAODTJ == true</c> e <c>INSTANCIA == 2</c>:
    ///     o caso está na segunda instância e a anulação DTJ foi decretada;
    ///     <c>FLAGCRZ</c>, <c>STATUSRECURSOS</c> e <c>INDNAORECORRER</c> são ajustados.
    ///   </item>
    ///   <item>
    ///     Se <c>ORIGEM</c> for <c>SW_NA</c> (não disponível): o campo de origem
    ///     é normalizado para vazio.
    ///   </item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// <b>Atenção:</b> os valores exactos atribuídos a <c>FLAGCRZ</c> e
    /// <c>STATUSRECURSOS</c> NÃO estão declarados no pacote — marcados como
    /// <c>naoSabemos</c> em rule-catalogue.json. Os corpos abaixo são a melhor
    /// interpretação possível da expressão observada; confirmar com o legado.
    /// </para>
    /// </summary>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    public static void Apply(AiimCase aiimCase)
    {
        // ── Condição 1: Anulação DTJ na 2.ª instância ─────────────────────────
        // expressão original: ANULACAODTJ == true && INSTANCIA == 2
        if (aiimCase.ANULACAODTJ == true && aiimCase.INSTANCIA == 2)
        {
            // FLAGCRZ: sinaliza cruzamento de flags de anulação.
            // Valor exacto NÃO confirmado — naoSabemos (rule-catalogue.json linha 1895).
            aiimCase.FLAGCRZ = true;

            // INDNAORECORRER: na 2.ª instância com anulação DTJ, não há recurso.
            // Valor inferido da expressão; deve ser confirmado contra o legado.
            aiimCase.INDNAORECORRER = true;
        }

        // ── Condição 2: ORIGEM == SW_NA → normalizar para Empty ───────────────
        // expressão original: ORIGEM == IPESystemValues.SW_NA
        // Decisão shim-tri-state: SW_NA é terceiro estado distinto de null/vazio.
        aiimCase.ORIGEM = aiimCase.ORIGEM.Match(
            hasValue:      v => FieldValue<string>.Of(v),
            notAvailable:  () => FieldValue<string>.Empty,  // SW_NA → normalizar
            empty:         () => FieldValue<string>.Empty);

        // ── STATUSRECURSOS: valor exacto NÃO declarado no pacote ──────────────
        // naoSabemos (rule-catalogue.json): o corpo completo da atribuição exige
        // confirmação contra o legado TIBCO antes de produção.
        // A variável não é alterada se nenhuma condição anterior se verificou.
    }
}
