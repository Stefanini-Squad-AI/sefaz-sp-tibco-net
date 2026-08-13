// scriptTask _XWivF1qTEfG5K7mY0I3I6w "Set Nome Etapa 2"
// Rule: RI-script-POC_EpatProcess-SetNomeEtapa2 · eRegraDeNegocio=false
// Assigns NOMEETAPA and resets DAYSOVER, DTFIMCQ, HRFIMCQ.
#nullable enable

using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Execution;

/// <summary>
/// scriptTask _XWivF1qTEfG5K7mY0I3I6w — "Set Nome Etapa 2".
/// Corpo técnico: define o nome da etapa e reinicia os campos de controlo de prazo.
/// Rule: RI-script-POC_EpatProcess-SetNomeEtapa2 (eRegraDeNegocio=false).
/// Atribui: NOMEETAPA, DAYSOVER, DTFIMCQ, HRFIMCQ.
/// </summary>
public static class SetNomeEtapa2Execution
{
    /// <summary>Valor que o legado atribui a NOMEETAPA neste scriptTask.</summary>
    public const string NomeEtapa = "Etapa 2";

    /// <summary>
    /// Executa o corpo do scriptTask Set Nome Etapa 2.
    /// Equivalente iProcess: NOMEETAPA = "Etapa 2"; DAYSOVER = 0; DTFIMCQ = default; HRFIMCQ = default;
    /// </summary>
    public static void Execute(AiimCase aiimCase)
    {
        ArgumentNullException.ThrowIfNull(aiimCase);

        aiimCase.NOMEETAPA = NomeEtapa;
        aiimCase.DAYSOVER = 0;
        aiimCase.DTFIMCQ = default;
        aiimCase.HRFIMCQ = default;
    }
}
