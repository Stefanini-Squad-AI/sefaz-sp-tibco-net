using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Execution.ATZINTPC;

public sealed class ATZINTPCWorkflowState
{
    private readonly List<string> _visitedNodeIds = [];

    public ATZINTPCWorkflowState(AiimCase caseData, ProcessExecutionContext executionContext)
    {
        CaseData = caseData ?? throw new ArgumentNullException(nameof(caseData));
        ExecutionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
    }

    public AiimCase CaseData { get; }

    public ProcessExecutionContext ExecutionContext { get; }

    public int SW_QRETRYCOUNT { get; set; }

    public string? ManualExceptionOutcome { get; set; }

    public string CurrentDateTimeText { get; set; } = DateTime.UtcNow.ToString("O");

    public bool HasEnteredLoop { get; set; }

    public IReadOnlyList<string> VisitedNodeIds => _visitedNodeIds;

    public void Visit(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        _visitedNodeIds.Add(nodeId);
    }
}
