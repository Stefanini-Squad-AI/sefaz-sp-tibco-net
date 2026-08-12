<#
.SYNOPSIS
    S2.5 - projecta os cards em carga pronta a entrar num board do Jira.

.DESCRIPTION
    O card ja e um objecto estruturado com id estavel; isto so o veste na forma que
    o Jira aceita. O mapa de campos e AUTORADO em config/jira-mapping.json, porque
    nomes de tipo de issue e ids de customfield variam de instancia para instancia
    e adivinha-los seria inventar.

    IDEMPOTENCIA. O id do card e a chave: reenviar o mesmo card tem de ACTUALIZAR e
    nunca duplicar. O Jira nao tem chave externa nativa, por isso ela viaja numa
    label 'card:<id>' e o envio procura por ela antes de criar. Sem isto, a segunda
    corrida do pipeline poe 80 issues novas por cima das 80 que ja la estavam.

    O ORACULO NAO VAI. Os valores esperados ficam nos ficheiros de fixture, fora do
    Jira. A issue aponta para o caminho; quem editar a descricao no Jira nao altera
    o que julga o codigo - e e essa a razao de ser da separacao.

    Saem tres coisas: issues.json e links.json para a via da API, e import.csv para
    a via do importador embutido, que nao suporta ligacoes mas nao precisa de
    credenciais nenhumas.
#>
[CmdletBinding()]
param(
    [string]$Package      = 'POC_Epat',
    [string]$ArtifactsDir = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$MappingPath  = "$PSScriptRoot/../config/jira-mapping.json",
    [string]$OutDir       = "$PSScriptRoot/../artifacts/POC_Epat/jira"
)

$ErrorActionPreference = 'Stop'

function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }
function Slug { param([string]$s) if (-not $s) { return '' }; return (($s -replace '[^A-Za-z0-9]+', '-').Trim('-').ToLowerInvariant()) }

