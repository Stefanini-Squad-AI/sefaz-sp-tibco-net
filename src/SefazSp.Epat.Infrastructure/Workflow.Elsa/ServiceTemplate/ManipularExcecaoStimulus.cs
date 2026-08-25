#nullable enable

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.ServiceTemplate;

/// <summary>
/// Estímulo do bookmark 'Manipular Excecao'. O hash é derivado por valor destes campos,
/// por isso o endpoint de retoma reconstrói o mesmo registo para libertar o bookmark.
/// </summary>
public sealed record ManipularExcecaoStimulus(string ProcessKey, string CorrelationKey);
