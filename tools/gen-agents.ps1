<#
.SYNOPSIS
    S2.2 - projecta os cards em manifestos de agente para a fase 2.

.DESCRIPTION
    O manifesto NAO faz raciocinio: delimita-o. O raciocinio acontece na fase 2,
    dentro do SAI APP 3.0, quando um agente pega num card e escreve C#. Isto aqui
    e a descricao de funcao de quem o vai escrever - e uma descricao de funcao e
    derivavel, ao contrario do trabalho.

    UM PAPEL E UM SITIO ONDE SE PODE ESCREVER. A fronteira do menor privilegio ja
    esta calculada: e o content.scaffold[] de cada card, que sai do mapa autorado
    em config/dotnet-architecture.json. Quem trata de Domain/Rules escreve em
    Domain/Rules e em mais lado nenhum - e a regra da dependencia entre camadas ja
    garante que nao consegue la chamar infraestrutura, porque nao compila.

    AS ESPECIALIDADES NAO SAO PAPEIS. Um bloqueador - a semantica dos builtins, o
    graft step - nao e um sitio onde se escreve, e conhecimento necessario onde se
    escreve. Entra como skill do papel, derivada dos gaps dos cards que ele trata.

    O QUE O AGENTE NUNCA PODE: escrever onde o card diz status 'final' (transcricao
    do WSDL ou do XPDL - reescrever quebra o contrato), tocar num oraculo
    (acceptance.oracle.immutable), ou decidir um gap por resolver (propoe, nunca
    decide).

    O elenco pode ser PROPOSTO por um agente de parecer, em analysis/backlog-review.json.
    Mas so o nome e a nota: as permissoes saem sempre das camadas e dos oraculos,
    nunca da proposta - senao quem esta a ser preso escolhia a corda.
#>
[CmdletBinding()]
param(
    [string]$Package      = 'POC_Epat',
    [string]$ArtifactsDir = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$MapPath      = "$PSScriptRoot/../config/dotnet-architecture.json",
    [string]$ReviewPath   = "$PSScriptRoot/../analysis/backlog-review.json",
    [string]$OutDir       = "$PSScriptRoot/../artifacts/POC_Epat/agents"
)

$ErrorActionPreference = 'Stop'

function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }
function Slug {
    param([string]$s)
    if (-not $s) { return 'x' }
    $d = $s.Normalize([Text.NormalizationForm]::FormD).ToCharArray()
    $keep = @($d | Where-Object { [Globalization.CharUnicodeInfo]::GetUnicodeCategory($_) -ne 'NonSpacingMark' })
    return (((-join $keep) -replace '[^A-Za-z0-9]+', '-').Trim('-').ToLowerInvariant())
}

