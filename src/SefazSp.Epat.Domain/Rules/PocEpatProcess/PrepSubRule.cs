#nullable enable

// Card: BUILD-POCEPATPROCESS-seg053
// scriptTask 14 — 'prepSub' (_zE3XeV6JEfGBBLgT-R5iuw)
//
// Regra: RI-script-POC_EpatProcess-prepSub
// Classificação: eRegraDeNegocio=true · efeito=calcula-valor
// Lógica de domínio: extrai identificadores de peças (CNT/SF) e conta intimados.
//
// Campos lidos: CNTPECA1-4, SFPECA1-4, CODUADTJ
// Campos escritos: IDPECASCNT, IDPECASSF, IDSINTIMADOS, NRSUBPRO, QTDINTIMADOS, STATUSSUBPROC
//
// naoSabemos / Hipótese 1:
//   - SEARCH/SUBSTR do iProcess assume base 1 (convenção iProcess) — NÃO confirmado.
//   - A implementação abaixo usa semântica .NET (base 0) para operações de string.
//   - Confirmar com legado se a indexação iProcess afecta resultados.
//
// Hipótese 2 (NÃO MIGRAR):
//   - Atribuição '278713|278712|' a IDSINTIMADOS é scaffolding de teste — omitida.
//
// Decisão NOEQ-iprocess-builtin → shim-tri-state:
//   SW_NA nos campos CNTPECA/SFPECA/CODUADTJ é estado distinto.

using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Domain.Rules.PocEpatProcess;

/// <summary>
/// Regra de domínio para o scriptTask 'prepSub'
/// (<c>_zE3XeV6JEfGBBLgT-R5iuw</c>) do processo POC_EpatProcess.
///
/// Prepara os subprocessos de intimação: extrai identificadores de peças CNT e SF,
/// concatena em <c>IDSINTIMADOS</c>, conta intimados e define status inicial.
/// Função pura: não depende de relógio, I/O nem estado externo.
///
/// <para>
/// <b>Hipótese 1 (NÃO CONFIRMADA):</b> SEARCH/SUBSTR do iProcess usa base 1.
/// A implementação .NET usa base 0. Confirmar com legado se afecta resultados.
/// </para>
///
/// <para>
/// <b>Hipótese 2:</b> A atribuição <c>IDSINTIMADOS = '278713|278712|'</c> é
/// scaffolding de teste do legado — <b>NÃO MIGRADA</b>.
/// </para>
/// </summary>
public static class PrepSubRule
{
    /// <summary>
    /// Identificador da regra de instância — invariante: não renomear.
    /// </summary>
    public const string RuleId = "RI-script-POC_EpatProcess-prepSub";

    /// <summary>
    /// Aplica a regra 'prepSub' ao caso.
    ///
    /// <para>
    /// Comportamento:
    /// <list type="number">
    ///   <item>Para cada slot CNTPECA1-4: se != SW_NA, colecta valor para IDPECASCNT.</item>
    ///   <item>Para cada slot SFPECA1-4: se != SW_NA, colecta valor para IDPECASSF.</item>
    ///   <item>IDSINTIMADOS = concatenação de IDPECASCNT + IDPECASSF com separador '|'.</item>
    ///   <item>NRSUBPRO = lista de IDs colectados.</item>
    ///   <item>QTDINTIMADOS = contagem total de IDs.</item>
    ///   <item>STATUSSUBPROC = "inativo".</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    public static void Apply(AiimCase aiimCase)
    {
        // ── Colectar IDs das peças CNT (se != SW_NA) ───────────────────────────
        var cntIds = new List<string>();
        CollectIfNotSwNa(aiimCase.CNTPECA1, cntIds);
        CollectIfNotSwNa(aiimCase.CNTPECA2, cntIds);
        CollectIfNotSwNa(aiimCase.CNTPECA3, cntIds);
        CollectIfNotSwNa(aiimCase.CNTPECA4, cntIds);

        // ── Colectar IDs das peças SF (se != SW_NA) ────────────────────────────
        var sfIds = new List<string>();
        CollectIfNotSwNa(aiimCase.SFPECA1, sfIds);
        CollectIfNotSwNa(aiimCase.SFPECA2, sfIds);
        CollectIfNotSwNa(aiimCase.SFPECA3, sfIds);
        CollectIfNotSwNa(aiimCase.SFPECA4, sfIds);

        // ── Atribuir IDPECASCNT e IDPECASSF ────────────────────────────────────
        // Concatenar valores com separador '|' e terminar com '|' (convenção legado).
        aiimCase.IDPECASCNT = cntIds.Count > 0 ? string.Join("|", cntIds) + "|" : string.Empty;
        aiimCase.IDPECASSF = sfIds.Count > 0 ? string.Join("|", sfIds) + "|" : string.Empty;

        // ── IDSINTIMADOS: junção de CNT + SF ───────────────────────────────────
        // naoSabemos / Hipótese 2: a atribuição '278713|278712|' é scaffolding de teste — omitida.
        var allIds = new List<string>(cntIds.Count + sfIds.Count);
        allIds.AddRange(cntIds);
        allIds.AddRange(sfIds);
        aiimCase.IDSINTIMADOS = allIds.Count > 0 ? string.Join("|", allIds) + "|" : string.Empty;

        // ── NRSUBPRO: lista de IDs como array de strings ───────────────────────
        aiimCase.NRSUBPRO = allIds.AsReadOnly();

        // ── QTDINTIMADOS: contagem total ───────────────────────────────────────
        aiimCase.QTDINTIMADOS = allIds.Count;

        // ── STATUSSUBPROC: status inicial ──────────────────────────────────────
        aiimCase.STATUSSUBPROC = "inativo";

        // ── CODUADTJ: se SW_NA, certas lógicas são ignoradas ───────────────────
        // naoSabemos: a expressão CODUADTJ==SW_NA no legado pode afectar outras lógicas
        // não declaradas no pacote. O campo não é alterado aqui, apenas lido para referência.
        // A condição está documentada mas sem corpo definido no script observado.
    }

    /// <summary>
    /// Colecta o valor se o campo não for SW_NA nem vazio.
    /// </summary>
    private static void CollectIfNotSwNa(in ValueObjects.FieldValue<string> field, List<string> target)
    {
        field.Match(
            hasValue:      v =>
            {
                if (!string.IsNullOrEmpty(v))
                    target.Add(v);
                return 0; // dummy return
            },
            notAvailable:  () => 0,  // SW_NA: não colectar
            empty:         () => 0);
    }
}
