#nullable enable
using SefazSp.Epat.Application.Abstractions;

namespace SefazSp.Epat.Infrastructure.Integration.Soap.ATZINTPC;

/// <summary>
/// Cliente SOAP para a operação AtualizarIntimacao (ATZINTPC _RNdKHF6PEfGBBLgT-R5iuw).
/// Operação WSDL: __sol_EPATInterfaceWrappers_sol_atualizarIntimacao.1 (EPAT.wsdl).
/// </summary>
public sealed class AtualizarIntimacaoSoapClient
{
    public static async Task<ServiceEnvelope> InvokeAsync(
        AiimCaseRef caseRef,
        HttpClient httpClient,
        CancellationToken ct)
    {
        _ = caseRef;
        _ = httpClient;
        _ = ct;

        await Task.CompletedTask;
        return new ServiceEnvelope(STATUS_CODE: null, STERRORCODE: null, STERRORDESC: null);
    }
}
