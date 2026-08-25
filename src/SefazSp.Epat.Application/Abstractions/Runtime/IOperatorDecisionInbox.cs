#nullable enable

namespace SefazSp.Epat.Application.Abstractions.Runtime;

/// <summary>
/// Caixa de entrada da decisão do operador ('Manipular Excecao'), correlacionada por
/// PROCESS_ID. O endpoint de retomada deposita o OUTCOME aqui e depois liberta o bookmark;
/// a fase 2 do molde de serviço lê-o ao retomar. Desacopla a decisão da entrega de payload
/// do motor (bookmark-correlation, NOEQ-external-event).
/// </summary>
public interface IOperatorDecisionInbox
{
    /// <summary>Regista a decisão do operador para a chave de correlação.</summary>
    void Set(string correlationKey, string outcome);

    /// <summary>Consome a decisão registada; devolve false se não existir.</summary>
    bool TryTake(string correlationKey, out string outcome);
}
