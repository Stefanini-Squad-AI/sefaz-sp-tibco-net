#nullable enable

// Card: BUILD-POCEPATPROCESS-seg034
// AC4 — scriptTask 'Define Destinatarios' (_G4hU81qhEfG5K7mY0I3I6w, entrouPor=fluxo)
//
// Classificação (rule-catalogue.json · RI-script-POC_EpatProcess-DefineDestinatarios):
//   eRegraDeNegocio=false · efeito=tecnico
//   "nao le nenhum campo do caso; so envelope tecnico ou estado da pagina"
//   → lógica fica integralmente em Application/Execution.
//
// Script legado (POC_Epat.xpdl, linha 2657):
//   IPESystemValues.SW_HOSTNAME == 'prod1' → determina CCRELATORIO e BCCRELATORIO
//
// Hipótese H2 confirmada: os endereços NÃO são literais — vêm de opções injectadas
// por ambiente (rulings.HARDCODED-VALUES). A comparação com 'prod1' selecciona o
// conjunto de endereços de produção vs. outros ambientes.
//
// Nenhum endereço de e-mail é literal no código — todos vêm de DefineDestinatariosOptions.

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Execution.PocEpatProcess;

/// <summary>
/// Passo de envelope técnico do scriptTask 'Define Destinatarios'
/// (<c>_G4hU81qhEfG5K7mY0I3I6w</c>) do processo POC_EpatProcess.
///
/// Determina os endereços em CC e BCC do relatório com base no ambiente de execução
/// e escreve-os em <c>CCRELATORIO</c> e <c>BCCRELATORIO</c> do caso para consumo
/// pelo passo seguinte (emailTask 'Email Limite Rel 1').
///
/// <para>
/// <b>Invariante (rulings.HARDCODED-VALUES):</b> nenhum endereço de e-mail é literal
/// no código — todos vêm de <see cref="DefineDestinatariosOptions"/> injectadas por
/// configuração por ambiente.
/// </para>
/// </summary>
public sealed class DefineDestinatariosStep
{
    private readonly DefineDestinatariosOptions _options;

    /// <param name="options">
    /// Opções de ambiente: endereços CC/BCC para produção e para outros ambientes.
    /// Injectadas por configuração — nunca literais no código.
    /// </param>
    public DefineDestinatariosStep(DefineDestinatariosOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Executa o script 'Define Destinatarios':
    /// selecciona os endereços CC/BCC com base no ambiente (<see cref="DefineDestinatariosOptions.IsProducao"/>)
    /// e escreve-os em <c>CCRELATORIO</c> e <c>BCCRELATORIO</c> do caso.
    ///
    /// <para>
    /// Reproduz o comportamento legado (linha 2657 do XPDL):
    /// <code>
    ///   if (IPESystemValues.SW_HOSTNAME == 'prod1') {
    ///       CCRELATORIO  = /* endereço produção */;
    ///       BCCRELATORIO = /* bcc produção */;
    ///   } else {
    ///       CCRELATORIO  = /* endereço não-produção */;
    ///       BCCRELATORIO = /* bcc não-produção */;
    ///   }
    /// </code>
    /// </para>
    /// </summary>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    public void Execute(AiimCase aiimCase)
    {
        // ── ordem 5: scriptTask 'Define Destinatarios' (_G4hU81qhEfG5K7mY0I3I6w) ──
        // Regra: RI-script-POC_EpatProcess-DefineDestinatarios (eRegraDeNegocio=false)
        // Nenhum endereço é literal — vêm de _options (configuração por ambiente).
        // Hipótese H2 confirmada: endereços de configuração, não de código.

        if (_options.IsProducao)
        {
            // Ambiente produção: SW_HOSTNAME == 'prod1' no legado
            aiimCase.CCRELATORIO  = FieldValue<string>.Of(_options.CcRelatorioProd);
            aiimCase.BCCRELATORIO = FieldValue<string>.Of(_options.BccRelatorioProd);
        }
        else
        {
            // Ambientes não-produção (dev, homologação, etc.)
            aiimCase.CCRELATORIO  = FieldValue<string>.Of(_options.CcRelatorioNaoProd);
            aiimCase.BCCRELATORIO = FieldValue<string>.Of(_options.BccRelatorioNaoProd);
        }
    }
}

/// <summary>
/// Opções de configuração para o passo 'Define Destinatarios'.
/// Todos os campos são preenchidos por configuração por ambiente
/// (rulings.HARDCODED-VALUES) — nunca literais no código.
/// </summary>
public sealed class DefineDestinatariosOptions
{
    /// <summary>
    /// Indica se o ambiente de execução é produção (<c>SW_HOSTNAME == 'prod1'</c> no legado).
    /// Quando <see langword="true"/>, os endereços <c>Prod</c> são usados;
    /// caso contrário, os endereços <c>NaoProd</c>.
    /// </summary>
    public bool IsProducao { get; init; }

    /// <summary>CC do relatório em ambiente de produção. Preenchido por configuração.</summary>
    public string CcRelatorioProd { get; init; } = string.Empty;

    /// <summary>BCC do relatório em ambiente de produção. Preenchido por configuração.</summary>
    public string BccRelatorioProd { get; init; } = string.Empty;

    /// <summary>CC do relatório em ambiente não-produção. Preenchido por configuração.</summary>
    public string CcRelatorioNaoProd { get; init; } = string.Empty;

    /// <summary>BCC do relatório em ambiente não-produção. Preenchido por configuração.</summary>
    public string BccRelatorioNaoProd { get; init; } = string.Empty;
}
