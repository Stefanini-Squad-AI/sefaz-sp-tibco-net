<#
.SYNOPSIS
    Envia a carga do Jira para o board, de forma idempotente.

.DESCRIPTION
    NAO faz parte do pipeline deterministico: escreve num sistema externo, por isso
    corre a mao e nunca a partir do run-extraction. Reenviar a mesma carga tem de
    ACTUALIZAR e nunca duplicar - a chave e a label 'card:<id>', procurada por JQL
    antes de cada escrita.

    CREDENCIAIS por variavel de ambiente, nunca por parametro: um parametro fica no
    historico da consola e nos logs.
        JIRA_BASE_URL   https://<instancia>.atlassian.net
        JIRA_EMAIL      o e-mail da conta
        JIRA_API_TOKEN  o token

    Por omissao corre em simulacao e nao escreve nada. Usar -Confirmar para enviar.

    -SoNovos cria apenas o que ainda nao existe e NAO toca no que ja la esta: nem
    campos, nem responsavel, nem estado. Serve para acrescentar cards a um board
    onde alguem ja mexeu - reabrir a issue para a reescrever apagaria esse trabalho.
    As ligacoes continuam a ser criadas, porque uma ligacao e um objecto novo entre
    duas issues e nao uma alteracao ao conteudo de nenhuma delas.
#>
[CmdletBinding()]
param(
    [string]$PayloadPath = "$PSScriptRoot/../artifacts/POC_Epat/jira/payload.json",
    [switch]$Confirmar,
    [switch]$Diagnostico,
    [string]$Card,
    [switch]$SoNovos,
    [switch]$Mapear
)

$ErrorActionPreference = 'Stop'

function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }

# O Jira explica a recusa no CORPO da resposta; a mensagem da excepcao so diz 400.
function Get-ErroJira {
    param($Erro)
    $corpo = $Erro.ErrorDetails.Message
    if (-not $corpo -and $Erro.Exception.Response) {
        try {
            $s = $Erro.Exception.Response.GetResponseStream()
            $corpo = (New-Object IO.StreamReader($s)).ReadToEnd()
        } catch { }
    }
    if (-not $corpo) { return $Erro.Exception.Message }
    try {
        $j = $corpo | ConvertFrom-Json
        $partes = @()
        foreach ($m in (Arr $j.errorMessages)) { $partes += $m }
        foreach ($p in @($j.errors.PSObject.Properties)) { $partes += "$($p.Name): $($p.Value)" }
        if ($partes.Count) { return ($partes -join ' | ') }
    } catch { }
    return $corpo
}

$base  = $env:JIRA_BASE_URL
$email = $env:JIRA_EMAIL
$token = $env:JIRA_API_TOKEN
if (-not $base -or -not $email -or -not $token) {
    throw "Faltam credenciais. Definir JIRA_BASE_URL, JIRA_EMAIL e JIRA_API_TOKEN no ambiente - nunca como parametro, que fica no historico da consola."
}
# Espaco a volta do valor - tipico de copiar de um .env com ' = ' - quebra o Basic auth
# com um 401 que depois aparece disfarcado de 'projecto nao existe'.
$base = $base.Trim().TrimEnd('/')
$email = $email.Trim()
$token = $token.Trim()
$auth = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("${email}:${token}"))
$cab  = @{ Authorization = $auth; 'Content-Type' = 'application/json' }

$carga = Get-Content $PayloadPath -Raw -Encoding UTF8 | ConvertFrom-Json

# Enviar um so card serve para ver como fica no board antes de povoar tudo. As
# ligacoes ficam de fora: a outra ponta ainda nao existe.
if ($Card) {
    $sel = @(@($carga.issues) | Where-Object { $_.chaveExterna -eq $Card })
    if ($sel.Count -eq 0) {
        throw "Card '$Card' nao existe na carga. Ver os ids em $PayloadPath."
    }
    $carga.issues = $sel
    $carga.ligacoes = @()
}

