#nullable enable

using SefazSp.Epat.Domain.Rules.AGPECASPC;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.AGPECASPC;

/// <summary>
/// Topologia do fluxo AGPECASPC, segmento 044 (passos 1–5 do cenário SC-AGPECASPC-003):
/// de "Start Event" (_i4UpgF9IEfGqPfX31TKC3w)
/// a "End Event"    (_EvOwQV6eEfGJqLUhfbpFcQ).
///
/// O processo AGPECASPC é chamado por 'CONTROPC/Aguardar Retorno' e herda daí a etapa 4.
///
/// Nós percorridos (checklist, card BUILD-AGPECASPC-seg044):
///   ordem 1 — startEvent  _i4UpgF9IEfGqPfX31TKC3w  Start Event                  (entrada)
///   ordem 2 — scriptTask  _EvOwTF6eEfGJqLUhfbpFcQ  Set Values                   entrouPor=fluxo
///   ordem 3 — gateway     _vshgkF6fEfGJqLUhfbpFcQ  (convergência de fluxo)      entrouPor=fluxo
///   ordem 4 — gateway     _EvOwVF6eEfGJqLUhfbpFcQ  "Já se esperou pelo prazo?"  entrouPor=fluxo
///   ordem 5 — endEvent    _EvOwQV6eEfGJqLUhfbpFcQ  End Event                    entrouPor=fluxo
///
/// Todos os 5 passos entram por transição de fluxo real do XPDL (entrouPor=fluxo);
/// nenhuma aresta explícita fora do fluxo é necessária neste segmento.
///
/// O gateway _EvOwVF6eEfGJqLUhfbpFcQ (ordem 4) usa
/// <see cref="AgpecaspcGatewayEvOwVF6eEfGJqLUhfbpFcQRule"/>:
///   ramo explícito (condição verdadeira) → fora deste segmento (SetPrazo, etapas anteriores).
///   ramo OTHERWISE (condição falsa)      → End Event (_EvOwQV6eEfGJqLUhfbpFcQ).
///
/// Invariantes — os seguintes identificadores não devem ser renomeados:
///   _i4UpgF9IEfGqPfX31TKC3w, _EvOwTF6eEfGJqLUhfbpFcQ, _vshgkF6fEfGJqLUhfbpFcQ,
///   _EvOwVF6eEfGJqLUhfbpFcQ, _EvOwQV6eEfGJqLUhfbpFcQ,
///   RI-script-AGPECASPC-SetValues, RI-transition-AGPECASPC-gatewayEvOwVF6eEfGJqLUhfbpFcQ.
/// </summary>
public sealed class AgpecaspcSeg044Workflow
{
    /// <summary>
    /// Executa o segmento 044 do processo AGPECASPC a partir do Start Event.
    ///
    /// Devolve o identificador do nó terminal alcançado:
    ///   <c>"_EvOwQV6eEfGJqLUhfbpFcQ"</c> — End Event (ramo OTHERWISE; cenário SC-AGPECASPC-003).
    ///   <c>"_EvOwVF6eEfGJqLUhfbpFcQ:explicit"</c> — ramo explícito do gateway (fora deste segmento).
    /// </summary>
    /// <param name="cntPeca1">Campo CNTPECA1 do caso (tri-estado, pode ser SW_NA).</param>
    /// <param name="cntPeca2">Campo CNTPECA2 do caso (tri-estado, pode ser SW_NA).</param>
    /// <param name="cntPeca3">Campo CNTPECA3 do caso (tri-estado, pode ser SW_NA).</param>
    /// <param name="cntPeca4">Campo CNTPECA4 do caso (tri-estado, pode ser SW_NA).</param>
    /// <param name="dataControle">Campo DATACONTROLE do caso (tri-estado, pode ser SW_NA).</param>
    /// <param name="prazoRecebiment">Campo PRAZORECEBIMENT do caso.</param>
    /// <param name="applySetValues">
    ///   Callback de envelope técnico invocado quando
    ///   <see cref="AgpecaspcSetValuesRule.ShouldSetValues"/> retorna verdadeiro.
    ///   Deve escrever FIELDSNAMES, FIELDSTYPES, FIELDSVALUES, IDPECAS, PERIODOEMDIAS
    ///   no contexto de execução (Application/Execution). Os valores concretos não estão
    ///   declarados no XPDL (naoSabemos); esta camada chama o callback e não interpreta
    ///   os valores atribuídos.
    /// </param>
    public string Execute(
        FieldValue<string> cntPeca1,
        FieldValue<string> cntPeca2,
        FieldValue<string> cntPeca3,
        FieldValue<string> cntPeca4,
        FieldValue<DateOnly> dataControle,
        DateOnly prazoRecebiment,
        Action? applySetValues = null)
    {
        // ordem 1: startEvent _i4UpgF9IEfGqPfX31TKC3w — Start Event
        // Ponto de entrada do segmento; nenhuma acção de fluxo além de registar a entrada.

        // ordem 2: scriptTask _EvOwTF6eEfGJqLUhfbpFcQ — Set Values  (entrouPor=fluxo)
        // RI-script-AGPECASPC-SetValues: a parte de domínio avalia a condição de guarda;
        // a parte de envelope técnico (atribuições a FIELDSNAMES, etc.) fica no callback.
        if (AgpecaspcSetValuesRule.ShouldSetValues(cntPeca1, cntPeca2, cntPeca3, cntPeca4))
            applySetValues?.Invoke();

        // ordem 3: gateway _vshgkF6fEfGJqLUhfbpFcQ — convergência de fluxo  (entrouPor=fluxo)
        // Gateway de junção sem condição de desvio neste percurso; passa o fluxo directamente
        // para o gateway de decisão seguinte.

        // ordem 4: gateway _EvOwVF6eEfGJqLUhfbpFcQ — "Já se esperou pelo prazo em vigor?"
        //   (entrouPor=fluxo)
        // RI-transition-AGPECASPC-gatewayEvOwVF6eEfGJqLUhfbpFcQ:
        //   condição verdadeira → ramo explícito (SetPrazo, fora deste segmento)
        //   condição falsa (OTHERWISE) → End Event
        if (AgpecaspcGatewayEvOwVF6eEfGJqLUhfbpFcQRule.ShouldTakeExplicitBranch(
                dataControle, prazoRecebiment))
        {
            // Ramo explícito: destino fora deste segmento (SetPrazo / etapas anteriores).
            return "_EvOwVF6eEfGJqLUhfbpFcQ:explicit";
        }

        // ordem 5: endEvent _EvOwQV6eEfGJqLUhfbpFcQ — End Event  (entrouPor=fluxo)
        // Ramo OTHERWISE: o segmento fecha e o controlo regressa ao chamador
        // 'CONTROPC/Aguardar Retorno' (herdadoDeNoId=_-bkw-V6JEfGBBLgT-R5iuw).
        return "_EvOwQV6eEfGJqLUhfbpFcQ";
    }
}
