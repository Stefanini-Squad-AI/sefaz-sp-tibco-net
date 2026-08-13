#nullable enable

using SefazSp.Epat.Domain.Cases;

// BUILD-POCEPATPROCESS-seg022 — scriptTask 'Flag Retirati True GS' (_0XWahVqNEfG5K7mY0I3I6w)
// Fonte XPDL: linha 1413
// Classificação: eRegraDeNegocio=false, efeito=tecnico
// Regra: RI-script-POC_EpatProcess-FlagRetiratiTrueGS
//
// O script faz:
//   FLAGRETIRATE    = true   (flag de controlo — indica que o retorno Retirati foi recebido)
//   EXISTENOTIFICAC = true   (confirma que existe notificação — estado pós-recepção do evento)
//
// Ambos são campos do caso (AiimCase, bool), não envelope técnico.
// O script original não lê nenhum campo do caso — só atribui estes dois.
// Não há lógica de negócio: a atribuição é incondicional, independente do conteúdo da notificação.
//
// Rastreia: BUILD-POCEPATPROCESS-seg022, checklist ordem 2
//   (_0XWahVqNEfG5K7mY0I3I6w, entrouPor=fluxo, RI-script-POC_EpatProcess-FlagRetiratiTrueGS)

namespace SefazSp.Epat.Application.Execution.POCEpatProcess;

/// <summary>
/// Passo scriptTask 'Flag Retirati True GS' do POC_EpatProcess
/// (<c>_0XWahVqNEfG5K7mY0I3I6w</c>).
///
/// Marca o flag de retorno Retirati (<c>FLAGRETIRATE = true</c>) e confirma
/// a existência de notificação (<c>EXISTENOTIFICAC = true</c>) no envelope do caso.
///
/// Classificado como técnico (<c>eRegraDeNegocio=false</c>) — nenhuma regra de domínio
/// reside aqui. A atribuição é incondicional e não depende do conteúdo da notificação.
/// </summary>
public static class FlagRetiratiTrueGsStep
{
    /// <summary>
    /// Executa o script 'Flag Retirati True GS':
    /// <c>FLAGRETIRATE = true</c> e <c>EXISTENOTIFICAC = true</c>.
    /// </summary>
    /// <param name="caseData">Dados do caso a actualizar.</param>
    public static void Execute(AiimCase caseData)
    {
        // O nome do passo diz explicitamente "True GS": marca o flag como verdadeiro.
        caseData.FLAGRETIRATE    = true;
        // Após receber o evento de notificação, a notificação existe.
        caseData.EXISTENOTIFICAC = true;
    }
}
