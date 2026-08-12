using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.UseCases.ATZINTPC;

public interface IAtualizarIntimacaoOperation
{
    Task<ServiceEnvelope> ExecuteAsync(
        AiimCase caseData,
        Execution.ProcessExecutionContext executionContext,
        CancellationToken ct);
}
