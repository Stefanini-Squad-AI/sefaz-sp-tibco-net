#nullable enable

using SefazSp.Epat.Application.Abstractions.Legacy;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Abstractions;

// BUILD-DEAT0050-seg012 — scriptTask HoraFimSC (_lrer3VqhEfG5K7mY0I3I6w)
// Fonte XPDL: linha 3895
// Classificação: eRegraDeNegocio=false, efeito=tecnico
//
// ATALHO des1 REMOVIDO (decisão ratificada no glossário, rulings.HARDCODED-ATALHOS):
//   O legado continha: if (SW_HOSTNAME == 'des1') { PRAZODEFESA = SW_DATE; PRAZODEFESAT = CALCTIME(SW_TIME,1,0,DAYSOVER) }
//   Este encurtamento de prazo para ambiente de desenvolvimento não é migrado.
//   A demonstração usa relógio controlável (IClock) em vez de prazos encurtados por nome de máquina.
//
// CALCTIME: reproduzido pela camada anticorrupção (IProcessBuiltins.CalcTime), base-1.

namespace SefazSp.Epat.Application.Execution.Deat0050;

/// <summary>
/// Passo HoraFimSC do DEAT0050 — calcula a hora de fim do dia de trabalho
/// e escreve <see cref="AiimCase.PRAZODEFESAT"/> a partir do resultado de CALCTIME.
///
/// Nunca usa <see cref="DateTime.Now"/>: o relógio é sempre <see cref="IClock"/> injectado.
/// </summary>
public sealed class HoraFimScStep
{
    private readonly IClock _clock;
    private readonly IProcessBuiltins _builtins;

    public HoraFimScStep(IClock clock, IProcessBuiltins builtins)
    {
        _clock = clock;
        _builtins = builtins;
    }

    /// <summary>
    /// Executa o script HoraFimSC sobre <paramref name="caseData"/>.
    /// O atalho des1 foi removido (decisão ratificada).
    /// </summary>
    public void Execute(AiimCase caseData)
    {
        // Produção: PRAZODEFESAT = CALCTIME(SW_TIME, 1, 0, DAYSOVER).
        // SW_TIME é a hora corrente do motor, fornecida pelo relógio injectável.
        var currentTime = TimeOnly.FromTimeSpan(_clock.Now.TimeOfDay);
        caseData.PRAZODEFESAT = _builtins.CalcTime(currentTime, 1, 0, caseData.DAYSOVER);
    }
}

