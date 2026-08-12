---
name: external-event
description: "Use when implementing any ePAT card that carries the external-event blocker: Passo diferido / por evento do iProcess. Explains the ratified .NET approach and why the alternatives were refused."
---
# external-event

## The construct

Passo diferido / por evento do iProcess

## Why .NET has no direct equivalent

O iProcess retoma o passo por identidade de caso implicita; .NET precisa de chave de correlacao explicita e de um ponto de entrada para o evento.

## What was decided

**bookmark-correlation**

Ratificado em 2026-08-06. queue-saga chegou a ser escolhido e foi revertido no mesmo dia: exigia infraestrutura de mensageria adicional, fora do escopo declarado da POC, e ninguem estava designado para a provisionar na demonstracao. bookmark-correlation usa o modelo de longa duracao do proprio motor, sem infraestrutura extra. Chave de correlacao ja existe e nao precisa de ser inventada: PROCESS_ID = 'idAiim-<n>idProc-<n>', montado pelos scripts antes de cada chamada. POR DEFINIR: proteccao do endpoint de retomada, e politica de idempotencia para entrega duplicada ou resposta atrasada - o teste de evento duplicado e exigido pela etapa 5 do plano de cumprimento.

## Risk if ignored

Sem chave de correlacao o processo nao sabe qual instancia retomar.

## Alternatives that were refused

- **queue-saga** - Mensageria com saga correlacionada (ex.: MassTransit).
  - Consequence: Escala melhor e desacopla os produtores de evento, ao custo de infraestrutura adicional fora do escopo da PoC.
