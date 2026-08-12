# Dossie de validacao - POC_Epat

Para os programadores que mantem o ePAT no TIBCO.

## O que se pede

Analisamos o pacote exportado e tomamos 34 decisoes sobre como o comportamento actual
deve ser reproduzido. Nao conhecemos o sistema em producao; voces conhecem.

**Nao e preciso rever tudo.** Cada item esta separado em tres partes:

| Parte | O que e | Precisa da vossa atencao? |
|---|---|---|
| **Prova** | o que o proprio pacote diz, extraido mecanicamente | so uma olhada; se estiver errado e porque lemos mal o ficheiro |
| **Decisao** | o que ficou decidido a partir dessa prova | **sim - e isto que precisa de conferencia** |
| **Risco** | o que parte se a decisao estiver errada | define a ordem: os primeiros itens sao os que doem |

Marque cada item com **confere** ou **nao confere**. Onde nao conferir, uma frase a dizer o que e
na realidade chega - nos tratamos do resto.

A ordem NAO e por assunto: e por risco. Quem tiver meia hora, le do principio e ja cobriu o essencial.

---

## Parte 1 - o que so voces conseguem responder

Estes 5 pontos ficaram por fechar porque a resposta nao esta no pacote exportado.
Nao sao conferencias: sao perguntas.

### 1.1  graft-step

POR DEFINIR na implementacao: a chave de correlacao formal e o criterio de encerramento, incluindo timeout para filho que nunca termina - hoje ambos sao implicitos na identidade do caso iProcess.

- No TIBCO: POC_EpatProcess / Inicia Graft Step (linha 1338)
- No TIBCO: POC_EpatProcess / Iniciar Novo Graft (linha 3032)

**Resposta:** 

### 1.2  rotulo aponta para outro campo

PENDENTE DE RATIFICACAO pela SEFAZ: sao deducoes, nao confirmacao.

- No TIBCO: NR_RATORIG
- No TIBCO: STSPETICAO

**Resposta:** 

### 1.3  expression-deadline

POR CONFIRMAR: fuso horario (assumido America/Sao_Paulo) e se UseWorkingDays=true do iProcess afecta o calculo.

- No TIBCO: DEAT0050 / Aguarda Defesa (linha 3872)
- No TIBCO: POC_EpatProcess / Fim de Prazo Mantendo Atividade (linha 2269)

**Resposta:** 

### 1.4  external-event

POR DEFINIR: proteccao do endpoint de retomada, e politica de idempotencia para entrega duplicada ou resposta atrasada - o teste de evento duplicado e exigido pela etapa 5 do plano de cumprimento.

- No TIBCO: AGPECASPC / Aguardar Interposicoes (linha 10506)
- No TIBCO: DEAT0050 / INICALC (linha 3982)
- No TIBCO: POC_EpatProcess / Iniciar Aguardar Notificacao (linha 1370)
- No TIBCO: POC_EpatProcess / Iniciar Novo Graft (linha 3032)

**Resposta:** 

### 1.5  non-interrupting-boundary

POR CONFIRMAR com o negocio: quem recebe o aviso, e se a actividade hospedeira pode mesmo terminar normalmente depois de o aviso ter disparado.

- No TIBCO: POC_EpatProcess / Fim de Prazo Mantendo Atividade (linha 2269)

**Resposta:** 

---

## Parte 2 - decisoes a conferir

### 2.1  pacotes externos referenciados e nao entregues

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 15 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> Substituir por dubles com contrato acordado, sem esperar pelos ficheiros. Cada duble e tipado a partir do WSDL ou da ProcessInterface correspondente e e conduzido por cenario.

Decidido em 2026-08-06. Nao e lacuna de analise, e lacuna de entrega, e nenhuma analise adicional a resolve. CONSEQUENCIA A ASSUMIR: ficou confirmado que os 6 destinos em falta de AGUARDAR (AgPRJ, AgRecPRJ, AgPRJR, AgRCRaz, AgCRaz, AgPetica) estao nestes pacotes, logo sao 6 dubles a construir, alem do AGPECASPC entregue. NAO SILENCIAR A FALHA: o legado declara HaltOnBadSubProcess='false' e falha em silencio; a migracao NAO deve herdar isso - destino sem duble tem de falhar de forma visivel, o que o registo validado em arranque (gaps.dynamic-subprocess) ja garante. POR REGISTAR na documentacao da POC: a lista dos dubles construidos e o contrato de cada um, como limitacao explicita e assumida.

**Se estiver errado:** Descobrir a meio da demonstracao que uma etapa chama um processo inexistente. E pior do que parece: o legado declara HaltOnBadSubProcess="false", ou seja, falha em silencio - a migracao pode herdar o mesmo comportamento e ninguem repara.

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.2  PRPINTPC

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- So em PRPINTPC: `STATUS_CODE!=IPESystemValues.SW_NA;`
- Nos processos irmaos: `STATUS_CODE!="0";`

**Decisao** _(e isto que precisa de conferencia)_

> Defeito de copia. Corrigir na migracao para STATUS_CODE != '0', alinhando com os quatro irmaos, e reportar a SEFAZ para correccao na origem.

Decidido em 2026-08-06. Os cinco gateways tem forma identica: ramo 'Good' sem condicao (default) e ramo 'AppError' para 'Set App Error'. So a condicao difere, e so num deles. Efeito em execucao: no PRPINTPC um STATUS_CODE = '0' (sucesso) e diferente de SW_NA, logo dispara o ramo de erro - a condicao esta invertida em relacao ao proposito. A unica leitura em que funcionaria e se CaptaParametros nunca preenchesse STATUS_CODE em sucesso, mas nesse caso seria estranho que so a condicao do gateway tivesse sido adaptada e nao o resto do template partilhado. ATENCAO NA MIGRACAO: corrigir muda comportamento observado - casos que hoje passam em silencio com erro passam a parar. Precisa de nota na demonstracao.

