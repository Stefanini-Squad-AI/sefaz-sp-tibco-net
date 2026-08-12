# Questionario de migracao - POC_Epat

Gerado de `review-dossier.json` em 2026-08-12 10:50. Regenerado a cada extracao: nao editar este arquivo.

As respostas vao para `config/glossary/POC_Epat.yaml`, na chave indicada em cada item - nunca neste documento.

## Como usar

| Prioridade | Significado | Efeito de nao responder |
|---|---|---|
| **P1** | Construcao sem equivalente em .NET, severidade alta | A implementacao dos passos afetados e um palpite que falha em silencio |
| **P2** | Sem equivalente (media) ou bloqueador | Politica de erro, prazo ou correlacao fica indefinida |
| **P3** | Comportamental | O ramo errado dispara em producao, sem erro de compilacao |
| **P4** | Cosmetico | Apenas nomenclatura; nao bloqueia implementacao |

| Prioridade | Perguntas |
|---|---:|
| P1 | 3 |
| P2 | 10 |
| P3 | 10 |
| P4 | 11 |
| **Total** | **34** |

---

## 1. [P1] dynamic-subprocess

`NOEQ-dynamic-subprocess` &middot; categoria: `no-net-equivalent` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? Confirmar que nao existem implementacoes adicionais das mesmas interfaces nos pacotes externos nao entregues.
2. Qual opcao adotar para 'dynamic-subprocess'? (indicar o id da opcao)
3. A opcao escolhida vale para todas as ocorrencias ou alguma exige tratamento proprio?

### Por que isso importa

Construcao do iProcess sem traducao direta. Ate ser decidida, qualquer implementacao dos passos afetados e um palpite que falha em silencio.

Chamada de subprocesso cujo destino e resolvido em runtime pelo valor de um campo do caso (xpdExt:ProcessIdentifierField). NAO HA equivalente direto em .NET: Nao ha vinculo estatico a resolver em tempo de compilacao: o nome do processo a instanciar so existe quando o caso executa. Em .NET nao ha construcao equivalente que escolha o tipo a instanciar por um valor de dado sem abrir mao da verificacao em build. Ocorre em 3 ponto(s), nos processos CONTROPC, POC_EpatProcess. Risco de ignorar: Chamada nao resolvida em producao. E o legado NAO para nesse caso: os tres passos declaram HaltOnBadSubProcess="false", ou seja, subprocesso invalido falha em silencio. As opcoes abaixo sao as alternativas conhecidas - a escolha e do gate humano e vale para todas as ocorrencias.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 8672 | `_-bkw-V6JEfGBBLgT-R5iuw` | CONTROPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 1350 | `_0XWagVqNEfG5K7mY0I3I6w` | POC_EpatProcess |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 2721 | `_nQntZ16JEfGBBLgT-R5iuw` | POC_EpatProcess |

### Evidencia

- Intencao no documento da POC: etapa 6 "Controle de Intimados" (casou por "Controle Intimados")

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **alta**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**A opcao 'interface-registry-validated' e a correcta, e o conjunto de destinos possiveis e derivavel do XPDL - ao contrario do que se julgava.**

O SubFlow nao aponta para um processo, aponta para um xpdExt:ProcessInterface. Ha tres interfaces no pacote (NOTFAIIM, CTRINTPC, AGURETPC) e cada uma tem exactamente UMA implementacao declarada via ImplementedInterface. O conjunto e portanto fechado e conhecido em tempo de compilacao, o que permite gerar o registo a partir do XPDL e validar no arranque.

- Para fechar a questao: Confirmar que nao existem implementacoes adicionais das mesmas interfaces nos pacotes externos nao entregues.
- Se a hipotese estiver errada: Se houver implementacoes noutros pacotes, o registo validado em arranque rejeita um destino legitimo. E um erro visivel e barato de corrigir - preferivel ao inverso.

### Sugestao

| Opcao | Padrao de projeto | Abordagem | Consequencia | Sugerida |
|---|---|---|---|:---:|
| `interface-registry-validated` |  | Uma interface C# por xpdExt:ProcessInterface e uma implementacao por processo que a declara. Resolucao em runtime por chave (Keyed DI do .NET 8: AddKeyedScoped/GetRequiredKeyedService), com o registo GERADO a partir do XPDL e verificado no arranque contra o conjunto de ImplementedInterface. | Mantem o comportamento dinamico do iProcess e recupera a verificacao em build: um destino em falta quebra o teste de registo, nao a producao. Custo: exige o passo de geracao do registo e um teste de conformidade. | **sim** |
| `closed-switch` |  | Switch explicito sobre o conjunto de processos que implementam a interface, derivado do XPDL. | Totalmente verificavel em build e simples de ler. Deixa de ser fiel se surgir uma implementacao nova (por exemplo, vinda de um pacote externo), porque exige recompilar. |  |
| `registry-late-binding` |  | Registo nome-para-tipo puro, resolvido apenas em runtime, com falha explicita quando o nome nao existe. | Reproduz o comportamento dinamico com o minimo de cerimonia. Erros de destino so aparecem em execucao - o mesmo ponto fraco do legado, sem o ganho da verificacao antecipada. |  |

_A opcao marcada como sugerida precisa de ratificacao explicita; ela nao e uma decisao._

### Resposta

_Escolher UMA das opcoes pelo id e justificar. A decisao vale para todas as ocorrencias listadas._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `gaps.dynamic-subprocess`

- [ ] **opcaoEscolhida**: 
- [ ] **justificativa**: 
- [ ] Respondido por / data: 

---

## 2. [P1] graft-step

`NOEQ-graft-step` &middot; categoria: `no-net-equivalent` &middot; confianca da deteccao: **medium**

### Pergunta

1. CONFIRMA a hipotese? Confirmar quantas instancias filhas podem ligar-se a um pai, e o que acontece se uma delas nunca terminar.
2. Qual opcao adotar para 'graft-step'? (indicar o id da opcao)
3. A opcao escolhida vale para todas as ocorrencias ou alguma exige tratamento proprio?

### Por que isso importa

Construcao do iProcess sem traducao direta. Ate ser decidida, qualquer implementacao dos passos afetados e um palpite que falha em silencio.

Graft Step: o passo pai NAO inicia o subprocesso - aguarda que instancias se ANEXEM a ele, possivelmente em momentos diferentes, e so prossegue quando todas terminarem. NAO HA equivalente direto em .NET: A juncao e invertida e a cardinalidade e definida em execucao: o pai nao sabe quantos filhos existirao nem quando aparecerao. .NET nao tem construcao equivalente - fan-out/fan-in classico exige conhecer o conjunto no momento da divisao. Detectado pelo NOME dos passos - a palavra 'graft' - e nao por hazard: as chamadas do pacote resolvem-se todas, logo nenhum detector estrutural dispara aqui. Ocorre em 3 ponto(s), nos processos POC_EpatProcess. Risco de ignorar: O pai encerra antes dos filhos (perde trabalho) ou aguarda indefinidamente (caso preso). Nenhum dos dois gera erro visivel.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 1338 | `_0XWagFqNEfG5K7mY0I3I6w` | POC_EpatProcess |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 2237 | `_Faq_RVqTEfG5K7mY0I3I6w` | POC_EpatProcess |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 3032 | `_OAgPol9UEfG6Lfb98zsREQ` | POC_EpatProcess |

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **alta**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**O graft step e usado a serio: cria uma instancia filha POR SOLIDARIO e liga-a a actividade que espera. Nao e uma chamada de subprocesso comum, apesar de a flag exportada dizer que nao.**

Os tres DynamicSubProcessTask trazem IsGraftStep="false", mas a descricao que o autor escreveu no proprio XPDL diz o contrario: 'inicia um processo de Notificacao do AIIM para cada solidario e o vincula a actividade'. Os campos envolvidos sao IsArray="true", o que confirma a cardinalidade multipla. A flag exportada e provavelmente artefacto da exportacao, nao a intencao.

- Para fechar a questao: Confirmar quantas instancias filhas podem ligar-se a um pai, e o que acontece se uma delas nunca terminar.
- Se a hipotese estiver errada: Implementar como chamada simples de subprocesso faz a notificacao a multiplos solidarios deixar de funcionar - e esse e o conceito de destaque da etapa 2 da PoC.

### Sugestao

| Opcao | Padrao de projeto | Abordagem | Consequencia | Sugerida |
|---|---|---|---|:---:|
| `correlation-join` |  | O pai suspende num bookmark correlacionado pelo caso; cada filho, ao terminar, sinaliza; um contador de filhos registados decide o encerramento. | Cobre fielmente a cardinalidade variavel e a anexacao em momentos diferentes. Exige definir formalmente a chave de correlacao e o criterio de encerramento (incluindo timeout), que hoje sao implicitos na identidade do caso iProcess. | **sim** |
| `child-registry` |  | Cada instancia filha anuncia-se ao pai ao iniciar e reporta ao concluir; o pai mantem a lista e o contador. | Mais simples de auditar e de mostrar numa demo, porque o estado e visivel. Altera o contrato dos processos filhos, que passam a ter de se registar - inclusive os que vierem de pacotes externos. |  |
| `one-to-one-call` |  | Tratar como chamada dinamica 1:1 e sincrona, seguindo a flag IsGraftStep="false": o pai instancia um unico filho e aguarda a conclusao. | Muito mais barato e coerente com o XPDL exportado, MAS nao valida o conceito de Graft Step que a POC exige demonstrar. So e aceitavel se o cliente confirmar a hipotese (a). |  |

_A opcao marcada como sugerida precisa de ratificacao explicita; ela nao e uma decisao._

### Resposta

_Escolher UMA das opcoes pelo id e justificar. A decisao vale para todas as ocorrencias listadas._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `gaps.graft-step`

- [ ] **opcaoEscolhida**: 
- [ ] **justificativa**: 
- [ ] Respondido por / data: 

---

## 3. [P1] iprocess-builtin

`NOEQ-iprocess-builtin` &middot; categoria: `no-net-equivalent` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? Aprovar que SW_NA vire um sentinela explicito em C# - um tipo proprio ou constante bem conhecida - e nunca null.
2. Qual opcao adotar para 'iprocess-builtin'? (indicar o id da opcao)
3. A opcao escolhida vale para todas as ocorrencias ou alguma exige tratamento proprio?

### Por que isso importa

Construcao do iProcess sem traducao direta. Ate ser decidida, qualquer implementacao dos passos afetados e um palpite que falha em silencio.

