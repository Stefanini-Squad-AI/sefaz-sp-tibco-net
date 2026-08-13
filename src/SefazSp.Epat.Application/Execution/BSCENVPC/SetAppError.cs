#nullable enable

using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Execution.BSCENVPC;

/// <summary>
/// Execucao do scriptTask 'Set App Error' (_qIDu4V6BEfGBBLgT-R5iuw).
///
/// Ocorre no ramo de falha do gateway _qIDu4l6BEfGBBLgT-R5iuw, quando
/// STATUS_CODE != "0". O script iProcess original definia ISAPPERROR = "Y",
/// sinalizando que o erro e de aplicacao (negocio) e nao de infraestrutura.
///
/// Esta distincao e usada a jusante pelo gateway App Error (_qIDuo16BEfGBBLgT-R5iuw)
/// para separar o caminho de retentativa automatica do caminho de escala manual.
/// </summary>
public static class SetAppError
{
    /// <summary>
    /// Aplica o estado de erro de aplicacao ao contexto de execucao.
    /// Equivale ao scriptTask 'Set App Error' do iProcess.
    /// </summary>
    /// <param name="ctx">Contexto de execucao mutavel do processo.</param>
    public static void Apply(ProcessExecutionContext ctx)
    {
        ctx.ISAPPERROR = "Y";
    }
}