**Se estiver errado:** Se for intencional e for corrigido, uma condicao de erro que hoje e ignorada passa a interromper o fluxo, e casos que hoje passam comecam a parar.

**Onde ver no TIBCO**

- declarado na linha 6377

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.3  semantica dos builtins iProcess nao confirmada

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 7 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> Consultar a documentacao TIBCO iProcess e fixar a semantica por escrito antes de implementar.

Decidido em 2026-08-06. Nao se assume base-1 por inferencia: o vector de teste de IDSINTIMADOS e compativel com base-1 mas nao a prova, e um desvio de uma posicao em SUBSTR nao falha, devolve valor errado. TRES PONTOS A FIXAR NA DOCUMENTACAO: (1) SUBSTR e SEARCH sao base 1 ou base 0; (2) o terceiro argumento de SUBSTR e COMPRIMENTO ou POSICAO FINAL - as duas chamadas existentes nao distinguem os casos; (3) o que SEARCH devolve quando o separador nao existe e o que SUBSTR faz com comprimento negativo. BLOQUEIA: prepSub do POC_EpatProcess, que e o script que alimenta o graft step - usa SEARCH, SUBSTR e STRLEN para partir IDSINTIMADOS. Ate estar fixado, qualquer implementacao do ciclo de intimados e palpite.

**Se estiver errado:** Um desvio de uma posicao em SUBSTR nao gera erro: gera um valor errado que segue pelo fluxo. E a classe de defeito mais dificil de detectar depois, porque nao falha, so mente.

**Onde ver no TIBCO**

- IPEConversionUtil.DATESTR
- IPEConversionUtil.NUM
- IPEConversionUtil.STR
- IPEDateTimeUtil.CALCTIME
- IPEStringUtil.SEARCH
- IPEStringUtil.STRLEN
- _(+ 1 outro(s))_

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.4  dynamic-subprocess

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 3 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> interface-registry-validated

Ratificado em 2026-08-06. O xpdExt:ProcessInterface do TIBCO ja e a interface: NOTFAIIM (l.12028) -> DEAT0050, CTRINTPC (l.12179) -> CONTROPC, AGURETPC (l.12463) -> AGPECASPC. A traducao e transcricao, nao invencao. RESSALVA: o campo AGUARDAR recebe 7 valores em CONTROPC/ISetSubProc (AgPRJ, AgRecPRJ, AgPRJR, AgPecas, AgRCRaz, AgCRaz, AgPetica) e so 1 implementacao de AGURETPC foi entregue - confirmado que os outros 6 processos estao nos pacotes externos nao entregues. O conjunto NAO e fechado, e por isso o registo validado em arranque e a escolha certa: torna a falta visivel no CI em vez de em producao. closed-switch foi recusada exactamente por isso.

**Se estiver errado:** Se houver implementacoes noutros pacotes, o registo validado em arranque rejeita um destino legitimo. E um erro visivel e barato de corrigir - preferivel ao inverso.

**Onde ver no TIBCO**

- CONTROPC / Aguardar Retorno (linha 8672)
- POC_EpatProcess / Aguardar evento de Notificacao do AIIM (linha 1350)
- POC_EpatProcess / Controlar Intimados (linha 2721)

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.5  expression-deadline

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 4 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> absolute-instant

Ratificado em 2026-08-06. recompute-on-resume chegou a ser escolhido e foi revertido no mesmo dia: obrigava a definir a politica para o instante recalculado que ja passou (dispara / ignora / escala), e essa politica nao existe no legado nem no documento da POC - ficaria a ser inventada em codigo. absolute-instant combina o par data+hora num DateTime absoluto no momento do agendamento. RISCO RESIDUAL ASSUMIDO: o timer nao acompanha prorrogacao do prazo feita depois do agendamento. MITIGACAO A IMPLEMENTAR: rearmar o temporizador sempre que o campo de prazo for escrito, o que cobre o caso real sem inventar politica para instante no passado. POR CONFIRMAR: fuso horario (assumido America/Sao_Paulo) e se UseWorkingDays=true do iProcess afecta o calculo.

**Se estiver errado:** Tratar como duracao faz o prazo disparar no momento errado. Num processo administrativo fiscal, um prazo que dispara cedo ou tarde tem consequencia legal, nao apenas tecnica.

**Onde ver no TIBCO**

- DEAT0050 / Aguarda Defesa (linha 3872)
- POC_EpatProcess / Fim de Prazo Mantendo Atividade (linha 2269)

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.6  graft-step

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 3 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> correlation-join

Ratificado em 2026-08-06. O contrato fica do lado do pai: o filho apenas sinaliza, o que evita obrigar processos de pacotes externos a registarem-se. child-registry foi recusada por empurrar contrato para os filhos. one-to-one-call foi recusada por nao demonstrar o conceito, que o cliente colocou em escopo a 2026-08-05. DECIDIDO TAMBEM: as duas valvulas de reinicio manual - 'Iniciar Aguardar Notificacao' (l.1370) e 'Iniciar Novo Graft' (l.3032), ambas TaskReceive - ficam EM ESCOPO, por serem hoje o unico mecanismo de recuperacao do graft. POR DEFINIR na implementacao: a chave de correlacao formal e o criterio de encerramento, incluindo timeout para filho que nunca termina - hoje ambos sao implicitos na identidade do caso iProcess.

**Se estiver errado:** Implementar como chamada simples de subprocesso faz a notificacao a multiplos solidarios deixar de funcionar - e esse e o conceito de destaque da etapa 2 da PoC.

**Onde ver no TIBCO**

- POC_EpatProcess / Inicia Graft Step (linha 1338)
- POC_EpatProcess / Iniciar Novo Graft (linha 3032)

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.7  iprocess-builtin

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 17 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> shim-tri-state

