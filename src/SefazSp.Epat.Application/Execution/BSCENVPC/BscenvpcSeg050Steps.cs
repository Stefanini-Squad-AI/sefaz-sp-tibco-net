#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;
using System.Globalization;

namespace SefazSp.Epat.Application.Execution.BSCENVPC;

/// <summary>
/// Passos de envelope técnico do processo BSCENVPC — segmento 050
/// (passos 1–15 do cenário SC-BSCENVPC-013).
///
/// Contém apenas lógica que toca STATUS_CODE, ISAPPERROR, ISTECHERROR e
/// contadores de retentativa — nunca cálculo ou decisão de domínio.
/// A separação segue <c>classification.eRegraDeNegocio</c> do rule-catalogue.json.
///
/// Invariantes (glossário POC_Epat.yaml, confirmados 2026-08-06):
///   STATUS_CODE  : '0' = sucesso; != '0' = erro.
///   ISAPPERROR   : 'N' = sem erro de aplicação; 'Y' = erro de aplicação.
///   ISTECHERROR  : 'N' = sem erro técnico;      'Y' = erro técnico.
///   MAXRETRIES   : 5 por omissão.
///   NUMAPPRETRIES: começa em 0, incrementa a cada falha de aplicação.
///   SW_QRETRYCOUNT: lido pelo motor, nunca escrito pelo processo.
///
/// Card: BUILD-BSCENVPC-seg050
/// </summary>
public static class BscenvpcSeg050Steps
{
    // ── Nó 2: SetParameters (_qIDulV6BEfGBBLgT-R5iuw) ──────────────────────

    /// <summary>
    /// Passo SetParameters — envelope técnico.
    /// Inicializa MAXRETRIES no contexto de execução.
    /// A decisão de domínio (se deve inicializar) já foi avaliada por
    /// <see cref="BscenvpcSetParametersRule.ShouldInitialize"/>.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplySetParameters(ProcessExecutionContext ctx)
    {
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = BscenvpcSetParametersRule.DefaultMaxRetries;
    }

    // ── Nó 3: Start Loop (_qIDul16BEfGBBLgT-R5iuw) ──────────────────────────

    /// <summary>
    /// Passo Start Loop — envelope técnico.
    /// Lê NUMAPPRETRIES para rastreabilidade — o iProcess verificava o valor aqui.
    /// NOEQ-iprocess-builtin: SW_NA via shim-tri-state, ratificado 2026-08-06.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        _ = ctx.NUMAPPRETRIES; // leitura explícita para rastreabilidade
    }

    // ── Nó 6: Start TX (_qIDu3F6BEfGBBLgT-R5iuw) ───────────────────────────

    /// <summary>
    /// Passo Start TX — envelope técnico.
    /// Marca o início da transacção de chamada ao subsistema de controlo.
    /// Sem efeito lateral observável no contexto de execução.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartTx(ProcessExecutionContext ctx)
    {
        // Passo de marcação topológica. Sem escrita de campos do caso.
    }

    // ── Nó 8: Set Technical Error (_qIDu4F6BEfGBBLgT-R5iuw) ────────────────

    /// <summary>
    /// Passo Set Technical Error — envelope técnico.
    /// Executado quando SW_QRETRYCOUNT &gt;= MAXRETRIES (motor esgotou retentativas).
    /// Regista a causa do esgotamento; NÃO altera ISTECHERROR nem ISAPPERROR —
    /// esses flags foram escritos no ciclo anterior pelo Set App Error.
    /// Fonte: RI-script-BSCENVPC-SetTechnicalError (expressão vazia, eRegraDeNegocio=false).
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="reason">Descrição da causa (fornecida pelo runtime).</param>
    public static void ApplySetTechnicalError(ProcessExecutionContext ctx, string reason)
    {
        ctx.STERRORDESC = reason;
    }

    // ── Condições de gateway (topologia como dado) ───────────────────────────

    /// <summary>
    /// Gateway Tech Error (_qIDupF6BEfGBBLgT-R5iuw), ramo "Yes".
    /// Verdadeiro quando ISTECHERROR == "Y" — erro técnico de infraestrutura.
    /// Ramo "No" (otherwise): → App Error.
    /// </summary>
    public static bool IsTechError(ProcessExecutionContext ctx)
        => ctx.ISTECHERROR == "Y";

    /// <summary>
    /// Gateway App Error (_qIDuo16BEfGBBLgT-R5iuw), ramo "Yes".
    /// Regra: ISAPPERROR == "Y".
    /// Fonte: SC-BSCENVPC-013, decisão App Error, tipo CONDITION.
    /// </summary>
    public static bool IsAppError(ProcessExecutionContext ctx)
        => ctx.ISAPPERROR == "Y";

    /// <summary>
    /// Gateway More Retries (_qIDuoF6BEfGBBLgT-R5iuw), ramo "Yes".
    /// Regra: NUMAPPRETRIES &lt; MAXRETRIES.
    /// Fonte: SC-BSCENVPC-013, decisão More Retries, tipo CONDITION.
    /// </summary>
    public static bool HasMoreRetries(ProcessExecutionContext ctx)
        => ctx.NUMAPPRETRIES < ctx.MAXRETRIES;

    // ── Utilitário: extracção de IDPROCESSO de ProcessId ─────────────────────

    /// <summary>
    /// Extrai o campo IDPROCESSO do identificador de correlação <paramref name="processId"/>.
    /// O formato legado é "idAiim-&lt;n&gt;idProc-&lt;n&gt;"; se o marcador "idProc-" não
    /// existir ou o valor for "NA", devolve o estado correspondente do shim tri-state.
    /// NOEQ-iprocess-builtin: SW_NA é o terceiro estado, nunca mapeado para null.
    /// </summary>
    /// <param name="processId">Identificador de correlação legado.</param>
    /// <returns>Valor tri-estado de IDPROCESSO.</returns>
    public static FieldValue<long> ParseIdProcesso(string? processId)
    {
        if (string.IsNullOrEmpty(processId))
            return FieldValue<long>.Empty;

        const string marker = "idProc-";
        var idx = processId.LastIndexOf(marker, StringComparison.Ordinal);
        if (idx < 0)
            return FieldValue<long>.Empty;

        var raw = processId[(idx + marker.Length)..];
        if (string.Equals(raw, "NA", StringComparison.OrdinalIgnoreCase))
            return FieldValue<long>.NotAvailable;

        return long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? FieldValue<long>.Of(v)
            : FieldValue<long>.Empty;
    }
}
