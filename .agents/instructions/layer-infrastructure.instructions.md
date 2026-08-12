---
description: "Use when writing or changing code in the Infrastructure layer of the ePAT migration. Covers what belongs there and what it may depend on."
applyTo: "src/SefazSp.Epat.Infrastructure/**"
---
# Infrastructure - SefazSp.Epat.Infrastructure

Implementa as portas. Toda a dependencia externa entra por aqui.

## What lives here

- **Legacy** - CAMADA ANTICORRUPCAO. O shim dos builtins iProcess (SUBSTR, SEARCH, CALCTIME), a conversao SW_NA <-> FieldValue<T>, e o mapeamento do envelope tecnico. E o unico sitio do codigo onde a palavra iProcess aparece.
- **Workflow.Elsa** - Traduz a topologia de Application/Workflows para o motor. O motor e um detalhe: trocar de motor mexe so aqui.
- **Integration.Soap** - As 5 operacoes realmente invocadas, sobre SOAP/JMS.
- **Integration.Doubles** - Os dubles decididos em rulings.MISSING-EXTERNAL-PACKAGES, incluindo os 6 destinos em falta de AGUARDAR. Tipados a partir do WSDL ou da ProcessInterface, conduzidos por cenario.
- **Rules.Dmn** - As 49 colunas Corticon como DMN. Ficam aqui porque o motor de regras e um detalhe - a porta IDecisionService e que e o contrato. A equivalencia ja esta provada por 3000 casos.
- **Persistence** - EF Core. Persistencia do caso e do estado de execucao.

## Dependency rule

- May reference only: Application, Domain.
- Each rule above is a ProjectReference. Breaking it does not compile.

## Naming

- Os identificadores originais sao preservados como nomes de propriedade - EXISTENOTIFICAC continua EXISTENOTIFICAC. O termo de negocio do glossario entra como comentario XML, nunca como renomeacao. O toolkit transcreve, nao baptiza.