$backlogDir = Join-Path $ArtifactsDir 'backlog'
if (-not (Test-Path (Join-Path $backlogDir 'index.json'))) { throw "backlog nao encontrado: $backlogDir" }
$idx  = Get-Content (Join-Path $backlogDir 'index.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$map  = Get-Content $MapPath -Raw -Encoding UTF8 | ConvertFrom-Json
$cards = @(Get-ChildItem $backlogDir -Filter '*.json' -File |
    Where-Object { $_.Name -ne 'index.json' } |
    ForEach-Object { Get-Content $_.FullName -Raw -Encoding UTF8 | ConvertFrom-Json })

$review = $null
$reviewSha = $null
if (Test-Path $ReviewPath) {
    $review = Get-Content $ReviewPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $reviewSha = (Get-FileHash -LiteralPath $ReviewPath -Algorithm SHA256).Hash.ToLowerInvariant()
}
$propostaDe = @{}
foreach ($e in (Arr $review.elenco)) { if ($e.id) { $propostaDe[$e.id] = $e } }

# --------------------------------------------------------- o que e proibido ---

# Caminho marcado 'final' e transcricao: reescrever quebra o contrato com o WSDL
# ou com o XPDL. Se um caminho e final nalgum card, e final para toda a gente.
$soLeitura = @{}
foreach ($c in $cards) {
    foreach ($s in (Arr $c.content.scaffold)) { if ($s.status -eq 'final') { $soLeitura[$s.path] = $s.note } }
}
$oraculos = @(@($cards | ForEach-Object { $_.acceptance.oracle.fixture }) | Sort-Object -Unique)

# --------------------------------------------------------------- os papeis ----

# Um papel por sitio onde ha corpo para escrever. Um card que atravessa camadas
# aparece em varios papeis: e o que significa atravessar camadas, e fica visivel
# em vez de implicito. Caminhos sob tests/ ficam de fora: sao sitio de arnes, e
# tem papel proprio - quem escreve o teste nao e quem escreve o que ele julga.
$porLocal = @{}
foreach ($c in $cards) {
    foreach ($s in (Arr $c.content.scaffold)) {
        if ($s.status -eq 'final') { continue }
        if ($s.path -like 'tests/*') { continue }
        if (-not $porLocal.ContainsKey($s.path)) {
            $porLocal[$s.path] = [ordered]@{ path = $s.path; nota = $s.note; cards = [System.Collections.Generic.List[object]]::new() }
        }
        $porLocal[$s.path].cards.Add($c)
    }
}

$papeis = [System.Collections.Generic.List[object]]::new()

# ------------------------------------------------- a fundacao que ninguem cobre

# Os cards nascem de NOS. Uma pasta da arquitectura que nao corresponda a nenhum
# tipo de no nunca recebe card - e se o scaffold tambem nao a preencher, fica
# trabalho real sem dono nenhum, invisivel em todos os relatorios de cobertura.
# E o caso do shim dos builtins iProcess, de que 29 cards dependem.
$fundacaoSemDono = @()
foreach ($l in $map.layers) {
    foreach ($f in $l.contains) {
        $path = "src/$($l.project)/$($f.folder)"
        if ($porLocal.ContainsKey($path)) { continue }
        if ($soLeitura.ContainsKey($path)) { continue }
        $noScaffold = Join-Path $ArtifactsDir "scaffold/$path"
        $n = 0
        if (Test-Path $noScaffold) { $n = @(Get-ChildItem $noScaffold -Recurse -File -ErrorAction SilentlyContinue).Count }
        if ($n -gt 0) { continue }
        # O mecanismo audita o julgamento: se o parecer nao propos papel para esta
        # pasta, o buraco continua la e fica dito.
        $quem = @(Arr $review.elenco | Where-Object { $path -in @(Arr $_.cobre) } | ForEach-Object { $_.id })
        $fundacaoSemDono += [ordered]@{
            caminho = $path; camada = $l.name; oQueLaVive = $f.what
            cobertaPeloParecer = ($quem.Count -gt 0)
            papelProposto = $(if ($quem.Count -gt 0) { $quem[0] } else { $null })
        }
    }
}
$fundacaoOrfa = @($fundacaoSemDono | Where-Object { -not $_.cobertaPeloParecer })

foreach ($path in ($porLocal.Keys | Sort-Object)) {
    $g = $porLocal[$path]
    $seus = @($g.cards)
    $pasta = ($path -split '/')[-1]
    $proj  = ($path -split '/')[1]

    # A especialidade e conhecimento, nao sitio: vem dos gaps dos cards deste papel.
    $skills = @($seus | ForEach-Object { Arr $_.gaps | ForEach-Object { $_.construct } } | Sort-Object -Unique)
    $tiposDeNo = @($seus | ForEach-Object { Arr $_.content.checklist | ForEach-Object { $_.kind } } | Sort-Object -Unique)
    $oraculosDoPapel = @($seus | ForEach-Object { $_.acceptance.oracle.kind } | Sort-Object -Unique)
    $etapas = @($seus | ForEach-Object { Arr $_.fulfills.etapas } | Sort-Object -Unique)
    $atravessam = @($seus | Where-Object { $_.dimensao.peso -eq 'atravessa-o-sistema' })
    $porResolver = @($seus | ForEach-Object { Arr $_.gaps | Where-Object { $_.status -eq 'unresolved' } })

    $id = "implementador-$(Slug $pasta)"
    $nome = "Implementador de $pasta"
    $prop = $propostaDe[$id]
    if ($prop -and $prop.nome) { $nome = $prop.nome }

    $papeis.Add([ordered]@{
        id = $id
        papel = $nome
        tipo = 'implementador'
        porqueExiste = "Ha corpo por escrever em $path. $($g.nota)"
        escreveEm = @("$path/**")
        cards = @($seus | ForEach-Object { $_.id } | Sort-Object)
        etapasQueToca = @($etapas)
        skills = @($skills)
        tiposDeNoQueTrata = @($tiposDeNo)
        oraculosQueOJulgam = @($oraculosDoPapel)
        orcamento = [ordered]@{
            cards = $seus.Count
            nos = @($seus | ForEach-Object { [int]$_.dimensao.nos } | Measure-Object -Sum).Sum
            cardsQueAtravessamOSistema = $atravessam.Count
            nota = $(if ($atravessam.Count -gt 0) { "$($atravessam.Count) card(s) deste papel tocam 3 ou mais projectos: exigem coordenacao com outros papeis, nao trabalho isolado." } else { 'Todos os cards deste papel cabem dentro do seu proprio sitio.' })
        }
        escalaQuando = @(
            'Um gap com status unresolved aparece no card: propor, nunca decidir.',
            'O oraculo exige alterar um caminho marcado final: e sinal de que a leitura do card esta errada.',
            'O card atravessa o sistema e o troco depende de trabalho de outro papel ainda por fazer.'
        )
        gapsPorResolver = @($porResolver | ForEach-Object { $_.construct } | Sort-Object -Unique)
        propostoPeloParecer = [bool]$prop
    })
}

# Autor de oraculos: um por projecto de teste declarado no mapa. Liga o arnes a
# fixture e NUNCA escreve o valor esperado - seria o teste a corrigir o proprio exame.
foreach ($t in (Arr $map.tests)) {
    $destino = "tests/$($t.project)"
    # Quem o card nomeia explicitamente; senao, quem escreve nas camadas que este
    # projecto de teste cobre.
    $seus = @($cards | Where-Object { @(Arr $_.content.scaffold | Where-Object { $_.path -eq $destino }).Count -gt 0 })
    if ($seus.Count -eq 0) {
        $alvo = @($t.dependsOn)
        $seus = @($cards | Where-Object {
            $c = $_
            @(Arr $c.content.scaffold | Where-Object { $s = $_; @($alvo | Where-Object { $s.path -like "src/*.$_/*" }).Count -gt 0 }).Count -gt 0
        })
    }
    $papeis.Add([ordered]@{
        id = "autor-de-testes-$(Slug ($t.project -replace '^SefazSp\.Epat\.', ''))"
        papel = "Autor de testes de $($t.project -replace '^SefazSp\.Epat\.', '')"
        tipo = 'autor-de-testes'
        porqueExiste = $t.what
        escreveEm = @("tests/$($t.project)/**")
        cards = @($seus | ForEach-Object { $_.id } | Sort-Object)
        etapasQueToca = @($seus | ForEach-Object { Arr $_.fulfills.etapas } | Sort-Object -Unique)
        skills = @()
        tiposDeNoQueTrata = @()
        oraculosQueOJulgam = @($seus | ForEach-Object { $_.acceptance.oracle.kind } | Sort-Object -Unique)
        orcamento = [ordered]@{ cards = $seus.Count; nos = 0; cardsQueAtravessamOSistema = 0
            nota = 'O esforco esta em ligar o arnes, nao em decidir o que e correcto: isso ja esta na fixture.' }
        escalaQuando = @(
            'A fixture nao cobre o caso que o card descreve: e defeito do kit, nao do teste.',
            'O teste so passa se o valor esperado for alterado: parar e escalar.'
        )
        gapsPorResolver = @()
        propostoPeloParecer = $false
    })
}

# --------------------------------------------- papeis propostos pelo parecer --

# O agente PROPOE o papel; o gerador PROJECTA a permissao. O que ele pode escrever
# sai de 'cobre' cruzado com o mapa de camadas - nunca de um campo de permissao no
# parecer. Um papel proposto para um caminho que o mapa nao declara e recusado.
$caminhosDoMapa = @{}
foreach ($l in $map.layers) {
    foreach ($f in $l.contains) { $caminhosDoMapa["src/$($l.project)/$($f.folder)"] = $f.what }
}
$propostasRecusadas = @()
foreach ($e in (Arr $review.elenco)) {
    $cobre = @(Arr $e.cobre | Where-Object { $caminhosDoMapa.ContainsKey($_) })
    $foraDoMapa = @(Arr $e.cobre | Where-Object { -not $caminhosDoMapa.ContainsKey($_) })
    foreach ($x in $foraDoMapa) {
        $propostasRecusadas += [ordered]@{ papel = $e.id; caminho = $x
            porque = 'O caminho nao existe no mapa de camadas. O parecer nao cria arquitectura.' }
    }
    # Papel de coordenacao sem caminho proprio - o molde dos clones - continua a
    # valer como papel, mas nao ganha permissao de escrita nenhuma. Os processos
    # que declara agrupar dao-lhe cards; a CONTAGEM e calculada, nao declarada.
    $seusCards = @()
    if (@(Arr $e.cobreProcessos).Count -gt 0) {
        $seusCards = @($cards | Where-Object { $_.scope.process -in @(Arr $e.cobreProcessos) })
    }
    $papeis.Add([ordered]@{
        id = $e.id
        papel = $e.nome
        tipo = $(if ($cobre.Count -gt 0) { 'fundacao' } else { 'coordenacao' })
        porqueExiste = $e.porque
        escreveEm = @($cobre | ForEach-Object { "$_/**" })
        cards = @($seusCards | ForEach-Object { $_.id } | Sort-Object)
        etapasQueToca = @($seusCards | ForEach-Object { Arr $_.fulfills.etapas } | Sort-Object -Unique)
        skills = @($seusCards | ForEach-Object { Arr $_.gaps | ForEach-Object { $_.construct } } | Sort-Object -Unique)
        tiposDeNoQueTrata = @()
        oraculosQueOJulgam = @($seusCards | ForEach-Object { $_.acceptance.oracle.kind } | Sort-Object -Unique)
        orcamento = [ordered]@{
            cards = $seusCards.Count
            nos = @($seusCards | ForEach-Object { [int]$_.dimensao.nos } | Measure-Object -Sum).Sum
            cardsQueAtravessamOSistema = @($seusCards | Where-Object { $_.dimensao.peso -eq 'atravessa-o-sistema' }).Count
            nota = $(if ($seusCards.Count -gt 0) { "Agrupa $($seusCards.Count) card(s) de $(@(Arr $e.cobreProcessos).Count) processos que o pacote repete identicos: construir uma vez, parametrizar as restantes." } else { 'Papel proposto pelo parecer: nao nasce de cards, nasce de trabalho que os cards assumem existir.' })
        }
        escalaQuando = @(
            'A decisao de que este papel depende continua sem resposta humana.',
            'Um papel de implementacao precisa deste trabalho antes de poder comecar.'
        )
        gapsPorResolver = @(Arr $e.bloqueadoPor)
        propostoPeloParecer = $true
        parecer = [ordered]@{
            origem = 'analise agentica sobre o backlog - proposta, nao facto'
            ordem = $e.ordem
            riscoSeIgnorado = $e.riscoSeIgnorado
            atencao = $e.atencao
            confianca = $e.confianca
        }
    })
}

# Revisor: nao escreve codigo. Existe para que o julgamento tenha um sitio que
# nao seja o teclado de quem implementa.
$papeis.Add([ordered]@{
    id = 'revisor'
    papel = 'Revisor'
    tipo = 'revisor'
    porqueExiste = 'O oraculo diz se o comportamento esta certo; nao diz se o codigo esta no sitio certo nem se o card foi lido todo. E isso que se revê.'
    escreveEm = @()
    cards = @($cards | ForEach-Object { $_.id } | Sort-Object)
    etapasQueToca = @(1..7)
    skills = @()
    tiposDeNoQueTrata = @()
    oraculosQueOJulgam = @()
    orcamento = [ordered]@{ cards = $cards.Count; nos = 0; cardsQueAtravessamOSistema = @($cards | Where-Object { $_.dimensao.peso -eq 'atravessa-o-sistema' }).Count
        nota = 'Revê tudo, escreve nada. O parecer nunca bloqueia sozinho: o gate deterministico e o oraculo.' }
    escalaQuando = @(
        'Um card foi dado por feito com itens da checklist por tratar.',
        'Um caminho marcado final foi alterado.',
        'Um gap unresolved foi resolvido pelo agente em vez de escalado.'
    )
    gapsPorResolver = @()
    propostoPeloParecer = $false
})

# O aviso do parecer vai para o papel a que se dirige, rotulado como parecer.
$avisoDe = @{}
foreach ($v in (Arr $review.avisos)) { $avisoDe[$v.alvo] = $v }
foreach ($p in $papeis) {
    $v = $avisoDe[$p.id]
    if (-not $v) { continue }
    $p['parecer'] = [ordered]@{
        origem = 'analise agentica sobre o backlog - parecer, nao facto'
        aviso = $v.aviso
        confianca = $v.confianca
    }
}

# ------------------------------------------------ a ordem, ligada aos cards ---

# A ordem vem do parecer; as CONTAGENS vem daqui. Sem isto a sequencia era prosa
# solta: nao se sabia quantos cards caem em cada volta, que bloqueadores essa
# volta tem de resolver, nem o que fica a espera.
$papelPorId = @{}
foreach ($p in $papeis) { $papelPorId[$p.id] = $p }
$jaAtacados = @{}
$ordem = @(foreach ($s in (Arr $review.ordemDeAtaque.sequencia)) {
    $p = $papelPorId[$s.quem]
    if (-not $p) {
        [ordered]@{ passo = $s.passo; quem = $s.quem; porque = $s.porque
            aviso = 'Papel nomeado na ordem de ataque que nao existe no elenco. A ordem ficou desalinhada do backlog.' }
        continue
    }
    $seus = @(Arr $p.cards)
    $novos = @($seus | Where-Object { -not $jaAtacados.ContainsKey($_) })
    foreach ($id in $novos) { $jaAtacados[$id] = $true }
    $gaps = @(Arr $p.skills)
    # Um card noutro papel que carregue um bloqueador que esta volta resolve fica
    # a espera dela, mesmo que a topologia nao o diga.
    $desbloqueia = @($cards | Where-Object {
        $c = $_
        ($c.id -notin $seus) -and (@(Arr $c.gaps | Where-Object { $_.construct -in $gaps }).Count -gt 0)
    })
    [ordered]@{
        passo = $s.passo
        quem = $s.quem
        papel = $p.papel
        porque = $s.porque
        cardsNestaVolta = $novos.Count
        cardsJaAtacados = @($seus).Count - $novos.Count
        bloqueadoresQueResolve = @($gaps)
        cardsQueDesbloqueia = $desbloqueia.Count
        aindaPorAtacar = $cards.Count - $jaAtacados.Count
        escreveEm = @(Arr $p.escreveEm)
        confianca = $p.parecer.confianca
    }
})
$naoAtacados = @($cards | Where-Object { -not $jaAtacados.ContainsKey($_.id) })

# Ficar fora da ordem tem duas causas muito diferentes, e trata-las como uma so
# transformava um facto tranquilo num alarme: ou o card nao tem nada por escrever
# - todos os seus caminhos sao transcricao - ou a ordem simplesmente nao o cobre.
$foraDaOrdem = @(foreach ($c in $naoAtacados) {
    $porEscrever = @(Arr $c.content.scaffold | Where-Object { $_.status -ne 'final' })
    [ordered]@{
        id = $c.id; cardType = $c.cardType
        porque = $(if ($porEscrever.Count -eq 0) {
            'Nada por escrever: todos os caminhos deste card sao transcricao marcada final. Nao e lacuna da ordem.'
        } else { 'A ordem de ataque nao cobre nenhum papel que escreva nos caminhos deste card.' })
        lacunaDaOrdem = ($porEscrever.Count -gt 0)
    }
})
$lacunaReal = @($foraDaOrdem | Where-Object { $_.lacunaDaOrdem })

# --------------------------------------------------------------- proibicoes ---
$proibidoParaTodos = @(
    @($soLeitura.Keys | Sort-Object | ForEach-Object { [ordered]@{ caminho = "$_/**"; porque = "Transcricao: $($soLeitura[$_])" } }) +
    @($oraculos | ForEach-Object { [ordered]@{ caminho = $_; porque = 'Oraculo imutavel: o agente liga o arnes a fixture e nunca escreve o valor esperado.' } }) +
    @([ordered]@{ caminho = 'artifacts/**'; porque = 'Saida do kit deterministico. Um agente que a altere quebra a rastreabilidade ate a fonte.' }) +
    @([ordered]@{ caminho = 'config/**'; porque = 'Decisoes autoradas - escopo, arquitectura, glossario. O agente propoe; nao edita.' })
)

# ------------------------------------------------------------------ write ----

if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }
Get-ChildItem $OutDir -Filter '*.json' -File -ErrorAction SilentlyContinue | Remove-Item -Force