Write-Host ("Carga: {0} epicas, {1} issues, {2} ligacoes -> projecto {3} em {4}" -f `
    @($carga.epicas).Count, @($carga.issues).Count, @($carga.ligacoes).Count, $carga.projeto, $base)
if ($Card) { Write-Host "  APENAS o card $Card" -ForegroundColor Cyan }
if (-not $Confirmar) { Write-Host '  SIMULACAO: nada sera escrito. Usar -Confirmar para enviar.' -ForegroundColor Yellow }

# Diagnostico: so leituras. Serve para saber PORQUE o envio e recusado sem tentar escrever.
if ($Diagnostico) {
    try {
        $eu = Invoke-RestMethod -Method Get -Uri "$base/rest/api/3/myself" -Headers $cab -ErrorAction Stop
        Write-Host "  autenticado como: $($eu.displayName) <$($eu.emailAddress)>" -ForegroundColor Green
    } catch { Write-Host "  AUTENTICACAO FALHOU: $(Get-ErroJira $_)" -ForegroundColor Red; exit 1 }

    try {
        $pj = Invoke-RestMethod -Method Get -Uri "$base/rest/api/3/project/$($carga.projeto)" -Headers $cab -ErrorAction Stop
        Write-Host "  projecto: $($pj.key) - $($pj.name) (estilo $($pj.style))" -ForegroundColor Green
    } catch { Write-Host "  PROJECTO '$($carga.projeto)' INACESSIVEL: $(Get-ErroJira $_)" -ForegroundColor Red; exit 1 }

    $tiposNoJira = @{}
    try {
        $r = Invoke-RestMethod -Method Get -Uri "$base/rest/api/3/issue/createmeta/$($carga.projeto)/issuetypes" -Headers $cab -ErrorAction Stop
        foreach ($t in (Arr $r.issueTypes)) { if (-not $t.subtask) { $tiposNoJira[$t.name] = $t.id } }
        Write-Host "  tipos disponiveis: $(($tiposNoJira.Keys | Sort-Object) -join ', ')"
    } catch { Write-Host "  nao foi possivel listar tipos: $(Get-ErroJira $_)" -ForegroundColor DarkYellow }

    $tiposUsados = @(@($carga.issues) | ForEach-Object { $_.campos.issuetype } | Sort-Object -Unique)
    Write-Host "  tipos exigidos pela carga: $($tiposUsados -join ', ')"
    foreach ($t in $tiposUsados) {
        if ($tiposNoJira.Count -and -not $tiposNoJira.ContainsKey($t)) {
            Write-Host "    NAO EXISTE no projecto: '$t' -> ajustar tiposDeIssue em config/jira-mapping.json" -ForegroundColor Red
        }
    }

    foreach ($t in $tiposUsados) {
        if (-not $tiposNoJira.ContainsKey($t)) { continue }
        try {
            $cf = Invoke-RestMethod -Method Get -Uri "$base/rest/api/3/issue/createmeta/$($carga.projeto)/issuetypes/$($tiposNoJira[$t])" -Headers $cab -ErrorAction Stop
            $obrig = @(Arr $cf.fields | Where-Object { $_.required -and $_.fieldId -notin @('project', 'issuetype', 'summary', 'reporter') })
            if ($obrig.Count) {
                Write-Host "    '$t' exige ainda: $((@($obrig | ForEach-Object { "$($_.name) [$($_.fieldId)]" })) -join ', ')" -ForegroundColor Yellow
            } else {
                Write-Host "    '$t' nao exige campos alem dos que a carga ja envia" -ForegroundColor Green
            }
        } catch { Write-Host "    nao foi possivel ler campos de '$t': $(Get-ErroJira $_)" -ForegroundColor DarkYellow }
    }
    Write-Host 'Diagnostico terminado. Nada foi escrito.'
    exit 0
}

function Find-PorChave {
    param([string]$Chave)
    $jql = "project = `"$($carga.projeto)`" AND labels = `"card:$Chave`""
    $u = "$base/rest/api/3/search/jql?jql=$([Uri]::EscapeDataString($jql))&maxResults=1&fields=key"
    try {
        $r = Invoke-RestMethod -Method Get -Uri $u -Headers $cab -ErrorAction Stop
        if (@($r.issues).Count -gt 0) { return $r.issues[0].key }
    } catch {
        Write-Host "    aviso: pesquisa falhou para $Chave - $($_.Exception.Message)" -ForegroundColor DarkYellow
    }
    return $null
}

