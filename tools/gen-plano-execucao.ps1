<#
.SYNOPSIS
    S2.6 - projecta o backlog num plano de execucao navegavel em HTML.

.DESCRIPTION
    A ordem NAO e inventada aqui. Sai de tres factos ja extraidos:
      1. dependsOn  - a ordem dos segmentos dentro da jornada, que da o nivel topologico
      2. fulfills.etapas - a etapa do plano de cumprimento a que o card responde
      3. cardType   - fundacao (duble e contrato) vem antes do que assenta nela

    O grafo de dependsOn e esparso de proposito: so liga segmentos consecutivos do
    mesmo cenario. Por isso a ordem final e por ONDAS - dentro de uma onda os cards
    sao independentes e podem correr em paralelo -, e nao uma fila unica que sugeria
    uma sequencia que os dados nao suportam.

    Saida: artifacts/<pacote>/plano-execucao.html, sem dependencias externas.
#>
[CmdletBinding()]
param(
    [string]$Package      = 'POC_Epat',
    [string]$ArtifactsDir = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$OutPath      = "$PSScriptRoot/../artifacts/POC_Epat/plano-execucao.html"
)

$ErrorActionPreference = 'Stop'

function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }
function Esc { param($t) if ($null -eq $t) { return '' }
    return ([string]$t) -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;' -replace '"', '&quot;' }

$idx  = Get-Content (Join-Path $ArtifactsDir 'backlog/index.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$conf = Get-Content (Join-Path $ArtifactsDir 'conformance.json')   -Raw -Encoding UTF8 | ConvertFrom-Json

# A chave do Jira nao e derivavel dos artefactos: e atribuida pelo board. Vem do
# ficheiro que o envio deixa. Sem ele o plano continua valido, apenas sem a coluna.
$chaveJira = @{}
$jiraBase = ''
$chavesPath = Join-Path $ArtifactsDir 'jira/chaves.json'
if (Test-Path $chavesPath) {
    $cj = Get-Content $chavesPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $jiraBase = [string]$cj.baseUrl
    foreach ($p in @($cj.issues.PSObject.Properties)) { $chaveJira[$p.Name] = $p.Value }
}

$cards = @($idx.cards)
$byId  = @{}; foreach ($c in $cards) { $byId[$c.id] = $c }

# Nivel topologico: 0 = nao espera por ninguem. E a unica ordem que os dados impoem.
$nivel = @{}
function Get-Nivel {
    param([string]$Id)
    if ($nivel.ContainsKey($Id)) { return $nivel[$Id] }
    $nivel[$Id] = 0
    $n = 0
    foreach ($d in (Arr $byId[$Id].dependsOn)) {
        if (-not $byId.ContainsKey($d)) { continue }
        $v = Get-Nivel $d
        if ($v + 1 -gt $n) { $n = $v + 1 }
    }
    $nivel[$Id] = $n
    return $n
}
foreach ($c in $cards) { [void](Get-Nivel $c.id) }

# Aresta inversa: quem fica a espera deste card. E o que distingue um card que so
# tem de ser feito de um card que TRAVA outros enquanto nao for feito.
$bloqueia = @{}
foreach ($c in $cards) { $bloqueia[$c.id] = [System.Collections.Generic.List[string]]::new() }
foreach ($c in $cards) {
    foreach ($d in (Arr $c.dependsOn)) {
        if ($bloqueia.ContainsKey($d)) { $bloqueia[$d].Add($c.id) }
    }
}

# Alcance transitivo: quantos cards, ao todo, so podem correr depois deste. Um card
# com alcance grande e um estrangulamento - atrasa-lo atrasa toda a cadeia atras.
$alcance = @{}
function Get-Alcance {
    param([string]$Id)
    if ($alcance.ContainsKey($Id)) { return $alcance[$Id] }
    $alcance[$Id] = 0
    $vistos = @{}
    $fila = [System.Collections.Generic.Queue[string]]::new()
    foreach ($f in $bloqueia[$Id]) { $fila.Enqueue($f) }
    while ($fila.Count -gt 0) {
        $x = $fila.Dequeue()
        if ($vistos.ContainsKey($x)) { continue }
        $vistos[$x] = $true
        foreach ($f in $bloqueia[$x]) { $fila.Enqueue($f) }
    }
    $alcance[$Id] = $vistos.Count
    return $alcance[$Id]
}
foreach ($c in $cards) { [void](Get-Alcance $c.id) }

$totalTrava = @($cards | Where-Object { $bloqueia[$_.id].Count -gt 0 }).Count
$maxParalelo = 0

$nomeEtapa = @{}
foreach ($e in (Arr $conf.etapas)) { $nomeEtapa[[int]$e.n] = $e.name }

function Get-PrimeiraEtapa { param($C) $e = @(Arr $C.etapas | ForEach-Object { [int]$_ } | Sort-Object); if ($e.Count) { return $e[0] } return 99 }

# ------------------------------------------------------------------ ondas ----

$ondas = [System.Collections.Generic.List[object]]::new()

# A fundacao vem primeiro porque tudo o resto assenta nela: sem porta, sem duble e
# sem motor de regras, um card de build nao tem onde ligar.
$fundacao = @($cards | Where-Object { $_.cardType -eq 'double' -or ($_.cardType -eq 'validation' -and $_.epic -eq 'fundacao') })
$ondas.Add([ordered]@{
    n = 0
    titulo = 'Fundacao'
    porque = 'Camada anticorrupcao, portas de servico, dubles dos processos nao entregues e motor de decisao. Nenhum card de build tem onde assentar antes disto. Sao independentes entre si.'
    cards = @($fundacao | Sort-Object cardType, id)
})

$vistos = @{}
foreach ($c in $fundacao) { $vistos[$c.id] = $true }

# Depois, etapa a etapa: construir o troco e so entao prova-lo. A etapa e a unidade
# que o cliente reconhece - ele nao pediu nos, pediu sete etapas.
$ordem = 1
foreach ($n in 1..7) {
    $daEtapa = @($cards | Where-Object { -not $vistos[$_.id] -and (Get-PrimeiraEtapa $_) -eq $n })
    if ($daEtapa.Count -eq 0) { continue }
    $build = @($daEtapa | Where-Object { $_.cardType -eq 'build' } | Sort-Object @{ e = { $nivel[$_.id] } }, id)
    $prova = @($daEtapa | Where-Object { $_.cardType -ne 'build' } | Sort-Object id)
    foreach ($c in $daEtapa) { $vistos[$c.id] = $true }
    $ondas.Add([ordered]@{
        n = $ordem
        titulo = "Etapa $n - $($nomeEtapa[$n])"
        porque = "Construir os trocos da jornada desta etapa e, no fim, prova-la por rasto de instancia. Dentro da onda, a coluna 'espera por' diz o que nao pode comecar antes."
        cards = @($build + $prova)
    })
    $ordem++
}

$resto = @($cards | Where-Object { -not $vistos[$_.id] } | Sort-Object cardType, id)
if ($resto.Count) {
    $ondas.Add([ordered]@{
        n = $ordem
        titulo = 'Conceitos transversais'
        porque = 'Provas que atravessam mais do que uma etapa e por isso so fecham quando as etapas envolvidas estiverem construidas.'
        cards = $resto
    })
}

# O maior lote diz quanta gente pode trabalhar ao mesmo tempo sem pisar dependencia.
foreach ($o in $ondas) {
    foreach ($g in @($o.cards | Group-Object { $nivel[$_.id] })) {
        if (@($g.Group).Count -gt $maxParalelo) { $maxParalelo = @($g.Group).Count }
    }
}

# ------------------------------------------------------------------- html ----

$corDoTipo = @{ build = 'tp-build'; validation = 'tp-valid'; double = 'tp-double' }
$totalBloq = @($cards | Where-Object { @(Arr $_.bloqueadores).Count -gt 0 }).Count

$h = [System.Collections.Generic.List[string]]::new()
$h.Add('<!DOCTYPE html>')
$h.Add('<html lang="pt-BR"><head><meta charset="utf-8">')
$h.Add('<meta name="viewport" content="width=device-width, initial-scale=1">')
$h.Add("<title>Plano de execucao - $(Esc $Package)</title>")
$h.Add(@'
<style>
:root{--bg:#0f1115;--card:#171a21;--line:#262b36;--txt:#e6e9ef;--dim:#9aa3b2;--acc:#4c9aff;--warn:#f5a623;--bad:#ff6b6b;--ok:#3ddc97}
*{box-sizing:border-box}
body{margin:0;background:var(--bg);color:var(--txt);font:14px/1.5 -apple-system,Segoe UI,Roboto,sans-serif}
header{padding:28px 32px;border-bottom:1px solid var(--line);background:linear-gradient(180deg,#171a21,#0f1115)}
h1{margin:0 0 6px;font-size:22px}
.sub{color:var(--dim);font-size:13px}
.kpis{display:flex;gap:12px;flex-wrap:wrap;margin-top:16px}
.kpi{background:var(--card);border:1px solid var(--line);border-radius:8px;padding:10px 14px;min-width:104px}
.kpi b{display:block;font-size:20px}
.kpi span{color:var(--dim);font-size:11px;text-transform:uppercase;letter-spacing:.5px}
main{padding:24px 32px;max-width:1500px}
.bar{display:flex;gap:10px;align-items:center;margin-bottom:18px;flex-wrap:wrap}
input[type=search]{background:var(--card);border:1px solid var(--line);color:var(--txt);border-radius:6px;padding:8px 12px;min-width:280px}
button{background:var(--card);border:1px solid var(--line);color:var(--txt);border-radius:6px;padding:8px 12px;cursor:pointer}
button.on{border-color:var(--acc);color:var(--acc)}
.onda{margin-bottom:28px;border:1px solid var(--line);border-radius:10px;overflow:hidden;background:var(--card)}
.onda>h2{margin:0;padding:14px 18px;font-size:15px;background:#1c2029;border-bottom:1px solid var(--line);display:flex;gap:10px;align-items:center}
.n{background:var(--acc);color:#05070c;border-radius:5px;padding:1px 9px;font-size:12px;font-weight:700}
.porque{padding:11px 18px;color:var(--dim);font-size:12.5px;border-bottom:1px solid var(--line)}
table{width:100%;border-collapse:collapse}
th{text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:.5px;color:var(--dim);padding:9px 12px;border-bottom:1px solid var(--line);font-weight:600}
td{padding:9px 12px;border-bottom:1px solid #1e222b;vertical-align:top}
tr:last-child td{border-bottom:0}
tr.hide{display:none}
code{font:12px ui-monospace,Consolas,monospace;color:var(--acc)}
.t{font-size:13px}
.tag{display:inline-block;font-size:10.5px;padding:1px 7px;border-radius:4px;border:1px solid var(--line);color:var(--dim);white-space:nowrap}
.tp-build{border-color:#2f6bd6;color:#79aaff}
.tp-valid{border-color:#2f8f6b;color:#5fd6a8}
.tp-double{border-color:#8f6b2f;color:#e0b562}
.bl{border-color:#7a3030;color:var(--bad)}
.trava{border-color:#7a5a20;color:var(--warn);font-weight:700}
.lote{padding:8px 18px;background:#131720;border-bottom:1px solid var(--line);border-top:1px solid var(--line);color:var(--dim);font-size:12px}
.lt{display:inline-block;background:#232937;color:var(--txt);border-radius:4px;padding:1px 8px;margin-right:8px;font-size:11px;font-weight:700}
.jk{display:inline-block;font:12px ui-monospace,Consolas,monospace;font-weight:700;color:var(--ok);text-decoration:none;border:1px solid #2f8f6b;border-radius:4px;padding:1px 7px;white-space:nowrap}
a.jk:hover{background:#1b2b24}
.act{display:flex;gap:4px;margin-top:5px}
.act button{font-size:10px;padding:2px 6px;border-radius:4px;line-height:1.3}
.act button.sel{border-color:var(--acc);color:var(--acc);background:#16233a}
tr.marcada td{background:#141b28}
#sel{position:sticky;bottom:0;z-index:5;background:#1c2029;border-top:1px solid var(--acc);padding:12px 32px;display:none;gap:12px;align-items:center;flex-wrap:wrap;margin:0 -32px}
#sel.on{display:flex}
#sel code{color:var(--txt)}
.cmd{flex:1;min-width:320px;background:#0f1115;border:1px solid var(--line);border-radius:6px;padding:8px 10px;font:11.5px ui-monospace,Consolas,monospace;color:var(--ok);overflow-x:auto;white-space:nowrap}
.pz-atravessa-o-sistema{border-color:#7a5a20;color:var(--warn)}
.dim{color:var(--dim);font-size:12px}
.esp{font:11.5px ui-monospace,Consolas,monospace;color:var(--dim)}
footer{padding:18px 32px;color:var(--dim);font-size:12px;border-top:1px solid var(--line)}
</style>
'@)
$h.Add('</head><body>')

$h.Add('<header>')
$h.Add("<h1>Plano de execucao &mdash; $(Esc $Package)</h1>")
$h.Add('<div class="sub">A ordem e derivada do backlog: dependencia entre segmentos, etapa do plano de cumprimento e precedencia da fundacao. Nada aqui foi estimado.</div>')
$h.Add('<div class="kpis">')
$h.Add("<div class=""kpi""><b>$(@($cards).Count)</b><span>cards</span></div>")
$h.Add("<div class=""kpi""><b>$(@($cards | Where-Object { $_.cardType -eq 'build' }).Count)</b><span>build</span></div>")
$h.Add("<div class=""kpi""><b>$(@($cards | Where-Object { $_.cardType -eq 'validation' }).Count)</b><span>validacao</span></div>")
$h.Add("<div class=""kpi""><b>$(@($cards | Where-Object { $_.cardType -eq 'double' }).Count)</b><span>duble</span></div>")
$h.Add("<div class=""kpi""><b>$($ondas.Count)</b><span>ondas</span></div>")
$h.Add("<div class=""kpi""><b>$maxParalelo</b><span>maior lote paralelo</span></div>")
$h.Add("<div class=""kpi""><b>$totalTrava</b><span>travam outros</span></div>")
$h.Add("<div class=""kpi""><b>$totalBloq</b><span>com bloqueador</span></div>")
if ($chaveJira.Count -gt 0) { $h.Add("<div class=""kpi""><b>$($chaveJira.Count)</b><span>no board</span></div>") }
$h.Add('</div></header>')

$h.Add('<main>')
$h.Add('<div class="bar">')
$h.Add('<input type="search" id="q" placeholder="filtrar por id, titulo ou bloqueador...">')
$h.Add('<button id="fb">so com bloqueador</button>')
$h.Add('<button id="ft">so os que travam outros</button>')
$h.Add('<button id="fg">so os que atravessam o sistema</button>')
$h.Add('<span class="dim" id="cnt"></span>')
$h.Add('</div>')

foreach ($o in $ondas) {
    $h.Add('<section class="onda">')
    $h.Add('<h2><span class="n">Onda ' + $o.n + '</span> ' + (Esc $o.titulo) + ' <span class="dim">(' + @($o.cards).Count + ' cards)</span></h2>')
    $h.Add('<div class="porque">' + (Esc $o.porque) + '</div>')

    # Dentro da onda, cards do mesmo nivel nao dependem uns dos outros: correm juntos.
    $lotes = @($o.cards | Group-Object { $nivel[$_.id] } | Sort-Object { [int]$_.Name })
    $nl = 0
    foreach ($lote in $lotes) {
        $nl++
        $qtd = @($lote.Group).Count
        if ($qtd -gt $maxParalelo) { $maxParalelo = $qtd }
        $txt = $(if ($qtd -eq 1) { '1 card, sozinho' } else { "$qtd cards em paralelo" })
        $extra = $(if ($lotes.Count -gt 1) { ' &mdash; so pode comecar depois do lote anterior' } else { ' &mdash; nenhum depende de outro' })
        $h.Add('<div class="lote"><span class="lt">Lote ' + $nl + '</span> ' + $txt + $extra + '</div>')
        $h.Add('<table><thead><tr><th>Jira</th><th>Card</th><th>Titulo</th><th>Tipo</th><th>Peso</th><th>Passos</th><th>Oraculo</th><th>Espera por</th><th>Bloqueia</th><th>Bloqueadores</th></tr></thead><tbody>')
        foreach ($c in ($lote.Group | Sort-Object @{ e = { -$alcance[$_.id] } }, id)) {
            $bl = @(Arr $c.bloqueadores)
            $dep = @(Arr $c.dependsOn)
            $trava = @($bloqueia[$c.id])
            $kj = $chaveJira[$c.id]
            $busca = (($c.id + ' ' + $c.title + ' ' + $kj + ' ' + ($bl -join ' ')).ToLowerInvariant())
            $temBl = $(if ($bl.Count) { '1' } else { '0' })
            $temGr = $(if ($c.peso -eq 'atravessa-o-sistema') { '1' } else { '0' })
            $temTv = $(if ($trava.Count) { '1' } else { '0' })
            # Aspas dentro de subexpressao dentro de aspas e onde o parser desiste: montar antes.
            $celDep = $(if ($dep.Count) { (@($dep | ForEach-Object { Esc $_ }) -join '<br>') } else { '&mdash;' })
            $celBl  = '<span class="dim">&mdash;</span>'
            if ($bl.Count) { $celBl = (@($bl | ForEach-Object { '<span class="tag bl">' + (Esc $_) + '</span>' }) -join ' ') }
            $celTv = '<span class="dim">&mdash;</span>'
            if ($trava.Count) {
                $celTv = '<span class="tag trava">trava ' + $trava.Count + '</span>'
                if ($alcance[$c.id] -gt $trava.Count) { $celTv += ' <span class="esp">' + $alcance[$c.id] + ' na cadeia</span>' }
                $celTv += '<br><span class="esp">' + (@($trava | ForEach-Object { Esc $_ }) -join '<br>') + '</span>'
            }
            $celJira = '<span class="dim">&mdash;</span>'
            if ($kj) {
                if ($jiraBase) { $celJira = '<a class="jk" target="_blank" href="' + (Esc $jiraBase) + '/browse/' + (Esc $kj) + '">' + (Esc $kj) + '</a>' }
                else { $celJira = '<span class="jk">' + (Esc $kj) + '</span>' }
                # O browser nao pode chamar o Jira a partir de file:// - CORS e o token
                # teria de viver no HTML. O botao marca; o comando e que move.
                $celJira += '<div class="act"><button data-k="' + (Esc $kj) + '" data-e="To Detail">To Detail</button>'
                $celJira += '<button data-k="' + (Esc $kj) + '" data-e="In Progress">In Progress</button></div>'
            }
            $h.Add('<tr data-b="' + $temBl + '" data-g="' + $temGr + '" data-t="' + $temTv + '" data-s="' + (Esc $busca) + '">')
            $h.Add('<td>' + $celJira + '</td>')
            $h.Add('<td><code>' + (Esc $c.id) + '</code></td>')
            $h.Add('<td class="t">' + (Esc $c.title) + '</td>')
            $h.Add('<td><span class="tag ' + $corDoTipo[$c.cardType] + '">' + (Esc $c.cardType) + '</span></td>')
            $h.Add('<td><span class="tag pz-' + (Esc $c.peso) + '">' + (Esc $c.peso) + '</span></td>')
            $h.Add('<td class="dim">' + $c.nos + '</td>')
            $h.Add('<td class="dim">' + (Esc $c.oraculo) + '<br><span class="esp">' + $c.casos + ' caso(s)</span></td>')
            $h.Add('<td class="esp">' + $celDep + '</td>')
            $h.Add('<td>' + $celTv + '</td>')
            $h.Add('<td>' + $celBl + '</td>')
            $h.Add('</tr>')
        }
        $h.Add('</tbody></table>')
    }
    $h.Add('</section>')
}
$h.Add('</main>')
$h.Add('<div id="sel"><b id="selN"></b><span class="cmd" id="selCmd"></span><button id="selCopy">copiar comando</button><button id="selClear">limpar</button></div>')

$h.Add("<footer>Gerado por <code>tools/gen-plano-execucao.ps1</code> a partir de <code>artifacts/$(Esc $Package)/backlog/index.json</code>. Manifesto <code>$(Esc $idx.manifestSha256)</code>. Regenerar sempre que o backlog mudar.</footer>")

$h.Add(@'
<script>
const rows=[...document.querySelectorAll('tbody tr')];
const q=document.getElementById('q'),fb=document.getElementById('fb'),ft=document.getElementById('ft'),fg=document.getElementById('fg'),cnt=document.getElementById('cnt');
let ob=false,ot=false,og=false;
function apply(){
  const t=q.value.trim().toLowerCase();let v=0;
  for(const r of rows){
    const ok=(!t||r.dataset.s.includes(t))&&(!ob||r.dataset.b==='1')&&(!ot||r.dataset.t==='1')&&(!og||r.dataset.g==='1');
    r.classList.toggle('hide',!ok); if(ok)v++;
  }
  cnt.textContent=v+' de '+rows.length+' cards visiveis';
}
q.addEventListener('input',apply);
fb.addEventListener('click',()=>{ob=!ob;fb.classList.toggle('on',ob);apply();});
ft.addEventListener('click',()=>{ot=!ot;ft.classList.toggle('on',ot);apply();});
fg.addEventListener('click',()=>{og=!og;fg.classList.toggle('on',og);apply();});
apply();

// Selecao para transicao. O HTML nao move nada: monta o comando que move.
const sel=new Map();
const bar=document.getElementById('sel'),selN=document.getElementById('selN'),selCmd=document.getElementById('selCmd');
function redraw(){
  const estados=[...new Set([...sel.values()])];
  selN.textContent=sel.size+' issue(s)';
  if(sel.size===0){bar.classList.remove('on');return;}
  bar.classList.add('on');
  const linhas=estados.map(e=>{
    const ks=[...sel.entries()].filter(([,v])=>v===e).map(([k])=>k).join(',');
    return `pwsh tools/jira-transicao.ps1 -Estado "${e}" -Chaves ${ks} -Confirmar`;
  });
  selCmd.textContent=linhas.join('  ;  ');
}
document.querySelectorAll('.act button').forEach(b=>{
  b.addEventListener('click',()=>{
    const k=b.dataset.k,e=b.dataset.e,tr=b.closest('tr');
    const irmaos=tr.querySelectorAll('.act button');
    if(sel.get(k)===e){sel.delete(k);irmaos.forEach(x=>x.classList.remove('sel'));tr.classList.remove('marcada');}
    else{sel.set(k,e);irmaos.forEach(x=>x.classList.toggle('sel',x===b));tr.classList.add('marcada');}
    redraw();
  });
});
document.getElementById('selCopy').addEventListener('click',async()=>{
  try{await navigator.clipboard.writeText(selCmd.textContent);}
  catch(_){const r=document.createRange();r.selectNode(selCmd);getSelection().removeAllRanges();getSelection().addRange(r);document.execCommand('copy');}
});
document.getElementById('selClear').addEventListener('click',()=>{
  sel.clear();
  document.querySelectorAll('.act button.sel').forEach(b=>b.classList.remove('sel'));
  document.querySelectorAll('tr.marcada').forEach(r=>r.classList.remove('marcada'));
  redraw();
});
</script>
'@)
$h.Add('</body></html>')

($h -join "`r`n") | Set-Content -LiteralPath $OutPath -Encoding UTF8
Write-Host ("Wrote {0}  ({1} ondas, {2} cards, {3} com bloqueador)" -f $OutPath, $ondas.Count, @($cards).Count, $totalBloq)
