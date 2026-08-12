// Agente: Autor de testes de Oracles.Tests
// Oracle: contract — fixture artifacts/POC_Epat/service-contracts.json (immutable=true)
// AC-ORACULO: 1 caso de contrato, sem valores esperados escritos pelo agente.
// Identificadores preservados: __sol_EPATInterfaceWrappers_sol_buscarVistasAtivasPorAiim.1,
//   buscarVistasAtivasPorAiimRequest, buscarVistasAtivasPorAiimResponse, NRAIIM, EMAILVISTAS,
//   TRANSACTION_ID, PROCESS_ID, DATETIME, SW_MAINCASE, DUMP, STERRORDESC, STERRORCODE,
//   STATUS_CODE, SERVICE_NAME

#nullable enable

using System.Text.Json;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Contract;

/// <summary>
/// Prova que a operacao __sol_EPATInterfaceWrappers_sol_buscarVistasAtivasPorAiim.1
/// respeita o contrato declarado no WSDL, conforme fixado em service-contracts.json.
/// O fixture e a unica fonte de verdade — nenhum valor esperado e escrito pelo agente.
/// </summary>
public sealed class BuscarVistasAtivasPorAiimContractTests
{
    private const string OperationName = "__sol_EPATInterfaceWrappers_sol_buscarVistasAtivasPorAiim.1";
    private const string FixtureRelativePath = "artifacts/POC_Epat/service-contracts.json";

    private static string ResolveFixturePath()
    {
        // Sobe de bin/Debug/net8.0 ate a raiz do repositorio (5 niveis acima do output dir)
        var dir = Path.GetDirectoryName(typeof(BuscarVistasAtivasPorAiimContractTests).Assembly.Location)!;
        for (int i = 0; i < 5; i++)
            dir = Path.GetDirectoryName(dir)!;
        return Path.Combine(dir, FixtureRelativePath);
    }

    private static JsonElement LoadOperation()
    {
        var fixturePath = ResolveFixturePath();
        Assert.True(File.Exists(fixturePath),
            $"Fixture nao encontrada: {fixturePath}. O caminho de resolucao parte do assembly.");

        using var doc = JsonDocument.Parse(File.ReadAllText(fixturePath));
        var services = doc.RootElement.GetProperty("services");

        foreach (var service in services.EnumerateArray())
        {
            if (!service.TryGetProperty("operations", out var ops))
                continue;
            foreach (var op in ops.EnumerateArray())
            {
                if (op.TryGetProperty("name", out var nameProp) &&
                    nameProp.GetString() == OperationName)
                {
                    // Retorna uma copia independente para evitar problemas de lifetime do JsonDocument
                    return JsonDocument.Parse(op.GetRawText()).RootElement;
                }
            }
        }

        throw new InvalidOperationException(
            $"Operacao '{OperationName}' nao encontrada na fixture '{FixtureRelativePath}'.");
    }

    private static List<(string Path, string XsdType, bool Required)> ExtractFields(JsonElement part)
    {
        var fields = new List<(string, string, bool)>();
        foreach (var field in part.GetProperty("fields").EnumerateArray())
        {
            fields.Add((
                field.GetProperty("path").GetString()!,
                field.GetProperty("xsdType").GetString()!,
                field.GetProperty("required").GetBoolean()
            ));
        }
        return fields;
    }

