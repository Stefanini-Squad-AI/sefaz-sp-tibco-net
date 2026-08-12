<#
.SYNOPSIS
    S1.13 - enumera as jornadas do processo e emite-as como oraculos de percurso.

.DESCRIPTION
    O schema do card exige um oraculo em acceptance.oracle. Tres dos quatro tipos
    ja sao deriváveis; o quarto - scenario-path - faltava, e e ele que bloqueia os
    cards de build. Sem oraculo, o corpo escrito na fase 2 nao tem quem o julgue.

    O GRAFO DO PACOTE ESTA CORTADO EM 32 SITIOS e tem de ser recosido antes de se
    poder falar de jornada:
      - 10 arestas de link (throw/catch), que o XPDL nao declara como transicao;
      -  7 eventos de fronteira, que se agarram a tarefa em vez de ter aresta;
      -  2 arestas de sinal entre ramos paralelos;
      -  5 descidas para ActivitySet, declaradas por referencia de escopo;
      -  8 chamadas de processo, 3 delas resolvidas por ProcessInterface.
    Percorrer sem elas produz FRAGMENTOS - percursos que comecam a meio porque o
    predecessor esta do outro lado de um corte - e nao jornadas.

    Recosidos os cortes INTERNOS ao processo (link e ActivitySet), cada processo
    tem um grafo unico e cada percurso tem origem e destino reais.

    Os cortes ENTRE processos nao se costuram por inlining: o grafo de chamadas e
    uma arvore de 8 arestas e inline daria produto cartesiano de percursos. Ficam
    como CONTEXTO DE JORNADA: cada cenario declara por quem foi chamado, para onde
    regressa, e que cenarios do processo chamado continuam a partir de cada passo.
    A jornada completa le-se seguindo essas ligacoes, sem as enumerar todas.

    A enumeracao e DETERMINISTICA: a condicao de cada transicao ja esta extraida,
    portanto o conjunto de percursos e um facto do grafo, nao uma escolha.

    Ciclos: o grafo tem lacos de prazo e de retentativa. Enumerar todos os percursos
    seria infinito, por isso cortam-se os percursos SIMPLES (sem no repetido) e o
    corte fica registado. A cobertura de arestas e reportada a seguir, que e a
    medida honesta: percursos sao amostra, arestas sao o universo.
#>
[CmdletBinding()]
param(
    [string]$Package       = 'POC_Epat',
    [string]$ArtifactsDir  = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$OutDir        = "$PSScriptRoot/../artifacts/POC_Epat/scenarios",
    [int]   $MaxPerProcess = 120
)

$ErrorActionPreference = 'Stop'

function Read-Artifact {
    param([string]$Name, [switch]$Optional)
    $p = Join-Path $ArtifactsDir $Name
    if (-not (Test-Path $p)) { if ($Optional) { return $null }; throw "artifact not found: $p" }
    return Get-Content $p -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }

$model = Read-Artifact 'process-model.json'
$scope = Read-Artifact 'scope.json'       -Optional
$conf  = Read-Artifact 'conformance.json' -Optional

$inScopeProcess = @{}
foreach ($e in (Arr $scope.elements)) {
    if ($e.kind -eq 'process' -and $e.inScope) { $inScopeProcess[$e.id] = $true }
}
if ($inScopeProcess.Count -eq 0) { foreach ($p in $model.processes) { $inScopeProcess[$p.name] = $true } }

function Get-Label {
    param($Node)
    if (-not $Node) { return '(desconhecido)' }
    $l = $Node.displayName
    if (-not $l) { $l = $Node.name }
    if (-not $l) { $l = "$($Node.kind) $($Node.id)" }
    return $l
}

# "TIPOVISTAS=='JUIZ'" -> o que o cenario tem de por na entrada para este ramo disparar.
function Get-Constraints {
    param([string]$Condition)
    $out = [System.Collections.Generic.List[object]]::new()
    if (-not $Condition) { return @($out) }
    foreach ($m in [regex]::Matches($Condition, "([A-Z_][A-Z0-9_]*)\s*(==|!=|<=|>=|<|>)\s*('[^']*'|""[^""]*""|IPESystemValues\.SW_NA|-?\d+(?:\.\d+)?|true|false)")) {
        $v = $m.Groups[3].Value
        $sentinela = $false
        if ($v -match 'SW_NA') { $v = 'SW_NA'; $sentinela = $true }
        $out.Add([ordered]@{
            campo = $m.Groups[1].Value
            operador = $m.Groups[2].Value
            valor = $v.Trim("'", '"')
            sentinelaSwNa = $sentinela
        })
    }
    return @($out)
}

# ------------------------------------------------------------------ index ----

$whereOf = @{}
foreach ($p in $model.processes) {
    foreach ($s in $p.scopes) {
        foreach ($n in $s.nodes) { $whereOf[$n.id] = @{ process = $p.name; scope = $s.scope } }
    }
}

# --------------------------------------------------------- etapas por no -----