foreach ($p in $papeis) {
    $doc = [ordered]@{
        '$schema' = 'sefaz-sp/tibco-intermediate/agent-manifest/v1'
        package = $Package
        nota = 'Manifesto de agente. NAO contem raciocinio: delimita-o. O que este agente pode escrever sai do mapa de camadas e do estado dos caminhos nos cards, nunca de uma proposta. O que o julga e o oraculo, que ele nao pode editar.'
        generatedAt = $idx.generatedAt
        manifestSha256 = $idx.manifestSha256
    }
    foreach ($k in $p.Keys) { $doc[$k] = $p[$k] }
    $doc['naoEscreveEm'] = @($proibidoParaTodos)
    $doc['le'] = @(
        [ordered]@{ caminho = 'context/**'; porque = 'A IR, os diagramas e as tabelas. So leitura: e o corpus de referencia.' }
        [ordered]@{ caminho = 'backlog/**'; porque = 'O card que lhe foi atribuido e as suas dependencias.' }
    )
    $doc | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $OutDir "$($p.id).json") -Encoding UTF8
}

$semDono = @($cards | Where-Object {
    $c = $_
    @($papeis | Where-Object { $_.tipo -ne 'revisor' -and ($c.id -in @($_.cards)) }).Count -eq 0
})

$index = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/agents/v1'
    package = $Package
    nota = 'Elenco derivado do backlog. Um papel e um SITIO onde se pode escrever - a fronteira do menor privilegio, que ja vem calculada do mapa de camadas. A especialidade nao e papel: e conhecimento, e entra como skill do papel que dela precisa. O parecer agentico pode propor nome e nota; nunca permissao.'
    generatedAt = $idx.generatedAt
    manifestSha256 = $idx.manifestSha256
    parecerAgentico = [ordered]@{
        ficheiro = 'analysis/backlog-review.json'
        presente = [bool]$review
        sha256 = $reviewSha
        elencoProposto = @(Arr $review.elenco).Count
        nota = 'Entrada fixada por sha256, como o XPDL e o glossario: se o parecer mudar, o elenco tem de ser regerado.'
    }
    ordemDeAtaque = [ordered]@{
        nota = $review.ordemDeAtaque.nota
        sequencia = @($ordem)
        cardsForaDaOrdem = @($foraDaOrdem)
    }
    propostasRecusadas = @($propostasRecusadas)
    summary = [ordered]@{
        papeis = $papeis.Count
        implementadores = @($papeis | Where-Object { $_.tipo -eq 'implementador' }).Count
        autoresDeTestes = @($papeis | Where-Object { $_.tipo -eq 'autor-de-testes' }).Count
        propostosPeloParecer = @($papeis | Where-Object { $_.propostoPeloParecer }).Count
        propostasRecusadas = $propostasRecusadas.Count
        cards = $cards.Count
        cardsSemDono = $semDono.Count
        caminhosSoLeitura = $soLeitura.Count
        oraculosImutaveis = @($oraculos).Count
        fundacaoSemDono = $fundacaoSemDono.Count
        fundacaoSemPapelProposto = $fundacaoOrfa.Count
        cardsForaDaOrdemDeAtaque = $naoAtacados.Count
        cardsForaDaOrdemPorLacuna = $lacunaReal.Count
    }
    fundacaoSemDono = @($fundacaoSemDono)
    cardsSemDono = @($semDono | ForEach-Object { [ordered]@{ id = $_.id; cardType = $_.cardType } })
    papeis = @($papeis | ForEach-Object {
        [ordered]@{
            id = $_.id; papel = $_.papel; tipo = $_.tipo
            escreveEm = @($_.escreveEm); cards = @($_.cards).Count
            etapas = @($_.etapasQueToca); skills = @($_.skills)
            atravessamOSistema = $_.orcamento.cardsQueAtravessamOSistema
            propostoPeloParecer = [bool]$_.propostoPeloParecer
        }
    })
}
$index | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $OutDir 'index.json') -Encoding UTF8

