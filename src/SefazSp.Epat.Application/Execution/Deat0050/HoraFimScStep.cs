#nullable enable

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
// CALCTIME: a chamada de produção usa IPEDateTimeUtil.CALCTIME(SW_TIME, 1, 0, DAYSOVER).
//   A semântica exacta (base-1/base-0, comprimento vs posição final) está pendente de
//   confirmação da documentação TIBCO (rulings.BUILTIN-SEMANTICS). O corpo será
//   completado quando IProcessBuiltins.CALCTIME estiver definido em
//   src/SefazSp.Epat.Infrastructure/Legacy.

namespace SefazSp.Epat.Application.Execution.Deat0050;

/// <summary>
/// Passo HoraFimSC do DEAT0050 — calcula a hora de fim do dia de trabalho
/// e escreve <see cref="AiimCase.PRAZODEFESAT"/> (e <see cref="AiimCase.DAYSOVER"/>)
/// a partir do resultado de CALCTIME.
///
/// Nunca usa <see cref="DateTime.Now"/>: o relógio é sempre <see cref="IClock"/> injectado.
/// </summary>
public sealed class HoraFimScStep
{
    private readonly IClock _clock;

    public HoraFimScStep(IClock clock) => _clock = clock;

    /// <summary>
    /// Executa o script HoraFimSC sobre <paramref name="caseData"/>.
    /// O atalho des1 foi removido (decisão ratificada).
    /// O corpo de CALCTIME será preenchido quando IProcessBuiltins estiver implementado.
    /// </summary>
    public void Execute(AiimCase caseData)
    {
        // PENDENTE: chamar IProcessBuiltins.CALCTIME(currentTime, 1, 0, caseData.DAYSOVER)
        // para calcular PRAZODEFESAT conforme o legado de produção.
        // O método CalcTime será adicionado a IProcessBuiltins quando rulings.BUILTIN-SEMANTICS
        // fixar a base de índice e a aridade.
        //
        // Exemplo de corpo esperado (sujeito a confirmação):
        //   var currentTime = TimeOnly.FromTimeSpan(_clock.Now.TimeOfDay);
        //   caseData.PRAZODEFESAT = _builtins.CalcTime(currentTime, 1, 0, caseData.DAYSOVER);
        _ = _clock; // referência ao relógio mantida para AC3 (nunca DateTime.Now)
    }
}