# A etapa vem do documento do cliente. intent-map casa o titulo contra nomes de
# PROCESSO; gen-conformance ancora tambem ao nivel do NO, que e o que resolve as
# etapas transversais. Aqui so se le o resultado.
$etapaOfNode    = @{}
$etapaOfProcess = @{}
foreach ($et in (Arr $conf.etapas)) {
    foreach ($an in (Arr $et.anchorNodes)) {
        if (-not $etapaOfNode.ContainsKey($an.nodeId)) { $etapaOfNode[$an.nodeId] = [System.Collections.Generic.List[int]]::new() }
        if ([int]$et.n -notin $etapaOfNode[$an.nodeId]) { $etapaOfNode[$an.nodeId].Add([int]$et.n) }
    }
    foreach ($pn in (Arr $et.processes)) {
        if (-not $etapaOfProcess.ContainsKey($pn)) { $etapaOfProcess[$pn] = [System.Collections.Generic.List[int]]::new() }
        if ([int]$et.n -notin $etapaOfProcess[$pn]) { $etapaOfProcess[$pn].Add([int]$et.n) }
    }
}

# ------------------------------------------------- arvore de invocacao -------

$callEdges = @(Arr $model.derived.callEdges)
$callerOf  = @{}
foreach ($ce in $callEdges) {
    if ($ce.kind -ne 'call' -or -not $ce.resolved -or -not $ce.toProcess) { continue }
    if (-not $callerOf.ContainsKey($ce.toProcess)) { $callerOf[$ce.toProcess] = [System.Collections.Generic.List[object]]::new() }
    $callerOf[$ce.toProcess].Add([ordered]@{
        processo = $ce.fromProcess; noId = $ce.fromNode; no = $ce.fromLabel
        dinamica = [bool]$ce.dynamic; resolvidaPor = $ce.resolvedVia
    })
}
$rootProcesses = @($model.processes | Where-Object { -not $callerOf.ContainsKey($_.name) } | ForEach-Object { $_.name })

# Cadeia de chamada desde a raiz: e isto que responde 'de onde vem'.
function Get-Chain {
    param([string]$Proc, $Seen)
    if (-not $Seen) { $Seen = @{} }
    if ($Seen.ContainsKey($Proc) -or -not $callerOf.ContainsKey($Proc)) { return @() }
    $Seen[$Proc] = $true
    $c = @($callerOf[$Proc])[0]
    return @(Get-Chain -Proc $c.processo -Seen $Seen) + @([ordered]@{ processo = $c.processo; no = $c.no; noId = $c.noId })
}

# A etapa desce pela arvore: um processo so alcancavel por dentro da etapa 2 faz
# parte da etapa 2, mesmo que o documento nunca o nomeie.
function Get-ProcessEtapas {
    param([string]$Proc, $Seen)
    if ($etapaOfProcess.ContainsKey($Proc)) { return @($etapaOfProcess[$Proc]) }
    if (-not $Seen) { $Seen = @{} }
    if ($Seen.ContainsKey($Proc) -or -not $callerOf.ContainsKey($Proc)) { return @() }
    $Seen[$Proc] = $true
    return @(Get-ProcessEtapas -Proc (@($callerOf[$Proc])[0].processo) -Seen $Seen)
}

# ------------------------------------------------------------------ walk -----

$scenarios = [System.Collections.Generic.List[object]]::new()
$coverage  = [System.Collections.Generic.List[object]]::new()
$stitchLog = [System.Collections.Generic.List[object]]::new()

