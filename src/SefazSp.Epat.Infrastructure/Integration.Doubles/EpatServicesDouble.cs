#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble em memória de <see cref="IEpatServices"/> para a PoC: devolve um envelope
/// configurável para as 5 operações. Por omissão devolve erro de aplicação
/// (STATUS_CODE = "1"), o que conduz o laço de retry até esgotar — útil para
/// demonstrar a suspensão na tarefa 'Manipular Excecao'.
/// Substituível pela implementação SOAP real quando o transporte estiver pronto.
/// </summary>
public sealed class EpatServicesDouble : IEpatServices
{
    private readonly ServiceEnvelope _envelope;

    public EpatServicesDouble(ServiceEnvelope? envelope = null)
        => _envelope = envelope ?? new ServiceEnvelope("1", "APP", "erro de aplicação simulado");

    public Task<ServiceEnvelope> PrepararintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct) => Task.FromResult(_envelope);
    public Task<ServiceEnvelope> AtualizarintimacaoAsync(AiimCaseRef caseRef, CancellationToken ct) => Task.FromResult(_envelope);
    public Task<ServiceEnvelope> BuscarvistasativasporaiimAsync(AiimCaseRef caseRef, CancellationToken ct) => Task.FromResult(_envelope);
    public Task<ServiceEnvelope> CriarnotificacoesaiimAsync(AiimCaseRef caseRef, CancellationToken ct) => Task.FromResult(_envelope);
    public Task<ServiceEnvelope> ObterprimeirodiautilaposperiododediascorridosdeatAsync(AiimCaseRef caseRef, CancellationToken ct) => Task.FromResult(_envelope);
}