Valores e funcoes de runtime do iProcess (IPESystemValues.SW_NA, SW_CASENUM, SW_DATE, IPEStringUtil.*, IPEDateTimeUtil.CALCTIME). NAO HA equivalente direto em .NET: SW_NA e um TERCEIRO estado distinto: nao e null e nao e string vazia. C# nao possui esse estado. As funcoes utilitarias tem semantica propria de indice e de calendario que nao coincide com a BCL. Ocorre em 17 ponto(s), nos processos AGPECASPC, ATZINTPC, BSCENVPC, CALCPRPC, CONTROPC, CRNOTPC, DEAT0050, POC_EpatProcess, PRPINTPC. Risco de ignorar: Mapear SW_NA para null colapsa dois estados diferentes e muda silenciosamente qual ramo dispara. Nao ha erro de compilacao nem teste vermelho. As opcoes abaixo sao as alternativas conhecidas - a escolha e do gate humano e vale para todas as ocorrencias.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 10549 | `_EvOwTF6eEfGJqLUhfbpFcQ` | AGPECASPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9622 | `_RNdJyl6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9684 | `_RNdJzF6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5832 | `_qIDulV6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5894 | `_qIDul16BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4730 | `_zJIHVlqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4798 | `_zJIHWFqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 8554 | `_-bkw-F6JEfGBBLgT-R5iuw` | CONTROPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11480 | `_NcJJ4l9KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11542 | `_NcJJ5F9KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 3895 | `_lrer3VqhEfG5K7mY0I3I6w` | DEAT0050 |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 2657 | `_G4hU81qhEfG5K7mY0I3I6w` | POC_EpatProcess |

_(+ 5 outra(s) ocorrencia(s) - ver review-dossier.json)_

### Evidencia

- Simbolos: `IPEDateTimeUtil.CALCTIME` , `IPEStringUtil.SEARCH` , `IPEStringUtil.STRLEN` , `IPEStringUtil.SUBSTR` , `IPESystemValues.SW_CASENUM` , `IPESystemValues.SW_DATE` , `IPESystemValues.SW_HOSTNAME` , `IPESystemValues.SW_NA` , `IPESystemValues.SW_PRONAME` , `IPESystemValues.SW_TIME`
- Intencao no documento da POC: etapa 6 "Controle de Intimados" (casou por "Controle Intimados")
- Intencao no documento da POC: etapa 2 "Notificação do AIIM" (casou por "Notificacao do AIIM")

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **alta**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**As 17 ocorrencias resolvem-se com uma camada de compatibilidade unica e pequena, nao com traducao caso a caso. O ponto critico e SW_NA, nao as funcoes de texto.**

Sao 13 builtins em 30 chamadas, a maioria manipulacao de texto e data com equivalente directo em .NET. O que nao tem equivalente e o SW_NA: um terceiro estado que nao e nulo nem vazio, e que 18 campos usam. Traduzi-lo para null muda o ramo que dispara, em silencio.

- Para fechar a questao: Aprovar que SW_NA vire um sentinela explicito em C# - um tipo proprio ou constante bem conhecida - e nunca null.
- Se a hipotese estiver errada: Cada campo com SW_NA tem tres caminhos possiveis. Colapsar dois deles muda o comportamento em pontos dispersos, sem erro de compilacao e sem teste vermelho.

### Sugestao

| Opcao | Padrao de projeto | Abordagem | Consequencia | Sugerida |
|---|---|---|---|:---:|
| `shim-tri-state` |  | Tipo de campo tri-estado dentro da camada anticorrupcao: readonly record struct FieldValue<T> com estados HasValue / IsNotAvailable / Empty, mais a classe IProcessValues com os utilitarios. O pattern matching exaustivo do C# obriga a tratar os tres casos. | Preserva a semantica exata dos tres estados e o compilador passa a exigir a decisao em cada uso. Custo: todo campo sentinela deixa de ser tipo primitivo e o codigo gerado fica menos idiomatico. | **sim** |
| `map-to-null` |  | Mapear SW_NA para null e usar tipos anulaveis. | Codigo idiomatico, porem SO e seguro para campos que nunca sao legitimamente nulos - e sao 18 campos sentinela, cada um exigindo essa prova. Onde a prova falhar, o ramo trocado nao gera erro visivel. |  |
| `preserve-literal` |  | Preservar o literal como constante de string e comparar textualmente. | Traducao literal e facil de auditar contra o XPDL, mas propaga tipagem fraca para todo o modelo de dominio e desiste da verificacao do compilador. |  |

_A opcao marcada como sugerida precisa de ratificacao explicita; ela nao e uma decisao._

### Resposta

_Escolher UMA das opcoes pelo id e justificar. A decisao vale para todas as ocorrencias listadas._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `gaps.iprocess-builtin`

- [ ] **opcaoEscolhida**: 
- [ ] **justificativa**: 
- [ ] Respondido por / data: 

---

## 4. [P2] semantica dos builtins iProcess nao confirmada

`BUILTIN-SEMANTICS` &middot; categoria: `builtin-semantics` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? Uma pagina da documentacao TIBCO iProcess, ou uma execucao no ambiente legado com um caso conhecido, que fixe o indice inicial de SUBSTR e o valor de retorno de SEARCH quando nao encontra.
2. SUBSTR e SEARCH sao base 1 ou base 0? (confirmar na documentacao do iProcess)
3. O terceiro argumento de SUBSTR e COMPRIMENTO ou POSICAO FINAL? Os dados atuais nao distinguem os dois casos.
4. O que SEARCH retorna quando o separador nao existe, e o que SUBSTR faz com comprimento negativo?

### Por que isso importa

A semantica dos builtins do iProcess nao e derivavel dos arquivos entregues. Sem confirmacao, qualquer shim em .NET e chute.

Estas funcoes nao tem equivalente direto em .NET e sua semantica NAO e derivavel da entrega: nao ha TIBCO em execucao nem documentacao do fornecedor. O risco concreto e SUBSTR/SEARCH: se forem base 1 no iProcess e forem portadas como base 0, o recorte perde um caractere e um id de documento chega truncado, sem excecao nenhuma. Ha um vetor de teste comportamental em builtin-contract.json que qualquer implementacao candidata precisa satisfazer.

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **media**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**Os builtins iProcess seguem a convencao base-1 do iProcess classico, nao a base-0 do C#. SUBSTR e SEARCH sao os de maior risco.**

O builtin-contract.json fixou um vector comportamental a partir de dados literais que os proprios scripts carregam: a divisao da lista separada por barra vertical em IDSINTIMADOS. Esse vector so fecha se os indices forem base-1. Nao e prova formal, mas e o unico teste disponivel sem a documentacao do produto.

- Para fechar a questao: Uma pagina da documentacao TIBCO iProcess, ou uma execucao no ambiente legado com um caso conhecido, que fixe o indice inicial de SUBSTR e o valor de retorno de SEARCH quando nao encontra.
- Se a hipotese estiver errada: Um desvio de uma posicao em SUBSTR nao gera erro: gera um valor errado que segue pelo fluxo. E a classe de defeito mais dificil de detectar depois, porque nao falha, so mente.

### Sugestao

Priorizar os builtins com mais pontos de chamada e fixar o comportamento por vetor de teste, nao por descricao em prosa.

### Resposta

_Confirmar o comportamento de cada builtin com a documentacao do fornecedor ou com uma execucao no TIBCO, e registrar vetores de teste (entrada -> saida esperada)._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.BUILTIN-SEMANTICS`

- [ ] **comportamento**: 
- [ ] **vetoresDeTeste**: 
- [ ] Respondido por / data: 

---

## 5. [P2] PRPINTPC

`CLONE-PRPINTPC` &middot; categoria: `clone-divergence` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? Confirmar com quem manteve o processo se houve alguma vez razao para o PRPINTPC tratar erro de forma diferente.
2. A diferenca em PRPINTPC e intencional ou e um defeito herdado do TIBCO?
3. Se for defeito: a migracao deve reproduzi-lo fielmente ou corrigi-lo? (decisao que precisa ficar registrada)

### Por que isso importa

Processos identicos por estrutura divergem nas condicoes. Como o template concentra tratamento de erro e retentativa, a divergencia muda a politica de erro de uma integracao especifica.

PRPINTPC tem estrutura identica a ATZINTPC, BSCENVPC, CALCPRPC, CRNOTPC, ou seja, sao copias do mesmo template. As condicoes, porem, divergem. Como esse template concentra o tratamento de erro e retentativa de TODAS as chamadas de servico, uma divergencia aqui muda a politica de erro de uma integracao especifica. E preciso decidir se e variacao intencional ou defeito de copia no original.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | ? | `PRPINTPC` | PRPINTPC |

### Evidencia

- So neste processo: `STATUS_CODE!=IPESystemValues.SW_NA;`
- Presente nos irmaos e ausente aqui: `STATUS_CODE!="0";`

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **media**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**A divergencia do PRPINTPC e defeito de copia, nao variacao deliberada. O passo de verificacao de erro ficou com o teste do template anterior.**

Os cinco subprocessos partilham assinatura estrutural identica: mesmo laco de retry, mesmos passos Start Loop, More Retries, Check Retries, Try Again, Manually Fixed. Quatro comparam STATUS_CODE com '0'; so o PRPINTPC compara com SW_NA. Nao ha nada no PRPINTPC que justifique um contrato de erro diferente - chama o mesmo tipo de servico pelo mesmo transporte. O padrao de defeito e o mesmo ja encontrado noutros pontos do pacote, como o 'if (CNTPECA1 != SW_NA \|\| CNTPECA1 != 9)' sempre verdadeiro no AGPECASPC.

- Para fechar a questao: Confirmar com quem manteve o processo se houve alguma vez razao para o PRPINTPC tratar erro de forma diferente.
- Se a hipotese estiver errada: Se for intencional e for corrigido, uma condicao de erro que hoje e ignorada passa a interromper o fluxo, e casos que hoje passam comecam a parar.

### Sugestao

Comparar as condicoes divergentes listadas abaixo com o comportamento esperado em producao antes de decidir reproduzir o defeito.

### Resposta

_Dizer se a diferenca e intencional ou defeito herdado. Se for defeito, decidir por escrito se a migracao reproduz ou corrige._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.CLONE-PRPINTPC`

- [ ] **intencional**: 
- [ ] **acaoNaMigracao**: 
- [ ] Respondido por / data: 

---

## 6. [P2] rotulo aponta para outro campo

`LABEL-CONFLICT` &middot; categoria: `label-conflict` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? O nome de negocio correcto para cada um dos campos afectados.
2. Para cada campo: qual e o nome de negocio correto?
3. O rotulo errado deve ser reportado a SEFAZ para correcao na origem?

### Por que isso importa

O formulario rotula um campo com o nome de OUTRO campo. Aceitar o rotulo sem conferir propaga um erro de nomenclatura para todo o modelo.

O rotulo que o formulario da a estes campos e, literalmente, o NOME DE OUTRO campo existente. Aceitar o rotulo renomearia o campo errado no modelo .NET, e o erro so apareceria em producao. Provavel defeito de copia no formulario TIBCO.

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **alta**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**E defeito de copia no formulario TIBCO: o rotulo foi copiado da linha de cima e nao foi editado.**