# A correspondencia id-do-card -> chave do Jira so existe do lado do Jira. Guardada
# aqui, deixa de ser preciso reconsultar para ligar um card a issue que o representa.
$chavesPath = Join-Path (Split-Path -Parent $PayloadPath) 'chaves.json'
function Write-Chaves {
    param([hashtable]$Mapa)
    if ($Mapa.Count -eq 0) { return }
    $doc = [ordered]@{
        '$schema' = 'sefaz-sp/tibco-intermediate/jira-chaves/v1'
        nota      = 'Correspondencia entre o id do card e a issue que o representa no Jira. Escrito pelo envio; lido por quem precise de apontar para o board.'
        baseUrl   = $base
        projeto   = $carga.projeto
        issues    = [ordered]@{}
    }
    foreach ($k in ($Mapa.Keys | Sort-Object)) { $doc.issues[$k] = $Mapa[$k] }
    $doc | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $chavesPath -Encoding UTF8
    Write-Host ("  chaves.json actualizado ({0} issues)" -f $Mapa.Count) -ForegroundColor DarkGray
}

# Reconstroi o mapa sem escrever nada no board.
if ($Mapear) {
    $mapa = @{}
    foreach ($i in (Arr $carga.issues)) {
        $k = Find-PorChave -Chave $i.chaveExterna
        if ($k) { $mapa[$i.chaveExterna] = $k; Write-Host ("  {0} -> {1}" -f $i.chaveExterna, $k) }
        else { Write-Host ("  {0} -> ainda nao existe" -f $i.chaveExterna) -ForegroundColor DarkYellow }
    }
    Write-Chaves -Mapa $mapa
    Write-Host 'Mapeamento terminado. Nada foi escrito no Jira.'
    exit 0
}

$chaveDe = @{}
$criadas = 0; $actualizadas = 0; $falhadas = 0; $ignoradas = 0

# O Jira Cloud identifica pessoas por accountId, nunca por e-mail.
$responsavelId = $null
if ($Confirmar -and $carga.responsavel) {
    if ($carga.responsavel.accountId) {
        $responsavelId = $carga.responsavel.accountId
    } elseif ($carga.responsavel.modo -eq 'eu') {
        try {
            $responsavelId = (Invoke-RestMethod -Method Get -Uri "$base/rest/api/3/myself" -Headers $cab -ErrorAction Stop).accountId
        } catch { Write-Host "    aviso: nao foi possivel resolver o responsavel - $(Get-ErroJira $_)" -ForegroundColor DarkYellow }
    }
}

