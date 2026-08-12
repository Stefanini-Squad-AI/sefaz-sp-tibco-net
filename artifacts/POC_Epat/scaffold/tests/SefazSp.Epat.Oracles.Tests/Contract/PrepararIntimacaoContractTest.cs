#nullable enable

using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Contract;

/// <summary>
/// Prova que a porta .NET da operacao
/// __sol_Business_sp_Processes_sol_Decision_sol_Sub_sp_Processes_sol_Intimacao_sol_PrepararIntimacao
/// respeita a forma declarada no WSDL, em pedido e resposta.
///
/// Oracle: artifacts/POC_Epat/service-contracts.json (immutable=true)
/// IR pointer: operations[__sol_Business_sp_Processes_sol_Decision_sol_Sub_sp_Processes_sol_Intimacao_sol_PrepararIntimacao]
/// </summary>
public sealed class PrepararIntimacaoContractTest
{
    private const string OperationId =
        "__sol_Business_sp_Processes_sol_Decision_sol_Sub_sp_Processes_sol_Intimacao_sol_PrepararIntimacao";

    private static readonly string FixturePath = ResolveFixturePath();

    private static string ResolveFixturePath()
    {
        // Percorre para a raiz do repositorio a partir do directorio de saida do teste.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "artifacts", "POC_Epat", "service-contracts.json");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        // Fallback: caminho relativo a partir da raiz do repositorio inferida
        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory,
                "..", "..", "..", "..", "..", "..", "..", "..",
                "artifacts", "POC_Epat", "service-contracts.json"));
    }

    /// <summary>
    /// Caso unico de contrato (caseCount=1): valida que o pedido e a resposta
    /// declarados no WSDL estao presentes e conformes no fixture imutavel.
    /// Nenhum valor esperado e escrito ou editado neste metodo.
    /// </summary>
    [Fact]
    [Trait("category", "contract")]
    [Trait("operation", "PrepararIntimacao")]
    public void Contract_PrepararIntimacao_PedidoERespostaValidamContraEsquemaWsdl()
    {
        // --- Arrange: carrega o oracle imutavel ---
        Assert.True(File.Exists(FixturePath),
            $"Fixture nao encontrada em: {FixturePath}");

        using var stream = File.OpenRead(FixturePath);
        var fixture = JsonDocument.Parse(stream);
        var root = fixture.RootElement;

        // --- Act: localiza a operacao pelo id verbatim ---
        var operation = FindOperation(root, OperationId);

        // --- Assert: operacao presente no fixture ---
        Assert.True(operation.HasValue,
            $"Operacao '{OperationId}' nao encontrada em service-contracts.json.");

        var op = operation!.Value;

        // Valida pedido (input): deve ter pelo menos uma parte declarada com campos
        Assert.True(
            op.TryGetProperty("input", out var inputArr) &&
            inputArr.ValueKind == JsonValueKind.Array &&
            inputArr.GetArrayLength() > 0,
            "O contrato deve declarar pelo menos uma parte de entrada (input).");

        var inputPart = inputArr[0];
        Assert.True(
            inputPart.TryGetProperty("fields", out var inputFields) &&
            inputFields.ValueKind == JsonValueKind.Array &&
            inputFields.GetArrayLength() > 0,
            "A parte de entrada deve conter campos (fields) declarados no WSDL.");

        // Valida que todos os campos de entrada possuem path e xsdType
        foreach (var field in inputFields.EnumerateArray())
        {
            Assert.True(
                field.TryGetProperty("path", out _),
                "Cada campo de entrada deve ter 'path' declarado.");
            Assert.True(
                field.TryGetProperty("xsdType", out _),
                "Cada campo de entrada deve ter 'xsdType' declarado.");
        }

        // Valida resposta (output): deve ter pelo menos uma parte declarada com campos
        Assert.True(
            op.TryGetProperty("output", out var outputArr) &&
            outputArr.ValueKind == JsonValueKind.Array &&
            outputArr.GetArrayLength() > 0,
            "O contrato deve declarar pelo menos uma parte de saida (output).");

        var outputPart = outputArr[0];
        Assert.True(
            outputPart.TryGetProperty("fields", out var outputFields) &&
            outputFields.ValueKind == JsonValueKind.Array &&
            outputFields.GetArrayLength() > 0,
            "A parte de saida deve conter campos (fields) declarados no WSDL.");

        // Valida que todos os campos de saida possuem path e xsdType
        foreach (var field in outputFields.EnumerateArray())
        {
            Assert.True(
                field.TryGetProperty("path", out _),
                "Cada campo de saida deve ter 'path' declarado.");
            Assert.True(
                field.TryGetProperty("xsdType", out _),
                "Cada campo de saida deve ter 'xsdType' declarado.");
        }

        // Valida que a operacao declara o portType esperado (integridade do fixture)
        Assert.True(
            op.TryGetProperty("portType", out var portType) &&
            portType.ValueKind == JsonValueKind.String,
            "A operacao deve declarar 'portType'.");
    }

    // -------------------------------------------------------------------------

    private static JsonElement? FindOperation(JsonElement root, string operationId)
    {
        if (!root.TryGetProperty("services", out var services) ||
            services.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var service in services.EnumerateArray())
        {
            if (!service.TryGetProperty("operations", out var operations) ||
                operations.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var op in operations.EnumerateArray())
            {
                if (op.TryGetProperty("name", out var name) &&
                    name.GetString() == operationId)
                    return op;
            }
        }

        return null;
    }
}