O rotulo coincide exactamente com o NOME de outro campo declarado, apos normalizacao. Nao e uma coincidencia plausivel de vocabulario: e o padrao classico de duplicar uma linha num editor de formularios e esquecer de mudar o texto.

- Para fechar a questao: O nome de negocio correcto para cada um dos campos afectados.
- Se a hipotese estiver errada: Aceitar o rotulo renomeia o campo errado no modelo .NET. O codigo compila, os testes passam, e o ecra mostra um rotulo que descreve outro dado.

### Sugestao

Tratar como provavel defeito do formulario legado; nao copiar o rotulo sem confirmacao.

### Resposta

_Confirmar qual e o rotulo correto do campo._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.LABEL-CONFLICT`

- [ ] **rotuloCorreto**: 
- [ ] Respondido por / data: 

---

## 7. [P2] expression-deadline

`NOEQ-expression-deadline` &middot; categoria: `no-net-equivalent` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? Confirmar o fuso e, sobretudo, o que acontece se o campo de data for alterado DEPOIS de o temporizador ja estar armado - o prazo reajusta-se ou mantem-se?
2. Qual opcao adotar para 'expression-deadline'? (indicar o id da opcao)
3. A opcao escolhida vale para todas as ocorrencias ou alguma exige tratamento proprio?

### Por que isso importa

Construcao do iProcess sem traducao direta. Ate ser decidida, qualquer implementacao dos passos afetados e um palpite que falha em silencio.

Prazo definido por expressao que combina um campo DATE e um campo TIME. NAO HA equivalente direto em .NET: O prazo nao e uma duracao: e um instante calculado a partir de dois campos de negocio, que podem mudar durante a execucao. Ocorre em 4 ponto(s), nos processos DEAT0050, POC_EpatProcess. Risco de ignorar: Tratar como duracao fixa dispara o timer no momento errado. As opcoes abaixo sao as alternativas conhecidas - a escolha e do gate humano e vale para todas as ocorrencias.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 3872 | `_lrer2lqhEfG5K7mY0I3I6w` | DEAT0050 |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 1649 | `_CtQ6_1qPEfG5K7mY0I3I6w` | POC_EpatProcess |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 1733 | `_CtQ7A1qPEfG5K7mY0I3I6w` | POC_EpatProcess |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 2269 | `_XWivFlqTEfG5K7mY0I3I6w` | POC_EpatProcess |

### Evidencia

- Intencao no documento da POC: etapa 2 "Notificação do AIIM" (casou por "Notificacao do AIIM")

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **media**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**Os prazos por expressao ('DATA; HORA;') sao instantes absolutos calculados a partir de campos do caso, nao duracoes. O fuso e o de Sao Paulo.**

As expressoes referem pares de campos data e hora que o proprio processo escreve, como PRAZODEFESA e HORAFINAL. Uma duracao nao precisaria de dois campos nem de os ler do caso. O sistema e da SEFAZ-SP e os prazos sao administrativos, portanto contam em horario local.

- Para fechar a questao: Confirmar o fuso e, sobretudo, o que acontece se o campo de data for alterado DEPOIS de o temporizador ja estar armado - o prazo reajusta-se ou mantem-se?
- Se a hipotese estiver errada: Tratar como duracao faz o prazo disparar no momento errado. Num processo administrativo fiscal, um prazo que dispara cedo ou tarde tem consequencia legal, nao apenas tecnica.

### Sugestao

| Opcao | Padrao de projeto | Abordagem | Consequencia | Sugerida |
|---|---|---|---|:---:|
| `absolute-instant` |  | Combinar o campo de data e o de hora num DateTime absoluto no momento do agendamento e programar o timer para esse instante. | Simples e previsivel. Nao reage a alteracao posterior dos campos: se o prazo for prorrogado depois do agendamento, o timer nao acompanha. | **sim** |
| `recompute-on-resume` |  | Recalcular o instante sempre que o processo e retomado. | Acompanha alteracoes dos campos, mas OBRIGA a definir a politica para o caso em que o novo instante ja passou: dispara imediatamente, ignora ou escala? |  |

_A opcao marcada como sugerida precisa de ratificacao explicita; ela nao e uma decisao._

### Resposta

_Escolher UMA das opcoes pelo id e justificar. A decisao vale para todas as ocorrencias listadas._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `gaps.expression-deadline`

- [ ] **opcaoEscolhida**: 
- [ ] **justificativa**: 
- [ ] Respondido por / data: 

---

## 8. [P2] external-event

`NOEQ-external-event` &middot; categoria: `no-net-equivalent` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? Confirmar se a fila garante entrega unica ou se pode entregar em duplicado, e se pode haver resposta atrasada depois de o caso ja ter seguido.
2. Qual opcao adotar para 'external-event'? (indicar o id da opcao)
3. A opcao escolhida vale para todas as ocorrencias ou alguma exige tratamento proprio?

### Por que isso importa

Construcao do iProcess sem traducao direta. Ate ser decidida, qualquer implementacao dos passos afetados e um palpite que falha em silencio.

Passo diferido / por evento do iProcess. NAO HA equivalente direto em .NET: O iProcess retoma o passo por identidade de caso implicita; .NET precisa de chave de correlacao explicita e de um ponto de entrada para o evento. Ocorre em 6 ponto(s), nos processos AGPECASPC, DEAT0050, POC_EpatProcess. Risco de ignorar: Sem chave de correlacao o processo nao sabe qual instancia retomar. As opcoes abaixo sao as alternativas conhecidas - a escolha e do gate humano e vale para todas as ocorrencias.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 10506 | `_EvOwQl6eEfGJqLUhfbpFcQ` | AGPECASPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 3982 | `_lrer81qhEfG5K7mY0I3I6w` | DEAT0050 |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 1370 | `_0XWaglqNEfG5K7mY0I3I6w` | POC_EpatProcess |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 3032 | `_OAgPol9UEfG6Lfb98zsREQ` | POC_EpatProcess |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 1535 | `_CtQ68lqPEfG5K7mY0I3I6w` | POC_EpatProcess |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 1608 | `_CtQ6-1qPEfG5K7mY0I3I6w` | POC_EpatProcess |

### Evidencia

- Intencao no documento da POC: etapa 2 "Notificação do AIIM" (casou por "Notificacao do AIIM")

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **alta**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**O evento externo e a resposta assincrona do BusinessWorks pela fila JMS, correlacionada pelo PROCESS_ID que os scripts ja constroem.**

Os scripts montam 'PROCESS_ID = "idAiim-<n>idProc-<n>"' antes de cada chamada. A chave de correlacao ja existe e ja e deterministica - nao e preciso inventar uma. O transporte e SOAP sobre JMS/EMS, declarado no WSDL.

- Para fechar a questao: Confirmar se a fila garante entrega unica ou se pode entregar em duplicado, e se pode haver resposta atrasada depois de o caso ja ter seguido.
- Se a hipotese estiver errada: Sem idempotencia, uma entrega duplicada faz o caso avancar duas vezes. Com o graft step envolvido, pode gerar notificacoes a mais.

### Sugestao

| Opcao | Padrao de projeto | Abordagem | Consequencia | Sugerida |
|---|---|---|---|:---:|
| `bookmark-correlation` |  | Bookmark do motor com chave de correlacao derivada do caso, exposta por um endpoint de retomada. No Elsa 3 corresponde a suspender a atividade e retomar por sinal correlacionado. | Alinhado ao modelo de longa duracao do Elsa e sem infraestrutura adicional. Exige definir formalmente a chave de correlacao e proteger o endpoint de retomada. | **sim** |
| `queue-saga` |  | Mensageria com saga correlacionada (ex.: MassTransit). | Escala melhor e desacopla os produtores de evento, ao custo de infraestrutura adicional fora do escopo da PoC. |  |

_A opcao marcada como sugerida precisa de ratificacao explicita; ela nao e uma decisao._

### Resposta

_Escolher UMA das opcoes pelo id e justificar. A decisao vale para todas as ocorrencias listadas._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `gaps.external-event`

- [ ] **opcaoEscolhida**: 
- [ ] **justificativa**: 
- [ ] Respondido por / data: 

---

## 9. [P2] link-goto

`NOEQ-link-goto` &middot; categoria: `no-net-equivalent` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? Nada do negocio - e decisao de arquitectura. Basta aprovar que o BPMN gerado mostre a seta explicita em vez do salto.
2. Qual opcao adotar para 'link-goto'? (indicar o id da opcao)
3. A opcao escolhida vale para todas as ocorrencias ou alguma exige tratamento proprio?

### Por que isso importa

Construcao do iProcess sem traducao direta. Ate ser decidida, qualquer implementacao dos passos afetados e um palpite que falha em silencio.

Evento Link do XPDL usado como GOTO entre raias. NAO HA equivalente direto em .NET: BPMN/Elsa nao tratam Link como desvio incondicional entre raias; e um artificio de diagramacao do iProcess. Ocorre em 20 ponto(s), nos processos ATZINTPC, BSCENVPC, CALCPRPC, CRNOTPC, POC_EpatProcess, PRPINTPC. Risco de ignorar: Manter o par throw/catch como evento cria estados de espera que nao existem no original. As opcoes abaixo sao as alternativas conhecidas - a escolha e do gate humano e vale para todas as ocorrencias.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9860 | `_RNdJ1F6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9737 | `_RNdJzV6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 6070 | `_qIDun16BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5947 | `_qIDumF6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4959 | `_zJIHYFqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4835 | `_zJIHWVqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11718 | `_NcJJ7F9KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11595 | `_NcJJ5V9KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 2372 | `_5E444FqTEfG5K7mY0I3I6w` | POC_EpatProcess |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 2359 | `_tN6q4lqTEfG5K7mY0I3I6w` | POC_EpatProcess |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 2237 | `_Faq_RVqTEfG5K7mY0I3I6w` | POC_EpatProcess |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 1338 | `_0XWagFqNEfG5K7mY0I3I6w` | POC_EpatProcess |

_(+ 8 outra(s) ocorrencia(s) - ver review-dossier.json)_

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **alta**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**Os 20 pares link/goto sao desvio de fluxo puro, sem semantica de negocio. Podem virar transicoes explicitas sem perda.**

Os 10 pares throw/catch resolvem-se todos deterministicamente no process-model, e cada catch tem exactamente um throw. Nenhum atravessa fronteira de processo. E a construcao classica de evitar cruzar setas num diagrama grande.

- Para fechar a questao: Nada do negocio - e decisao de arquitectura. Basta aprovar que o BPMN gerado mostre a seta explicita em vez do salto.
- Se a hipotese estiver errada: Baixo. O pior caso e um diagrama mais dificil de ler.

### Sugestao

| Opcao | Padrao de projeto | Abordagem | Consequencia | Sugerida |
|---|---|---|---|:---:|
| `flatten-edge` |  | Achatar cada par throw/catch em uma aresta explicita de fluxo (ja resolvido em derived.linkEdges: 10 throw / 10 catch, 10 resolvidos). | Grafo mais simples e fiel a execucao. O diagrama perde a marcacao visual de salto entre raias. | **sim** |
| `keep-as-signal` |  | Manter como evento de sinal intermediario no motor de workflow. | Preserva o desenho original, mas INTRODUZ pontos de persistencia e espera inexistentes no TIBCO: o motor passa a parar onde o original nao parava. |  |

_A opcao marcada como sugerida precisa de ratificacao explicita; ela nao e uma decisao._

### Resposta

_Escolher UMA das opcoes pelo id e justificar. A decisao vale para todas as ocorrencias listadas._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `gaps.link-goto`

- [ ] **opcaoEscolhida**: 
- [ ] **justificativa**: 
- [ ] Respondido por / data: 

---

## 10. [P2] non-interrupting-boundary

`NOEQ-non-interrupting-boundary` &middot; categoria: `no-net-equivalent` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? Confirmar quem recebe o aviso e se a actividade principal pode mesmo terminar normalmente depois de o aviso ter disparado.
2. Qual opcao adotar para 'non-interrupting-boundary'? (indicar o id da opcao)
3. A opcao escolhida vale para todas as ocorrencias ou alguma exige tratamento proprio?

### Por que isso importa

Construcao do iProcess sem traducao direta. Ate ser decidida, qualquer implementacao dos passos afetados e um palpite que falha em silencio.

Evento de borda nao interruptivo: a tarefa hospedeira continua executando enquanto um ramo lateral dispara. NAO HA equivalente direto em .NET: Exige execucao concorrente dentro do escopo da tarefa, sem cancelar a tarefa hospedeira. Ocorre em 1 ponto(s), nos processos POC_EpatProcess. Risco de ignorar: Implementar como interruptivo cancela a tarefa original e perde o trabalho em andamento. As opcoes abaixo sao as alternativas conhecidas - a escolha e do gate humano e vale para todas as ocorrencias.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 2269 | `_XWivFlqTEfG5K7mY0I3I6w` | POC_EpatProcess |

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **media**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**A fronteira nao interruptiva existe para avisar sem parar: o prazo expira, alguem e notificado, e a actividade principal continua a correr.**

Ha uma unica ocorrencia. O padrao em processos administrativos e o aviso de prazo a decorrer, que nao pode interromper o trabalho ja em curso.

- Para fechar a questao: Confirmar quem recebe o aviso e se a actividade principal pode mesmo terminar normalmente depois de o aviso ter disparado.
- Se a hipotese estiver errada: Implementar como interruptiva cancela trabalho em curso quando o prazo passa - perda de trabalho do utilizador.

### Sugestao

| Opcao | Padrao de projeto | Abordagem | Consequencia | Sugerida |
|---|---|---|---|:---:|
| `parallel-branch` |  | Ramo lateral paralelo dentro do mesmo escopo, sem cancelamento do hospedeiro. | Fiel ao original e visivel no diagrama. Exige que o motor suporte ramos paralelos com escopos independentes. | **sim** |
| `external-subscription` |  | Assinatura de evento fora do fluxo principal, reagindo em paralelo. | Desacopla, mas o ramo lateral DEIXA DE APARECER no diagrama do processo - perde-se a rastreabilidade visual, que e justamente o que o cliente quer ver na PoC. |  |

_A opcao marcada como sugerida precisa de ratificacao explicita; ela nao e uma decisao._

### Resposta

_Escolher UMA das opcoes pelo id e justificar. A decisao vale para todas as ocorrencias listadas._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `gaps.non-interrupting-boundary`

- [ ] **opcaoEscolhida**: 
- [ ] **justificativa**: 
- [ ] Respondido por / data: 

---

## 11. [P2] valores fixos embutidos em scriptTask

`SCRIPT-HARDCODED` &middot; categoria: `script-scaffolding` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? Confirmar que 278713 e 278712 nao sao intimados reais em producao, e autorizar a remocao da linha na migracao.
2. Cada valor fixo e andaime de POC (remover) ou parametro legitimo (externalizar em configuracao)?
3. Os destinatarios de e-mail devem vir de configuracao ou de cadastro?

### Por que isso importa

O corpo do script e logica que precisa ser reescrita; a traducao exige entender a intencao, nao so a sintaxe.

Scripts atribuem valores literais a campos de caso. Entre eles ha uma lista de ids de teste que SOBRESCREVE a entrada real depois do processamento, e enderecos de e-mail nominais fixos no codigo. Portar isso literalmente carrega dado de teste e destinatario pessoal para producao.

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **alta**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**Sao dados de teste esquecidos, nao configuracao. Em particular, o 'IDSINTIMADOS = "278713\|278712\|"' no fim do prepSub sobrepoe a lista real calculada acima.**

A atribuicao esta no FIM do script, depois de a lista ter sido montada a partir dos dados do caso, e usa dois identificadores concretos com aspecto de chave de base de dados. Um valor de configuracao estaria no inicio ou num parametro, nao a sobrepor o resultado do proprio script.

- Para fechar a questao: Confirmar que 278713 e 278712 nao sao intimados reais em producao, e autorizar a remocao da linha na migracao.
- Se a hipotese estiver errada: Se for reproduzido fielmente, toda a demonstracao notifica sempre os mesmos dois solidarios, independentemente do AIIM - e o graft step, que e conceito de destaque da PoC, seria demonstrado com dados falsos.

### Sugestao

Especificar o resultado esperado do script; a implementacao em C# fica a cargo da fase de construcao, validada por teste.

### Resposta

_Descrever o que o script deve garantir ao final, em termos de negocio._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.SCRIPT-HARDCODED`

