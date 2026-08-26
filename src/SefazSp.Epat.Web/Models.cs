namespace SefazSp.Epat.Web;

// Mirrors the Api's read-model JSON (Part 2, Phase 1). Kept local so the Web project does not
// take a dependency on the Api's composition graph.
public sealed record JourneyView(
    string ProcessId,
    string BpmnKey,
    string Status,
    List<JourneyStep> Traversed,
    string? CurrentNodeId,
    List<InteractionView> Interactions);

public sealed record JourneyStep(int Index, string NodeId);

public sealed record InteractionView(
    string Port, string Operation, bool Success, string? Failure, DateTimeOffset At, long DurationMs);
