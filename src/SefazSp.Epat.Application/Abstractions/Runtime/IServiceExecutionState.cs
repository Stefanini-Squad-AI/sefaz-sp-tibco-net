#nullable enable

using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Abstractions.Runtime;

/// <summary>
/// Estado de execução do molde de serviço que precisa de sobreviver à suspensão em
/// 'Manipular Excecao'. Correlacionado por PROCESS_ID. Em memória para a PoC; a persistência
/// durável entra com fundacao-persistencia.
/// </summary>
public sealed record ServiceExecutionSnapshot(
    string ProcessKey, string ProcessId, long IdAiim, ProcessExecutionContext Ctx);

/// <summary>Guarda e recupera o <see cref="ServiceExecutionSnapshot"/> por chave de correlação.</summary>
public interface IServiceExecutionState
{
    void Save(string correlationKey, ServiceExecutionSnapshot snapshot);
    ServiceExecutionSnapshot? Load(string correlationKey);
    void Clear(string correlationKey);
}