foreach ($proc in $model.processes) {
    if (-not $inScopeProcess.ContainsKey($proc.name)) { continue }

    # --- grafo unico do processo: todos os escopos, mais as arestas recosidas ---
    $byId       = @{}
    $outOf      = @{}
    $allEdges   = [System.Collections.Generic.List[object]]::new()
    $scopeStart = @{}
    $scopeEnds  = @{}

    foreach ($s in $proc.scopes) {
        foreach ($n in $s.nodes) { $byId[$n.id] = $n }
        $st = @($s.nodes | Where-Object { $_.kind -eq 'startEvent' })
        if ($st.Count -gt 0) { $scopeStart[$s.scope] = $st[0].id }
        $scopeEnds[$s.scope] = @($s.nodes | Where-Object { $_.kind -eq 'endEvent' } | ForEach-Object { $_.id })
        foreach ($e in $s.edges) {
            $allEdges.Add([pscustomobject]@{
                id = $e.id; from = $e.from; to = $e.to; label = $e.label
                conditionType = $e.conditionType; condition = $e.condition
                via = 'fluxo'
            })
        }
    }

    $stitched = 0

    # link throw -> catch: GOTO implicito, ja decidido como aresta explicita (flatten-edge).
    foreach ($le in (Arr $model.derived.linkEdges)) {
        if ($le.process -ne $proc.name) { continue }
        if (-not $byId.ContainsKey($le.from) -or -not $byId.ContainsKey($le.to)) { continue }
        $allEdges.Add([pscustomobject]@{
            id = "LINK-$($le.from)-$($le.to)"; from = $le.from; to = $le.to; label = $le.name
            conditionType = 'UNCONDITIONAL'; condition = $null; via = 'link'
        })
        $stitched++
        $stitchLog.Add([ordered]@{ processo = $proc.name; tipo = 'link'; de = $le.fromLabel; para = $le.toLabel })
    }

    # ActivitySet: o no de escopo desce para dentro e regressa aos seus proprios sucessores.
    foreach ($ce in $callEdges) {
        if ($ce.kind -ne 'activitySet' -or $ce.fromProcess -ne $proc.name -or -not $ce.resolved) { continue }
        $hostId = $ce.fromNode
        $entra  = $scopeStart[$ce.toScope]
        if (-not $entra) { continue }
        $sucessores = @($allEdges | Where-Object { $_.from -eq $hostId -and $_.via -eq 'fluxo' })
        foreach ($e in $sucessores) { $e.via = 'substituida-por-descida' }
        $allEdges.Add([pscustomobject]@{
            id = "DESC-$hostId"; from = $hostId; to = $entra; label = 'entra no escopo'
            conditionType = 'UNCONDITIONAL'; condition = $null; via = 'descida'
        })
        $stitched++
        foreach ($fim in @($scopeEnds[$ce.toScope])) {
            foreach ($e in $sucessores) {
                $allEdges.Add([pscustomobject]@{
                    id = "REG-$fim-$($e.to)"; from = $fim; to = $e.to; label = $e.label
                    conditionType = $e.conditionType; condition = $e.condition; via = 'regresso'
                })
                $stitched++
            }
        }
        $stitchLog.Add([ordered]@{ processo = $proc.name; tipo = 'activitySet'; de = $ce.fromLabel; para = $ce.toScope })
    }

    # Evento de fronteira: nao tem aresta de entrada porque se agarra a tarefa. Sem
    # esta aresta o ramo de prazo aparece como um percurso que comeca do nada.
    foreach ($n in $byId.Values) {
        if (-not $n.boundary -or -not $n.attachedTo -or -not $byId.ContainsKey($n.attachedTo)) { continue }
        $allEdges.Add([pscustomobject]@{
            id = "FRONT-$($n.attachedTo)-$($n.id)"; from = $n.attachedTo; to = $n.id
            label = $(if ($n.interrupting -eq $false) { 'aviso, sem interromper' } else { 'interrompe a tarefa' })
            conditionType = 'UNCONDITIONAL'; condition = $null
            via = $(if ($n.interrupting -eq $false) { 'fronteira-paralela' } else { 'fronteira' })
        })
        $stitched++
        $stitchLog.Add([ordered]@{ processo = $proc.name; tipo = 'fronteira'; de = (Get-Label $byId[$n.attachedTo]); para = (Get-Label $n) })
    }

    # Sinal throw -> catch: difusao entre ramos paralelos, tambem ausente do XPDL
    # como transicao.
    foreach ($se in (Arr $model.derived.signalEdges)) {
        if ($se.process -ne $proc.name) { continue }
        if (-not $byId.ContainsKey($se.from) -or -not $byId.ContainsKey($se.to)) { continue }
        $allEdges.Add([pscustomobject]@{
            id = "SIG-$($se.from)-$($se.to)"; from = $se.from; to = $se.to; label = $se.name
            conditionType = 'UNCONDITIONAL'; condition = $null; via = 'sinal'
        })
        $stitched++
        $stitchLog.Add([ordered]@{ processo = $proc.name; tipo = 'sinal'; de = $se.fromLabel; para = $se.toLabel })
    }

    $liveEdges = @($allEdges | Where-Object { $_.via -ne 'substituida-por-descida' })
    foreach ($e in $liveEdges) {
        if (-not $outOf.ContainsKey($e.from)) { $outOf[$e.from] = [System.Collections.Generic.List[object]]::new() }
        $outOf[$e.from].Add($e)
    }

    # --- pontos de entrada, agora que o grafo esta ligado ---
    # Um startEvent de ActivitySet passa a ter predecessor depois de recoser, e por
    # isso deixa de ser entrada: entrada e o que continua sem predecessor nenhum.
    $incoming = @{}
    foreach ($e in $liveEdges) { $incoming[$e.to] = $true }
    $starts = @($byId.Values | Where-Object { -not $incoming.ContainsKey($_.id) })
    if ($starts.Count -eq 0) { $starts = @($byId.Values | Where-Object { $_.kind -eq 'startEvent' }) }
    if ($starts.Count -eq 0) { $starts = @($byId.Values | Select-Object -First 1) }

    $paths     = [System.Collections.Generic.List[object]]::new()
    $edgesSeen = @{}
    # Contadores partilhados com a funcao recursiva; hashtable para serem por referencia.
    $walkState = @{ Truncated = $false; LoopCuts = 0 }

    # Percursos SIMPLES: um no nao se repete no mesmo percurso. O laco fica
    # registado como corte, nao silenciado.
    function Walk {
        param($NodeId, $Visited, $PathNodes, $PathEdges)
        if ($paths.Count -ge $MaxPerProcess) { $walkState.Truncated = $true; return }
        $newNodes = @($PathNodes) + @($NodeId)
        $next = @(Arr $outOf[$NodeId])
        if ($next.Count -eq 0) {
            $paths.Add([pscustomobject]@{ Nodes = $newNodes; Edges = @($PathEdges); Cortado = $false })
            return
        }
        $avancou = $false
        foreach ($e in $next) {
            $edgesSeen[$e.id] = $true
            if ($Visited.ContainsKey($e.to)) { $walkState.LoopCuts++; continue }
            $avancou = $true
            $v = @{}
            foreach ($k in $Visited.Keys) { $v[$k] = $true }
            $v[$e.to] = $true
            Walk -NodeId $e.to -Visited $v -PathNodes $newNodes -PathEdges (@($PathEdges) + @($e))
            if ($paths.Count -ge $MaxPerProcess) { return }
        }
        # Todos os sucessores fecham laco: o percurso termina aqui, e termina por corte.
        if (-not $avancou) { $paths.Add([pscustomobject]@{ Nodes = $newNodes; Edges = @($PathEdges); Cortado = $true }) }
    }

    foreach ($s in $starts) {
        $v = @{}; $v[$s.id] = $true
        Walk -NodeId $s.id -Visited $v -PathNodes @() -PathEdges @()
    }

    # --- contexto de jornada do processo ---
    $chamadores = @(Arr $callerOf[$proc.name])
    $cadeia     = @(Get-Chain -Proc $proc.name)
    $etapasProc = @(Get-ProcessEtapas -Proc $proc.name)

    $i = 0
    foreach ($p in $paths) {
        $i++
        $decisions   = [System.Collections.Generic.List[object]]::new()
        $constraints = [System.Collections.Generic.List[object]]::new()
        foreach ($e in $p.Edges) {
            if (-not $e.condition -and $e.conditionType -ne 'OTHERWISE') { continue }
            $gw = $byId[$e.from]
            $cs = Get-Constraints $e.condition
            foreach ($c in $cs) { $constraints.Add($c) }
            $decisions.Add([ordered]@{
                decisao = (Get-Label $gw)
                decisaoId = $e.from
                ramo = $e.label
                tipo = $e.conditionType
                condicao = $e.condition
                leva = (Get-Label $byId[$e.to])
                exige = @($cs)
            })
        }

        # Passos que saem do processo: e por aqui que a jornada continua noutro ficheiro.
        $descidas = @(foreach ($nid in $p.Nodes) {
            $n = $byId[$nid]
            if ($n.kind -ne 'callActivity') { continue }
            $alvo = @($callEdges | Where-Object { $_.fromNode -eq $nid })
            [ordered]@{
                passo = (Get-Label $n); passoId = $nid
                continuaEm   = $(if ($alvo.Count -gt 0) { $alvo[0].toProcess } else { $null })
                resolvidaPor = $(if ($alvo.Count -gt 0) { $alvo[0].resolvedVia } else { $null })
                dinamica     = $(if ($alvo.Count -gt 0) { [bool]$alvo[0].dynamic } else { $false })
                graftStep    = [bool]$n.call.isGraftStep
                cenarios     = @()
                nota = $(if ($alvo.Count -gt 0 -and $alvo[0].resolved) { 'A jornada desce para este processo e regressa ao passo seguinte.' } else { 'Alvo fora do pacote entregue: a jornada continua num duble.' })
            }
        })

        $svcCalls = @(foreach ($nid in $p.Nodes) {
            $n = $byId[$nid]
            if ($n.kind -ne 'serviceTask') { continue }
            [ordered]@{ passo = (Get-Label $n); passoId = $n.id; operacao = $n.operation.name }
        })

        # --- origem e destino ---
        $primeiro = $byId[$p.Nodes[0]]
        $comoEntra = switch ($primeiro.kind) {
            'startEvent'  { $(if ($chamadores.Count -gt 0) { 'chamado-por' } else { 'inicio-do-caso' }) }
            'receiveTask' { 'evento-externo' }
            default       { 'sem-predecessor' }
        }
        $origem = [ordered]@{
            id = $primeiro.id; nome = (Get-Label $primeiro); tipo = $primeiro.kind
            como = $comoEntra
            chamadoPor = $(if ($comoEntra -eq 'chamado-por') { @($chamadores | ForEach-Object { "$($_.processo)/$($_.no)" }) } else { @() })
        }

        $ultimo = $byId[$p.Nodes[-1]]
        $comoTermina = 'sem-sucessor'
        if ($p.Cortado) { $comoTermina = 'corte-de-laco' }
        elseif ($ultimo.kind -eq 'endEvent') { $comoTermina = $(if ($chamadores.Count -gt 0) { 'regressa-ao-chamador' } else { 'fim-do-caso' }) }
        elseif ($ultimo.kind -eq 'callActivity' -and -not $ultimo.call.resolved) { $comoTermina = 'chamada-nao-entregue' }
        $destino = [ordered]@{
            id = $ultimo.id; nome = (Get-Label $ultimo); tipo = $ultimo.kind
            como = $comoTermina
            regressaA = $(if ($comoTermina -eq 'regressa-ao-chamador') { @($chamadores | ForEach-Object { "$($_.processo)/$($_.no)" }) } else { @() })
        }

        # --- etapa: uniao do que os nos ancoram com o que a arvore de chamada herda ---
        $etapasNoPercurso = [System.Collections.Generic.List[int]]::new()
        foreach ($nid in $p.Nodes) {
            foreach ($en in (Arr $etapaOfNode[$nid])) { if ($en -notin $etapasNoPercurso) { $etapasNoPercurso.Add([int]$en) } }
        }
        $etapas = @(@(@($etapasNoPercurso) + @($etapasProc)) | Sort-Object -Unique)
        $etapaOrigem = $(
            if ($etapasNoPercurso.Count -gt 0 -and $etapasProc.Count -gt 0) { 'no-do-percurso+arvore-de-chamada' }
            elseif ($etapasNoPercurso.Count -gt 0) { 'no-do-percurso' }
            elseif ($etapas.Count -gt 0) { 'herdada-da-arvore-de-chamada' }
            else { 'sem-etapa' })

        # --- segmentos: a etapa e um TROCO da jornada, nao um conjunto de nos solto ---
        # Cada ancora abre um troco que se fecha na ancora seguinte. Assim a etapa 3 e
        # a 5 deixam de ter a mesma pegada: passam a ser intervalos disjuntos.
        $segmentos = [System.Collections.Generic.List[object]]::new()
        $atual = $null
        for ($k = 0; $k -lt @($p.Nodes).Count; $k++) {
            $nid = $p.Nodes[$k]
            $anc = @(Arr $etapaOfNode[$nid] | Sort-Object -Unique)
            if ($anc.Count -gt 0 -and ($null -eq $atual -or (($anc -join ',') -ne ($atual.etapas -join ',')))) {
                $atual = [ordered]@{
                    etapas = @($anc); ordemNaJornada = $segmentos.Count + 1
                    doPasso = $k + 1; aoPasso = $k + 1
                    abrePor = (Get-Label $byId[$nid]); fechaEm = (Get-Label $byId[$nid])
                    nos = [System.Collections.Generic.List[object]]::new()
                }
                $segmentos.Add($atual)
            }
            if ($null -eq $atual) { continue }   # passos antes da primeira ancora nao tem dono
            $atual.aoPasso = $k + 1
            $atual.fechaEm = (Get-Label $byId[$nid])
            $atual.nos.Add([ordered]@{ id = $nid; nome = (Get-Label $byId[$nid]); tipo = $byId[$nid].kind })
        }
        $semDono = @($p.Nodes).Count - @($segmentos | ForEach-Object { @($_.nos).Count } | Measure-Object -Sum).Sum

        # Passos antes da primeira ancora: pertencem a etapa para onde levam, porque
        # nao existe etapa anterior a que possam pertencer. Fica rotulado como prologo
        # para nao se confundir com um troco que o documento delimita.
        if ($semDono -gt 0 -and $segmentos.Count -gt 0) {
            $ate = [int]$segmentos[0].doPasso - 1
            $prologo = [ordered]@{
                etapas = @($segmentos[0].etapas); ordemNaJornada = 0
                doPasso = 1; aoPasso = $ate
                abrePor = (Get-Label $byId[$p.Nodes[0]]); fechaEm = (Get-Label $byId[$p.Nodes[$ate - 1]])
                prologo = $true
                nos = [System.Collections.Generic.List[object]]::new()
            }
            for ($k = 0; $k -lt $ate; $k++) {
                $nid = $p.Nodes[$k]
                $prologo.nos.Add([ordered]@{ id = $nid; nome = (Get-Label $byId[$nid]); tipo = $byId[$nid].kind })
            }
            $segmentos.Insert(0, $prologo)
            $semDono = 0
        }

        # Processo chamado nao tem ancora propria: o troco inteiro pertence a etapa
        # do SITIO DE CHAMADA. E a mesma heranca da arvore, ao nivel do segmento.
        if ($segmentos.Count -eq 0 -and $cadeia.Count -gt 0) {
            $doSitio = @(Arr $etapaOfNode[$cadeia[-1].noId] | Sort-Object -Unique)
            if ($doSitio.Count -eq 0) { $doSitio = @($etapasProc) }
            if ($doSitio.Count -gt 0) {
                $segmentos.Add([ordered]@{
                    etapas = @($doSitio); ordemNaJornada = 1
                    doPasso = 1; aoPasso = @($p.Nodes).Count
                    abrePor = (Get-Label $byId[$p.Nodes[0]]); fechaEm = (Get-Label $byId[$p.Nodes[-1]])
                    herdadoDe = "$($cadeia[-1].processo)/$($cadeia[-1].no)"
                    herdadoDeNoId = $cadeia[-1].noId
                    nos = [System.Collections.Generic.List[object]]::new()
                })
                foreach ($nid in $p.Nodes) { $segmentos[0].nos.Add([ordered]@{ id = $nid; nome = (Get-Label $byId[$nid]); tipo = $byId[$nid].kind }) }
                $semDono = 0
            }
        }

        $kind = 'percurso'
        if ($ultimo.kind -eq 'endEvent') { $kind = 'ate-ao-fim' }
        if (@($p.Edges | Where-Object { $_.label -match 'AppError|Error' }).Count -gt 0) { $kind = 'erro' }
        if (@($p.Nodes | ForEach-Object { $byId[$_] } | Where-Object { $_.kind -eq 'timerEvent' }).Count -gt 0) { $kind = 'prazo' }

        # O percurso guarda tambem COMO se passou de um no ao seguinte: 'link' e
        # 'descida' nao existem no XPDL como transicao e tem de aparecer
        # explicitamente no fluxo .NET.
        $passos = [System.Collections.Generic.List[object]]::new()
        for ($k = 0; $k -lt @($p.Nodes).Count; $k++) {
            $n = $byId[$p.Nodes[$k]]
            $passos.Add([ordered]@{
                id = $n.id; nome = (Get-Label $n); tipo = $n.kind
                escopo = $whereOf[$n.id].scope
                entrouPor = $(if ($k -eq 0) { $null } else { @($p.Edges)[$k - 1].via })
            })
        }

        $scenarios.Add([ordered]@{
            id = "SC-$($proc.name)-$('{0:D3}' -f $i)"
            process = $proc.name
            etapa = $(if ($etapas.Count -gt 0) { [int]$etapas[0] } else { $null })
            etapas = @($etapas)
            etapaOrigem = $etapaOrigem
            kind = $kind
            representative = $null
            representativeNote = 'Qual destes percursos e o caminho normal e escolha humana: o pacote nao declara qual valor de dominio e o caso comum. Enquanto for null, nenhum card deve trata-lo como caminho feliz.'
            jornada = [ordered]@{
                raiz = $(if ($cadeia.Count -gt 0) { $cadeia[0].processo } else { $proc.name })
                profundidade = $cadeia.Count
                cadeia = @($cadeia | ForEach-Object { "$($_.processo)/$($_.no)" })
            }
            origem = $origem
            destino = $destino
            lengthNodes = @($p.Nodes).Count
            segmentos = @($segmentos | ForEach-Object { $_.nos = @($_.nos); $_ })
            passosSemEtapa = $semDono
            path = @($passos)
            decisions = @($decisions)
            inputsRequired = @($constraints | Group-Object { "$($_.campo)|$($_.operador)|$($_.valor)" } | ForEach-Object { $_.Group[0] })
            serviceCalls = @($svcCalls)
            descidas = @($descidas)
        })
    }

    $totalEdges = @($liveEdges).Count
    $coverage.Add([ordered]@{
        process = $proc.name
        nodes = @($byId.Keys).Count
        edges = $totalEdges
        edgesStitched = $stitched
        edgesCovered = @($edgesSeen.Keys).Count
        edgesUncovered = @($liveEdges | Where-Object { -not $edgesSeen.ContainsKey($_.id) } | ForEach-Object {
            [ordered]@{ id = $_.id; via = $_.via; de = (Get-Label $byId[$_.from]); para = (Get-Label $byId[$_.to]); condicao = $_.condition }
        })
        entryPoints = @($starts | ForEach-Object { [ordered]@{ nome = (Get-Label $_); tipo = $_.kind } })
        scenarios = $paths.Count
        loopCuts = $walkState.LoopCuts
        truncated = $walkState.Truncated
    })
}