- [ ] **intencao**: 
- [ ] **resultadoEsperado**: 
- [ ] Respondido por / data: 

---

## 12. [P2] pacotes externos referenciados e nao entregues

`MISSING-EXTERNAL-PACKAGES` &middot; categoria: `source-not-delivered` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? Confirmar, etapa a etapa, que nenhuma das sete precisa de um processo que nao foi entregue. Se precisar, decidir entre obter o ficheiro ou substituir por um duble com contrato acordado.
2. A SEFAZ pode entregar estes pacotes? Quais sao viaveis e em que prazo?
3. Para os que nao vierem: a semantica sera autorizada por escrito ou o escopo sera reduzido?

### Por que isso importa

O pacote referencia artefatos que nao vieram na entrega. O que eles fazem nao pode ser analisado nem reproduzido.

O XPDL referencia estes pacotes, mas os arquivos nunca foram entregues. E a raiz de varios outros achados deste dossie: identificadores sem declaracao, o campo que nomeia o subprocesso grafado e nunca escrito, e campos de tela fora do dicionario. Nao e lacuna de analise - e lacuna de entrega, e nenhuma analise adicional a resolve.

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **media**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**Os 15 pacotes nao entregues nao sao todos necessarios a PoC. A trilha narrada usa apenas os processos entregues; os pacotes em falta cobrem o resto do ePAT.**

O intent-map liga as sete etapas do documento a elementos que existem todos no pacote entregue. Os subprocessos invocados pela trilha - DEAT0050, CONTROPC e os cinco de servico - estao presentes. As referencias externas aparecem sobretudo em ramos fora da trilha.

- Para fechar a questao: Confirmar, etapa a etapa, que nenhuma das sete precisa de um processo que nao foi entregue. Se precisar, decidir entre obter o ficheiro ou substituir por um duble com contrato acordado.
- Se a hipotese estiver errada: Descobrir a meio da demonstracao que uma etapa chama um processo inexistente. E pior do que parece: o legado declara HaltOnBadSubProcess="false", ou seja, falha em silencio - a migracao pode herdar o mesmo comportamento e ninguem repara.

### Sugestao

Pedir a entrega do pacote externo; se nao houver, declarar explicitamente a limitacao no relatorio da POC.

### Resposta

_Solicitar o arquivo ao cliente ou registrar por escrito que o item fica fora do escopo._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.MISSING-EXTERNAL-PACKAGES`

- [ ] **providencia**: 
- [ ] **foraDeEscopo**: 
- [ ] Respondido por / data: 

---

## 13. [P2] tipo divergente entre XPDL e formulario

`TYPE-XPDL-VS-FORM` &middot; categoria: `type-conflict` &middot; confianca da deteccao: **high**

### Pergunta

1. CONFIRMA a hipotese? Confirmar a ordem de grandeza real de IDAIIM e NR_AIIM na base de producao. Se couberem folgadamente em int, a divergencia e irrelevante.
2. Qual fonte prevalece quando XPDL e formulario discordam: a precisao do XPDL ou o tipo do formulario?
3. Ha algum campo da lista que exija excecao a essa regra?

### Por que isso importa

XPDL e formulario declaram tipos diferentes para o mesmo campo. Um dos dois esta errado, e a escolha muda faixa de valores e validacao.

A precisao declarada no XPDL implica um tipo mais largo do que o formulario TIBCO declara (tipicamente long contra Integer de 32 bits). Estreitar pode estourar em numero de AIIM; alargar pode quebrar o contrato com a tela. E UMA decisao de padrao, valida para todos os campos abaixo, nao uma decisao por campo.

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **alta**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**O XPDL esta certo e o formulario esta errado. A precisao declarada no XPDL vem do dominio (numero de AIIM, identificadores), enquanto o tipo do formulario e o que coube no controlo do ecra.**

Os 14 casos sao todos do mesmo feitio: XPDL declara long, formulario declara int. Sao campos como IDAIIM, NR_AIIM, NR_RAT, QTDINTIMADOS - identificadores e contadores que crescem com o tempo. A escolha conservadora e sempre o tipo mais largo, porque estreitar trunca em silencio.

- Para fechar a questao: Confirmar a ordem de grandeza real de IDAIIM e NR_AIIM na base de producao. Se couberem folgadamente em int, a divergencia e irrelevante.
- Se a hipotese estiver errada: Adoptar int e ver o identificador exceder o limite so acontece anos depois, e quando acontece corrompe o numero sem erro visivel.

### Sugestao

Adotar o tipo de maior precisao quando houver duvida, e validar contra o dado real antes de fechar.

### Resposta

_Confirmar o tipo real com a base de dados de origem e registrar qual declaracao prevalece._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.TYPE-XPDL-VS-FORM`

- [ ] **tipoCorreto**: 
- [ ] **fonteDaVerdade**: 
- [ ] Respondido por / data: 

---

## 14. [P3] DATACONTROLE, PRAZORECEBIMENT

`SENTINEL-AGPECASPC-_EvOwZF6eE` &middot; categoria: `sentinel-branch` &middot; confianca da deteccao: **high**

### Pergunta

1. O que significa, no negocio, este campo estar "nao preenchido" neste ponto?
2. O ramo de SW_NA deve seguir junto com algum dos outros ou merece tratamento proprio?

### Por que isso importa