# Estado na criacao nao existe: a issue nasce no primeiro estado do fluxo e so
# depois se transiciona. Uma issue ja no estado certo nao precisa de nada.
function Set-Estado {
    param([string]$Chave, [string]$Estado)
    if (-not $Estado) { return }
    try {
        $actual = (Invoke-RestMethod -Method Get -Uri "$base/rest/api/2/issue/$Chave`?fields=status" -Headers $cab -ErrorAction Stop).fields.status.name
        if ($actual -eq $Estado) { return }
        $ts = (Invoke-RestMethod -Method Get -Uri "$base/rest/api/2/issue/$Chave/transitions" -Headers $cab -ErrorAction Stop).transitions
        $t = @($ts | Where-Object { $_.to.name -eq $Estado })[0]
        if (-not $t) {
            Write-Host ("    aviso: {0} esta em '{1}' e nao ha transicao directa para '{2}' (disponiveis: {3})" -f `
                $Chave, $actual, $Estado, ((@($ts | ForEach-Object { $_.to.name })) -join ', ')) -ForegroundColor DarkYellow
            return
        }
        $b = @{ transition = @{ id = $t.id } } | ConvertTo-Json -Depth 4
        Invoke-RestMethod -Method Post -Uri "$base/rest/api/2/issue/$Chave/transitions" -Headers $cab -Body $b -ErrorAction Stop | Out-Null
    } catch {
        Write-Host ("    aviso: nao foi possivel por {0} em '{1}' - {2}" -f $Chave, $Estado, (Get-ErroJira $_)) -ForegroundColor DarkYellow
    }
}

foreach ($i in (Arr $carga.issues)) {
    $existente = $null
    if ($Confirmar) { $existente = Find-PorChave -Chave $i.chaveExterna }

    # A chave fica registada mesmo ao ignorar: sem ela as ligacoes que apontam para
    # esta issue nao teriam como resolver a outra ponta.
    if ($SoNovos -and $existente) {
        $chaveDe[$i.chaveExterna] = $existente
        $ignoradas++
        Write-Host ("  ignorada    {0} -> {1}  (ja existe)" -f $i.chaveExterna, $existente) -ForegroundColor DarkGray
        continue
    }

    $campos = @{
        project     = @{ key = $carga.projeto }
        summary     = $i.campos.summary
        issuetype   = @{ name = $i.campos.issuetype }
        labels      = @($i.campos.labels)
        description = $i.campos.description
    }
    if ($responsavelId) { $campos.assignee = @{ id = $responsavelId } }
    $corpo = @{ fields = $campos } | ConvertTo-Json -Depth 10

    if (-not $Confirmar) {
        Write-Host ("  [simulado] {0}  {1}" -f $i.chaveExterna, $i.campos.summary)
        continue
    }
    try {
        if ($existente) {
            Invoke-RestMethod -Method Put -Uri "$base/rest/api/2/issue/$existente" -Headers $cab -Body $corpo -ErrorAction Stop | Out-Null
            $chaveDe[$i.chaveExterna] = $existente
            $actualizadas++
            Set-Estado -Chave $existente -Estado $carga.estadoInicial
            Write-Host ("  actualizada {0} -> {1}" -f $i.chaveExterna, $existente)
        } else {
            $r = Invoke-RestMethod -Method Post -Uri "$base/rest/api/2/issue" -Headers $cab -Body $corpo -ErrorAction Stop
            $chaveDe[$i.chaveExterna] = $r.key
            $criadas++
            Set-Estado -Chave $r.key -Estado $carga.estadoInicial
            Write-Host ("  criada      {0} -> {1}" -f $i.chaveExterna, $r.key)
        }
    } catch {
        $falhadas++
        $msg = Get-ErroJira $_
        Write-Host ("  FALHOU      {0}: {1}" -f $i.chaveExterna, $msg) -ForegroundColor Red
        # Credencial ou permissao nao melhora na issue seguinte: parar em vez de repetir 80 vezes.
        if ($msg -match 'authenticated|permission|doesn''t exist') {
            Write-Host '  ABORTADO: a falha e de acesso, nao desta issue. Correr -Diagnostico.' -ForegroundColor Red
            break
        }
    }
}

# As ligacoes so depois de tudo existir: uma delas aponta para um card que pode
# ainda nao ter sido criado quando a sua vez chegar.
$ligadas = 0
foreach ($l in (Arr $carga.ligacoes)) {
    if (-not $Confirmar) { continue }
    $de = $chaveDe[$l.de]; $para = $chaveDe[$l.para]
    if (-not $de -or -not $para) { continue }
    $corpo = @{ type = @{ name = $l.tipo }; inwardIssue = @{ key = $de }; outwardIssue = @{ key = $para } } | ConvertTo-Json -Depth 6
    try {
        Invoke-RestMethod -Method Post -Uri "$base/rest/api/2/issueLink" -Headers $cab -Body $corpo -ErrorAction Stop | Out-Null
        $ligadas++
    } catch {
        Write-Host ("  ligacao falhou {0} -> {1}: {2}" -f $l.de, $l.para, (Get-ErroJira $_)) -ForegroundColor DarkYellow
    }
}

if ($Confirmar) {
    Write-Chaves -Mapa $chaveDe
    Write-Host ("Feito: {0} criada(s), {1} actualizada(s), {2} ignorada(s), {3} falhada(s), {4} ligacao(oes)." -f $criadas, $actualizadas, $ignoradas, $falhadas, $ligadas)
    if ($SoNovos -and $ignoradas -gt 0) {
        Write-Host "  -SoNovos activo: as issues ja existentes nao foram alteradas." -ForegroundColor DarkGray
    }
} else {
    Write-Host 'Simulacao terminada. Nada foi escrito.'
}
