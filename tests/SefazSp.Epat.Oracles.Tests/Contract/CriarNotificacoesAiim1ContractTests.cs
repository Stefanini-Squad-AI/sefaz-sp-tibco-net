// Oracle: contract — fixture artifacts/POC_Epat/service-contracts.json (immutable=true, caseCount=1)
// Card:   VALID-SOLEPATINTERFACEWRAPPERSSOLCRIARNOTIFICACOESAIIM1-contrato
// Rastreia: AC1 (pedido e resposta validam contra o esquema do WSDL) e AC-ORACULO.
// INVARIANTE: este ficheiro nunca escreve nem edita valores esperados.

#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Contract;

/// <summary>
/// Prova que o contrato declarado para a operação
/// <c>__sol_EPATInterfaceWrappers_sol_criarNotificacoesAIIM.1</c>
/// na fixture <c>artifacts/POC_Epat/service-contracts.json</c>
/// é consistente com o esquema do WSDL entregue (EPAT.wsdl).
///
/// O oráculo é imutável: os valores esperados vêm exclusivamente da fixture —
/// nenhum valor esperado foi escrito nem editado neste ficheiro.
/// </summary>
public sealed class CriarNotificacoesAiim1ContractTests
{
    private const string OperationName = "__sol_EPATInterfaceWrappers_sol_criarNotificacoesAIIM.1";
    private const string FixtureRelativePath = "artifacts/POC_Epat/service-contracts.json";

