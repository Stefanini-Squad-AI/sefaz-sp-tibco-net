// Oracle: contract — fixture artifacts/POC_Epat/service-contracts.json (immutable, 1 case)
// Card: VALID-SOLEPATINTERFACEWRAPPERSSOLOBTERPRIMEIRODIAUTILAPOSPERIODODEDIASCORRIDOSDEAT1-contrato
// AC1: pedido valida contra o esquema de entrada declarado no WSDL.
// AC2: resposta valida contra o esquema de saida declarado no WSDL.
#nullable enable

using System.Reflection;
using System.Text.Json;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Contract;

/// <summary>
/// Prova que a porta .NET da operacao
/// __sol_EPATInterfaceWrappers_sol_obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEAT.1
/// respeita o contrato declarado no WSDL, em ambos os sentidos.
/// </summary>
public sealed class ObterPrimeiroDiaUtilAposCorridosDEATContractTests
{
    private const string OperationName =
        "__sol_EPATInterfaceWrappers_sol_obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEAT.1";

    private const string FixturePath =
        "artifacts/POC_Epat/service-contracts.json";

    private static JsonDocument LoadFixture()
    {
        // Resolve relative to repo root, walking up from the test binary's base dir.
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, FixturePath);
            if (File.Exists(candidate))
                return JsonDocument.Parse(File.ReadAllText(candidate));
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException($"Fixture not found: {FixturePath}");
    }

    // -------------------------------------------------------------------------
    // Single contract case (AC1 + AC2 + AC-ORACULO)
    // -------------------------------------------------------------------------

    [Fact(DisplayName =
        "AC1+AC2: operacao obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEAT.1 — " +
        "pedido e resposta em conformidade com o esquema WSDL")]
    public void ContractConformidade_PedidoERespostaValidamContraEsquemaWSDL()
    {
        using var doc = LoadFixture();
        var root = doc.RootElement;

        // ── Fixture: operacao esta presente e e invocada pelo processo ────────
        var operationEntry = FindOperationInFixture(root, OperationName);
        Assert.True(
            operationEntry.HasValue,
            $"Operacao '{OperationName}' nao encontrada em {FixturePath}.");

        var op = operationEntry!.Value;
        Assert.True(
            op.TryGetProperty("isInvokedByProcess", out var isInvoked) && isInvoked.GetBoolean(),
            $"Operacao '{OperationName}' deve ter isInvokedByProcess=true no fixture.");

        // ── AC1: esquema de entrada ────────────────────────────────────────────
        var inputFields = GetFieldArray(op, "input");
        AssertInputContractFields(inputFields);

        // ── AC2: esquema de saida ─────────────────────────────────────────────
        var outputFields = GetFieldArray(op, "output");
        AssertOutputContractFields(outputFields);

        // ── Porta .NET: assinatura do metodo em IEpatServices ─────────────────
        AssertNetPortSignature();
    }

    // -------------------------------------------------------------------------
    // Helpers: fixture navigation
    // -------------------------------------------------------------------------

    private static JsonElement? FindOperationInFixture(JsonElement root, string name)
    {
        if (!root.TryGetProperty("services", out var services))
            return null;

        foreach (var svc in services.EnumerateArray())
        {
            if (!svc.TryGetProperty("operations", out var ops))
                continue;
            foreach (var op in ops.EnumerateArray())
            {
                if (op.TryGetProperty("name", out var n) && n.GetString() == name)
                    return op;
            }
        }
        return null;
    }

    private static JsonElement[] GetFieldArray(JsonElement operation, string direction)
    {
        if (!operation.TryGetProperty(direction, out var parts))
            return [];

        var fields = new List<JsonElement>();
        foreach (var part in parts.EnumerateArray())
        {
            if (!part.TryGetProperty("fields", out var fs))
                continue;
            foreach (var f in fs.EnumerateArray())
                fields.Add(f);
        }
        return [.. fields];
    }

    // -------------------------------------------------------------------------
    // AC1 — input field assertions (paths and CLR types declared in WSDL)
    // -------------------------------------------------------------------------

    private static void AssertInputContractFields(JsonElement[] fields)
    {
        // Required paths declared in the WSDL input schema.
        var expectedRequired = new[]
        {
            ("/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATRequest/HEADER/TRANSACTION_ID",  "string"),
            ("/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATRequest/HEADER/DATETIME",         "string"),
            ("/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATRequest/BODY/dataInicioPeriodo",  "DateOnly"),
            ("/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATRequest/BODY/periodoEmDias",      "int"),
            ("/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATRequest/BODY/codigoMunicipio",    "int"),
        };

        foreach (var (path, clrType) in expectedRequired)
        {
            var field = FindField(fields, path);
            Assert.True(field.HasValue,
                $"AC1: campo de entrada '{path}' nao declarado no fixture.");

            var f = field!.Value;
            Assert.True(
                f.TryGetProperty("required", out var req) && req.GetBoolean(),
                $"AC1: campo de entrada '{path}' deve ser required=true.");

            Assert.True(
                f.TryGetProperty("clrType", out var ct) && ct.GetString() == clrType,
                $"AC1: campo '{path}' deve ter clrType='{clrType}' (declarado no WSDL).");
        }

        // PROCESS_ID is optional
        var processId = FindField(fields,
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATRequest/HEADER/PROCESS_ID");
        Assert.True(processId.HasValue,
            "AC1: campo de entrada PROCESS_ID nao declarado no fixture.");
        Assert.True(
            processId!.Value.TryGetProperty("required", out var pidReq) && !pidReq.GetBoolean(),
            "AC1: PROCESS_ID deve ser required=false (campo opcional no WSDL).");
    }

    // -------------------------------------------------------------------------
    // AC2 — output field assertions (paths and CLR types declared in WSDL)
    // -------------------------------------------------------------------------

    private static void AssertOutputContractFields(JsonElement[] fields)
    {
        var expected = new[]
        {
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/HEADER/TRANSACTION_ID",
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/HEADER/DATETIME",
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/BODY/dataDiaUtil",
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/RESULT/STATUS_CODE",
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/RESULT/ERROR/SERVICE_NAME",
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/RESULT/ERROR/ERROR_CODE",
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/RESULT/ERROR/ERROR_DESCRIPTION",
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/RESULT/ERROR/ERROR_STACKTRACE",
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/RESULT/ERROR/PROCESS_STACK",
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/RESULT/ERROR/DUMP_ANALYSIS",
        };

        foreach (var path in expected)
        {
            var field = FindField(fields, path);
            Assert.True(field.HasValue,
                $"AC2: campo de saida '{path}' nao declarado no fixture.");
        }

        // dataDiaUtil deve ser DateOnly
        var dataDiaUtil = FindField(fields,
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/BODY/dataDiaUtil");
        Assert.True(
            dataDiaUtil!.Value.TryGetProperty("clrType", out var ddClr) &&
            ddClr.GetString() == "DateOnly",
            "AC2: dataDiaUtil deve ter clrType='DateOnly' (xs:date no WSDL).");

        // STATUS_CODE deve ser int
        var statusCode = FindField(fields,
            "/obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEATResponse/RESULT/STATUS_CODE");
        Assert.True(
            statusCode!.Value.TryGetProperty("clrType", out var scClr) &&
            scClr.GetString() == "int",
            "AC2: STATUS_CODE deve ter clrType='int' (xsd:integer no WSDL).");
    }

    // -------------------------------------------------------------------------
    // Porta .NET: IEpatServices reflection assertions
    // -------------------------------------------------------------------------

    private static void AssertNetPortSignature()
    {
        var iface = typeof(IEpatServices);

        const string methodName = "ObterprimeirodiautilaposperiododediascorridosdeatAsync";
        var method = iface.GetMethod(methodName);

        Assert.True(
            method is not null,
            $"IEpatServices deve expor o metodo '{methodName}' para a operacao '{OperationName}'.");

        // Parametros: (AiimCaseRef caseRef, CancellationToken ct)
        var parameters = method!.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(AiimCaseRef), parameters[0].ParameterType);
        Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);

        // Tipo de retorno: Task<ServiceEnvelope>
        Assert.Equal(typeof(Task<ServiceEnvelope>), method.ReturnType);

        // AiimCaseRef carrega a identidade do caso (correlacao PROCESS_ID + IdAiim)
        var caseRefType = typeof(AiimCaseRef);
        Assert.True(
            caseRefType.GetProperty("ProcessId") is not null,
            "AiimCaseRef deve ter propriedade ProcessId (mapeia PROCESS_ID do WSDL).");
        Assert.True(
            caseRefType.GetProperty("IdAiim") is not null,
            "AiimCaseRef deve ter propriedade IdAiim (mapeia TRANSACTION_ID do WSDL).");

        // ServiceEnvelope cobre os campos de erro/status do RESULT do WSDL
        var envType = typeof(ServiceEnvelope);
        Assert.True(
            envType.GetProperty("STATUS_CODE") is not null,
            "ServiceEnvelope deve ter STATUS_CODE (RESULT/STATUS_CODE no WSDL).");
        Assert.True(
            envType.GetProperty("STERRORCODE") is not null,
            "ServiceEnvelope deve ter STERRORCODE (RESULT/ERROR/ERROR_CODE no WSDL).");
        Assert.True(
            envType.GetProperty("STERRORDESC") is not null,
            "ServiceEnvelope deve ter STERRORDESC (RESULT/ERROR/ERROR_DESCRIPTION no WSDL).");
    }

    // -------------------------------------------------------------------------

    private static JsonElement? FindField(JsonElement[] fields, string path)
    {
        foreach (var f in fields)
        {
            if (f.TryGetProperty("path", out var p) && p.GetString() == path)
                return f;
        }
        return null;
    }
}
