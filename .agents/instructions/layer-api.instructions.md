---
description: "Use when writing or changing code in the Api layer of the ePAT migration. Covers what belongs there and what it may depend on."
applyTo: "src/SefazSp.Epat.Api/**"
---
# Api - SefazSp.Epat.Api

Raiz de composicao. E o unico projecto que conhece toda a gente, e nao tem logica nenhuma.

## What lives here

- **Endpoints** - Superficie minima para conduzir a demonstracao: iniciar caso, listar tarefas, submeter tarefa, retomar por correlacao.
- **Composition** - Registo de DI, incluindo o registo de processos gerado do XPDL e validado no arranque.

## Dependency rule

- May reference only: Application, Infrastructure, Domain.
- Each rule above is a ProjectReference. Breaking it does not compile.

## Naming

- Os identificadores originais sao preservados como nomes de propriedade - EXISTENOTIFICAC continua EXISTENOTIFICAC. O termo de negocio do glossario entra como comentario XML, nunca como renomeacao. O toolkit transcreve, nao baptiza.