Ratificado em 2026-08-06. SW_NA e um terceiro estado distinto de null e de vazio, usado por 18 campos. O tipo tri-estado obriga o compilador a exigir a decisao em cada uso, atraves de pattern matching exaustivo. map-to-null foi recusada porque exigiria provar, campo a campo, que nenhum dos 18 e legitimamente nulo - e onde a prova falhasse o ramo trocado nao daria erro visivel. preserve-literal foi recusada por propagar tipagem fraca para todo o modelo de dominio.

**Se estiver errado:** Cada campo com SW_NA tem tres caminhos possiveis. Colapsar dois deles muda o comportamento em pontos dispersos, sem erro de compilacao e sem teste vermelho.

**Onde ver no TIBCO**

- AGPECASPC / Set Values (linha 10549)
- ATZINTPC / SetParameters (linha 9622)
- ATZINTPC / Start Loop (linha 9684)
- BSCENVPC / SetParameters (linha 5832)
- BSCENVPC / Start Loop (linha 5894)
- CALCPRPC / SetParameters (linha 4730)
- _(+ 11 outro(s))_

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.8  non-interrupting-boundary

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 1 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> parallel-branch

Ratificado em 2026-08-06. external-subscription chegou a ser escolhido e foi revertido no mesmo dia: o catalogo regista que com ele o ramo lateral deixa de aparecer no diagrama do processo, e a rastreabilidade visual e um objectivo declarado da POC - um comportamento que funciona mas nao se ve nao serve para demonstrar aderencia funcional. parallel-branch mantem o ramo dentro do escopo, visivel no diagrama, e resolve de graca a limpeza exigida pela etapa 7 do plano: a subscricao morre com o escopo, em vez de ficar orfa. POR CONFIRMAR com o negocio: quem recebe o aviso, e se a actividade hospedeira pode mesmo terminar normalmente depois de o aviso ter disparado.

**Se estiver errado:** Implementar como interruptiva cancela trabalho em curso quando o prazo passa - perda de trabalho do utilizador.

**Onde ver no TIBCO**

- POC_EpatProcess / Fim de Prazo Mantendo Atividade (linha 2269)

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.9  rotulo aponta para outro campo

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 2 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> Recusar os dois rotulos do formulario e nomear por deducao: NR_RATORIG = 'Numero do RAT original', STSPETICAO = 'Status da Peticao'. Reportar o defeito a SEFAZ para correccao na origem.

Decidido em 2026-08-06. NR_RATORIG recebeu como rotulo o nome de outro campo existente (NR_RAT); STSPETICAO recebeu 'StatusPeticao', que colide com STATUSPETICAO. Aceitar qualquer um renomearia o campo errado no modelo .NET, e o erro so apareceria em producao. Os nomes deduzidos derivam do proprio identificador (RATORIG = RAT original; STS = status) e nao de interpretacao de negocio. PENDENTE DE RATIFICACAO pela SEFAZ: sao deducoes, nao confirmacao.

**Se estiver errado:** Aceitar o rotulo renomeia o campo errado no modelo .NET. O codigo compila, os testes passam, e o ecra mostra um rotulo que descreve outro dado.

**Onde ver no TIBCO**

- NR_RATORIG
- STSPETICAO

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.10  tipo divergente entre XPDL e formulario

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 14 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> A precisao do XPDL prevalece. Os 14 campos passam a long (Int64) no modelo .NET. Sem excepcoes.

Decidido em 2026-08-06, como decisao de padrao unica e nao campo a campo. Alargar nunca trunca; estreitar trunca em silencio. Caso decisivo: SW_CASENUMPOC tem precisao 15, que nao cabe em Int32 de forma nenhuma - se houvesse duvida sobre os outros, este resolve-a. Os restantes sao identificadores e contadores que crescem com o tempo (IDAIIM, NR_AIIM, NR_RAT, NR_RATORIG, IDAIIMORIGINAL com precisao 11; QTDINTIMADOS, CDIMPOSTO e os auxiliares de string com precisao 10). CONSEQUENCIA: o contrato com a tela ASP.NET muda, porque o formulario REALATVI declara Integer - a conversao tem de ser explicita na fronteira, e um valor que nao caiba deve falhar de forma visivel em vez de truncar.

**Se estiver errado:** Adoptar int e ver o identificador exceder o limite so acontece anos depois, e quando acontece corrompe o numero sem erro visivel.

**Onde ver no TIBCO**

- IDAIIM
- NRAIIM
- NR_RATORIG
- QTDINTIMADOS
- SW_CASENUMPOC
- NR_AIIM
- _(+ 8 outro(s))_

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.11  valores fixos embutidos em scriptTask

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 4 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> Tratamento diferente por valor. (1) IDSINTIMADOS = '278713\|278712\|' no fim do prepSub: REMOVER, e andaime de teste. (2) CCRELATORIO e BCCRELATORIO: EXTERNALIZAR em configuracao por ambiente. (3) STATUSSUBPROC = 'inativo': MANTER, e constante de dominio. (4) atalho de prazo do DEAT0050/HoraFimSC: REMOVER, a demonstracao usa relogio controlavel nos testes.

Decidido em 2026-08-06, valor a valor. IDSINTIMADOS: a atribuicao esta DEPOIS do ciclo que consome a lista, logo nao afecta a execucao corrente mas contamina a seguinte; ao lado ha um comentario com mais vinte ids que alguem foi cortando - e o registo de uma sessao de testes. Serve de oraculo: a lista '278713\|278712\|' com QTDINTIMADOS=2 e um caso pronto para validar o graft step com dois filhos. DESTINATARIOS: o script ja e configuracao por ambiente feita a mao - 'if (SW_HOSTNAME == prod1)' envia copia, senao poe SW_NA; alem disso acsimoes@ e endereco nominal de uma pessoa, logo o processo deixa de avisar quem devia se ela sair. Passa a configuracao por ambiente, que e o que o autor tentou fazer. STATUSSUBPROC: passo de uma linha so, transicao de estado do dominio, nao andaime; NOTA para a implementacao - 'inativo' deve virar valor de enumeracao, nao string literal. ATALHO des1 (achado em 2026-08-07, NAO estava na lista original dos quatro porque o detector so apanha atribuicao de literal): DEAT0050/HoraFimSC tem 'if (SW_HOSTNAME == des1) { PRAZODEFESA = SW_DATE; PRAZODEFESAT = CALCTIME(SW_TIME,1,0,DAYSOVER) }', que encurta o prazo de defesa para daqui a uma hora em ambiente de desenvolvimento. Decidido REMOVER: a demonstracao usa relogio controlavel nos testes, nao prazos encurtados por nome de maquina.

