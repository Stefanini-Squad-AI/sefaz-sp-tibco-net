#nullable enable

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Graft;

/// <summary>Estímulo do bookmark de prosseguimento do graft (correlation-join), por PROCESS_ID.</summary>
public sealed record GraftProceedStimulus(string CorrelationKey);