SW_NA e um terceiro estado, distinto de nulo e de vazio. Se ele virar null em C#, dois estados diferentes viram um so e o ramo que dispara muda sem erro de compilacao.

Esta condicao testa o sentinela SW_NA do iProcess, que e um terceiro estado distinto: nao e null e nao e string vazia. O campo tem tres caminhos possiveis (valor definido / SW_NA / demais valores). Traduzir SW_NA para null em C# muda silenciosamente qual ramo dispara.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 10690 | `_EvOwZF6eEfGJqLUhfbpFcQ` | AGPECASPC |

### Evidencia

- Condicao: `DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZORECEBIMENT;`
- Chega de: scriptTask 'Controla Datas' ; scriptTask 'Set Values'
- Ramo [OTHERWISE] -> endEvent 'End Event'
- Ramo `DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZORECEBIMENT;` -> scriptTask 'SetPrazo'

### Sugestao

Preservar os tres estados no modelo .NET e tratar o ramo SW_NA explicitamente ate que o negocio confirme que ele pode ser fundido com outro.

### Resposta

_Dizer o que 'nao preenchido' significa no negocio neste ponto e se esse caso segue junto com algum outro ramo ou merece tratamento proprio._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.SENTINEL-AGPECASPC-_EvOwZF6eE`

- [ ] **significado**: 
- [ ] **ramoDestino**: 
- [ ] Respondido por / data: 

---

## 15. [P3] DATACONTROLE, PRAZODEFESA

`SENTINEL-DEAT0050-_lresFVqhE` &middot; categoria: `sentinel-branch` &middot; confianca da deteccao: **high**

### Pergunta

1. O que significa, no negocio, este campo estar "nao preenchido" neste ponto?
2. O ramo de SW_NA deve seguir junto com algum dos outros ou merece tratamento proprio?

### Por que isso importa

SW_NA e um terceiro estado, distinto de nulo e de vazio. Se ele virar null em C#, dois estados diferentes viram um so e o ramo que dispara muda sem erro de compilacao.

Esta condicao testa o sentinela SW_NA do iProcess, que e um terceiro estado distinto: nao e null e nao e string vazia. O campo tem tres caminhos possiveis (valor definido / SW_NA / demais valores). Traduzir SW_NA para null em C# muda silenciosamente qual ramo dispara.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4011 | `_lresFVqhEfG5K7mY0I3I6w` | DEAT0050 |

### Evidencia

- Condicao: `DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZODEFESA;`
- Chega de: scriptTask 'HoraFimSC'
- Ramo `DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZODEFESA;` -> timerEvent 'Aguarda Defesa'
- Ramo [OTHERWISE] -> endEvent «sem rotulo»
- Intencao no documento da POC: etapa 2 "Notificação do AIIM" (casou por "Notificacao do AIIM")

### Sugestao

Preservar os tres estados no modelo .NET e tratar o ramo SW_NA explicitamente ate que o negocio confirme que ele pode ser fundido com outro.

### Resposta

_Dizer o que 'nao preenchido' significa no negocio neste ponto e se esse caso segue junto com algum outro ramo ou merece tratamento proprio._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.SENTINEL-DEAT0050-_lresFVqhE`

- [ ] **significado**: 
- [ ] **ramoDestino**: 
- [ ] Respondido por / data: 

---

## 16. [P3] TIPOVISTAS

`SENTINEL-POC_EpatProcess-_CtQ7DVqPE` &middot; categoria: `sentinel-branch` &middot; confianca da deteccao: **high**

### Pergunta

1. O que significa, no negocio, este campo estar "nao preenchido" neste ponto?
2. O ramo de SW_NA deve seguir junto com algum dos outros ou merece tratamento proprio?

### Por que isso importa

SW_NA e um terceiro estado, distinto de nulo e de vazio. Se ele virar null em C#, dois estados diferentes viram um so e o ramo que dispara muda sem erro de compilacao.

Esta condicao testa o sentinela SW_NA do iProcess, que e um terceiro estado distinto: nao e null e nao e string vazia. O campo tem tres caminhos possiveis (valor definido / SW_NA / demais valores). Traduzir SW_NA para null em C# muda silenciosamente qual ramo dispara.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 1761 | `_CtQ7DVqPEfG5K7mY0I3I6w` | POC_EpatProcess |

### Evidencia

- Condicao: `TIPOVISTAS=='JUIZ' || TIPOVISTAS == IPESystemValues.SW_NA;`
- Chega de: linkCatch 'Validação Paralelos'
- Ramo `TIPOVISTAS=='JUIZ' || TIPOVISTAS == IPESystemValues.SW_NA;` -> receiveTask 'Vistas do Juiz'
- Ramo [OTHERWISE] -> gateway «sem rotulo»

### Sugestao

Preservar os tres estados no modelo .NET e tratar o ramo SW_NA explicitamente ate que o negocio confirme que ele pode ser fundido com outro.

### Resposta

_Dizer o que 'nao preenchido' significa no negocio neste ponto e se esse caso segue junto com algum outro ramo ou merece tratamento proprio._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.SENTINEL-POC_EpatProcess-_CtQ7DVqPE`

- [ ] **significado**: 
- [ ] **ramoDestino**: 
- [ ] Respondido por / data: 

---

## 17. [P3] STATUS_CODE

`SENTINEL-PRPINTPC-_KEwDYl6EE` &middot; categoria: `sentinel-branch` &middot; confianca da deteccao: **high**

### Pergunta

1. O que significa, no negocio, este campo estar "nao preenchido" neste ponto?
2. O ramo de SW_NA deve seguir junto com algum dos outros ou merece tratamento proprio?

### Por que isso importa

SW_NA e um terceiro estado, distinto de nulo e de vazio. Se ele virar null em C#, dois estados diferentes viram um so e o ramo que dispara muda sem erro de compilacao.

Esta condicao testa o sentinela SW_NA do iProcess, que e um terceiro estado distinto: nao e null e nao e string vazia. O campo tem tres caminhos possiveis (valor definido / SW_NA / demais valores). Traduzir SW_NA para null em C# muda silenciosamente qual ramo dispara.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7083 | `_KEwDYl6EEfGBBLgT-R5iuw` | PRPINTPC |

### Evidencia

- Condicao: `STATUS_CODE!=IPESystemValues.SW_NA;`
- Chega de: serviceTask 'CaptaParametros'
- Ramo `STATUS_CODE!=IPESystemValues.SW_NA;` -> scriptTask 'Set App Error'
- Ramo [OTHERWISE] -> gateway «sem rotulo»

### Sugestao

Preservar os tres estados no modelo .NET e tratar o ramo SW_NA explicitamente ate que o negocio confirme que ele pode ser fundido com outro.

### Resposta

_Dizer o que 'nao preenchido' significa no negocio neste ponto e se esse caso segue junto com algum outro ramo ou merece tratamento proprio._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.SENTINEL-PRPINTPC-_KEwDYl6EE`

- [ ] **significado**: 
- [ ] **ramoDestino**: 
- [ ] Respondido por / data: 

---

## 18. [P3] ISAPPERROR

`IDENT-ISAPPERROR` &middot; categoria: `unresolved-identifier` &middot; confianca da deteccao: **medium**

### Pergunta

1. CONFIRMA a hipotese? Confirmar que 'N' e ausencia de erro e nao 'nao avaliado'. A diferenca importa porque o campo pode estar por preencher antes da primeira chamada.
2. Confirmar o dominio de valores de 'ISAPPERROR' e qual deles significa sucesso.
3. Definir onde 'ISAPPERROR' passa a morar no modelo .NET (contexto de execucao? resultado da chamada?).

### Por que isso importa

O identificador decide ramificacao mas nao e um dos campos de negocio declarados. Sem o dominio de valores nao da para reproduzir a decisao.

'ISAPPERROR' nao e um dos 209 campos de negocio, mas TEM declaracao: o formulario TIBCO ATZINTPC/MANEXC, BSCENVPC/MANEXC, CALCPRPC/MANEXC, CRNOTPC/MANEXCPC, POC_EpatProcess/REALATVI, PRPINTPC/MANEXC o declara como Text (tamanho 1), e so e lido pelo processo (IN). Ele decide o fluxo em 5 ponto(s), nos processos ATZINTPC, BSCENVPC, CALCPRPC, CRNOTPC, PRPINTPC, portanto o modelo .NET precisa expor esse valor - fora do modelo de dominio.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5030 | `_zJIHbFqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 6141 | `_qIDuq16BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7691 | `_KEwC9F6EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9931 | `_RNdJ4F6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11789 | `_NcJw9l9KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5024 | `_zJIHZFqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 6135 | `_qIDuo16BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7685 | `_KEwC7F6EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9925 | `_RNdJ2F6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11783 | `_NcJJ8F9KEfGqPfX31TKC3w` | CRNOTPC |

### Evidencia

- Comparado com: `== 'Y'`

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **media**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**ISAPPERROR e um booleano codificado como texto 'Y'/'N' que marca falha de negocio devolvida pelo servico, por oposicao a falha de infraestrutura. 'N' significa ausencia de erro.**

So aparecem os literais 'N' e 'Y' em todo o pacote. Vive lado a lado com ISTECHERROR, com o mesmo dominio, e ambos sao lidos no formulario MANEXC - o formulario de excepcao manual que o operador ve quando o retry se esgota. A separacao aplicacao/tecnico e a mesma que o envelope tecnico do WSDL faz entre STATUS_CODE e ERROR_CODE.

- Para fechar a questao: Confirmar que 'N' e ausencia de erro e nao 'nao avaliado'. A diferenca importa porque o campo pode estar por preencher antes da primeira chamada.
- Se a hipotese estiver errada: Se 'N' significar 'ainda nao avaliado', o ramo de sucesso dispara antes de haver resposta do servico, e o caso avanca sem ter sido processado.

### Sugestao

Confirmar o dominio de valores com quem opera o processo hoje; nao assumir booleano so porque a unica comparacao vista e com um valor.

### Resposta

_Informar de onde vem o valor, quem escreve, quando, e a lista completa de valores possiveis com o significado de cada um._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `unresolved.ISAPPERROR`

- [ ] **origin**: 
- [ ] **term**: 
- [ ] **description**: 
- [ ] **values**: 
- [ ] Respondido por / data: 

---

## 19. [P3] ISTECHERROR

`IDENT-ISTECHERROR` &middot; categoria: `unresolved-identifier` &middot; confianca da deteccao: **medium**

### Pergunta

1. CONFIRMA a hipotese? Confirmar que so o erro tecnico e retentavel automaticamente, e que o erro de aplicacao vai sempre para o operador.
2. Confirmar o dominio de valores de 'ISTECHERROR' e qual deles significa sucesso.
3. Definir onde 'ISTECHERROR' passa a morar no modelo .NET (contexto de execucao? resultado da chamada?).

### Por que isso importa

O identificador decide ramificacao mas nao e um dos campos de negocio declarados. Sem o dominio de valores nao da para reproduzir a decisao.

'ISTECHERROR' nao e um dos 209 campos de negocio, mas TEM declaracao: o formulario TIBCO ATZINTPC/MANEXC, BSCENVPC/MANEXC, CALCPRPC/MANEXC, CRNOTPC/MANEXCPC, POC_EpatProcess/REALATVI, PRPINTPC/MANEXC o declara como Text (tamanho 1), e so e lido pelo processo (IN). Ele decide o fluxo em 5 ponto(s), nos processos ATZINTPC, BSCENVPC, CALCPRPC, CRNOTPC, PRPINTPC, portanto o modelo .NET precisa expor esse valor - fora do modelo de dominio.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5051 | `_zJIHdlqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 6162 | `_qIDutV6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7712 | `_KEwC_l6EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9952 | `_RNdJ6l6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11810 | `_NcJxAF9KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5044 | `_zJIHZVqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 6155 | `_qIDupF6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7705 | `_KEwC7V6EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9945 | `_RNdJ2V6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11803 | `_NcJJ8V9KEfGqPfX31TKC3w` | CRNOTPC |