**Se estiver errado:** Se for reproduzido fielmente, toda a demonstracao notifica sempre os mesmos dois solidarios, independentemente do AIIM - e o graft step, que e conceito de destaque da PoC, seria demonstrado com dados falsos.

**Onde ver no TIBCO**

- POC_EpatProcess / Define Destinatarios
- POC_EpatProcess / prepSub
- CONTROPC / Desativa Subs

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.12  external-event

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 6 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> bookmark-correlation

Ratificado em 2026-08-06. queue-saga chegou a ser escolhido e foi revertido no mesmo dia: exigia infraestrutura de mensageria adicional, fora do escopo declarado da POC, e ninguem estava designado para a provisionar na demonstracao. bookmark-correlation usa o modelo de longa duracao do proprio motor, sem infraestrutura extra. Chave de correlacao ja existe e nao precisa de ser inventada: PROCESS_ID = 'idAiim-<n>idProc-<n>', montado pelos scripts antes de cada chamada. POR DEFINIR: proteccao do endpoint de retomada, e politica de idempotencia para entrega duplicada ou resposta atrasada - o teste de evento duplicado e exigido pela etapa 5 do plano de cumprimento.

**Se estiver errado:** Sem idempotencia, uma entrega duplicada faz o caso avancar duas vezes. Com o graft step envolvido, pode gerar notificacoes a mais.

**Onde ver no TIBCO**

- AGPECASPC / Aguardar Interposicoes (linha 10506)
- DEAT0050 / INICALC (linha 3982)
- POC_EpatProcess / Iniciar Aguardar Notificacao (linha 1370)
- POC_EpatProcess / Iniciar Novo Graft (linha 3032)
- POC_EpatProcess / Pedido de Vistas (linha 1535)
- POC_EpatProcess / Vistas do Juiz (linha 1608)

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.13  link-goto

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 20 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> flatten-edge

Ratificado em 2026-08-06, de acordo com a sugestao. Os 10 pares throw/catch ja estao resolvidos em derived.linkEdges e nenhum atravessa fronteira de processo. keep-as-signal foi recusada por introduzir pontos de persistencia e espera que o TIBCO nao tem: o motor passaria a parar onde o original nao parava.

**Se estiver errado:** Baixo. O pior caso e um diagrama mais dificil de ler.

**Onde ver no TIBCO**

- ATZINTPC / Link To: Try Task (linha 9860)
- ATZINTPC / Try Task (linha 9737)
- BSCENVPC / Link To: Try Task (linha 6070)
- BSCENVPC / Try Task (linha 5947)
- CALCPRPC / Link To: Try Task (linha 4959)
- CALCPRPC / Try Task (linha 4835)
- _(+ 10 outro(s))_

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.14  ISAPPERROR

**Decisao** _(e isto que precisa de conferencia)_

> Indicador de erro de aplicacao

Marca que a chamada de servico falhou por regra de negocio, por oposicao a falha de infraestrutura. E esta distincao que decide se o caso volta ao laco de retry ou vai para tratamento manual: erro de negocio nao se resolve repetindo a chamada. Confirmado em 2026-08-06 que 'N' e ausencia de erro, e nao 'ainda nao avaliado'.

**Se estiver errado:** Se 'N' significar 'ainda nao avaliado', o ramo de sucesso dispara antes de haver resposta do servico, e o caso avanca sem ter sido processado.

**Onde ver no TIBCO**

- CALCPRPC / gateway 'App Error' (linha 5024)
- BSCENVPC / gateway 'App Error' (linha 6135)
- PRPINTPC / gateway 'App Error' (linha 7685)
- ATZINTPC / gateway 'App Error' (linha 9925)
- CRNOTPC / gateway 'App Error' (linha 11783)

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.15  ISTECHERROR

**Decisao** _(e isto que precisa de conferencia)_

> Indicador de erro tecnico

Marca que a chamada de servico falhou por infraestrutura - fila, rede, indisponibilidade - e nao por regra de negocio. E o par de ISAPPERROR: so o erro tecnico e retentavel automaticamente.

**Se estiver errado:** Repetir automaticamente uma chamada que falhou por regra de negocio consome as tentativas todas sem hipotese de sucesso, e atrasa a entrada em tratamento manual.

**Onde ver no TIBCO**

- CALCPRPC / gateway 'Tech Error' (linha 5044)
- BSCENVPC / gateway 'Tech Error' (linha 6155)
- PRPINTPC / gateway 'Tech Error' (linha 7705)
- ATZINTPC / gateway 'Tech Error' (linha 9945)
- CRNOTPC / gateway 'Tech Error' (linha 11803)

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.16  OUTCOME

**Decisao** _(e isto que precisa de conferencia)_

> Decisao do operador na excepcao

Decisao que uma pessoa toma quando o laco de retry se esgota e o caso cai em tratamento manual. Lido nos passos Try Again e Manually Fixed, que ficam depois do formulario MANEXC. Os dois literais NAO sao divergencia entre clones: sao passos diferentes do mesmo laco.

**Se estiver errado:** Um terceiro valor nao previsto cai no ramo por omissao e o caso segue como se tivesse sido resolvido, sem o ter sido.