# ------------------------------------------- ligar as jornadas entre ficheiros

$byProcess = @{}
foreach ($s in $scenarios) {
    if (-not $byProcess.ContainsKey($s.process)) { $byProcess[$s.process] = [System.Collections.Generic.List[string]]::new() }
    $byProcess[$s.process].Add($s.id)
}
foreach ($s in $scenarios) {
    foreach ($d in @($s.descidas)) {
        if ($d.continuaEm -and $byProcess.ContainsKey($d.continuaEm)) { $d.cenarios = @($byProcess[$d.continuaEm]) }
    }
}

# ------------------------------- afinar a heranca de etapa do processo chamado

# Na primeira passagem o filho herda as etapas do processo pai INTEIRO, o que e
# grosseiro: um subprocesso chamado a meio da etapa 2 nao pertence as seis etapas
# que o pai atravessa. Agora que todos os segmentos existem, herda-se do SEGMENTO
# onde o sitio de chamada esta - o que exige varias voltas, porque a arvore tem
# profundidade e um filho so pode herdar depois de o pai estar afinado.
$ancoradoNoNo = @{}
foreach ($s in $scenarios) {
    foreach ($seg in @($s.segmentos)) {
        if ($seg.herdadoDeNoId) { continue }   # so segmentos ancorados alimentam a heranca
        foreach ($no in @($seg.nos)) {
            if (-not $ancoradoNoNo.ContainsKey($no.id)) { $ancoradoNoNo[$no.id] = @{} }
            foreach ($e in @($seg.etapas)) { $ancoradoNoNo[$no.id][[int]$e] = $true }
        }
    }
}
for ($volta = 1; $volta -le 5; $volta++) {
    $mudou = $false
    foreach ($s in $scenarios) {
        foreach ($seg in @($s.segmentos)) {
            if (-not $seg.herdadoDeNoId) { continue }
            $novo = @()
            if ($ancoradoNoNo.ContainsKey($seg.herdadoDeNoId)) { $novo = @($ancoradoNoNo[$seg.herdadoDeNoId].Keys | Sort-Object) }
            if ($novo.Count -eq 0 -or (($novo -join ',') -eq (@($seg.etapas) -join ','))) { continue }
            $seg.etapas = @($novo)
            $seg.herdadoPor = 'segmento-do-sitio-de-chamada'
            $mudou = $true
            foreach ($no in @($seg.nos)) {
                if (-not $ancoradoNoNo.ContainsKey($no.id)) { $ancoradoNoNo[$no.id] = @{} }
                foreach ($e in $novo) { $ancoradoNoNo[$no.id][[int]$e] = $true }
            }
        }
    }
    if (-not $mudou) { break }
}
foreach ($s in $scenarios) {
    $u = @(@($s.segmentos | ForEach-Object { $_.etapas }) | ForEach-Object { [int]$_ } | Sort-Object -Unique)
    if ($u.Count -gt 0) { $s.etapas = @($u); $s.etapa = [int]$u[0] }
}