### Evidencia

- Comparado com: `== 'Y'`

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **media**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**ISTECHERROR e o par de ISAPPERROR para falha de infraestrutura - fila, rede, indisponibilidade - com o mesmo dominio 'Y'/'N'. Governa se a falha e retentavel automaticamente.**

Dominio identico e declaracao no mesmo conjunto de formularios MANEXC. A distincao aplicacao/tecnico e a que decide, no legado, se o caso volta ao laco de retry ou vai para tratamento manual: um erro de negocio nao se resolve repetindo a chamada, um erro tecnico resolve-se.

- Para fechar a questao: Confirmar que so o erro tecnico e retentavel automaticamente, e que o erro de aplicacao vai sempre para o operador.
- Se a hipotese estiver errada: Repetir automaticamente uma chamada que falhou por regra de negocio consome as tentativas todas sem hipotese de sucesso, e atrasa a entrada em tratamento manual.

### Sugestao

Confirmar o dominio de valores com quem opera o processo hoje; nao assumir booleano so porque a unica comparacao vista e com um valor.

### Resposta

_Informar de onde vem o valor, quem escreve, quando, e a lista completa de valores possiveis com o significado de cada um._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `unresolved.ISTECHERROR`

- [ ] **origin**: 
- [ ] **term**: 
- [ ] **description**: 
- [ ] **values**: 
- [ ] Respondido por / data: 

---

## 20. [P3] MAXRETRIES

`IDENT-MAXRETRIES` &middot; categoria: `unresolved-identifier` &middot; confianca da deteccao: **medium**

### Pergunta

1. CONFIRMA a hipotese? Confirmar que 5 e o valor de producao e que nao existe configuracao externa que o sobreponha por ambiente ou por servico.
2. Confirmar o dominio de valores de 'MAXRETRIES' e qual deles significa sucesso.
3. Definir onde 'MAXRETRIES' passa a morar no modelo .NET (contexto de execucao? resultado da chamada?).

### Por que isso importa

O identificador decide ramificacao mas nao e um dos campos de negocio declarados. Sem o dominio de valores nao da para reproduzir a decisao.

'MAXRETRIES' nao e um dos 209 campos de negocio, mas TEM declaracao: o formulario TIBCO ATZINTPC/MANEXC, BSCENVPC/MANEXC, CALCPRPC/MANEXC, CRNOTPC/MANEXCPC, POC_EpatProcess/REALATVI, PRPINTPC/MANEXC o declara como Integer (tamanho 10), e so e lido pelo processo (IN). Ele decide o fluxo em 10 ponto(s), nos processos ATZINTPC, BSCENVPC, CALCPRPC, CRNOTPC, PRPINTPC, portanto o modelo .NET precisa expor esse valor - fora do modelo de dominio.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4977 | `_zJIHbVqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4432 | `_zJIudlqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 6088 | `_qIDurF6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5528 | `_qIDu6V6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7638 | `_KEwC9V6EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 6977 | `_KEwDXl6EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9878 | `_RNdJ4V6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9268 | `_RNdKIV6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11736 | `_NcJw919KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11126 | `_NcJxNV9KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4971 | `_zJIHYVqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4426 | `_zJIubVqiEfG5K7mY0I3I6w` | CALCPRPC |

_(+ 8 outra(s) ocorrencia(s) - ver review-dossier.json)_

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **alta**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**MAXRETRIES e o tecto de tentativas do laco de retry e vale 5 por omissao. Nao tem significado de negocio: e parametro de resiliencia da chamada de servico.**

Os cinco subprocessos de servico (CALCPRPC, BSCENVPC, PRPINTPC, ATZINTPC, CRNOTPC) tem no passo SetParameters a atribuicao 'if (MAXRETRIES == null) MAXRETRIES = 5', identica nos cinco. E lido em duas condicoes: 'NUMAPPRETRIES < MAXRETRIES' no passo More Retries, e 'IPESystemValues.SW_QRETRYCOUNT < MAXRETRIES' no passo Check Retries. Nunca aparece num formulario visivel ao utilizador nem numa regra Corticon.

- Para fechar a questao: Confirmar que 5 e o valor de producao e que nao existe configuracao externa que o sobreponha por ambiente ou por servico.
- Se a hipotese estiver errada: Se na producao o valor vier de configuracao e for diferente, o numero de tentativas muda e com ele o tempo ate o caso cair no tratamento manual. Nao quebra o fluxo, altera o SLA.

### Sugestao

Confirmar o dominio de valores com quem opera o processo hoje; nao assumir booleano so porque a unica comparacao vista e com um valor.

### Resposta

_Informar de onde vem o valor, quem escreve, quando, e a lista completa de valores possiveis com o significado de cada um._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `unresolved.MAXRETRIES`

- [ ] **origin**: 
- [ ] **term**: 
- [ ] **description**: 
- [ ] **values**: 
- [ ] Respondido por / data: 

---

## 21. [P3] NUMAPPRETRIES

`IDENT-NUMAPPRETRIES` &middot; categoria: `unresolved-identifier` &middot; confianca da deteccao: **medium**

### Pergunta

1. CONFIRMA a hipotese? Confirmar que sao mesmo dois contadores independentes com o mesmo tecto, e nao um engano em que se pretendia partilhar o mesmo contador.
2. Confirmar o dominio de valores de 'NUMAPPRETRIES' e qual deles significa sucesso.
3. Definir onde 'NUMAPPRETRIES' passa a morar no modelo .NET (contexto de execucao? resultado da chamada?).

### Por que isso importa

O identificador decide ramificacao mas nao e um dos campos de negocio declarados. Sem o dominio de valores nao da para reproduzir a decisao.

'NUMAPPRETRIES' nao e um dos 209 campos de negocio, mas TEM declaracao: o formulario TIBCO POC_EpatProcess/REALATVI o declara como Integer (tamanho 10), e e alterado pelo processo ou pelo usuario (INOUT). Ele decide o fluxo em 5 ponto(s), nos processos ATZINTPC, BSCENVPC, CALCPRPC, CRNOTPC, PRPINTPC, portanto o modelo .NET precisa expor esse valor - fora do modelo de dominio.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4977 | `_zJIHbVqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 6088 | `_qIDurF6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7638 | `_KEwC9V6EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9878 | `_RNdJ4V6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11736 | `_NcJw919KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4971 | `_zJIHYVqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 6082 | `_qIDuoF6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7632 | `_KEwC6V6EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9872 | `_RNdJ1V6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11730 | `_NcJJ7V9KEfGqPfX31TKC3w` | CRNOTPC |

### Evidencia

- Comparado com: `< MAXRETRIES`

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **alta**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**NUMAPPRETRIES e o contador de tentativas de erro de APLICACAO, distinto do contador tecnico do motor. Comeca em zero e incrementa de um. Nao e campo de negocio.**

Inicializado com 'if (NUMAPPRETRIES == null)' no passo Start Loop dos cinco subprocessos. A condicao 'NUMAPPRETRIES < MAXRETRIES' vive no passo More Retries. Corre em paralelo com 'IPESystemValues.SW_QRETRYCOUNT < MAXRETRIES', que e o contador do proprio motor iProcess. Ha portanto dois lacos de retry sobrepostos com o mesmo tecto: um conta falhas de aplicacao, o outro falhas de entrega da fila.

- Para fechar a questao: Confirmar que sao mesmo dois contadores independentes com o mesmo tecto, e nao um engano em que se pretendia partilhar o mesmo contador.
- Se a hipotese estiver errada: Se forem o mesmo conceito duplicado por engano, o numero real de tentativas em .NET pode chegar ao dobro do pretendido.

### Sugestao

Confirmar o dominio de valores com quem opera o processo hoje; nao assumir booleano so porque a unica comparacao vista e com um valor.

### Resposta

_Informar de onde vem o valor, quem escreve, quando, e a lista completa de valores possiveis com o significado de cada um._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `unresolved.NUMAPPRETRIES`

- [ ] **origin**: 
- [ ] **term**: 
- [ ] **description**: 
- [ ] **values**: 
- [ ] Respondido por / data: 

---

## 22. [P3] OUTCOME

`IDENT-OUTCOME` &middot; categoria: `unresolved-identifier` &middot; confianca da deteccao: **medium**

### Pergunta

1. CONFIRMA a hipotese? Confirmar que o dominio e exactamente {'R', 'OK'} e que nao existe um terceiro valor - por exemplo abandonar o caso - tratado num pacote externo nao entregue.
2. Confirmar o dominio de valores de 'OUTCOME' e qual deles significa sucesso.
3. Definir onde 'OUTCOME' passa a morar no modelo .NET (contexto de execucao? resultado da chamada?).

### Por que isso importa

O identificador decide ramificacao mas nao e um dos campos de negocio declarados. Sem o dominio de valores nao da para reproduzir a decisao.

