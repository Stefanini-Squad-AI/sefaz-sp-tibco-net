#nullable enable

// ICorrelationStore — contrato de retomada de workflow por chave de correlacao.
// Decisao NOEQ-external-event ratificada em 2026-08-06: bookmark-correlation.
//
// Esta interface expoe o minimo necessario para o endpoint de retomada:
// retomar uma instancia de workflow suspensa identificada por correlationKey.
//
// A implementacao concreta pertence ao adaptador do motor Elsa 3 (Infrastructure/Workflow.Elsa),
// entregue pelo papel fundacao-motor. Nos testes usa-se um duble em memoria.

namespace SefazSp.Epat.Application.Abstractions.Runtime;

/// <summary>
/// Armazena e resolve a associacao entre uma chave de correlacao e a instancia
/// de workflow suspensa que aguarda retomada.
///
/// Decisao NOEQ-external-event = bookmark-correlation (2026-08-06).
/// A chave de correlacao e PROCESS_ID = 'idAiim-n idProc-n', montada pelos scripts
/// antes de cada chamada — nao precisa de ser inventada.
///
/// POR DEFINIR (etapa 5 do plano de cumprimento):
///   • Proteccao do endpoint de retomada.
///   • Politica de idempotencia para entrega duplicada ou resposta atrasada.
/// </summary>
public interface ICorrelationStore
{
    /// <summary>
    /// Retoma a instancia de workflow suspensa identificada por <paramref name="correlationKey"/>.
    /// </summary>
    /// <param name="correlationKey">
    /// Chave de correlacao — para DEAT0050/INICALC esta chave e o valor de PROCESS_ID.
    /// </param>
    /// <param name="payload">Dados entregues ao bookmark ao ser retomado.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// <c>true</c> se uma instancia suspensa foi encontrada e retomada;
    /// <c>false</c> se nenhuma instancia aguardava com essa chave.
    /// </returns>
    Task<bool> ResumeAsync(string correlationKey, object? payload, CancellationToken ct);
}
