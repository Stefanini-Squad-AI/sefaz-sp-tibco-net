<#
.SYNOPSIS
    O eixo unico de classificacao de regra, partilhado pelos tres geradores.

.DESCRIPTION
    Antes disto cada gerador tinha o seu vocabulario, e a palavra 'regra-de-negocio'
    nao queria dizer o mesmo nos dois: no XPDL era a categoria principal, nas telas
    era a categoria residual. Somar as duas colunas com o mesmo nome era somar coisas
    diferentes.

    Passam a existir duas dimensoes ortogonais.

    EFEITO - o que a regra faz ao caso:
      decide-fluxo     escolhe por onde o caso segue
      calcula-valor    calcula ou atribui o valor de um campo do caso
      valida-entrada   impede a accao e diz ao utilizador porque
      fixa-prazo       compromisso de tempo
      restringe-ecra   liga ou desliga um controlo em funcao de dado do caso
      tecnico          nao e regra de negocio

    PORTADOR - onde a regra esta escrita, o que determina o que e preciso fazer
    para a mudar. Esta e a dimensao que interessa a migracao, porque separa o que
    um analista pode alterar do que obriga a reimplantar ou a recompilar.

    O portao e sempre o mesmo: sem leitura de campo do caso nao ha regra de negocio.
    Um retry sobre NUMAPPRETRIES le campos, mas sao campos do envelope tecnico.
#>

$script:AlteracaoPorPortador = @{
    corticon    = 'publicar-planilha'
    transition  = 'reimplantar-processo'
    script      = 'reimplantar-processo'
    deadline    = 'reimplantar-processo'
    dataMapping = 'reimplantar-processo'
    formScript  = 'reimplantar-processo'
    screenCode  = 'recompilar-aplicacao'
}

$script:EfeitoDescricao = [ordered]@{
    'decide-fluxo'    = 'escolhe por onde o caso segue'
    'calcula-valor'   = 'calcula ou atribui o valor de um campo do caso'
    'valida-entrada'  = 'impede a accao e diz ao utilizador porque'
    'fixa-prazo'      = 'compromisso de tempo'
    'restringe-ecra'  = 'liga ou desliga um controlo em funcao de dado do caso'
    'tecnico'         = 'nao e regra de negocio: retry, nulo, sessao, envelope de erro'
}

function Get-EfeitoDescricao { param([string]$Efeito) return $script:EfeitoDescricao[$Efeito] }
function Get-EfeitosConhecidos { return @($script:EfeitoDescricao.Keys) }

<#
    Cada gerador reune as evidencias que a sua fonte permite e chama isto uma vez.
    Nenhum gerador decide sozinho o que e regra.
#>
function New-Classification {
    param(
        [Parameter(Mandatory)][ValidateSet('corticon', 'transition', 'script', 'deadline', 'dataMapping', 'formScript', 'screenCode')]
        [string]$Portador,
        [bool]$LeCamposDoCaso   = $false,
        [bool]$TemCondicao      = $false,
        [bool]$EscreveCampoDoCaso = $false,
        [bool]$EscreveNoMotor   = $false,
        [bool]$MostraMensagem   = $false,
        [bool]$MudaEcra         = $false,
        [bool]$EsperaDeRetentativa = $false
    )

    $efeito = $null
    $nota = $null

    if ($Portador -eq 'deadline') {
        if ($EsperaDeRetentativa) { $efeito = 'tecnico'; $nota = 'espera entre tentativas, nao e prazo de negocio' }
        else { $efeito = 'fixa-prazo' }
    }
    elseif (-not $LeCamposDoCaso) {
        $efeito = 'tecnico'
        $nota = 'nao le nenhum campo do caso; so envelope tecnico ou estado da pagina'
    }
    elseif ($EscreveNoMotor)   { $efeito = 'decide-fluxo' }
    elseif ($MostraMensagem)   { $efeito = 'valida-entrada' }
    elseif ($Portador -eq 'transition') { $efeito = 'decide-fluxo' }
    elseif ($EscreveCampoDoCaso) { $efeito = 'calcula-valor' }
    elseif ($MudaEcra)         { $efeito = 'restringe-ecra' }
    elseif ($TemCondicao)      { $efeito = 'decide-fluxo' }
    else                       { $efeito = 'calcula-valor' }

    return [ordered]@{
        efeito          = $efeito
        efeitoDescricao = $script:EfeitoDescricao[$efeito]
        portador        = $Portador
        alteracaoRequer = $script:AlteracaoPorPortador[$Portador]
        eRegraDeNegocio = ($efeito -ne 'tecnico')
        nota            = $nota
    }
}