**Onde ver no TIBCO**

- CALCPRPC / gateway 'Manually Fixed' (linha 4778)
- CALCPRPC / gateway 'Try Again' (linha 4867)
- BSCENVPC / gateway 'Manually Fixed' (linha 5874)
- BSCENVPC / gateway 'Try Again' (linha 5979)
- PRPINTPC / gateway 'Manually Fixed' (linha 7409)
- PRPINTPC / gateway 'Try Again' (linha 7529)
- _(+ 4 outro(s))_

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.17  AGPECASPC / _EvOwVF6eE

**Decisao** _(e isto que precisa de conferencia)_

> Ja se esperou pelo prazo em vigor?

**Onde ver no TIBCO**

- AGPECASPC

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.18  ATZINTPC / _RNdKGl6PE

**Decisao** _(e isto que precisa de conferencia)_

> A chamada a AtualizarIntimacao foi bem sucedida?

**Onde ver no TIBCO**

- ATZINTPC

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.19  BSCENVPC / _qIDu4l6BE

**Decisao** _(e isto que precisa de conferencia)_

> A chamada a Busca Envolvidos Vista Por AIIM foi bem sucedida?

**Onde ver no TIBCO**

- BSCENVPC

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.20  CALCPRPC / _zJIuclqiE

**Decisao** _(e isto que precisa de conferencia)_

> A chamada a CalcularPrazo foi bem sucedida?

**Onde ver no TIBCO**

- CALCPRPC

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.21  campo lido pela tela e ausente do dicionario

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 1 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> Limitacao assumida, pela mesma razao do SCREEN-MISSING-CONTROLS: o front-end esta diferido para o MVP.

Decidido em 2026-08-07. E um unico campo, lido pela tela e ausente do dicionario de 209 campos - quase de certeza declarado num dos 15 pacotes externos nao entregues, que ja foram tratados em rulings.MISSING-EXTERNAL-PACKAGES com decisao de simular por dubles. Nao bloqueia nenhuma das sete etapas da POC.

**Onde ver no TIBCO**

- CPFCNPJNOTIFICA

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.22  controles .ascx referenciados e nao entregues

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 5 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> Limitacao assumida. Os 5 controlos .ascx nao sao pedidos nem reconstruidos: o front-end esta diferido para o MVP.

Decidido em 2026-08-07, coerente com a decisao de escopo ja tomada de diferir backend e frontend para o MVP. A POC valida a aderencia funcional do motor BPM, nao a interface. O que interessa das telas ja foi extraido e nao depende dos .ascx: o contrato tela-processo (que campos sao escritos de volta no motor) e as 154 decisoes do code-behind. FICA REGISTADO: Pecas.ascx, Cabecalho_AIIM.ascx, AdicionarPecas.ascx, Cabecalho_AIIM_DEAT.ascx e o quinto controlo continuam por entregar, e qualquer regra que viva dentro deles NAO foi analisada.

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.23  CRNOTPC / _NcJxLl9KE

**Decisao** _(e isto que precisa de conferencia)_

> A chamada a CriaNotificacao foi bem sucedida?

**Onde ver no TIBCO**

- CRNOTPC

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.24  DATACONTROLE, PRAZODEFESA

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Condicao no XPDL: `DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZODEFESA;`

**Decisao** _(e isto que precisa de conferencia)_

> Mesma decisao do SENTINEL-AGPECASPC: SW_NA e a primeira volta do laco de prazo. Segue junto com 'prazo mudou', unido pelo \|\|.

Decidido em 2026-08-07. Ciclo identico ao do AGPECASPC, com um passo a mais: [gateway] -> Aguarda Defesa (timer) -> Controlar Data (DATACONTROLE = PRAZODEFESA) -> CalculaPrazo (chama CALCPRPC) -> HoraFimSC -> [gateway]. O CALCPRPC RECALCULA o prazo a cada volta, e se ele mudou o processo volta a esperar: e um laco de PRORROGACAO de prazo. ISTO VALIDA a decisao tomada em gaps.expression-deadline: escolhemos absolute-instant e registamos como mitigacao 'rearmar o temporizador quando o prazo mudar' - o legado ja faz isso, nao rearmando mas voltando ao gateway e criando um timer novo. A mitigacao e transcricao, nao invencao. DATACONTROLE e a memoria que impede o ciclo infinito.

**Onde ver no TIBCO**

- DEAT0050 / gateway «sem rotulo»

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.25  DATACONTROLE, PRAZORECEBIMENT

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Condicao no XPDL: `DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZORECEBIMENT;`

**Decisao** _(e isto que precisa de conferencia)_

> SW_NA significa PRIMEIRA VOLTA do laco de prazo: ainda nao se esperou por prazo nenhum. Segue junto com 'prazo mudou', unido pelo \|\|, exactamente como no legado. Nao merece ramo proprio.

Decidido em 2026-08-07. O ciclo e: [gateway] -> SetPrazo -> Aguardar Interposicoes -> Controla Datas -> [gateway]. O 'Controla Datas' faz DATACONTROLE = PRAZORECEBIMENT, ou seja, DATACONTROLE guarda o prazo pelo qual JA se esperou e PRAZORECEBIMENT e o prazo actual; quando coincidem, o ciclo termina. Se SW_NA saisse do ciclo em vez de entrar, o processo nunca esperaria, porque na primeira volta o campo esta sempre por preencher. IMPLEMENTACAO: DATACONTROLE e DateOnly com sentinela; o estado 'nunca esperou' tem de ser representavel e distinto de qualquer data valida - e o caso de uso directo do tipo tri-estado escolhido em gaps.iprocess-builtin.

**Onde ver no TIBCO**

- AGPECASPC / gateway «sem rotulo»

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.26  DEAT0050 / _lrer_VqhE

**Decisao** _(e isto que precisa de conferencia)_

