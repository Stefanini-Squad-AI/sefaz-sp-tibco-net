---
name: link-goto
description: "Use when implementing any ePAT card that carries the link-goto blocker: Evento Link do XPDL usado como GOTO entre raias. Explains the ratified .NET approach and why the alternatives were refused."
---
# link-goto

## The construct

Evento Link do XPDL usado como GOTO entre raias

## Why .NET has no direct equivalent

BPMN/Elsa nao tratam Link como desvio incondicional entre raias; e um artificio de diagramacao do iProcess.

## What was decided

**flatten-edge**

Ratificado em 2026-08-06, de acordo com a sugestao. Os 10 pares throw/catch ja estao resolvidos em derived.linkEdges e nenhum atravessa fronteira de processo. keep-as-signal foi recusada por introduzir pontos de persistencia e espera que o TIBCO nao tem: o motor passaria a parar onde o original nao parava.

## Risk if ignored

Manter o par throw/catch como evento cria estados de espera que nao existem no original.

## Alternatives that were refused

- **keep-as-signal** - Manter como evento de sinal intermediario no motor de workflow.
  - Consequence: Preserva o desenho original, mas INTRODUZ pontos de persistencia e espera inexistentes no TIBCO: o motor passa a parar onde o original nao parava.
