#nullable enable

// Card: BUILD-POCEPATPROCESS-seg034
// AC2 — scriptTask 'Verificar Anulacao' (_CI6lx1qREfG5K7mY0I3I6w, entrouPor=fluxo)
//
// Classificação (rule-catalogue.json · RI-script-POC_EpatProcess-VerificarAnulacao):
//   eRegraDeNegocio=true · efeito=calcula-valor
//   → Lógica de domínio em Domain/Rules/PocEpatProcess/VerificarAnulacaoRule.cs
//   → Este passo é o envelope de Application/Execution: invoca a regra pura.

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Rules.PocEpatProcess;

namespace SefazSp.Epat.Application.Execution.PocEpatProcess;

/// <summary>
/// Envelope de execução do scriptTask 'Verificar Anulacao'
/// (<c>_CI6lx1qREfG5K7mY0I3I6w</c>) do processo POC_EpatProcess.
///
/// Delega toda a lógica de negócio à regra pura
/// <see cref="VerificarAnulacaoRule"/> (Domain/Rules).
/// Este passo não contém decisões de negócio — apenas coordena a invocação.
/// </summary>
public sealed class VerificarAnulacaoStep
{
    /// <summary>
    /// Executa o script 'Verificar Anulacao' — aplica
    /// <see cref="VerificarAnulacaoRule.Apply"/> ao caso.
    /// </summary>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    public void Execute(AiimCase aiimCase)
    {
        // ── ordem 3: scriptTask 'Verificar Anulacao' (_CI6lx1qREfG5K7mY0I3I6w) ──
        // Regra de negócio: RI-script-POC_EpatProcess-VerificarAnulacao
        // Delegado integralmente à função pura em Domain/Rules.
        VerificarAnulacaoRule.Apply(aiimCase);
    }
}
