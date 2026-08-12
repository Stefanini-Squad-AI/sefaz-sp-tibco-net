<#
.SYNOPSIS
    Traduz uma expressao numa frase em portugues, sem inventar significado.

.DESCRIPTION
    Dot-source a partir dos geradores. A frase e montada por substituicao mecanica:
    o identificador e trocado pelo rotulo que o dicionario de campos ja conhece, o
    operador e trocado pela palavra correspondente, e o literal fica visivel entre
    aspas angulares.

    O que a expressao NAO diz continua a nao ser dito. Um literal como 'JUIZ' sai
    marcado como significado nao declarado, porque em lado nenhum do pacote existe
    a lista de valores possiveis. Isso e informacao: diz ao leitor exactamente onde
    a fonte e omissa.
#>

$script:MarcaLiteral = [char]0x0002
$script:MarcaCampo   = [char]0x0003
$script:MarcaNA      = [char]0x0001

# Rotulo utilizavel e um que acrescenta alguma coisa ao identificador.
function Get-FieldLabel {
    param($Field)
    if (-not $Field) { return $null }
    foreach ($c in @($Field.fullName, $Field.labelSuggestion)) {
        if ($c -and ($c -replace '[^A-Za-z0-9]', '').ToUpperInvariant() -ne $Field.name.ToUpperInvariant()) { return $c }
    }
    foreach ($u in @($Field.usedInForm)) {
        $l = $u.label
        if ($l -and ($l -replace '[^A-Za-z0-9]', '').ToUpperInvariant() -ne $Field.name.ToUpperInvariant()) { return $l }
    }
    return $null
}

function Get-FieldTypeText {
    param($Field)
    if (-not $Field) { return $null }
    $t = $Field.clrType
    if ($Field.isArray) { $t = "lista de $t" }
    $extra = @()
    if ($Field.maxLength)  { $extra += "$($Field.maxLength) caracteres" }
    if ($Field.precision)  { $extra += "$($Field.precision) digito(s)" }
    if ($extra.Count) { $t += ' (' + ($extra -join ', ') + ')' }
    return $t
}

# Todos os literais contra os quais o campo e comparado no pacote inteiro.
function New-DomainIndex {
    param($Fields)
    $dom = @{}
    foreach ($f in @($Fields.fields)) {
        $vals = [System.Collections.Generic.HashSet[string]]::new()
        foreach ($u in @($f.usedInConditions)) {
            foreach ($m in [regex]::Matches([string]$u.expression, "$([regex]::Escape($f.name))\s*[=!]=\s*'([^']*)'")) {
                [void]$vals.Add($m.Groups[1].Value)
            }
        }
        if ($vals.Count) { $dom[$f.name] = @($vals | Sort-Object) }
    }
    return $dom
}

function New-FieldIndex {
    param($Fields)
    $idx = @{}
    $dom = New-DomainIndex $Fields
    foreach ($f in @($Fields.fields)) {
        $idx[$f.name.ToUpperInvariant()] = [pscustomobject]@{
            Name     = $f.name
            Label    = (Get-FieldLabel $f)
            Type     = (Get-FieldTypeText $f)
            Sentinel = [bool]$f.usesSwNaSentinel
            Domain   = @($dom[$f.name])
        }
    }
    return $idx
}

# fieldsIProcess["X"].Value e campoIProcess.X sao o mesmo campo X.
function ConvertTo-BareFields {
    param([string]$Expression)
    $e = $Expression
    $e = [regex]::Replace($e, '(?:\w+\.)?fieldsIProcess\s*\[\s*"(\w+)"\s*\]\s*(?:\.Value)?', '$1')
    $e = [regex]::Replace($e, '(?:\w+\.)?fieldsIProcess\s*\[\s*campoIProcess\.(\w+)\s*\]\s*(?:\.Value)?', '$1')
    $e = [regex]::Replace($e, 'campoIProcess\.(\w+)', '$1')
    return $e
}

