#nullable enable

// Card: BUILD-CONTROPC-seg039 — AgPecas (destino entregue)
//
// AgPecas é o único dos 7 destinos de AGUARDAR (CONTROPC/ISetSubProc) cujo pacote foi
// entregue: mapeia para o subprocesso AGPECASPC (implementa IAGURETPC).
// Ao contrário dos 6 doubles de pacote externo, este devolve um resultado de sucesso —
// demonstra o caminho feliz da resolução dinâmica (interface-registry-validated).

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Destino <c>AgPecas</c> da interface <see cref="IAGURETPC"/> — o subprocesso AGPECASPC,
/// o único pacote entregue na POC. Devolve <see cref="ProcessCallResult"/> de sucesso.
/// </summary>
public sealed class AgPecasDouble : IAGURETPC
{
    public Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        Console.WriteLine(
            $"[CONTROPC][AgPecas→AGPECASPC] subprocesso dinâmico resolvido e executado " +
            $"(PROCESS_ID={caseRef.ProcessId}).");
        return Task.FromResult(new ProcessCallResult(
            Started: true,
            ChildInstanceId: $"AGPECASPC-{caseRef.ProcessId}",
            Failure: null));
    }
}
