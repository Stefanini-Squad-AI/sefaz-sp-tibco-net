// Oráculo de contrato — imutável.
// Este ficheiro lê a fixture sem a alterar; nunca escreve valores esperados.
// Fonte: artifacts/POC_Epat/service-contracts.json
// Operação: __sol_EPATInterfaceWrappers_sol_atualizarIntimacao.1

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Contract;

/// <summary>
/// Verifica que a operação __sol_EPATInterfaceWrappers_sol_atualizarIntimacao.1
/// declarada em EPAT.wsdl respeita o contrato registado na fixture imutável
/// artifacts/POC_Epat/service-contracts.json — pedido e resposta validam
/// contra o esquema XSD declarado no WSDL.
/// </summary>
public sealed class AtualizarIntimacao1ContractTests
{
    private const string OperationName = "__sol_EPATInterfaceWrappers_sol_atualizarIntimacao.1";

    // Caminho relativo à raiz do repositório; o teste é executado a partir do directório do projecto.
    private static readonly string FixturePath = Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "artifacts", "POC_Epat", "service-contracts.json"));

    private static JsonElement LoadOperation()
    {
        var json = File.ReadAllText(FixturePath);
        var doc = JsonDocument.Parse(json);
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
                    return op;
                }
            }
        }

        throw new InvalidOperationException(
            $"Operação '{OperationName}' não encontrada em {FixturePath}. " +
            "A fixture pode estar corrompida ou o nome da operação foi alterado.");
    }

    /// <summary>
    /// AC1 + AC2 + AC3: a fixture contém a operação e os seus inputs e outputs
    /// estão conformes com o esquema XSD declarado no WSDL.
    /// </summary>
    [Fact]
    public void AtualizarIntimacao1_ContractConformsToWsdl()
    {
        // Arrange — carrega a fixture imutável sem a alterar.
        var op = LoadOperation();

        // AC1: a operação existe na fixture (isInvokedByProcess = true)
        Assert.True(
            op.TryGetProperty("isInvokedByProcess", out var invoked) && invoked.GetBoolean(),
            $"A operação '{OperationName}' deve ter isInvokedByProcess=true na fixture.");

        Assert.True(
            op.TryGetProperty("logicalPath", out var logicalPath) &&
            logicalPath.GetString() == "/EPATInterfaceWrappers/atualizarIntimacao.1",
            "logicalPath não corresponde ao percurso declarado no WSDL.");

        // AC2: pedido (request) — valida campos obrigatórios declarados no WSDL
        var input = op.GetProperty("input");
        var inputPart = input.EnumerateArray().Single();
        Assert.Equal("param", inputPart.GetProperty("partName").GetString());

        var requestFields = inputPart.GetProperty("fields")
            .EnumerateArray()
            .Select(f => f.GetProperty("path").GetString()!)
            .ToList();

        Assert.Contains("/atualizarIntimacaoRequest/HEADER/TRANSACTION_ID", requestFields);
        Assert.Contains("/atualizarIntimacaoRequest/HEADER/DATETIME", requestFields);
        Assert.Contains("/atualizarIntimacaoRequest/BODY/idIntimacao", requestFields);
        Assert.Contains("/atualizarIntimacaoRequest/BODY/caseNumber", requestFields);

        var requestRequiredTypes = inputPart.GetProperty("fields")
            .EnumerateArray()
            .Where(f => f.GetProperty("required").GetBoolean())
            .ToDictionary(
                f => f.GetProperty("path").GetString()!,
                f => f.GetProperty("clrType").GetString()!);

        Assert.Equal("string", requestRequiredTypes["/atualizarIntimacaoRequest/HEADER/TRANSACTION_ID"]);
        Assert.Equal("string", requestRequiredTypes["/atualizarIntimacaoRequest/HEADER/DATETIME"]);
        Assert.Equal("long",   requestRequiredTypes["/atualizarIntimacaoRequest/BODY/idIntimacao"]);
        Assert.Equal("long",   requestRequiredTypes["/atualizarIntimacaoRequest/BODY/caseNumber"]);

        // AC3: resposta (response) — valida campos obrigatórios declarados no WSDL
        var output = op.GetProperty("output");
        var outputPart = output.EnumerateArray().Single();
        Assert.Equal("return", outputPart.GetProperty("partName").GetString());

        var responseFields = outputPart.GetProperty("fields")
            .EnumerateArray()
            .Select(f => f.GetProperty("path").GetString()!)
            .ToList();

        Assert.Contains("/atualizarIntimacaoResponse/HEADER/TRANSACTION_ID", responseFields);
        Assert.Contains("/atualizarIntimacaoResponse/HEADER/DATETIME", responseFields);
        Assert.Contains("/atualizarIntimacaoResponse/RESULT/STATUS_CODE", responseFields);
        Assert.Contains("/atualizarIntimacaoResponse/BODY/idTipoIntimacao", responseFields);
        Assert.Contains("/atualizarIntimacaoResponse/BODY/numDoctoIntimado", responseFields);

        var responseRequiredTypes = outputPart.GetProperty("fields")
            .EnumerateArray()
            .Where(f => f.GetProperty("required").GetBoolean())
            .ToDictionary(
                f => f.GetProperty("path").GetString()!,
                f => f.GetProperty("clrType").GetString()!);

        Assert.Equal("string", responseRequiredTypes["/atualizarIntimacaoResponse/HEADER/TRANSACTION_ID"]);
        Assert.Equal("int",    responseRequiredTypes["/atualizarIntimacaoResponse/RESULT/STATUS_CODE"]);
        Assert.Equal("long",   responseRequiredTypes["/atualizarIntimacaoResponse/BODY/idTipoIntimacao"]);
        Assert.Equal("string", responseRequiredTypes["/atualizarIntimacaoResponse/BODY/numDoctoIntimado"]);
    }
}
