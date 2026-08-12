#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Implementacao SOAP/JMS das operacoes de IEpatServices invocadas pelo processo BSCENVPC.
/// A porta (IEpatServices) e transcricao do WSDL e nao se mexe — so esta implementacao
/// se escreve, conforme nota do scaffold.
///
/// A operacao BuscarvistasativasporaiimAsync corresponde ao TIBCO:
///   __sol_EPATInterfaceWrappers_sol_buscarVistasAtivasPorAiim.1
/// declarada em EPAT.wsdl e mapeada no serviceTask _qIDu5F6BEfGBBLgT-R5iuw.
///
/// Implementacao em stub: a chamada SOAP real fica pendente da fundacao de transporte
/// (fundacao-anticorrupcao / fundacao-motor). O contrato de retorno (ServiceEnvelope)
/// e o mesmo para todos os cinco clones (ATZINTPC, BSCENVPC, CALCPRPC, CRNOTPC, PRPINTPC).
/// </summary>
public sealed class EpatSoapServices : IEpatServices
{
    /// <inheritdoc />
    /// <remarks>
    /// Operacao: buscarVistasAtivasPorAiim.1 — invocada no serviceTask Busca Envolvidos Vista Por AIIM.
    /// STATUS_CODE='0' indica sucesso; qualquer outro valor activa o ramo AppError
    /// no gateway _qIDu4l6BEfGBBLgT-R5iuw (condicao: STATUS_CODE != "0").
    /// Confirmado em 2026-08-06 que a condicao de sucesso e STATUS_CODE="0",
    /// distinto do PRPINTPC que compara com SW_NA (tratado em rulings.CLONE-PRPINTPC).
    /// </remarks>
    public Task<ServiceEnvelope> BuscarvistasativasporaiimAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        // Stub: a implementacao SOAP/JMS real depende da fundacao de transporte.
        // O retorno e ServiceEnvelope(STATUS_CODE, STERRORCODE, STERRORDESC).
        throw new NotImplementedException(
            "BuscarvistasativasporaiimAsync: implementacao SOAP/JMS pendente da fundacao de transporte.");
    }

    /// <inheritdoc />
    public Task<ServiceEnvelope> PrepararintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<ServiceEnvelope> AtualizarintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<ServiceEnvelope> CriarnotificacoesaiimAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<ServiceEnvelope> ObterprimeirodiautilaposperiododediascorridosdeatAsync(AiimCaseRef caseRef, CancellationToken ct) =>
        throw new NotImplementedException();
}