'OUTCOME' nao e um dos 209 campos de negocio, mas TEM declaracao: o formulario TIBCO ATZINTPC/MANEXC, BSCENVPC/MANEXC, CALCPRPC/MANEXC, CRNOTPC/MANEXCPC, POC_EpatProcess/REALATVI, PRPINTPC/MANEXC o declara como Text (tamanho 10), e e alterado pelo processo ou pelo usuario (INOUT). Ele decide o fluxo em 10 ponto(s), nos processos ATZINTPC, BSCENVPC, CALCPRPC, CRNOTPC, PRPINTPC, portanto o modelo .NET precisa expor esse valor - fora do modelo de dominio.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4784 | `_zJIHclqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4874 | `_zJIHd1qiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5880 | `_qIDusV6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5986 | `_qIDutl6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7415 | `_KEwC-l6EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7536 | `_KEwC_16EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9670 | `_RNdJ5l6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9776 | `_RNdJ616PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11528 | `_NcJw_F9KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11634 | `_NcJxAV9KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4778 | `_zJIHV1qiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4867 | `_zJIHXFqiEfG5K7mY0I3I6w` | CALCPRPC |

_(+ 8 outra(s) ocorrencia(s) - ver review-dossier.json)_

### Evidencia

- Comparado com: `== 'OK'` , `== 'R'`

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **media**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**OUTCOME e a decisao que o operador toma no formulario de excepcao manual: 'R' para repetir a chamada, 'OK' para dar por resolvido. NAO ha divergencia entre clones - sao dois valores do mesmo dominio, lidos em passos diferentes.**

O literal 'R' aparece na condicao do passo Try Again e o literal 'OK' na condicao do passo Manually Fixed. Sao dois passos distintos do mesmo laco de tratamento de excepcao, nao duas versoes do mesmo passo. A leitura inicial de que os clones divergiam vem de os passos nao existirem todos em todos os subprocessos.

- Para fechar a questao: Confirmar que o dominio e exactamente {'R', 'OK'} e que nao existe um terceiro valor - por exemplo abandonar o caso - tratado num pacote externo nao entregue.
- Se a hipotese estiver errada: Um terceiro valor nao previsto cai no ramo por omissao e o caso segue como se tivesse sido resolvido, sem o ter sido.

### Sugestao

Confirmar o dominio de valores com quem opera o processo hoje; nao assumir booleano so porque a unica comparacao vista e com um valor.

### Resposta

_Informar de onde vem o valor, quem escreve, quando, e a lista completa de valores possiveis com o significado de cada um._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `unresolved.OUTCOME`

- [ ] **origin**: 
- [ ] **term**: 
- [ ] **description**: 
- [ ] **values**: 
- [ ] Respondido por / data: 

---

## 23. [P3] STATUS_CODE

`IDENT-STATUS_CODE` &middot; categoria: `unresolved-identifier` &middot; confianca da deteccao: **medium**

### Pergunta

1. CONFIRMA a hipotese? Confirmar que '0' e o unico codigo de sucesso, e decidir se a comparacao com SW_NA no PRPINTPC se corrige ou se reproduz fielmente.
2. Confirmar o dominio de valores de 'STATUS_CODE' e qual deles significa sucesso.
3. Definir onde 'STATUS_CODE' passa a morar no modelo .NET (contexto de execucao? resultado da chamada?).

### Por que isso importa

O identificador decide ramificacao mas nao e um dos campos de negocio declarados. Sem o dominio de valores nao da para reproduzir a decisao.

'STATUS_CODE' nao e um dos 209 campos de negocio, mas TEM declaracao: o formulario TIBCO ATZINTPC/MANEXC, BSCENVPC/MANEXC, CALCPRPC/MANEXC, CRNOTPC/MANEXCPC, POC_EpatProcess/REALATVI, PRPINTPC/MANEXC o declara como Text (tamanho 50), e so e lido pelo processo (IN). Origem no envelope: RESULT/STATUS_CODE - elemento do envelope tecnico declarado em EPAT.wsdl. Ele decide o fluxo em 5 ponto(s), nos processos ATZINTPC, BSCENVPC, CALCPRPC, CRNOTPC, PRPINTPC, portanto o modelo .NET precisa expor esse valor - fora do modelo de dominio.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4524 | `_zJIue1qiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5634 | `_qIDu7l6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7083 | `_KEwDYl6EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9374 | `_RNdKJV6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11232 | `_NcJxOV9KEfGqPfX31TKC3w` | CRNOTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4517 | `_zJIuclqiEfG5K7mY0I3I6w` | CALCPRPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5627 | `_qIDu4l6BEfGBBLgT-R5iuw` | BSCENVPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7076 | `_KEwDVl6EEfGBBLgT-R5iuw` | PRPINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9367 | `_RNdKGl6PEfGBBLgT-R5iuw` | ATZINTPC |
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11225 | `_NcJxLl9KEfGqPfX31TKC3w` | CRNOTPC |

### Evidencia

- Comparado com: `!= "0"` , `!= IPESystemValues.SW_NA`

### Hipotese de trabalho

:warning: **Analise agentica, NAO verificada.** Confianca: **alta**. Serve para acelerar a confirmacao, nunca para dispensar a resposta.

**STATUS_CODE e o codigo de retorno do envelope tecnico do BusinessWorks, onde '0' e sucesso. Nao pertence ao dominio de negocio e deve viver no resultado da chamada, nao no estado do caso.**

Declarado no envelope tecnico do EPAT.wsdl. Comparado com '0' em quatro subprocessos (ATZINTPC, CRNOTPC, BSCENVPC, CALCPRPC) atraves de 'STATUS_CODE != "0"'. O PRPINTPC e a excepcao: usa 'STATUS_CODE != IPESystemValues.SW_NA'. Como os cinco subprocessos sao copias do mesmo template, esta diferenca e provavelmente defeito e nao intencao.

- Para fechar a questao: Confirmar que '0' e o unico codigo de sucesso, e decidir se a comparacao com SW_NA no PRPINTPC se corrige ou se reproduz fielmente.
- Se a hipotese estiver errada: No PRPINTPC, um servico que devolva erro com codigo preenchido passa no teste 'diferente de nao preenchido' e o fluxo segue como se tivesse corrido bem. O erro fica invisivel.

### Sugestao

Confirmar o dominio de valores com quem opera o processo hoje; nao assumir booleano so porque a unica comparacao vista e com um valor.

### Resposta

_Informar de onde vem o valor, quem escreve, quando, e a lista completa de valores possiveis com o significado de cada um._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `unresolved.STATUS_CODE`

- [ ] **origin**: 
- [ ] **term**: 
- [ ] **description**: 
- [ ] **values**: 
- [ ] Respondido por / data: 

---

## 24. [P4] rotulos de formulario nao verificados

`LABEL-SUGGESTION` &middot; categoria: `label-suggestion` &middot; confianca da deteccao: **high**

### Pergunta

1. Aceitar em bloco os rotulos do formulario como termo de negocio, ou revisar caso a caso?
2. Havendo divergencia com o vocabulario oficial da SEFAZ, qual prevalece?

### Por que isso importa

O formulario sugere um nome de negocio para o campo, mas a sugestao nunca foi verificada.

O formulario TIBCO da a estes campos um rotulo de negocio que NAO deriva do nome. Sao afirmacoes de terceiros nunca verificadas - ha erro de digitacao conhecido ('Contorle') e ao menos um rotulo truncado. Estao propostos como comentario no glossario, campo a campo, para aceitar ou recusar.

### Sugestao

Baixo risco: aprovar em lote com o time de negocio. Nao bloqueia implementacao.

### Resposta

_Aprovar ou corrigir o rotulo sugerido._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.LABEL-SUGGESTION`

- [ ] **term**: 
- [ ] **description**: 
- [ ] Respondido por / data: 

---

## 25. [P4] regra comentada divergente da ativa

`SCRIPT-COMMENTED-LOGIC` &middot; categoria: `script-scaffolding` &middot; confianca da deteccao: **high**

### Pergunta

1. A regra comentada foi desativada de proposito ou e residuo de teste?
2. A migracao deve reproduzir o comportamento ATIVO (mais permissivo) ou o COMENTADO (restrito)?

### Por que isso importa

O corpo do script e logica que precisa ser reescrita; a traducao exige entender a intencao, nao so a sintaxe.

Ha logica comentada nos scripts. Em ao menos um caso a regra desativada testava codigos especificos e foi substituida por uma comparacao generica contra SW_NA - ou seja, o comportamento ATIVO e mais permissivo que o comentado. Precisa-se saber qual dos dois e a regra de negocio correta.

### Sugestao

Especificar o resultado esperado do script; a implementacao em C# fica a cargo da fase de construcao, validada por teste.

### Resposta

_Descrever o que o script deve garantir ao final, em termos de negocio._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.SCRIPT-COMMENTED-LOGIC`

- [ ] **intencao**: 
- [ ] **resultadoEsperado**: 
- [ ] Respondido por / data: 

---

## 26. [P4] controles .ascx referenciados e nao entregues

`SCREEN-MISSING-CONTROLS` &middot; categoria: `source-not-delivered` &middot; confianca da deteccao: **high**

### Pergunta

1. Os controles serao entregues ou a interface sera redesenhada do zero em .NET?

### Por que isso importa

O pacote referencia artefatos que nao vieram na entrega. O que eles fazem nao pode ser analisado nem reproduzido.

As telas registram estes controles de usuario, mas os arquivos nao vieram. A maior parte da interface das duas tarefas humanas esta dentro deles, entao o que o operador ve permanece desconhecido - o catalogo de telas so consegue descrever o contrato de work item.

### Sugestao

Pedir a entrega do pacote externo; se nao houver, declarar explicitamente a limitacao no relatorio da POC.

### Resposta

_Solicitar o arquivo ao cliente ou registrar por escrito que o item fica fora do escopo._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.SCREEN-MISSING-CONTROLS`

- [ ] **providencia**: 
- [ ] **foraDeEscopo**: 
- [ ] Respondido por / data: 

---

## 27. [P4] campo lido pela tela e ausente do dicionario

`SCREEN-UNDECLARED-FIELD` &middot; categoria: `source-not-delivered` &middot; confianca da deteccao: **high**

### Pergunta

1. De qual pacote vem cada campo, e qual o seu tipo e dominio?

### Por que isso importa

O pacote referencia artefatos que nao vieram na entrega. O que eles fazem nao pode ser analisado nem reproduzido.

A tela ASP.NET trava o work item pedindo estes campos, mas eles nao estao entre os campos de caso do pacote. Vem provavelmente de um dos pacotes externos nao entregues. Sem eles a tarefa humana nao pode ser reproduzida por completo.

### Sugestao

Pedir a entrega do pacote externo; se nao houver, declarar explicitamente a limitacao no relatorio da POC.

### Resposta

_Solicitar o arquivo ao cliente ou registrar por escrito que o item fica fora do escopo._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `rulings.SCREEN-UNDECLARED-FIELD`

- [ ] **providencia**: 
- [ ] **foraDeEscopo**: 
- [ ] Respondido por / data: 

---

## 28. [P4] AGPECASPC / _EvOwVF6eE

`DECISION-AGPECASPC-_EvOwVF6eE` &middot; categoria: `unlabeled-decision` &middot; confianca da deteccao: **high**

### Pergunta

1. Qual pergunta de negocio este gateway faz? (vira o rotulo do losango no BPMN)
2. Cada ramo esta rotulado com a resposta correspondente?

### Por que isso importa

O gateway nao tem nome no XPDL. No diagrama e um losango vazio, e ninguem aprova o que nao consegue ler.