# ------------------------------------------------- pegada de cada etapa ------

# A pegada e a uniao dos nos dos SEGMENTOS da etapa, nao dos percursos que passam
# por ela. A diferenca nao e de detalhe: com o percurso inteiro, a etapa 3 e a 5
# ficam com o mesmo conjunto de nos e deixam de ser distinguiveis.
$dossier = Read-Artifact 'review-dossier.json' -Optional
$noEq = @(Arr $dossier.items | Where-Object { $_.category -eq 'no-net-equivalent' })

$footNodes = @{}      # etapa -> nodeId -> $true
$footScen  = @{}      # etapa -> id de cenario -> $true
$ordemVista = @{}     # etapa -> posicoes observadas na jornada
foreach ($s in $scenarios) {
    foreach ($seg in @($s.segmentos)) {
        foreach ($n in @($seg.etapas)) {
            $k = [int]$n
            if (-not $footNodes.ContainsKey($k)) { $footNodes[$k] = @{}; $footScen[$k] = @{}; $ordemVista[$k] = [System.Collections.Generic.List[int]]::new() }
            foreach ($no in @($seg.nos)) { $footNodes[$k][$no.id] = $true }
            $footScen[$k][$s.id] = $true
            $ordemVista[$k].Add([int]$seg.ordemNaJornada)
        }
    }
}

