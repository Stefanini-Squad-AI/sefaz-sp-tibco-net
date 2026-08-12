---
name: non-interrupting-boundary
description: "Use when implementing any ePAT card that carries the non-interrupting-boundary blocker: Evento de borda nao interruptivo: a tarefa hospedeira continua executando enquanto um ramo lateral dispara. Explains the ratified .NET approach and why the alternatives were refused."
---
# non-interrupting-boundary

## The construct

Evento de borda nao interruptivo: a tarefa hospedeira continua executando enquanto um ramo lateral dispara

## Why .NET has no direct equivalent

Exige execucao concorrente dentro do escopo da tarefa, sem cancelar a tarefa hospedeira.

## What was decided

**parallel-branch**

Ratificado em 2026-08-06. external-subscription chegou a ser escolhido e foi revertido no mesmo dia: o catalogo regista que com ele o ramo lateral deixa de aparecer no diagrama do processo, e a rastreabilidade visual e um objectivo declarado da POC - um comportamento que funciona mas nao se ve nao serve para demonstrar aderencia funcional. parallel-branch mantem o ramo dentro do escopo, visivel no diagrama, e resolve de graca a limpeza exigida pela etapa 7 do plano: a subscricao morre com o escopo, em vez de ficar orfa. POR CONFIRMAR com o negocio: quem recebe o aviso, e se a actividade hospedeira pode mesmo terminar normalmente depois de o aviso ter disparado.

## Risk if ignored

Implementar como interruptivo cancela a tarefa original e perde o trabalho em andamento.

## Alternatives that were refused

- **external-subscription** - Assinatura de evento fora do fluxo principal, reagindo em paralelo.
  - Consequence: Desacopla, mas o ramo lateral DEIXA DE APARECER no diagrama do processo - perde-se a rastreabilidade visual, que e justamente o que o cliente quer ver na PoC.