$map     = Get-Content $MappingPath -Raw -Encoding UTF8 | ConvertFrom-Json
$backlog = Get-Content (Join-Path $ArtifactsDir 'backlog/index.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$conf    = Get-Content (Join-Path $ArtifactsDir 'conformance.json')   -Raw -Encoding UTF8 | ConvertFrom-Json
$cards   = @(Get-ChildItem (Join-Path $ArtifactsDir 'backlog') -Filter '*.json' -File |
    Where-Object { $_.Name -ne 'index.json' } |
    ForEach-Object { Get-Content $_.FullName -Raw -Encoding UTF8 | ConvertFrom-Json })

$proj = $map.projeto.key

# ------------------------------------------------------------------ epicas ---

# Uma epica por etapa do plano do cliente, mais uma de fundacao. E a unica
# agregacao que o cliente reconhece: ele nao pediu nos, pediu sete etapas.
$epicas = @(foreach ($et in (Arr $conf.etapas)) {
    [ordered]@{
        chaveExterna = "EPIC-etapa-$($et.n)"
        summary = "Etapa $($et.n) - $($et.name)"
        descricao = "Etapa $($et.n) do cenario de sete passos do plano de cumprimento. Conceitos declarados no documento: $((Arr $et.conceptsInDocument) -join ', ')."
    }
})
$epicas += [ordered]@{
    chaveExterna = 'EPIC-fundacao'
    summary = 'Fundacao'
    descricao = 'Trabalho que os cards assumem existir e que nao nasce de nenhum passo do processo: camada anticorrupcao, ligacao ao motor, motor de regras, registo de processos, persistencia.'
}

# ------------------------------------------------------------------ issues ---

function Get-Descricao {
    param($C)
    $l = [System.Collections.Generic.List[string]]::new()
    $l.Add("*$($C.content.intent)*")
    $l.Add('')
    $l.Add("h3. Contexto")
    $l.Add($C.content.injectedContext.summary)
    if (@(Arr $C.content.injectedContext.hypotheses).Count -gt 0) {
        $l.Add('')
        $l.Add('h3. Hipoteses a confirmar')
        $l.Add('_Parecer de analise, nao facto. Confirmar antes de assumir._')
        foreach ($h in (Arr $C.content.injectedContext.hypotheses)) { $l.Add("* $h") }
    }
    if (@(Arr $C.content.checklist).Count -gt 0) {
        $l.Add('')
        $l.Add('h3. Passos')
        foreach ($i in (Arr $C.content.checklist)) {
            $extra = @()
            if ($i.entrouPor -and $i.entrouPor -ne 'fluxo') { $extra += "entra por *$($i.entrouPor)* - nao existe como transicao no XPDL" }
            if ($i.decideQue) { $extra += "decide: $($i.decideQue)" }
            if ($i.nota) { $extra += $i.nota }
            $l.Add("# ($($i.kind)) $($i.nome)$(if ($extra.Count) { ' -- ' + ($extra -join '; ') })")
        }
    }
    if (@(Arr $C.content.scaffold).Count -gt 0) {
        $l.Add('')
        $l.Add('h3. Onde aterra')
        foreach ($s in (Arr $C.content.scaffold)) { $l.Add("* {{$($s.path)}} -- _$($s.status)_ -- $($s.note)") }
    }
    $l.Add('')
    $l.Add('h3. Como se prova')
    $l.Add("Oraculo *$($C.acceptance.oracle.kind)*, $($C.acceptance.oracle.caseCount) caso(s), fixture {{$($C.acceptance.oracle.fixture)}}.")
    $l.Add('{panel:bgColor=#fffae6}Os valores esperados sao do kit e sao imutaveis. Ligar o arnes a fixture; nunca escrever nem editar um valor esperado.{panel}')
    foreach ($c in (Arr $C.acceptance.criteria)) { $l.Add("* $c") }
    if (@(Arr $C.gaps).Count -gt 0) {
        $l.Add('')
        $l.Add('h3. Bloqueadores')
        foreach ($g in (Arr $C.gaps)) { $l.Add("* *$($g.construct)* ($($g.status)) -- $($g.detail)") }
    }
    $l.Add('')
    $l.Add("h3. Rasto")
    $l.Add("Card {{$($C.id)}} -- IR {{$($C.irRef.pointer)}} -- pacote {{$($C.provenance.package)}} -- manifesto {{$($C.provenance.manifestSha256)}}")
    $l.Add('_Antes de implementar, reconferir o manifesto: se nao bater, o card tem de ser regerado e nao implementado._')
    return ($l -join "`n")
}

$issues = @(foreach ($c in ($cards | Sort-Object id)) {
    $labels = @(Arr $map.labels.sempre)
    $labels += "$($map.campos.idDoCard.prefixo)$($c.id)"
    if ($map.labels.porTipo)  { $labels += "tipo:$($c.cardType)" }
    if ($map.labels.porPeso)  { $labels += "peso:$($c.dimensao.peso)" }
    if ($map.labels.porEtapa) { foreach ($e in (Arr $c.fulfills.etapas)) { $labels += "etapa:$e" } }
    if ($map.labels.porBloqueador) { foreach ($g in (Arr $c.gaps)) { $labels += "bloq:$($g.construct)" } }

    $etapa = @(Arr $c.fulfills.etapas)
    $epica = $(if ($c.epic -eq 'fundacao' -or $etapa.Count -eq 0) { 'EPIC-fundacao' } else { "EPIC-etapa-$($etapa[0])" })

    $campos = [ordered]@{
        summary = "[$($c.id)] $($c.title)"
        issuetype = $map.tiposDeIssue.($c.cardType)
        labels = @($labels | Sort-Object -Unique)
        description = (Get-Descricao $c)
    }
    if ($map.campos.storyPoints.campo) { $campos[$map.campos.storyPoints.campo] = [int]$c.dimensao.nos }
    foreach ($cp in (Arr $map.campos.camposProprios)) {
        $v = switch ($cp.valorDe) {
            'id'        { $c.id }
            'cardType'  { $c.cardType }
            'etapas'    { (Arr $c.fulfills.etapas) -join ',' }
            'peso'      { $c.dimensao.peso }
            'oraculo'   { $c.acceptance.oracle.kind }
            'casos'     { [int]$c.acceptance.oracle.caseCount }
            'processo'  { $c.scope.process }
            default     { $null }
        }
        if ($null -ne $v) { $campos[$cp.campo] = $v }
    }

    [ordered]@{
        chaveExterna = $c.id
        projeto = $proj
        epicaExterna = $epica
        campos = $campos
    }
})

$links = @(foreach ($c in ($cards | Sort-Object id)) {
    foreach ($d in (Arr $c.dependsOn)) {
        [ordered]@{ tipo = $map.ligacoes.dependsOn; de = $d; para = $c.id
            nota = 'Derivado da ordem dos segmentos na jornada, nao estimado.' }
    }
})

# ---------------------------------------------------------------- csv --------

# Via sem credenciais: o importador embutido do Jira. Nao leva ligacoes - por isso
# fica dito no proprio ficheiro, senao alguem importa e assume que ficou completo.
$csv = @($issues | ForEach-Object {
    [pscustomobject]@{
        'Summary'     = $_.campos.summary
        'Issue Type'  = $_.campos.issuetype
        'Description' = $_.campos.description
        'Labels'      = (@($_.campos.labels) -join ' ')
        'Epic Name'   = $_.epicaExterna
        'External ID' = $_.chaveExterna
    }
})

if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }
Get-ChildItem $OutDir -File -ErrorAction SilentlyContinue | Remove-Item -Force

$carga = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/jira-payload/v1'
    package = $Package
    nota = 'Carga para o Jira. A chave de idempotencia e a label card:<id> - o envio procura por ela antes de criar, senao a segunda corrida duplica tudo. Os valores esperados NAO estao aqui: ficam nas fixtures, e a issue so aponta para o caminho.'
    generatedAt = $backlog.generatedAt
    manifestSha256 = $backlog.manifestSha256
    projeto = $proj
    mapaDeCampos = 'config/jira-mapping.json'
    summary = [ordered]@{
        epicas = @($epicas).Count
        issues = @($issues).Count
        ligacoes = @($links).Count
        porTipo = [ordered]@{}
    }
    epicas = @($epicas)
    issues = @($issues)
    ligacoes = @($links)
}
foreach ($g in ($cards | Group-Object cardType | Sort-Object Name)) { $carga.summary.porTipo[$g.Name] = $g.Count }

$carga | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $OutDir 'payload.json') -Encoding UTF8
$csv | Export-Csv -LiteralPath (Join-Path $OutDir 'import.csv') -NoTypeInformation -Encoding UTF8

Write-Host ("Wrote {0}  ({1} epicas, {2} issues, {3} ligacao(oes); projecto {4})" -f `
    $OutDir, $carga.summary.epicas, $carga.summary.issues, $carga.summary.ligacoes, $proj)
if ($proj -eq 'EPAT') {
    Write-Host '    ATENCAO: a chave do projecto ainda e a de exemplo. Ajustar config/jira-mapping.json antes de enviar.' -ForegroundColor Yellow
}
Write-Host '    import.csv nao leva ligacoes - use payload.json com push-jira.ps1 para as criar.' -ForegroundColor DarkGray