$etapaFootprint = @(foreach ($et in (Arr $conf.etapas)) {
    $k = [int]$et.n
    $nos = @(if ($footNodes.ContainsKey($k)) { $footNodes[$k].Keys } else { @() })
    $bloq = @(foreach ($it in $noEq) {
        $hits = @(Arr $it.occurrences | Where-Object { $footNodes.ContainsKey($k) -and $footNodes[$k].ContainsKey($_.nodeId) })
        if ($hits.Count -gt 0) {
            [ordered]@{ id = $it.id; ocorrenciasNaEtapa = $hits.Count; ocorrenciasNoPacote = @($it.occurrences).Count
                nos = @($hits | ForEach-Object { "$($_.process)/$($_.node)" } | Sort-Object -Unique) }
        }
    })
    $pos = @(if ($ordemVista.ContainsKey($k)) { $ordemVista[$k] } else { @() })
    [ordered]@{
        n = $k
        name = $et.name
        nos = $nos.Count
        cenarios = @(if ($footScen.ContainsKey($k)) { $footScen[$k].Keys } else { @() }).Count
        ancoras = @($et.anchorNodes | ForEach-Object { "$($_.process)/$($_.node)" })
        posicaoNaJornada = [ordered]@{
            minima = $(if ($pos.Count) { ($pos | Measure-Object -Minimum).Minimum } else { $null })
            maxima = $(if ($pos.Count) { ($pos | Measure-Object -Maximum).Maximum } else { $null })
        }
        bloqueadores = @($bloq)
        nodeIds = @($nos)
    }
})