> Ja se esperou pelo prazo em vigor?

**Onde ver no TIBCO**

- DEAT0050

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.27  MAXRETRIES

**Decisao** _(e isto que precisa de conferencia)_

> Tecto de tentativas

Numero maximo de tentativas do laco de retry das chamadas de servico. Parametro de resiliencia, sem significado de negocio. Comparado contra DOIS contadores independentes: NUMAPPRETRIES (falhas de aplicacao) e IPESystemValues.SW_QRETRYCOUNT (falhas de entrega da fila) - confirmado em 2026-08-06 que sao mesmo independentes e devem ser reproduzidos tal e qual.

**Se estiver errado:** Se na producao o valor vier de configuracao e for diferente, o numero de tentativas muda e com ele o tempo ate o caso cair no tratamento manual. Nao quebra o fluxo, altera o SLA.

**Onde ver no TIBCO**

- CALCPRPC / gateway 'More Retries' (linha 4971)
- CALCPRPC / gateway 'Check Retries SW_QRETRYCOUNT' (linha 4426)
- BSCENVPC / gateway 'More Retries' (linha 6082)
- BSCENVPC / gateway 'Check Retries SW_QRETRYCOUNT' (linha 5522)
- PRPINTPC / gateway 'More Retries' (linha 7632)
- PRPINTPC / gateway 'Check Retries SW_QRETRYCOUNT' (linha 6971)
- _(+ 4 outro(s))_

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.28  NUMAPPRETRIES

**Decisao** _(e isto que precisa de conferencia)_

> Contador de tentativas de aplicacao

Conta as falhas de APLICACAO ja tentadas no laco de retry, distinto do contador tecnico do motor (SW_QRETRYCOUNT). Inicializado no passo Start Loop com 'if (NUMAPPRETRIES == null)' e comparado no passo More Retries contra MAXRETRIES.

**Se estiver errado:** Se forem o mesmo conceito duplicado por engano, o numero real de tentativas em .NET pode chegar ao dobro do pretendido.

**Onde ver no TIBCO**

- CALCPRPC / gateway 'More Retries' (linha 4971)
- BSCENVPC / gateway 'More Retries' (linha 6082)
- PRPINTPC / gateway 'More Retries' (linha 7632)
- ATZINTPC / gateway 'More Retries' (linha 9872)
- CRNOTPC / gateway 'More Retries' (linha 11730)

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.29  PRPINTPC / _KEwDVl6EE

**Decisao** _(e isto que precisa de conferencia)_

> A chamada a CaptaParametros foi bem sucedida?

**Onde ver no TIBCO**

- PRPINTPC

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.30  regra comentada divergente da ativa

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 3 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> A regra ATIVA prevalece. O codigo comentado e historico e nao migra.

Decidido em 2026-08-07. A migracao reproduz o que o sistema faz hoje, nao o que fez em alguma versao anterior. Nos tres casos o comentado e mais restritivo - por exemplo, no prepSub a versao comentada filtrava CNTPECA1 por uma lista de codigos ('110','24','53','55','22') e a activa aceita qualquer valor diferente de SW_NA. NOTA: o codigo comentado fica registado neste artefacto como evidencia; se a SEFAZ confirmar que a restricao devia estar activa, e uma alteracao de regra e nao um defeito de migracao.

**Onde ver no TIBCO**

- POC_EpatProcess / prepSub

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.31  rotulos de formulario nao verificados

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Ocorre em 21 ponto(s).

**Decisao** _(e isto que precisa de conferencia)_

> Aceitar em bloco os 21 rotulos do formulario como termo de negocio, corrigindo os erros de escrita evidentes (ex.: 'Contorle' -> 'Controle'). Havendo divergencia futura, o vocabulario oficial da SEFAZ prevalece sobre o rotulo do formulario.

Decidido em 2026-08-07. Sao a unica fonte de nome de negocio que o pacote oferece, e sem eles o modelo .NET fica com os identificadores truncados a 15 caracteres do iProcess. Risco baixo e reversivel: renomear uma propriedade e refactoring mecanico. EXCLUIDOS deste bloco os dois rotulos que apontam para o nome de OUTRO campo - esses foram recusados em rulings.LABEL-CONFLICT e nomeados por deducao.

**Onde ver no TIBCO**

- AFR
- CD_DRT
- CDIMPOSTO
- CODMUNAIIM
- DATAENCPREPNOT
- DESCREGRA
- _(+ 15 outro(s))_

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.32  STATUS_CODE

**Decisao** _(e isto que precisa de conferencia)_

> Codigo de retorno do servico

Codigo devolvido pelo envelope tecnico do BusinessWorks a cada chamada de servico. Decide o desvio para o ramo AppError em cinco subprocessos.

**Se estiver errado:** No PRPINTPC, um servico que devolva erro com codigo preenchido passa no teste 'diferente de nao preenchido' e o fluxo segue como se tivesse corrido bem. O erro fica invisivel.

**Onde ver no TIBCO**

- CALCPRPC / gateway «sem rotulo» (linha 4517)
- BSCENVPC / gateway «sem rotulo» (linha 5627)
- PRPINTPC / gateway «sem rotulo» (linha 7076)
- ATZINTPC / gateway «sem rotulo» (linha 9367)
- CRNOTPC / gateway «sem rotulo» (linha 11225)

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.33  STATUS_CODE

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Condicao no XPDL: `STATUS_CODE!=IPESystemValues.SW_NA;`

**Decisao** _(e isto que precisa de conferencia)_

> Nao ha decisao a tomar aqui: a sentinela DESAPARECE. Ver rulings.CLONE-PRPINTPC, onde ficou decidido corrigir a condicao para STATUS_CODE != '0', alinhando com os quatro processos irmaos.