function ConvertTo-Phrase {
    param([string]$Expression, [hashtable]$Index)

    $e = (ConvertTo-BareFields $Expression).Trim().TrimEnd(';')
    $e = $e -replace '\s+', ' '

    $e = $e -replace 'IPESystemValues\.SW_NA|(?<![\w.])SW_NA(?![\w])', $script:MarcaNA
    $e = [regex]::Replace($e, "'([^']*)'", "$script:MarcaLiteral`$1$script:MarcaLiteral")
    $e = [regex]::Replace($e, '"([^"]*)"', "$script:MarcaLiteral`$1$script:MarcaLiteral")

    # So identificadores conhecidos ganham rotulo; o resto fica como esta escrito.
    $e = [regex]::Replace($e, "(?<![\w.$script:MarcaLiteral])([A-Za-z_][A-Za-z0-9_]{2,})(?![\w$script:MarcaLiteral])", {
        param($m)
        $k = $m.Groups[1].Value.ToUpperInvariant()
        if ($Index.ContainsKey($k) -and $Index[$k].Label) { return "$script:MarcaCampo$($Index[$k].Label)$script:MarcaCampo" }
        return $m.Groups[1].Value
    })

    foreach ($p in @(
        @('\s*==\s*true\b',  ' e verdadeiro'),
        @('\s*==\s*false\b', ' e falso'),
        @('\s*!=\s*true\b',  ' nao e verdadeiro'),
        @('\s*!=\s*false\b', ' nao e falso'),
        @('\s*>=\s*',        ' e maior ou igual a '),
        @('\s*<=\s*',        ' e menor ou igual a '),
        @('\s*==\s*',        ' e igual a '),
        @('\s*!=\s*',        ' e diferente de '),
        @('\s*&&\s*',        ' E '),
        @('\s*\|\|\s*',      ' OU '),
        @('\s*>\s*',         ' e maior que '),
        @('\s*<\s*',         ' e menor que '),
        @('(?<![\w])!(?=[\w' + $script:MarcaCampo + '])', 'nao ')
    )) { $e = $e -replace $p[0], $p[1] }

    $e = $e -replace $script:MarcaNA, '<nao preenchido>'
    $e = $e -replace $script:MarcaLiteral, '"'
    $e = $e -replace $script:MarcaCampo, ''
    return ($e -replace '\s+', ' ').Trim()
}

<#
    Devolve a leitura completa de uma expressao: a frase, os termos que aparecem
    nela com o que se sabe de cada um, os literais sem significado declarado, a
    consequencia do ramo e a lista explicita do que continua por saber.
#>
function New-Reading {
    param(
        [string]$Expression,
        [hashtable]$Index,
        [string[]]$Fields = @(),
        [string]$Consequence = ''
    )

    $frase = ConvertTo-Phrase -Expression $Expression -Index $Index
    $bare  = ConvertTo-BareFields $Expression

    $termos = @(foreach ($n in ($Fields | Sort-Object -Unique)) {
        $k = $n.ToUpperInvariant()
        if (-not $Index.ContainsKey($k)) { continue }
        $f = $Index[$k]
        [ordered]@{
            campo   = $f.Name
            rotulo  = $f.Label
            tipo    = $f.Type
            sentinelaSwNa = $f.Sentinel
            valoresObservadosNoPacote = @($f.Domain)
        }
    })

    $literais = @(foreach ($m in [regex]::Matches($bare, "'([^']*)'|`"([^`"]*)`"")) {
        $v = $(if ($m.Groups[1].Success) { $m.Groups[1].Value } else { $m.Groups[2].Value })
        if ([string]::IsNullOrWhiteSpace($v)) { continue }
        $v
    }) | Sort-Object -Unique

    $naoSabemos = @()
    foreach ($t in $termos) {
        if (-not $t.rotulo) { $naoSabemos += "o nome completo de $($t.campo) nao esta declarado em lado nenhum do pacote" }
        if ($t.sentinelaSwNa) { $naoSabemos += "$($t.campo) e comparado com SW_NA, que nao e nulo nem vazio; em .NET precisa de sentinela explicita" }
    }
    foreach ($l in $literais) {
        $naoSabemos += "o significado de `"$l`" nao esta declarado; o pacote so mostra que o valor e usado"
    }

    return [ordered]@{
        frase        = $frase
        termos       = @($termos)
        literais     = @($literais)
        consequencia = $Consequence
        naoSabemos   = @($naoSabemos | Sort-Object -Unique)
        origem       = 'derivado da expressao; nenhum significado foi acrescentado'
    }
}