# Duas etapas com o mesmo conjunto de nos sao indistinguiveis, e um card que
# declare cumprir uma delas nao prova nada sobre a outra.
$colisoes = @(
    for ($i = 0; $i -lt $etapaFootprint.Count; $i++) {
        for ($j = $i + 1; $j -lt $etapaFootprint.Count; $j++) {
            $a = @($etapaFootprint[$i].nodeIds); $b = @($etapaFootprint[$j].nodeIds)
            if ($a.Count -eq 0 -or $b.Count -eq 0) { continue }
            $inter = @($a | Where-Object { $_ -in $b }).Count
            $uniao = @(@($a) + @($b) | Sort-Object -Unique).Count
            if ($uniao -gt 0 -and ($inter / $uniao) -ge 0.9) {
                [ordered]@{ etapas = @($etapaFootprint[$i].n, $etapaFootprint[$j].n); sobreposicao = "$([Math]::Round(100 * $inter / $uniao))%" }
            }
        }
    }
)

# ------------------------------------------------------------------ write ----

if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }
Get-ChildItem $OutDir -Filter '*.json' -File -ErrorAction SilentlyContinue | Remove-Item -Force

foreach ($s in $scenarios) {
    $s | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutDir "$($s.id).json") -Encoding UTF8
}

# Measure-Object -Property nao enxerga chaves de [ordered]; somar a mao.
$totalEdges = 0; $coveredEdges = 0; $totalStitched = 0
foreach ($c in $coverage) { $totalEdges += [int]$c.edges; $coveredEdges += [int]$c.edgesCovered; $totalStitched += [int]$c.edgesStitched }

