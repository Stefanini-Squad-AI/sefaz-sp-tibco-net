// userTask _xWNLe1qSEfG5K7mY0I3I6w "Finalizar AIIM"
// Rule: RI-formScript-POC_EpatProcess-FinalizarAIIM (eRegraDeNegocio=true)
// Expressão: AFR = IPEStarterUtil.GETATTRIBUTE("Name"); CNTINSTANCIASUF = 0;
#nullable enable

using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.UseCases;

/// <summary>
/// Caso de uso para a tarefa humana _xWNLe1qSEfG5K7mY0I3I6w — "Finalizar AIIM".
/// Aplica a regra RI-formScript-POC_EpatProcess-FinalizarAIIM ao submeter o formulário:
///   AFR             ← IPEStarterUtil.GETATTRIBUTE("Name") [via shim IStarterNameProvider]
///   CNTINSTANCIASUF ← 0
/// A lógica é regra de negócio (eRegraDeNegocio=true); o shim resolve o builtin iProcess.
/// </summary>
public sealed class FinalizarAiimUseCase
{
    private readonly IStarterNameProvider _starterNameProvider;

    public FinalizarAiimUseCase(IStarterNameProvider starterNameProvider)
    {
        ArgumentNullException.ThrowIfNull(starterNameProvider);
        _starterNameProvider = starterNameProvider;
    }

    /// <summary>
    /// Aplica o form-script de submissão do formulário Finalizar AIIM.
    /// </summary>
    public void Execute(AiimCase aiimCase)
    {
        ArgumentNullException.ThrowIfNull(aiimCase);

        // AFR = IPEStarterUtil.GETATTRIBUTE("Name")
        // Shim tri-state: pattern match exaustivo exigido pelo compilador (FieldValue<T>).
        var starterName = _starterNameProvider.GetStarterName();
        aiimCase.AFR = starterName.Match(
            hasValue: name => name,
            notAvailable: () => aiimCase.AFR,   // SW_NA: preserva o valor actual
            empty: () => aiimCase.AFR);          // Empty: preserva o valor actual

        // CNTINSTANCIASUF = 0
        aiimCase.CNTINSTANCIASUF = 0;
    }
}