    // Raiz do repositório: sobe até encontrar a fixture (robusto em qualquer CWD de test runner).
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, FixtureRelativePath)))
            dir = dir.Parent;
        if (dir == null)
            throw new FileNotFoundException(
                $"Não foi possível localizar '{FixtureRelativePath}' subindo a partir de '{AppContext.BaseDirectory}'.");
        return dir.FullName;
    }

    private static JsonDocument LoadFixture()
    {
        var path = Path.Combine(RepoRoot(), FixtureRelativePath);
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    /// <summary>
    /// AC1 + AC-ORACULO: localiza o processBinding da operação na fixture e valida
    /// pedido (inputs) e resposta (outputs) contra os soapPaths declarados no WSDL.
    /// 1 caso, conforme acceptance.oracle.caseCount = 1.
    /// </summary>
    [Fact(DisplayName =
        "[contract] criarNotificacoesAIIM.1 — pedido e resposta validam contra o esquema do WSDL")]
    public void PedidoERespostaValidamContraEsquemaWsdl()
    {
        using var fixture = LoadFixture();
        var root = fixture.RootElement;

        // ── 1. Localizar o processBinding da operação ──────────────────────────
        var bindings = root.GetProperty("processBindings").EnumerateArray().ToList();
        var binding = bindings.FirstOrDefault(b =>
            b.TryGetProperty("operationName", out var op) &&
            op.GetString() == OperationName);

        Assert.True(binding.ValueKind != JsonValueKind.Undefined,
            $"A fixture não declara processBinding para '{OperationName}'.");

        // ── 2. Metadados do contrato ───────────────────────────────────────────
        Assert.Equal("EPAT.wsdl", binding.GetProperty("wsdl").GetString());
        Assert.Equal(OperationName, binding.GetProperty("operationName").GetString());

        var transport = binding.GetProperty("transport").GetString();
        Assert.False(string.IsNullOrWhiteSpace(transport),
            "O campo 'transport' deve estar preenchido.");

        var serviceName = binding.GetProperty("serviceName").GetString();
        Assert.False(string.IsNullOrWhiteSpace(serviceName),
            "O campo 'serviceName' deve estar preenchido.");

        var portName = binding.GetProperty("portName").GetString();
        Assert.False(string.IsNullOrWhiteSpace(portName),
            "O campo 'portName' deve estar preenchido.");

        // ── 3. Validar pedido (inputs) contra os soapPaths do WSDL ────────────
        //
        // O WSDL declara criarNotificacoesAIIMRequest com:
        //   HEADER (tobjects:HEADER)  — TRANSACTION_ID, PROCESS_ID, DATETIME
        //   BODY   (BODY_criarNotificacoesAIIMRequest) — idAiim, idAiimAnterior,
        //          caseNumber, loginCoordenador, login, autuadosConcatenados,
        //          dtEnvioNotificacao (optional)
        //
        // Cada soapPath na fixture deve começar com o prefixo de envelope
        // da operação, seguido de /param/sequence/criarNotificacoesAIIMRequest/...
        const string inputPrefix =
            OperationName + "/param/sequence/criarNotificacoesAIIMRequest/sequence/";

        var inputs = binding.GetProperty("inputs").EnumerateArray().ToList();
        Assert.NotEmpty(inputs);

        foreach (var input in inputs)
        {
            var soapPath = input.GetProperty("soapPath").GetString()!;
            Assert.True(soapPath.StartsWith(inputPrefix, StringComparison.Ordinal),
                $"Input soapPath '{soapPath}' não segue o envelope do pedido declarado no WSDL " +
                $"(esperado prefixo '{inputPrefix}').");

            var caseField = input.GetProperty("caseField").GetString();
            Assert.False(string.IsNullOrWhiteSpace(caseField),
                $"O campo 'caseField' deve estar preenchido para o input com soapPath '{soapPath}'.");
        }

        // Os campos WSDL obrigatórios devem estar mapeados na fixture.
        AssertInputFieldMapped(inputs, "BODY/sequence/idAiim");
        AssertInputFieldMapped(inputs, "BODY/sequence/idAiimAnterior");
        AssertInputFieldMapped(inputs, "BODY/sequence/caseNumber");
        AssertInputFieldMapped(inputs, "BODY/sequence/login");
        AssertInputFieldMapped(inputs, "BODY/sequence/loginCoordenador");
        AssertInputFieldMapped(inputs, "BODY/sequence/autuadosConcatenados");
        AssertInputFieldMapped(inputs, "HEADER/sequence/TRANSACTION_ID");
        AssertInputFieldMapped(inputs, "HEADER/sequence/PROCESS_ID");
        AssertInputFieldMapped(inputs, "HEADER/sequence/DATETIME");

        // ── 4. Validar resposta (outputs) contra os soapPaths do WSDL ─────────
        //
        // O WSDL declara criarNotificacoesAIIMResponse com:
        //   RESULT/sequence/STATUS_CODE
        //   RESULT/sequence/ERROR/sequence/ERROR_CODE
        //   RESULT/sequence/ERROR/sequence/ERROR_DESCRIPTION
        //   RESULT/sequence/ERROR/sequence/DUMP_ANALYSIS
        //   RESULT/sequence/ERROR/sequence/SERVICE_NAME
        const string outputPrefix =
            OperationName + "/return/sequence/criarNotificacoesAIIMResponse/sequence/";

        var outputs = binding.GetProperty("outputs").EnumerateArray().ToList();
        Assert.NotEmpty(outputs);

        foreach (var output in outputs)
        {
            var soapPath = output.GetProperty("soapPath").GetString()!;
            Assert.True(soapPath.StartsWith(outputPrefix, StringComparison.Ordinal),
                $"Output soapPath '{soapPath}' não segue o envelope da resposta declarado no WSDL " +
                $"(esperado prefixo '{outputPrefix}').");

            var caseField = output.GetProperty("caseField").GetString();
            Assert.False(string.IsNullOrWhiteSpace(caseField),
                $"O campo 'caseField' deve estar preenchido para o output com soapPath '{soapPath}'.");
        }

        // Os campos WSDL obrigatórios devem estar mapeados na fixture.
        AssertOutputFieldMapped(outputs, "RESULT/sequence/STATUS_CODE");
        AssertOutputFieldMapped(outputs, "RESULT/sequence/ERROR/sequence/ERROR_CODE");
        AssertOutputFieldMapped(outputs, "RESULT/sequence/ERROR/sequence/ERROR_DESCRIPTION");
        AssertOutputFieldMapped(outputs, "RESULT/sequence/ERROR/sequence/DUMP_ANALYSIS");
        AssertOutputFieldMapped(outputs, "RESULT/sequence/ERROR/sequence/SERVICE_NAME");

        // ── 5. Operação reconhecida como invocada no processo ──────────────────
        var invokedOps = root.GetProperty("invokedOperations").EnumerateArray()
            .Select(op => op.GetString())
            .ToList();
        Assert.Contains(OperationName, invokedOps);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static void AssertInputFieldMapped(
        System.Collections.Generic.List<JsonElement> inputs, string suffixAfterRequest)
    {
        var expectedSuffix = "criarNotificacoesAIIMRequest/sequence/" + suffixAfterRequest;
        Assert.True(
            inputs.Any(i => i.GetProperty("soapPath").GetString()!
                             .EndsWith(expectedSuffix, StringComparison.Ordinal)),
            $"Nenhum input mapeia o campo WSDL '.../{expectedSuffix}'.");
    }

    private static void AssertOutputFieldMapped(
        System.Collections.Generic.List<JsonElement> outputs, string suffixAfterResponse)
    {
        var expectedSuffix = "criarNotificacoesAIIMResponse/sequence/" + suffixAfterResponse;
        Assert.True(
            outputs.Any(o => o.GetProperty("soapPath").GetString()!
                              .EndsWith(expectedSuffix, StringComparison.Ordinal)),
            $"Nenhum output mapeia o campo WSDL '.../{expectedSuffix}'.");
    }
}
