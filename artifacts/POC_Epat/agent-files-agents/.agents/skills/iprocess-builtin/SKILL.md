---
name: iprocess-builtin
description: "Use when implementing any ePAT card that carries the iprocess-builtin blocker: Valores e funcoes de runtime do iProcess (IPESystemValues.SW_NA, SW_CASENUM, SW_DATE, IPEStringUtil.*, IPEDateTimeUtil.CALCTIME). Explains the ratified .NET approach and why the alternatives were refused."
---
# iprocess-builtin

## The construct

Valores e funcoes de runtime do iProcess (IPESystemValues.SW_NA, SW_CASENUM, SW_DATE, IPEStringUtil.*, IPEDateTimeUtil.CALCTIME)

## Why .NET has no direct equivalent

SW_NA e um TERCEIRO estado distinto: nao e null e nao e string vazia. C# nao possui esse estado. As funcoes utilitarias tem semantica propria de indice e de calendario que nao coincide com a BCL.

## What was decided

**shim-tri-state**

Ratificado em 2026-08-06. SW_NA e um terceiro estado distinto de null e de vazio, usado por 18 campos. O tipo tri-estado obriga o compilador a exigir a decisao em cada uso, atraves de pattern matching exaustivo. map-to-null foi recusada porque exigiria provar, campo a campo, que nenhum dos 18 e legitimamente nulo - e onde a prova falhasse o ramo trocado nao daria erro visivel. preserve-literal foi recusada por propagar tipagem fraca para todo o modelo de dominio.

## Risk if ignored

Mapear SW_NA para null colapsa dois estados diferentes e muda silenciosamente qual ramo dispara. Nao ha erro de compilacao nem teste vermelho.

## Alternatives that were refused

- **map-to-null** - Mapear SW_NA para null e usar tipos anulaveis.
  - Consequence: Codigo idiomatico, porem SO e seguro para campos que nunca sao legitimamente nulos - e sao 18 campos sentinela, cada um exigindo essa prova. Onde a prova falhar, o ramo trocado nao gera erro visivel.
- **preserve-literal** - Preservar o literal como constante de string e comparar textualmente.
  - Consequence: Traducao literal e facil de auditar contra o XPDL, mas propaga tipagem fraca para todo o modelo de dominio e desiste da verificacao do compilador.