Write-Host ("Wrote {0}  ({1} papeis: {2} implementadores, {3} autores de testes, 1 revisor; {4} caminho(s) so de leitura, {5} oraculo(s) imutavel(eis))" -f `
    $OutDir, $index.summary.papeis, $index.summary.implementadores, $index.summary.autoresDeTestes,
    $index.summary.caminhosSoLeitura, $index.summary.oraculosImutaveis)
if ($semDono.Count -gt 0) {
    Write-Host ("    {0} card(s) sem papel que os trate - ver index.json > cardsSemDono" -f $semDono.Count) -ForegroundColor Yellow
}
if ($fundacaoSemDono.Count -gt 0) {
    Write-Host ("    {0} pasta(s) da arquitectura sem card E sem scaffold; {1} continua(m) sem papel proposto - ver index.json > fundacaoSemDono" -f `
        $fundacaoSemDono.Count, $fundacaoOrfa.Count) -ForegroundColor $(if ($fundacaoOrfa.Count -gt 0) { 'Red' } else { 'Yellow' })
}
if ($lacunaReal.Count -gt 0) {
    Write-Host ("    {0} card(s) com trabalho por escrever que a ordem de ataque nao cobre - ver index.json > ordemDeAtaque.cardsForaDaOrdem" -f $lacunaReal.Count) -ForegroundColor Red
}
