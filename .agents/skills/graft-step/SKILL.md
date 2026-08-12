---
name: graft-step
description: "Use when implementing any ePAT card that carries the graft-step blocker: Graft Step: o passo pai NAO inicia o subprocesso - aguarda que instancias se ANEXEM a ele, possivelmente em momentos diferentes, e so prossegue quando todas terminarem. Explains the ratified .NET approach and why the alternatives were refused."
---
# graft-step

## The construct

Graft Step: o passo pai NAO inicia o subprocesso - aguarda que instancias se ANEXEM a ele, possivelmente em momentos diferentes, e so prossegue quando todas terminarem

## Why .NET has no direct equivalent

A juncao e invertida e a cardinalidade e definida em execucao: o pai nao sabe quantos filhos existirao nem quando aparecerao. .NET nao tem construcao equivalente - fan-out/fan-in classico exige conhecer o conjunto no momento da divisao.

## What was decided

**correlation-join**

Ratificado em 2026-08-06. O contrato fica do lado do pai: o filho apenas sinaliza, o que evita obrigar processos de pacotes externos a registarem-se. child-registry foi recusada por empurrar contrato para os filhos. one-to-one-call foi recusada por nao demonstrar o conceito, que o cliente colocou em escopo a 2026-08-05. DECIDIDO TAMBEM: as duas valvulas de reinicio manual - 'Iniciar Aguardar Notificacao' (l.1370) e 'Iniciar Novo Graft' (l.3032), ambas TaskReceive - ficam EM ESCOPO, por serem hoje o unico mecanismo de recuperacao do graft. POR DEFINIR na implementacao: a chave de correlacao formal e o criterio de encerramento, incluindo timeout para filho que nunca termina - hoje ambos sao implicitos na identidade do caso iProcess.

## Risk if ignored

O pai encerra antes dos filhos (perde trabalho) ou aguarda indefinidamente (caso preso). Nenhum dos dois gera erro visivel.

## Alternatives that were refused

- **child-registry** - Cada instancia filha anuncia-se ao pai ao iniciar e reporta ao concluir; o pai mantem a lista e o contador.
  - Consequence: Mais simples de auditar e de mostrar numa demo, porque o estado e visivel. Altera o contrato dos processos filhos, que passam a ter de se registar - inclusive os que vierem de pacotes externos.
- **one-to-one-call** - Tratar como chamada dinamica 1:1 e sincrona, seguindo a flag IsGraftStep="false": o pai instancia um unico filho e aguarda a conclusao.
  - Consequence: Muito mais barato e coerente com o XPDL exportado, MAS nao valida o conceito de Graft Step que a POC exige demonstrar. So e aceitavel se o cliente confirmar a hipotese (a).
