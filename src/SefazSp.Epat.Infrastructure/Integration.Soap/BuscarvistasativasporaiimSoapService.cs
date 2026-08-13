#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Implementação SOAP/JMS da operação BuscarVistasAtivasPorAiim,
/// declarada em EPAT.wsdl:
///   __sol_EPATInterfaceWrappers_sol_buscarVistasAtivasPorAiim.1
///
/// A PORTA (<see cref="IEpatServices.BuscarvistasativasporaiimAsync"/>) é final
/// (status: final) e não se toca; a implementação concreta fica aqui.
///
/// Mapeamento de saída: STATUS_CODE, STERRORCODE, STERRORDESC são copiados
/// do envelope técnico (ServiceEnvelope) para ProcessExecutionContext por
/// BscenvpcExecutionSteps.MapServiceEnvelope — não aparecem no contexto
/// por si próprios (nota de implementação do glossário, campo STATUS_CODE).
/// </summary>
public sealed class BuscarvistasativasporaiimSoapService : IEpatServices
{
    // A implementação completa do cliente SOAP/JMS está fora do escopo deste
    // segmento (card BUILD-BSCENVPC-seg004 cobre topologia e envelope técnico).
    // Este stub satisfaz a interface para compilação e será completado pelo
    // implementador-integration-soap (AGENTS.md).

    public Task<ServiceEnvelope> BuscarvistasativasporaiimAsync(
        AiimCaseRef caseRef, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        throw new NotImplementedException(
            "BuscarvistasativasporaiimAsync: implementação SOAP/JMS pendente. " +
            "Ver card BUILD-BSCENVPC-seg004, scaffold Integration.Soap.");
    }

    public Task<ServiceEnvelope> AtualizarintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct)
        => throw new NotImplementedException();

    public Task<ServiceEnvelope> CriarnotificacoesaiimAsync(AiimCaseRef caseRef, CancellationToken ct)
        => throw new NotImplementedException();

    public Task<ServiceEnvelope> ObterprimeirodiautilaposperiododediascorridosdeatAsync(AiimCaseRef caseRef, CancellationToken ct)
        => throw new NotImplementedException();

    public Task<ServiceEnvelope> PrepararintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct)
        => throw new NotImplementedException();
}