$index = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/scenarios/v1'
    package = $Package
    note = 'Jornadas enumeradas do grafo RECOSIDO - arestas de link e descidas para ActivitySet repostas -, uma por ficheiro, para servir de oraculo scenario-path aos cards de build. Cada cenario declara origem, destino e a cadeia de chamada desde a raiz, e cada passo de chamada aponta para os cenarios que continuam no processo chamado: e assim que se le a jornada de ponta a ponta sem a enumerar. A SELECCAO do percurso representativo continua a ser humana. Percursos sao simples (sem no repetido); os lacos ficam registados como cortes. A medida honesta de completude e a cobertura de arestas.'
    summary = [ordered]@{
        scenarios = $scenarios.Count
        processes = @($coverage).Count
        edges = $totalEdges
        edgesStitched = $totalStitched
        edgesCovered = $coveredEdges
        edgeCoverage = "$([Math]::Round(100 * $coveredEdges / [Math]::Max(1, $totalEdges)))%"
        semEtapa = @($scenarios | Where-Object { $null -eq $_.etapa }).Count
        etapaHerdada = @($scenarios | Where-Object { $_.etapaOrigem -eq 'herdada-da-arvore-de-chamada' }).Count
        nosSemSegmento = @($scenarios | ForEach-Object { [int]$_.passosSemEtapa } | Measure-Object -Sum).Sum
        representativeChosen = @($scenarios | Where-Object { $null -ne $_.representative }).Count
        byKind = [ordered]@{}
        byEtapa = [ordered]@{}
    }
    callTree = [ordered]@{
        roots = @($rootProcesses)
        edges = @($callEdges | Where-Object { $_.kind -eq 'call' } | ForEach-Object {
            [ordered]@{ de = "$($_.fromProcess)/$($_.fromLabel)"; para = $_.toProcess; dinamica = [bool]$_.dynamic; resolvidaPor = $_.resolvedVia }
        })
    }
    etapas = [ordered]@{
        nota = 'Pegada de cada etapa medida no SEGMENTO da jornada - da ancora da etapa ate a ancora da seguinte - e nao no percurso inteiro. Os bloqueadores sao os itens sem equivalente .NET cujo nodeId cai dentro dessa pegada, o que e uma medida exacta e nao herdada do processo. posicaoNaJornada diz em que ordem a etapa aparece de facto no fluxo, que pode nao ser a ordem do documento quando a jornada e de recuperacao.'
        footprint = @($etapaFootprint | ForEach-Object { $x = [ordered]@{}; foreach ($k in $_.Keys) { if ($k -ne 'nodeIds') { $x[$k] = $_[$k] } }; $x })
        colisoes = @($colisoes)
    }
    stitches = @($stitchLog)
    coverage = @($coverage)
    scenarios = @($scenarios | ForEach-Object {
        [ordered]@{
            id = $_.id; process = $_.process; etapa = $_.etapa; etapaOrigem = $_.etapaOrigem; kind = $_.kind
            de = $_.origem.nome; ate = $_.destino.nome; termina = $_.destino.como
            etapas = @($_.etapas)
            passos = $_.lengthNodes; decisoes = @($_.decisions).Count; entradas = @($_.inputsRequired).Count
        }
    })
}
foreach ($g in ($scenarios | Group-Object { $_.kind } | Sort-Object Name)) { $index.summary.byKind[$g.Name] = $g.Count }
# Um cenario que atravessa tres etapas conta nas tres: a pergunta a que isto
# responde e 'quantos cenarios exercitam a etapa N', nao 'a que etapa pertence'.
foreach ($n in 1..7) {
    $index.summary.byEtapa["etapa $n"] = @($scenarios | Where-Object { $n -in @($_.etapas) }).Count
}
$index.summary.byEtapa['sem-etapa'] = @($scenarios | Where-Object { @($_.etapas).Count -eq 0 }).Count

$index | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutDir 'index.json') -Encoding UTF8

$semCobertura = @($coverage | Where-Object { @($_.edgesUncovered).Count -gt 0 })
Write-Host ("Wrote {0}  ({1} cenarios em {2} processos; {3} arestas recosidas; cobertura {4}; {5} sem etapa; {6} representativos escolhidos)" -f `
    $OutDir, $index.summary.scenarios, $index.summary.processes, $totalStitched, $index.summary.edgeCoverage, `
    $index.summary.semEtapa, $index.summary.representativeChosen)
if ($semCobertura.Count -gt 0) {
    Write-Host ("    {0} processo(s) com aresta nao coberta - ver coverage.edgesUncovered" -f $semCobertura.Count) -ForegroundColor Yellow
}
