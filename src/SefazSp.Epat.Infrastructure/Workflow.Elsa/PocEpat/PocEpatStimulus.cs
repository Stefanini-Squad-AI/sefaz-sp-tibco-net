#nullable enable

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.PocEpat;

/// <summary>Estímulo dos bookmarks do fluxo principal POC_EpatProcess, correlacionado por PROCESS_ID.</summary>
public sealed record PocEpatStimulus(string CorrelationKey);