Este ponto de decisao nao tem nome nenhum no XPDL - nem name, nem xpdExt:DisplayName. No diagrama BPMN ele aparece como um losango vazio, e nenhum revisor consegue aprovar o que nao consegue ler. O contexto abaixo (o que acontece antes e para onde cada ramo leva) existe para que se possa nomear a pergunta que este gateway faz.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 10684 | `_EvOwVF6eEfGJqLUhfbpFcQ` | AGPECASPC |

### Evidencia

- Chega de: scriptTask 'Controla Datas' ; scriptTask 'Set Values'
- Ramo [OTHERWISE] -> endEvent 'End Event'
- Ramo `DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZORECEBIMENT;` -> scriptTask 'SetPrazo'

### Sugestao

Derivar o nome a partir das condicoes dos ramos listados abaixo e validar com quem conhece o fluxo.

### Resposta

_Escrever a pergunta de negocio que este ponto faz e o rotulo de cada ramo._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `decisions.AGPECASPC/_EvOwVF6eE`

- [ ] **question**: 
- [ ] **branches**: 
- [ ] Respondido por / data: 

---

## 29. [P4] ATZINTPC / _RNdKGl6PE

`DECISION-ATZINTPC-_RNdKGl6PE` &middot; categoria: `unlabeled-decision` &middot; confianca da deteccao: **high**

### Pergunta

1. Qual pergunta de negocio este gateway faz? (vira o rotulo do losango no BPMN)
2. Cada ramo esta rotulado com a resposta correspondente?

### Por que isso importa

O gateway nao tem nome no XPDL. No diagrama e um losango vazio, e ninguem aprova o que nao consegue ler.

Este ponto de decisao nao tem nome nenhum no XPDL - nem name, nem xpdExt:DisplayName. No diagrama BPMN ele aparece como um losango vazio, e nenhum revisor consegue aprovar o que nao consegue ler. O contexto abaixo (o que acontece antes e para onde cada ramo leva) existe para que se possa nomear a pergunta que este gateway faz.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 9367 | `_RNdKGl6PEfGBBLgT-R5iuw` | ATZINTPC |

### Evidencia

- Chega de: serviceTask 'AtualizarIntimacao'
- Ramo [OTHERWISE] -> gateway «sem rotulo»
- Ramo `STATUS_CODE!="0";` -> scriptTask 'Set App Error'

### Sugestao

Derivar o nome a partir das condicoes dos ramos listados abaixo e validar com quem conhece o fluxo.

### Resposta

_Escrever a pergunta de negocio que este ponto faz e o rotulo de cada ramo._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `decisions.ATZINTPC/_RNdKGl6PE`

- [ ] **question**: 
- [ ] **branches**: 
- [ ] Respondido por / data: 

---

## 30. [P4] BSCENVPC / _qIDu4l6BE

`DECISION-BSCENVPC-_qIDu4l6BE` &middot; categoria: `unlabeled-decision` &middot; confianca da deteccao: **high**

### Pergunta

1. Qual pergunta de negocio este gateway faz? (vira o rotulo do losango no BPMN)
2. Cada ramo esta rotulado com a resposta correspondente?

### Por que isso importa

O gateway nao tem nome no XPDL. No diagrama e um losango vazio, e ninguem aprova o que nao consegue ler.

Este ponto de decisao nao tem nome nenhum no XPDL - nem name, nem xpdExt:DisplayName. No diagrama BPMN ele aparece como um losango vazio, e nenhum revisor consegue aprovar o que nao consegue ler. O contexto abaixo (o que acontece antes e para onde cada ramo leva) existe para que se possa nomear a pergunta que este gateway faz.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 5627 | `_qIDu4l6BEfGBBLgT-R5iuw` | BSCENVPC |

### Evidencia

- Chega de: serviceTask 'Busca Envolvidos Vista Por AIIM'
- Ramo `STATUS_CODE!="0";` -> scriptTask 'Set App Error'
- Ramo [OTHERWISE] -> gateway «sem rotulo»

### Sugestao

Derivar o nome a partir das condicoes dos ramos listados abaixo e validar com quem conhece o fluxo.

### Resposta

_Escrever a pergunta de negocio que este ponto faz e o rotulo de cada ramo._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `decisions.BSCENVPC/_qIDu4l6BE`

- [ ] **question**: 
- [ ] **branches**: 
- [ ] Respondido por / data: 

---

## 31. [P4] CALCPRPC / _zJIuclqiE

`DECISION-CALCPRPC-_zJIuclqiE` &middot; categoria: `unlabeled-decision` &middot; confianca da deteccao: **high**

### Pergunta

1. Qual pergunta de negocio este gateway faz? (vira o rotulo do losango no BPMN)
2. Cada ramo esta rotulado com a resposta correspondente?

### Por que isso importa

O gateway nao tem nome no XPDL. No diagrama e um losango vazio, e ninguem aprova o que nao consegue ler.

Este ponto de decisao nao tem nome nenhum no XPDL - nem name, nem xpdExt:DisplayName. No diagrama BPMN ele aparece como um losango vazio, e nenhum revisor consegue aprovar o que nao consegue ler. O contexto abaixo (o que acontece antes e para onde cada ramo leva) existe para que se possa nomear a pergunta que este gateway faz.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4517 | `_zJIuclqiEfG5K7mY0I3I6w` | CALCPRPC |

### Evidencia

- Chega de: serviceTask 'CalcularPrazo'
- Ramo `STATUS_CODE!="0";` -> scriptTask 'Set App Error'
- Ramo [OTHERWISE] -> gateway «sem rotulo»

### Sugestao

Derivar o nome a partir das condicoes dos ramos listados abaixo e validar com quem conhece o fluxo.

### Resposta

_Escrever a pergunta de negocio que este ponto faz e o rotulo de cada ramo._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `decisions.CALCPRPC/_zJIuclqiE`

- [ ] **question**: 
- [ ] **branches**: 
- [ ] Respondido por / data: 

---

## 32. [P4] CRNOTPC / _NcJxLl9KE

`DECISION-CRNOTPC-_NcJxLl9KE` &middot; categoria: `unlabeled-decision` &middot; confianca da deteccao: **high**

### Pergunta

1. Qual pergunta de negocio este gateway faz? (vira o rotulo do losango no BPMN)
2. Cada ramo esta rotulado com a resposta correspondente?

### Por que isso importa

O gateway nao tem nome no XPDL. No diagrama e um losango vazio, e ninguem aprova o que nao consegue ler.

Este ponto de decisao nao tem nome nenhum no XPDL - nem name, nem xpdExt:DisplayName. No diagrama BPMN ele aparece como um losango vazio, e nenhum revisor consegue aprovar o que nao consegue ler. O contexto abaixo (o que acontece antes e para onde cada ramo leva) existe para que se possa nomear a pergunta que este gateway faz.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 11225 | `_NcJxLl9KEfGqPfX31TKC3w` | CRNOTPC |

### Evidencia

- Chega de: serviceTask 'CriaNotificacao'
- Ramo [OTHERWISE] -> gateway «sem rotulo»
- Ramo `STATUS_CODE!="0";` -> scriptTask 'Set App Error'

### Sugestao

Derivar o nome a partir das condicoes dos ramos listados abaixo e validar com quem conhece o fluxo.

### Resposta

_Escrever a pergunta de negocio que este ponto faz e o rotulo de cada ramo._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `decisions.CRNOTPC/_NcJxLl9KE`

- [ ] **question**: 
- [ ] **branches**: 
- [ ] Respondido por / data: 

---

## 33. [P4] DEAT0050 / _lrer_VqhE

`DECISION-DEAT0050-_lrer_VqhE` &middot; categoria: `unlabeled-decision` &middot; confianca da deteccao: **high**

### Pergunta

1. Qual pergunta de negocio este gateway faz? (vira o rotulo do losango no BPMN)
2. Cada ramo esta rotulado com a resposta correspondente?

### Por que isso importa

O gateway nao tem nome no XPDL. No diagrama e um losango vazio, e ninguem aprova o que nao consegue ler.

Este ponto de decisao nao tem nome nenhum no XPDL - nem name, nem xpdExt:DisplayName. No diagrama BPMN ele aparece como um losango vazio, e nenhum revisor consegue aprovar o que nao consegue ler. O contexto abaixo (o que acontece antes e para onde cada ramo leva) existe para que se possa nomear a pergunta que este gateway faz.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 4005 | `_lrer_VqhEfG5K7mY0I3I6w` | DEAT0050 |

### Evidencia

- Chega de: scriptTask 'HoraFimSC'
- Ramo `DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZODEFESA;` -> timerEvent 'Aguarda Defesa'
- Ramo [OTHERWISE] -> endEvent «sem rotulo»
- Intencao no documento da POC: etapa 2 "Notificação do AIIM" (casou por "Notificacao do AIIM")

### Sugestao

Derivar o nome a partir das condicoes dos ramos listados abaixo e validar com quem conhece o fluxo.

### Resposta

_Escrever a pergunta de negocio que este ponto faz e o rotulo de cada ramo._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `decisions.DEAT0050/_lrer_VqhE`

- [ ] **question**: 
- [ ] **branches**: 
- [ ] Respondido por / data: 

---

## 34. [P4] PRPINTPC / _KEwDVl6EE

`DECISION-PRPINTPC-_KEwDVl6EE` &middot; categoria: `unlabeled-decision` &middot; confianca da deteccao: **high**

### Pergunta

1. Qual pergunta de negocio este gateway faz? (vira o rotulo do losango no BPMN)
2. Cada ramo esta rotulado com a resposta correspondente?

### Por que isso importa

O gateway nao tem nome no XPDL. No diagrama e um losango vazio, e ninguem aprova o que nao consegue ler.

Este ponto de decisao nao tem nome nenhum no XPDL - nem name, nem xpdExt:DisplayName. No diagrama BPMN ele aparece como um losango vazio, e nenhum revisor consegue aprovar o que nao consegue ler. O contexto abaixo (o que acontece antes e para onde cada ramo leva) existe para que se possa nomear a pergunta que este gateway faz.

### Onde olhar

| Arquivo | Linha | Elemento | Processo |
|---|---:|---|---|
| `input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl` | 7076 | `_KEwDVl6EEfGBBLgT-R5iuw` | PRPINTPC |

### Evidencia

- Chega de: serviceTask 'CaptaParametros'
- Ramo `STATUS_CODE!=IPESystemValues.SW_NA;` -> scriptTask 'Set App Error'
- Ramo [OTHERWISE] -> gateway «sem rotulo»

### Sugestao

Derivar o nome a partir das condicoes dos ramos listados abaixo e validar com quem conhece o fluxo.

### Resposta

_Escrever a pergunta de negocio que este ponto faz e o rotulo de cada ramo._

Onde registrar: `config/glossary/POC_Epat.yaml` -> `decisions.PRPINTPC/_KEwDVl6EE`

- [ ] **question**: 
- [ ] **branches**: 
- [ ] Respondido por / data: 

---

