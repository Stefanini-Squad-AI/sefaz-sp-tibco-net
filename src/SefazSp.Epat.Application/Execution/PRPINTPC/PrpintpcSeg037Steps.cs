#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Execution.PRPINTPC;

/// <summary>
/// Passos de envelope técnico do processo PRPINTPC — segmento 037
/// (passos 1–15 do cenário SC-PRPINTPC-010, de 'Start Event' a 'Done - Success').
///
/// Contém apenas lógica que toca o envelope técnico — STATUS_CODE, ISAPPERROR,
/// ISTECHERROR, contadores de retentativa — nunca cálculo ou decisão de domínio.
/// A separação segue <c>classification.eRegraDeNegocio</c> do rule-catalogue.json.
///
/// Diferença face a <see cref="PrpintpcSeg035Steps"/>:
///   • ApplyStartLoop: no segmento 037 (cenário SC-PRPINTPC-010, percurso one-shot),
///     NUMAPPRETRIES é sempre reinicializado a zero. O segmento 035 (SC-PRPINTPC-009,
///     percurso com laço de retentativas) tem semântica distinta neste passo.
///
/// Invariantes (glossário POC_Epat.yaml, confirmados 2026-08-06):
///   STATUS_CODE  : '0' = sucesso; != '0' = erro.   ATENÇÃO: o XPDL original usava SW_NA —
///                  corrigido para '0' per rulings.CLONE-PRPINTPC.
///   ISAPPERROR   : 'N' = sem erro de aplicação; 'Y' = erro de aplicação.
///   ISTECHERROR  : 'N' = sem erro técnico;      'Y' = erro técnico.
///   MAXRETRIES   : 5 por omissão.
///   NUMAPPRETRIES: começa em 0, incrementa a cada falha de aplicação.
///   SW_QRETRYCOUNT: lido, nunca escrito.
///
/// Card: BUILD-PRPINTPC-seg037
/// </summary>
public static class PrpintpcSeg037Steps
{
    // ── Nó 3: Start Loop (_KEwC4F6EEfGBBLgT-R5iuw) ─────────────────────────

    /// <summary>
    /// Passo Start Loop — envelope técnico (segmento 037, one-shot).
    /// Regra de domínio: RI-script-PRPINTPC-StartLoop (ver <c>PrpintpcStartLoopRule</c>).
    ///
    /// Quando o guard de domínio indicar que a inicialização é necessária,
    /// reinicializa NUMAPPRETRIES a zero para a iteração em curso.
    /// O segmento 037 é um percurso one-shot (SC-PRPINTPC-010): não há laço de
    /// retentativas no âmbito MAIN — NUMAPPRETRIES é sempre reinicializado.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        ctx.NUMAPPRETRIES = 0;
    }
}
