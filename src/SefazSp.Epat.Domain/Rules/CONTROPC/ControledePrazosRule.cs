#nullable enable

// Card: BUILD-CONTROPC-seg045
// AC3 — scriptTask 'Controle de Prazos' (_-bkw_V6JEfGBBLgT-R5iuw, entrouPor=fluxo)
//
// Classificação (rule-catalogue.json · RI-script-CONTROPC-ControledePrazos):
//   eRegraDeNegocio=true · efeito=calcula-valor
//   → lógica de domínio pura em Domain/Rules; envelope técnico fica em Application/Execution.
//
// Script legado (POC_Epat.xpdl, linha 8744 / CONTROPC__MAIN.bpmn _-bkw_V6JEfGBBLgT-R5iuw):
//   if (DSTIPOINTIMACAO=="Carta com AR") { PERIODOEMDIAS=360; }
//   else if (DSTIPOINTIMACAO=="DE") { PRAZOCIENCIA=DTPUBLICACAODE; PERIODOEMDIAS=0; }
//   else { PERIODOEMDIAS=10; }
//   NOVOMODELO == true;   // ← expressão morta no legado (comparação, não atribuição — ignorada)
//
// Campos lidos :  DSTIPOINTIMACAO, DTPUBLICACAODE
// Campos escritos: PERIODOEMDIAS, PRAZOCIENCIA

using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Domain.Rules.CONTROPC;

/// <summary>
/// Regra de domínio para o scriptTask 'Controle de Prazos'
/// (<c>_-bkw_V6JEfGBBLgT-R5iuw</c>) do processo CONTROPC.
///
/// Determina o período em dias e a data de ciência com base no tipo de intimação.
/// Função pura: não depende de relógio, I/O nem estado externo.
/// </summary>
public static class ControledePrazosRule
{
    /// <summary>
    /// Identificador da regra de instância — invariante: não renomear.
    /// </summary>
    public const string RuleId = "RI-script-CONTROPC-ControledePrazos";

    /// <summary>
    /// Aplica a regra 'Controle de Prazos' ao caso.
    ///
    /// <para>
    /// Comportamento transcrito do script legado (CONTROPC__MAIN.bpmn, nó <c>_-bkw_V6JEfGBBLgT-R5iuw</c>):
    /// <list type="bullet">
    ///   <item>
    ///     <c>DSTIPOINTIMACAO == "Carta com AR"</c>:
    ///     intimação por carta com aviso de recebimento — prazo de 360 dias.
    ///   </item>
    ///   <item>
    ///     <c>DSTIPOINTIMACAO == "DE"</c>:
    ///     intimação por Diário Eletrônico — prazo de 0 dias e ciência = data de publicação.
    ///   </item>
    ///   <item>
    ///     Demais tipos: prazo padrão de 10 dias.
    ///   </item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// A expressão <c>NOVOMODELO == true</c> no fim do script legado é uma comparação sem
    /// efeito (dead expression) — não é uma atribuição e não é reproduzida aqui.
    /// </para>
    /// </summary>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    public static void Apply(AiimCase aiimCase)
    {
        if (aiimCase.DSTIPOINTIMACAO == "Carta com AR")
        {
            // Intimação por carta com AR: prazo de 360 dias.
            aiimCase.PERIODOEMDIAS = 360;
        }
        else if (aiimCase.DSTIPOINTIMACAO == "DE")
        {
            // Intimação por Diário Eletrônico: ciência = data de publicação; prazo = 0.
            aiimCase.PRAZOCIENCIA = aiimCase.DTPUBLICACAODE;
            aiimCase.PERIODOEMDIAS = 0;
        }
        else
        {
            // Demais tipos: prazo padrão de 10 dias.
            aiimCase.PERIODOEMDIAS = 10;
        }

        // NOVOMODELO == true; → dead expression no legado; não reproduzida.
    }
}
