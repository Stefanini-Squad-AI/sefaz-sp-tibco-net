#nullable enable

// Card: BUILD-CONTROPC-seg039
// Passo técnico do segmento SC-CONTROPC-001 · passos 5–7 · etapa 4.
//
// Desativa Subs (_-bkxKF6JEfGBBLgT-R5iuw) — ordem 2 — RI-script-CONTROPC-DesativaSubs
//
// Classificação (rule-catalogue.json · RI-script-CONTROPC-DesativaSubs):
//   eRegraDeNegocio=false · efeito=tecnico
//   "nao le nenhum campo do caso; so envelope tecnico ou estado da pagina"
//   → lógica de envelope técnico permanece em Application/Execution.
//
// Script legado (POC_Epat.xpdl, linha 8860):
//   Expressão original: vazia (corpo opaco no pacote)
//   Campos lidos:  nenhum (campos=[])
//   Campos escritos: STATUSSUBPROC

using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Execution.CONTROPC;

/// <summary>
/// Passos técnicos de execução do segmento CONTROPC-seg039
/// (passos 5–7 do cenário SC-CONTROPC-001).
///
/// <para>
/// Estes passos manipulam exclusivamente o envelope técnico do caso;
/// não contêm lógica de domínio (<c>eRegraDeNegocio=false</c>).
/// </para>
/// </summary>
public static class ControlopcSeg039Steps
{
    // -----------------------------------------------------------------------
    // Passo 6 — Desativa Subs (_-bkxKF6JEfGBBLgT-R5iuw) — scriptTask
    // -----------------------------------------------------------------------
    // RI-script-CONTROPC-DesativaSubs
    // Fonte XPDL: linha 8860
    // Classificação: eRegraDeNegocio=false; efeito=tecnico
    // Atribui: STATUSSUBPROC
    // Expressão original: vazia (corpo opaco no pacote POC_Epat)
    //
    // Semântica observada: o passo desactiva a referência ao subprocesso dinâmico
    // após o retorno de 'Aguardar Retorno' (_-bkw-V6JEfGBBLgT-R5iuw).
    // STATUSSUBPROC é esvaziado para indicar que nenhum subprocesso está activo.
    // Não lê nenhum campo de caso.

    /// <summary>
    /// Executa o scriptTask <c>Desativa Subs</c> (<c>_-bkxKF6JEfGBBLgT-R5iuw</c>).
    ///
    /// <para>
    /// Limpa <see cref="AiimCase.STATUSSUBPROC"/> após o retorno do subprocesso
    /// dinâmico 'Aguardar Retorno'. A expressão XPDL está vazia (corpo opaco em
    /// <c>RI-script-CONTROPC-DesativaSubs</c>); a atribuição reproduz o efeito
    /// observável — desactivar a referência ao subprocesso — sem inventar lógica ausente.
    /// Nenhum campo do caso é lido (<c>campos=[]</c>).
    /// </para>
    /// </summary>
    /// <param name="caso">Estado mutável do caso AIIM.</param>
    public static void ExecuteDesativaSubs(AiimCase caso)
    {
        // STATUSSUBPROC := "" — desactiva a referência ao subprocesso dinâmico.
        // Corpo do script opaco no pacote (RI-script-CONTROPC-DesativaSubs);
        // nenhum campo de caso é lido (campos=[]).
        caso.STATUSSUBPROC = string.Empty;
    }
}