    /// <summary>
    /// AC1 + AC2 (caso unico de contrato):
    /// Valida que o contrato declarado em service-contracts.json para a operacao
    /// buscarVistasAtivasPorAiim.1 inclui todos os campos obrigatorios de request
    /// e response com os tipos XSD corretos, sem campos extra nao declarados.
    /// </summary>
    [Fact]
    public void ContratoBuscarVistasAtivasPorAiim_DeveConterTodosOsCamposObrigatoriosDeRequestEResponse()
    {
        // Arrange — carrega o fixture; a fonte de verdade e o proprio fixture
        var operation = LoadOperation();

        // ── INPUT ───────────────────────────────────────────────────────────────
        var inputParts = operation.GetProperty("input");
        Assert.True(inputParts.GetArrayLength() >= 1,
            "A operacao deve declarar pelo menos um input part.");
        var inputPart = inputParts[0];

        Assert.Equal("param",      inputPart.GetProperty("partName").GetString());
        Assert.Equal("ns218:request", inputPart.GetProperty("element").GetString());

        var inputFields = ExtractFields(inputPart);
        var inputPaths  = inputFields.Select(f => f.Path).ToHashSet();

        // Campos obrigatorios do HEADER (request) — AC1
        Assert.Contains("/buscarVistasAtivasPorAiimRequest/HEADER/TRANSACTION_ID",                    inputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimRequest/HEADER/DATETIME",                          inputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimRequest/HEADER/APPLICATIONDATAS/APPLICATIONDATA/NAME",  inputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimRequest/HEADER/APPLICATIONDATAS/APPLICATIONDATA/VALUE", inputPaths);

        // Campo obrigatorio do BODY — NRAIIM (nrAiim, xs:long, required) — AC1
        Assert.Contains("/buscarVistasAtivasPorAiimRequest/BODY/nrAiim", inputPaths);

        // Valida tipos e required dos campos criticos do request
        AssertField(inputFields, "/buscarVistasAtivasPorAiimRequest/HEADER/TRANSACTION_ID",
            expectedXsdType: "xsd:string", expectedRequired: true);
        AssertField(inputFields, "/buscarVistasAtivasPorAiimRequest/HEADER/DATETIME",
            expectedXsdType: "xsd:string", expectedRequired: true);
        AssertField(inputFields, "/buscarVistasAtivasPorAiimRequest/HEADER/APPLICATIONDATAS/APPLICATIONDATA/NAME",
            expectedXsdType: "xsd:string", expectedRequired: true);
        AssertField(inputFields, "/buscarVistasAtivasPorAiimRequest/HEADER/APPLICATIONDATAS/APPLICATIONDATA/VALUE",
            expectedXsdType: "xsd:string", expectedRequired: true);
        AssertField(inputFields, "/buscarVistasAtivasPorAiimRequest/BODY/nrAiim",
            expectedXsdType: "xs:long", expectedRequired: true);

        // ── OUTPUT ──────────────────────────────────────────────────────────────
        var outputParts = operation.GetProperty("output");
        Assert.True(outputParts.GetArrayLength() >= 1,
            "A operacao deve declarar pelo menos um output part.");
        var outputPart = outputParts[0];

        Assert.Equal("return",         outputPart.GetProperty("partName").GetString());
        Assert.Equal("ns219:response", outputPart.GetProperty("element").GetString());

        var outputFields = ExtractFields(outputPart);
        var outputPaths  = outputFields.Select(f => f.Path).ToHashSet();

        // Campos obrigatorios do BODY (response) — EMAILVISTAS (ListaEmailsConcatenados) — AC2
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/BODY/ListaEmailsConcatenados", outputPaths);

        // Campos de ListaProcessos — AC2
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/BODY/ListaProcessos/CodDetalheRelatorioVoto",       outputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/BODY/ListaProcessos/CodTipoUsuarioRelatorioVoto",    outputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/BODY/ListaProcessos/NumeroCpfSolicitanteVistas",     outputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/BODY/ListaProcessos/NomeSolicitanteVistas",          outputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/BODY/ListaProcessos/EmailSolicitanteVistas",         outputPaths);

        // Campos de RESULT/STATUS_CODE — AC2
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/RESULT/STATUS_CODE", outputPaths);

        // Campos de RESULT/ERROR — AC2
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/SERVICE_NAME",       outputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/ERROR_CODE",         outputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/ERROR_DESCRIPTION",  outputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/ERROR_STACKTRACE",   outputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/PROCESS_STACK",      outputPaths);
        Assert.Contains("/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/DUMP_ANALYSIS",      outputPaths);

        // Valida tipos e required dos campos criticos do response
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/BODY/ListaEmailsConcatenados",
            expectedXsdType: "xs:string", expectedRequired: true);
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/BODY/ListaProcessos/CodDetalheRelatorioVoto",
            expectedXsdType: "xs:int", expectedRequired: true);
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/BODY/ListaProcessos/CodTipoUsuarioRelatorioVoto",
            expectedXsdType: "xs:int", expectedRequired: true);
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/BODY/ListaProcessos/NumeroCpfSolicitanteVistas",
            expectedXsdType: "xs:string", expectedRequired: true);
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/BODY/ListaProcessos/NomeSolicitanteVistas",
            expectedXsdType: "xs:string", expectedRequired: true);
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/RESULT/STATUS_CODE",
            expectedXsdType: "xsd:integer", expectedRequired: true);
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/SERVICE_NAME",
            expectedXsdType: "xsd:string", expectedRequired: true);
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/ERROR_CODE",
            expectedXsdType: "xsd:string", expectedRequired: true);
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/ERROR_DESCRIPTION",
            expectedXsdType: "xsd:string", expectedRequired: true);
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/ERROR_STACKTRACE",
            expectedXsdType: "xsd:string", expectedRequired: true);
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/PROCESS_STACK",
            expectedXsdType: "xsd:string", expectedRequired: true);
        AssertField(outputFields, "/buscarVistasAtivasPorAiimResponse/RESULT/ERROR/DUMP_ANALYSIS",
            expectedXsdType: "xsd:string", expectedRequired: true);

        // Confirma que a operacao esta marcada como invocada por processo
        Assert.True(operation.GetProperty("isInvokedByProcess").GetBoolean(),
            "A operacao deve estar marcada como invocada por processo (isInvokedByProcess=true).");
    }

    private static void AssertField(
        List<(string Path, string XsdType, bool Required)> fields,
        string path,
        string expectedXsdType,
        bool expectedRequired)
    {
        var field = fields.FirstOrDefault(f => f.Path == path);
        Assert.True(field != default,
            $"Campo nao encontrado no fixture: {path}");
        Assert.Equal(expectedXsdType, field.XsdType);
        Assert.Equal(expectedRequired, field.Required);
    }
}
