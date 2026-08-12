---
name: expression-deadline
description: "Use when implementing any ePAT card that carries the expression-deadline blocker: Prazo definido por expressao que combina um campo DATE e um campo TIME. Explains the ratified .NET approach and why the alternatives were refused."
---
# expression-deadline

## The construct

Prazo definido por expressao que combina um campo DATE e um campo TIME

## Why .NET has no direct equivalent

O prazo nao e uma duracao: e um instante calculado a partir de dois campos de negocio, que podem mudar durante a execucao.

## What was decided

**absolute-instant**

Ratificado em 2026-08-06. recompute-on-resume chegou a ser escolhido e foi revertido no mesmo dia: obrigava a definir a politica para o instante recalculado que ja passou (dispara / ignora / escala), e essa politica nao existe no legado nem no documento da POC - ficaria a ser inventada em codigo. absolute-instant combina o par data+hora num DateTime absoluto no momento do agendamento. RISCO RESIDUAL ASSUMIDO: o timer nao acompanha prorrogacao do prazo feita depois do agendamento. MITIGACAO A IMPLEMENTAR: rearmar o temporizador sempre que o campo de prazo for escrito, o que cobre o caso real sem inventar politica para instante no passado. POR CONFIRMAR: fuso horario (assumido America/Sao_Paulo) e se UseWorkingDays=true do iProcess afecta o calculo.

## Risk if ignored

Tratar como duracao fixa dispara o timer no momento errado.

## Alternatives that were refused

- **recompute-on-resume** - Recalcular o instante sempre que o processo e retomado.
  - Consequence: Acompanha alteracoes dos campos, mas OBRIGA a definir a politica para o caso em que o novo instante ja passou: dispara imediatamente, ignora ou escala?
