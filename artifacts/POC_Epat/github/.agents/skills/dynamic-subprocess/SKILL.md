---
name: dynamic-subprocess
description: "Use when implementing any ePAT card that carries the dynamic-subprocess blocker: Chamada de subprocesso cujo destino e resolvido em runtime pelo valor de um campo do caso (xpdExt:ProcessIdentifierField). Explains the ratified .NET approach and why the alternatives were refused."
---
# dynamic-subprocess

## The construct

Chamada de subprocesso cujo destino e resolvido em runtime pelo valor de um campo do caso (xpdExt:ProcessIdentifierField)

## Why .NET has no direct equivalent

Nao ha vinculo estatico a resolver em tempo de compilacao: o nome do processo a instanciar so existe quando o caso executa. Em .NET nao ha construcao equivalente que escolha o tipo a instanciar por um valor de dado sem abrir mao da verificacao em build.

## What was decided

**interface-registry-validated**

Ratificado em 2026-08-06. O xpdExt:ProcessInterface do TIBCO ja e a interface: NOTFAIIM (l.12028) -> DEAT0050, CTRINTPC (l.12179) -> CONTROPC, AGURETPC (l.12463) -> AGPECASPC. A traducao e transcricao, nao invencao. RESSALVA: o campo AGUARDAR recebe 7 valores em CONTROPC/ISetSubProc (AgPRJ, AgRecPRJ, AgPRJR, AgPecas, AgRCRaz, AgCRaz, AgPetica) e so 1 implementacao de AGURETPC foi entregue - confirmado que os outros 6 processos estao nos pacotes externos nao entregues. O conjunto NAO e fechado, e por isso o registo validado em arranque e a escolha certa: torna a falta visivel no CI em vez de em producao. closed-switch foi recusada exactamente por isso.

## Risk if ignored

Chamada nao resolvida em producao. E o legado NAO para nesse caso: os tres passos declaram HaltOnBadSubProcess="false", ou seja, subprocesso invalido falha em silencio.

## Alternatives that were refused

- **closed-switch** - Switch explicito sobre o conjunto de processos que implementam a interface, derivado do XPDL.
  - Consequence: Totalmente verificavel em build e simples de ler. Deixa de ser fiel se surgir uma implementacao nova (por exemplo, vinda de um pacote externo), porque exige recompilar.
- **registry-late-binding** - Registo nome-para-tipo puro, resolvido apenas em runtime, com falha explicita quando o nome nao existe.
  - Consequence: Reproduz o comportamento dinamico com o minimo de cerimonia. Erros de destino so aparecem em execucao - o mesmo ponto fraco do legado, sem o ganho da verificacao antecipada.
