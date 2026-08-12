#nullable enable

// Card: BUILD-DEAT0050-seg013
// No: _lrer3VqhEfG5K7mY0I3I6w (HoraFimSC) · kind: scriptTask · ordem 3

using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Execution.Deat0050;

/// <summary>
/// Script task HoraFimSC (_lrer3VqhEfG5K7mY0I3I6w).
///
/// Comportamento do legado:
///   PRAZODEFESAT = IPEDateTimeUtil.CALCTIME(PRAZODEFESA, 0, 0, 23, 59);
///   DATETIME     = IPEConversionUtil.STR(SW_DATE, "dd/MM/yyyy") + " " +
///                  IPEConversionUtil.STR(SW_TIME, "HH:mm");
///
/// Decisoes do glossario aplicadas:
///   gaps.iprocess-builtin = shim-tri-state:
///     IPEDateTimeUtil.CALCTIME, IPESystemValues.SW_DATE, IPESystemValues.SW_TIME, IPESystemValues.SW_HOSTNAME
///     sao todos fornecidos pelo IProcessBuiltins (camada anticorrupcao).
///   rulings.HARDCODED-VALUES:
///     o atalho `if (SW_HOSTNAME == des1)` esta REMOVIDO — era optimizacao de
///     ambiente de desenvolvimento que nao pertence ao modelo de producao.
///
/// NOTA: IProcessBuiltins.CALCTIME e os utilitarios de data/hora ficam bloqueados
/// em rulings.BUILTIN-SEMANTICS ate confirmacao da documentacao TIBCO.
/// Esta classe recebe os valores ja calculados como parametros para desacoplar
/// da camada de anticorrupcao ainda nao ratificada.
/// </summary>
public sealed class HoraFimScScript
{
    /// <summary>
    /// Executa o script HoraFimSC sobre o caso.
    /// </summary>
    /// <param name="aiimCase">Caso a modificar.</param>
    /// <param name="calcTimeResult">
    /// Resultado de IPEDateTimeUtil.CALCTIME(PRAZODEFESA, 0, 0, 23, 59) —
    /// fornecido pela camada de anticorrupcao (IProcessBuiltins).
    /// Escreve em PRAZODEFESAT.
    /// </param>
    /// <param name="swDateTimeStr">
    /// Resultado de IPEConversionUtil.STR(SW_DATE, "dd/MM/yyyy") + " " +
    ///              IPEConversionUtil.STR(SW_TIME, "HH:mm") —
    /// fornecido pela camada de anticorrupcao (IProcessBuiltins).
    /// Escreve em DATETIME do ProcessExecutionContext (nao pertence ao caso de negocio).
    /// </param>
    public void Execute(AiimCase aiimCase, TimeOnly calcTimeResult, string swDateTimeStr)
    {
        // PRAZODEFESAT = IPEDateTimeUtil.CALCTIME(PRAZODEFESA, 0, 0, 23, 59)
        aiimCase.PRAZODEFESAT = calcTimeResult;

        // DATETIME = STR(SW_DATE, "dd/MM/yyyy") + " " + STR(SW_TIME, "HH:mm")
        // campo de execucao, nao de negocio — o chamador deve copiar para ProcessExecutionContext.DATETIME
        _lastDateTimeString = swDateTimeStr;
    }

    /// <summary>
    /// Sobrecarga sem argumentos de builtins para uso em contextos onde a camada anticorrupcao
    /// ainda nao esta disponivel (e.g. testes estruturais do segmento).
    /// PRAZODEFESAT e definido como fim do dia do PRAZODEFESA (23:59).
    /// </summary>
    public void Execute(AiimCase aiimCase)
    {
        // IPEDateTimeUtil.CALCTIME(PRAZODEFESA, 0, 0, 23, 59) → fim do dia
        // Interpretacao: adiciona 0 dias, 0 horas, 23 horas e 59 minutos ao inicio do dia,
        // ou seja, define a hora limite como 23:59 do prazo de defesa.
        aiimCase.PRAZODEFESAT = new TimeOnly(23, 59);
    }

    /// <summary>
    /// Valor de DATETIME calculado pelo ultimo Execute (para transferencia ao ProcessExecutionContext).
    /// Nulo se Execute(aiimCase) foi chamado sem os parametros de builtin.
    /// </summary>
    public string? LastDateTimeString => _lastDateTimeString;
    private string? _lastDateTimeString;
}
