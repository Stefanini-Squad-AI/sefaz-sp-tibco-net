using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.UseCases.ATZINTPC;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Infrastructure.Integration.Soap.ATZINTPC;

public sealed class AtualizarIntimacaoSoapOperation : IAtualizarIntimacaoOperation
{
    private readonly IEpatServices _services;

    public AtualizarIntimacaoSoapOperation(IEpatServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public Task<ServiceEnvelope> ExecuteAsync(
        AiimCase caseData,
        ProcessExecutionContext executionContext,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(caseData);
        ArgumentNullException.ThrowIfNull(executionContext);

        var processId = executionContext.PROCESS_ID
            ?? FormattableString.Invariant($"idAiim-{caseData.IDAIIM}idProc-NA");

        return _services.AtualizarintimacaoAsync(new AiimCaseRef(caseData.IDAIIM, processId), ct);
    }
}
