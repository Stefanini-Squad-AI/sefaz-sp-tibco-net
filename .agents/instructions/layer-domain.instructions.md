---
description: "Use when writing or changing code in the Domain layer of the ePAT migration. Covers what belongs there and what it may depend on."
applyTo: "src/SefazSp.Epat.Domain/**"
---
# Domain - SefazSp.Epat.Domain

Zero dependencias, incluindo de pacotes NuGet de infraestrutura. Nada aqui sabe que existe TIBCO, Elsa, SOAP ou base de dados.

## What lives here

- **Cases** - O agregado do caso, com os 190 campos de negocio. Os 19 do envelope tecnico NAO entram.
- **ValueObjects** - FieldValue<T> tri-estado (HasValue / IsNotAvailable / Empty), decidido em gaps.iprocess-builtin. O tipo vive aqui porque tres estados sao um facto do dominio; a CONVERSAO a partir de SW_NA vive na infraestrutura.
- **Enums** - Dominios fechados observados no pacote: TIPOVISTAS {JUIZ, MISTA}, decisao do operador {R, OK}, REGRAINSDOC {1,2,3}, estado do subprocesso {inativo}.
- **Rules** - As 32 regras de negocio do XPDL como funcoes puras sobre o caso. Sem I/O, sem relogio, sem motor.
- **Abstractions** - IClock. O prazo por expressao precisa de tempo, e tempo e uma dependencia - injectada, nunca DateTime.Now.

## Dependency rule

- Depends on nothing. No project reference, no infrastructure package.
- Each rule above is a ProjectReference. Breaking it does not compile.

## Naming

- Os identificadores originais sao preservados como nomes de propriedade - EXISTENOTIFICAC continua EXISTENOTIFICAC. O termo de negocio do glossario entra como comentario XML, nunca como renomeacao. O toolkit transcreve, nao baptiza.
