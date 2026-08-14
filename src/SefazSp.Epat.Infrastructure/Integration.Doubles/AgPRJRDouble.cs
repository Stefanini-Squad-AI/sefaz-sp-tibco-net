#nullable enable

// Card: BUILD-CONTROPC-seg039 — AC2
// Duble para o processo AgPRJR (pacote externo não entregue na POC).
//
// AgPRJR é um dos 6 destinos ausentes de AGUARDAR (CONTROPC/ISetSubProc).
// Destino seleccionado quando IDTIPOIMPUGNACA == "4" (rito próprio do PRJ).
// O duble existe para satisfazer o registo AGUARDARRegistry no arranque,
// mas lança excepção imediata ao ser invocado — tornando visível a ausência do pacote.
// Este comportamento é INTENCIONAL e contrário à falha silenciosa do legado
// (HaltOnBadSubProcess=false do TIBCO iProcess).
//
// LIMITAÇÃO DOCUMENTADA DA POC (AC2, BUILD-CONTROPC-seg039):
//   AgPRJR pertence a um pacote externo não entregue nesta POC.
//   Substituir este duble por uma implementação concreta quando o pacote for entregue.

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Duble de pacote externo para o processo <c>AgPRJR</c> (interface <see cref="IAGURETPC"/>).
///
/// <para>
/// <b>Este duble lança excepção ao ser invocado.</b>
/// O processo AgPRJR pertence a um pacote externo não entregue na POC.
/// A excepção torna visível a ausência do pacote, em contraste com a falha silenciosa
/// do legado TIBCO (<c>HaltOnBadSubProcess=false</c>).
/// </para>
///
/// <para>
/// O duble é registado em <see cref="AGUARDARRegistry"/> com a chave <c>"AgPRJR"</c>
/// para satisfazer a validação no arranque.
/// Substituir por implementação concreta quando o pacote AgPRJR for entregue.
/// </para>
/// </summary>
public sealed class AgPRJRDouble : IAGURETPC
{
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Sempre lançada — AgPRJR é um pacote externo não entregue nesta POC.
    /// </exception>
    public Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        throw new InvalidOperationException(
            "AgPRJRDouble: o processo AgPRJR pertence a um pacote externo não entregue nesta POC. " +
            "Invocar este duble não é suportado. " +
            "Substituir por implementação concreta quando o pacote AgPRJR for entregue. " +
            "Esta falha é intencional e visível — NAO herda HaltOnBadSubProcess=false do legado TIBCO " +
            "(BUILD-CONTROPC-seg039, AC2).");
    }
}
