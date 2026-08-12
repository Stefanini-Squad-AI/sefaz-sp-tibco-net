---
description: "Use when writing or changing code in the Application layer of the ePAT migration. Covers what belongs there and what it may depend on."
applyTo: "src/SefazSp.Epat.Application/**"
---
# Application - SefazSp.Epat.Application

Depende so do Dominio. Declara PORTAS; nao conhece nenhuma implementacao.

## What lives here

- **Abstractions/Services** - Uma porta por operacao invocada pelo processo (5 das 127 catalogadas).
- **Abstractions/Processes** - Uma interface por xpdExt:ProcessInterface do XPDL: NOTFAIIM, CTRINTPC, AGURETPC. Decidido em gaps.dynamic-subprocess: o registo e gerado do XPDL e validado no arranque.
- **Abstractions/Runtime** - IWorkflowRuntime, ICorrelationStore, IGraftJoin. O graft step (gaps.graft-step, correlation-join) declara-se aqui e implementa-se la fora.
- **Execution** - Os 19 campos do envelope tecnico. STATUS_CODE, ISAPPERROR, MAXRETRIES e companhia vivem aqui, nao no dominio - foi o que ficou decidido na ronda dos identificadores.
- **UseCases** - Um caso de uso por tarefa humana e por etapa da POC. As 32 regras de negocio do code-behind das telas aterram aqui, porque decidem o desfecho do processo (CORRECAO, NOTIFICACAO) e nao a apresentacao.
- **Workflows** - A topologia do processo como DADOS, independente de motor - a projeccao directa do process-model.json.

## Dependency rule

- May reference only: Domain.
- Each rule above is a ProjectReference. Breaking it does not compile.

## Naming

- Os identificadores originais sao preservados como nomes de propriedade - EXISTENOTIFICAC continua EXISTENOTIFICAC. O termo de negocio do glossario entra como comentario XML, nunca como renomeacao. O toolkit transcreve, nao baptiza.