Decidido em 2026-08-07. Esta sentinela e um sintoma do defeito de copia, nao uma regra: o PRPINTPC compara STATUS_CODE com SW_NA enquanto ATZINTPC, BSCENVPC, CALCPRPC e CRNOTPC comparam com '0'. Corrigida a condicao, o teste de SW_NA deixa de existir neste ponto e o campo passa a ser tratado como nos outros quatro. Fechada por remissao para nao ficar a aparecer como pergunta em aberto sem dono.

**Onde ver no TIBCO**

- PRPINTPC / gateway «sem rotulo»

- [ ] Confere
- [ ] Nao confere. Na realidade: 

### 2.34  TIPOVISTAS

**Prova** _(extraido do pacote, so confirmar que lemos bem)_

- Condicao no XPDL: `TIPOVISTAS=='JUIZ' || TIPOVISTAS == IPESystemValues.SW_NA;`

**Decisao** _(e isto que precisa de conferencia)_

> INTENCIONAL e preservar. Tipo de vista igual a 'JUIZ' OU nao aplicavel segue o caminho de Vistas do Juiz. O ramo alternativo, rotulado 'DRF', e o ramo por omissao e significa 'tudo o que nao e JUIZ', incluindo MISTA - nao e um terceiro valor do dominio.

Decidido em 2026-08-07. E regra de negocio real, nao acidente: vista nao especificada e tratada deliberadamente como nao aplicavel e segue para o juiz. O dominio observado de TIPOVISTAS em todo o pacote e apenas {JUIZ, MISTA}; 'DRF' nunca aparece em condicao nenhuma, e apenas o rotulo do ramo por omissao. IMPLEMENTACAO: como SW_NA e agrupado com um valor de negocio concreto e nao com uma condicao de repeticao, o tipo tri-estado tem de preservar os tres casos - colapsar SW_NA em null faria o caso cair no ramo DRF em vez do ramo do juiz, invertendo a regra sem erro visivel.

**Onde ver no TIBCO**

- POC_EpatProcess / gateway 'Vistas do Juiz ?'

- [ ] Confere
- [ ] Nao confere. Na realidade: 

---

## Parte 3 - defeitos que encontramos no pacote

Isto nao e critica ao trabalho de ninguem: e o que aparece quando se le 765 KB de XPDL a maquina.
Vale a pena confirmar se ja sao conhecidos, e se algum ja foi corrigido em producao depois desta exportacao.

