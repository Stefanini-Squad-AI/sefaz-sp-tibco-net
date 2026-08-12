#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Workflows.BSCENVPC;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.BSCENVPC;

/// <summary>
/// Oracle scenario-path para SC-BSCENVPC-016.
/// Verifica que a implementacao do workflow BSCENVPC visita os nos na ordem
/// descrita em artifacts/POC_Epat/scenarios/SC-BSCENVPC-016.json.
///
/// Fixture: imutavel — nenhum valor esperado e escrito ou editado por este teste.
/// Caso 1 de 1: troco de 'Start Event' a 'Done - Success' (ordemNaJornada=1, passos 1-12).
/// </summary>
public sealed class SC_BSCENVPC_016_OracleTest
{
    private static readonly string FixturePath = Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..", "..", "..",
            "artifacts", "POC_Epat", "scenarios", "SC-BSCENVPC-016.json"));

    [Fact(DisplayName = "SC-BSCENVPC-016 — scenario-path: de 'Start Event' a 'Done - Success' (12 nos)")]
    public void SegmentoUm_DeveProduzirCaminhoExpertoFixtura()
    {
        // ── Arrange: ler o fixture (imutavel) ───────────────────────────────────────
        Assert.True(File.Exists(FixturePath), $"Fixture nao encontrada: {FixturePath}");

        using var json = JsonDocument.Parse(File.ReadAllText(FixturePath));
        var pathNodes = json.RootElement
            .GetProperty("path")
            .EnumerateArray()
            .Select(n => n.GetProperty("id").GetString()!)
            .ToList();

        Assert.Equal(12, pathNodes.Count);

        // ── Arrange: contexto de entrada para o segmento 1 ─────────────────────────
        // Inputs que conduzem ao caminho SC-BSCENVPC-016:
        //   • SW_QRETRYCOUNT = 5, MAXRETRIES = 5 (nao inicializado) → apos SetParameters
        //     MAXRETRIES = 5, e SW_QRETRYCOUNT >= MAXRETRIES → ramo Maxretriesexceeded
        //   • O ActivitySet corre em escopo fechado: ISTECHERROR='Y' fica local.
        //   • Contexto pai apos Start Loop: ISAPPERROR='N', ISTECHERROR='N'
        //     → Tech Error: ramo No → App Error: ramo No → Done - Success
        var ctx = new ProcessExecutionContext
        {
            MAXRETRIES = 0,   // nao inicializado; SetParameters escreve 5
            ISAPPERROR = "N",
            ISTECHERROR = "N",
            OUTCOME = "OK",
        };
        long swQRetryCount = 5; // >= MAXRETRIES (5) apos inicializacao → Maxretriesexceeded

        // ── Act ─────────────────────────────────────────────────────────────────────
        var workflow = new BscenvpcWorkflow();
        var actualPath = workflow.Execute(ctx, swQRetryCount);

        // ── Assert ──────────────────────────────────────────────────────────────────
        Assert.Equal(pathNodes, actualPath);
    }
}