| # | Onde | O que encontramos | Ja conhecido? |
|---:|---|---|:---:|
| 1 | CONTROPC / ISetSubProc (linha 8554) | As atribuicoes NAO sao mutuamente exclusivas: os blocos sao avaliados em sequencia e um valor posterior sobrescreve o anterior. A ORDEM importa, tal como nas colunas do Corticon. | [ ] sim  [ ] nao |
| 2 | CONTROPC / ISetSubProc (linha 8554) | O ultimo bloco 'if (NOVOMODELO == true)' sobrescreve QUALQUER decisao anterior com AgPecas. E um interruptor global que anula a tabela inteira. | [ ] sim  [ ] nao |
| 3 | CONTROPC / ISetSubProc (linha 8554) | Ha um comentario do autor admitindo pendencia: 'falta implementar regras para PRJ'. | [ ] sim  [ ] nao |
| 4 | CONTROPC / ISetSubProc (linha 8554) | IDDECISAODEBITO == 0 e testado mas nao consta na legenda do autor (que so documenta 1, 2 e 3): valor sem significado declarado. | [ ] sim  [ ] nao |
| 5 | BSCENVPC / Start Loop (linha 5894) | RESPONDE A PERGUNTAS EM ABERTO: o dominio de ISAPPERROR e ISTECHERROR e 'N'/'Y', e NUMAPPRETRIES comeca em 0 e incrementa de 1 em 1. | [ ] sim  [ ] nao |
| 6 | BSCENVPC / Start Loop (linha 5894) | DIVERGENCIA ENTRE CLONES: CALCPRPC e BSCENVPC inicializam OUTCOME='OK'; PRPINTPC, ATZINTPC e CRNOTPC inicializam OUTCOME='R'. O script e identico em tudo o resto. E preciso decidir se a diferenca e intencional. | [ ] sim  [ ] nao |
| 7 | BSCENVPC / Start Loop (linha 5894) | O valor inicial de OUTCOME importa porque OUTCOME e comparado com 'OK' e 'R' em condicoes de desvio. | [ ] sim  [ ] nao |
| 8 | BSCENVPC / SetParameters (linha 5832) | RESPONDE A UMA PERGUNTA EM ABERTO: o valor inicial de MAXRETRIES e 5, definido aqui e nao no formulario nem em pacote externo. | [ ] sim  [ ] nao |
| 9 | BSCENVPC / SetParameters (linha 5832) | PROCESS_ID e exatamente a chave de correlacao que a migracao precisa tornar explicita: ja existe, so nao esta declarada como contrato. | [ ] sim  [ ] nao |
| 10 | BSCENVPC / SetParameters (linha 5832) | O script repete-se identico em CALCPRPC, BSCENVPC, PRPINTPC, ATZINTPC e CRNOTPC. | [ ] sim  [ ] nao |
| 11 | POC_EpatProcess / prepSub (linha 2848) | DEFEITO PROVAVEL: a ultima linha faz IDSINTIMADOS = '278713\|278712\|', sobrescrevendo a lista real DEPOIS de ela ja ter sido consumida. Ha ainda uma lista maior comentada logo abaixo. Tem cara de dado de teste esquecido no processo. | [ ] sim  [ ] nao |
| 12 | POC_EpatProcess / prepSub (linha 2848) | O parsing manual de string com separador '\|' e fragil: nao trata lista vazia nem separador final ausente. | [ ] sim  [ ] nao |
| 13 | POC_EpatProcess / prepSub (linha 2848) | As listas de pecas usam concatenacao de string com '\|' - o modelo .NET provavelmente quer uma colecao, nao uma string. | [ ] sim  [ ] nao |
| 14 | POC_EpatProcess / Define Destinatarios (linha 2657) | Contem enderecos de e-mail fixos no codigo (ja detetados como scriptHazards): um em copia e outro em copia oculta. | [ ] sim  [ ] nao |
| 15 | POC_EpatProcess / Define Destinatarios (linha 2657) | Enderecos fixos num script sao configuracao disfarcada de logica: mudam sem aviso e nao passam por revisao. | [ ] sim  [ ] nao |
| 16 | POC_EpatProcess / Verificar Anulacao (linha 1895) | O bloco if/else-if/else sobre INDNAORECORRER e um no-op nos dois primeiros ramos (atribui o valor a si proprio). O unico efeito real e o else: quando INDNAORECORRER nao e nem true nem false - isto e, quando esta em SW_NA - forca false. | [ ] sim  [ ] nao |
| 17 | POC_EpatProcess / Verificar Anulacao (linha 1895) | A troca de SW_NA por 'NA' e uma conversao de sentinela para literal de dominio: confirma que o terceiro estado precisa de representacao explicita no modelo .NET. | [ ] sim  [ ] nao |
| 18 | CRNOTPC / SetParameters (linha 11480) | RESPONDE A UMA PERGUNTA EM ABERTO: o valor inicial de MAXRETRIES e 5, definido aqui e nao no formulario nem em pacote externo. | [ ] sim  [ ] nao |
| 19 | CRNOTPC / SetParameters (linha 11480) | PROCESS_ID e exatamente a chave de correlacao que a migracao precisa tornar explicita: ja existe, so nao esta declarada como contrato. | [ ] sim  [ ] nao |
| 20 | CRNOTPC / SetParameters (linha 11480) | O script repete-se identico em CALCPRPC, BSCENVPC, PRPINTPC, ATZINTPC e CRNOTPC. | [ ] sim  [ ] nao |
| 21 | CRNOTPC / Start Loop (linha 11542) | RESPONDE A PERGUNTAS EM ABERTO: o dominio de ISAPPERROR e ISTECHERROR e 'N'/'Y', e NUMAPPRETRIES comeca em 0 e incrementa de 1 em 1. | [ ] sim  [ ] nao |
| 22 | CRNOTPC / Start Loop (linha 11542) | DIVERGENCIA ENTRE CLONES: CALCPRPC e BSCENVPC inicializam OUTCOME='OK'; PRPINTPC, ATZINTPC e CRNOTPC inicializam OUTCOME='R'. O script e identico em tudo o resto. E preciso decidir se a diferenca e intencional. | [ ] sim  [ ] nao |
| 23 | CRNOTPC / Start Loop (linha 11542) | O valor inicial de OUTCOME importa porque OUTCOME e comparado com 'OK' e 'R' em condicoes de desvio. | [ ] sim  [ ] nao |
| 24 | PRPINTPC / Start Loop (linha 7429) | RESPONDE A PERGUNTAS EM ABERTO: o dominio de ISAPPERROR e ISTECHERROR e 'N'/'Y', e NUMAPPRETRIES comeca em 0 e incrementa de 1 em 1. | [ ] sim  [ ] nao |
| 25 | PRPINTPC / Start Loop (linha 7429) | DIVERGENCIA ENTRE CLONES: CALCPRPC e BSCENVPC inicializam OUTCOME='OK'; PRPINTPC, ATZINTPC e CRNOTPC inicializam OUTCOME='R'. O script e identico em tudo o resto. E preciso decidir se a diferenca e intencional. | [ ] sim  [ ] nao |
| 26 | PRPINTPC / Start Loop (linha 7429) | O valor inicial de OUTCOME importa porque OUTCOME e comparado com 'OK' e 'R' em condicoes de desvio. | [ ] sim  [ ] nao |
| 27 | PRPINTPC / SetParameters (linha 7367) | RESPONDE A UMA PERGUNTA EM ABERTO: o valor inicial de MAXRETRIES e 5, definido aqui e nao no formulario nem em pacote externo. | [ ] sim  [ ] nao |
| 28 | PRPINTPC / SetParameters (linha 7367) | PROCESS_ID e exatamente a chave de correlacao que a migracao precisa tornar explicita: ja existe, so nao esta declarada como contrato. | [ ] sim  [ ] nao |
| 29 | PRPINTPC / SetParameters (linha 7367) | O script repete-se identico em CALCPRPC, BSCENVPC, PRPINTPC, ATZINTPC e CRNOTPC. | [ ] sim  [ ] nao |
| 30 | CALCPRPC / Start Loop (linha 4798) | RESPONDE A PERGUNTAS EM ABERTO: o dominio de ISAPPERROR e ISTECHERROR e 'N'/'Y', e NUMAPPRETRIES comeca em 0 e incrementa de 1 em 1. | [ ] sim  [ ] nao |

---

## Parte 4 - o que deixamos de fora, e porque

A prova de conceito nao migra o ePAT inteiro: valida um cenario representativo.
Confirmem que nada aqui e essencial ao cenario que vao ver demonstrado.

| Tipo | Dentro | Total | O que ficou de fora |
|---|---:|---:|---|
| field | 147 | 209 | Campo que ninguem toca nao tem comportamento a demonstrar. Continua no dicionario como facto do legado, mas nao gera trabalho. |
| operation | 5 | 127 | O WSDL descreve a superficie inteira do ePAT. O cenario toca uma fracao dela. Gerar card para as restantes seria encomendar a migracao completa da integracao. |
| rule | 185 | 307 | Backend e frontend estao diferidos para o MVP por decisao do cliente. Da tela interessa o contrato com o motor, nao a mecanica da pagina. |

- [ ] Confere
- [ ] Falta alguma coisa essencial: 

---

## Quem reviu

| Nome | Papel | Data |
|---|---|---|
|  |  |  |

