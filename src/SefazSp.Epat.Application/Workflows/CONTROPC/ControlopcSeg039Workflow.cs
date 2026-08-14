#nullable enable

// Card: BUILD-CONTROPC-seg039
// Cenário de referência: SC-CONTROPC-001 · segmento 1 (ordemNaJornada=1) · passos 5–7 · etapa 4
// Herdado de: POC_EpatProcess / Controlar Intimados
//
// Topologia (3 nós — todos entrouPor=fluxo):
//   [callActivity  _-bkw-V6JEfGBBLgT-R5iuw]  Aguardar Retorno (dynamic-subprocess IAGURETPC)
//     → [scriptTask   _-bkxKF6JEfGBBLgT-R5iuw]  Desativa Subs    (envelope técnico)
//     → [endEvent     _-bkw8l6JEfGBBLgT-R5iuw]  REGRESSO ao chamador (nao encerramento)
//
// ATENÇÃO — semântica do endEvent:
//   _-bkw8l6JEfGBBLgT-R5iuw é o endEvent do SUBPROCESSO CONTROPC.
//   Indica o REGRESSO ao processo pai (POC_EpatProcess / Controlar Intimados),
//   NÃO o encerramento da instância raiz.
//   O workflow devolve o identificador do endEvent ao chamador como sinal de conclusão.
//   Fonte: SC-CONTROPC-001.destino.como="regressa-ao-chamador".
//
// Decisão NOEQ-dynamic-subprocess (interface-registry-validated, ratificada 2026-08-06):
//   O destino de 'Aguardar Retorno' é resolvido em runtime pelo campo AGUARDAR[IDX_AGUARDAR].
//   A resolução usa Keyed DI (.NET 8 AddKeyedScoped / GetRequiredKeyedService) via
//   AGUARDARRegistry (Infrastructure/Integration.Doubles).
//   O registo é verificado no arranque; destino sem implementação falha visivelmente.
//   NAO herda HaltOnBadSubProcess=false do legado TIBCO.
//
// Invariantes — os seguintes identificadores não devem ser renomeados:
//   _-bkw-V6JEfGBBLgT-R5iuw, _-bkxKF6JEfGBBLgT-R5iuw, _-bkw8l6JEfGBBLgT-R5iuw,
//   AGUARDAR, AGURETPC, SC-CONTROPC-001.

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Application.Execution.CONTROPC;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.CONTROPC;

/// <summary>
/// Define a topologia do segmento CONTROPC-seg039:
/// de 'Aguardar Retorno' (<c>_-bkw-V6JEfGBBLgT-R5iuw</c>)
/// a 'endEvent _-bkw8l6JEfGBBLgT-R5iuw' (<c>_-bkw8l6JEfGBBLgT-R5iuw</c>).
///
/// <para>
/// O processo CONTROPC é chamado pelo processo raiz <c>POC_EpatProcess</c> via callActivity
/// 'Controlar Intimados'. O <c>endEvent</c> deste subprocesso é um <b>regresso ao chamador</b>,
/// não o encerramento da instância raiz (AC4, card BUILD-CONTROPC-seg039).
/// O método <see cref="ExecuteAsync"/> devolve o identificador do endEvent ao chamador.
/// </para>
///
/// <para>
/// O callActivity 'Aguardar Retorno' (<c>_-bkw-V6JEfGBBLgT-R5iuw</c>) usa o padrão
/// <b>interface-registry-validated</b> (NOEQ-dynamic-subprocess, 2026-08-06):
/// o destino é lido de <c>AGUARDAR[IDX_AGUARDAR]</c> e resolvido em runtime
/// pelo delegado <c>resolveAguardar</c> injectado no construtor.
/// O delegado é fornecido por <c>AGUARDARRegistry.Resolve</c> (Infrastructure/Integration.Doubles),
/// validado no arranque da aplicação.
/// </para>
///
/// <list type="number">
///   <item>
///     <description>
///       ordem 1 — callActivity <c>_-bkw-V6JEfGBBLgT-R5iuw</c> — Aguardar Retorno
///       (dynamic-subprocess, IAGURETPC, interface-registry-validated)
///     </description>
///   </item>
///   <item>
///     <description>
///       ordem 2 — scriptTask <c>_-bkxKF6JEfGBBLgT-R5iuw</c> — Desativa Subs
///       (envelope técnico, RI-script-CONTROPC-DesativaSubs)
///     </description>
///   </item>
///   <item>
///     <description>
///       ordem 3 — endEvent <c>_-bkw8l6JEfGBBLgT-R5iuw</c> — regresso ao chamador
///     </description>
///   </item>
/// </list>
/// </summary>
public sealed class ControlopcSeg039Workflow
{
    // Delegado que resolve IAGURETPC pelo valor de AGUARDAR[IDX_AGUARDAR].
    // Em produção: fornecido por AGUARDARRegistry.Resolve (Keyed DI, .NET 8).
    // Em testes:   fornecido por AGUARDARRegistry configurado com doubles.
    // A resolução falha visivelmente se o destino não estiver registado.
    private readonly Func<string, IAGURETPC> _resolveAguardar;

    /// <summary>
    /// Inicializa o workflow com o delegado de resolução de subprocessos dinâmicos.
    /// </summary>
    /// <param name="resolveAguardar">
    /// Delegado que mapeia a chave de destino (valor de <c>AGUARDAR[IDX_AGUARDAR]</c>)
    /// para a implementação <see cref="IAGURETPC"/> correspondente.
    /// Tipicamente <c>aguardarRegistry.Resolve</c>.
    /// Lança <see cref="InvalidOperationException"/> se o destino não estiver registado.
    /// </param>
    public ControlopcSeg039Workflow(Func<string, IAGURETPC> resolveAguardar)
    {
        _resolveAguardar = resolveAguardar;
    }

    // -----------------------------------------------------------------------
    // ordem 1 — callActivity _-bkw-V6JEfGBBLgT-R5iuw — Aguardar Retorno
    // -----------------------------------------------------------------------
    // Decisão NOEQ-dynamic-subprocess (interface-registry-validated, 2026-08-06):
    //   xpdExt:ProcessInterface=AGURETPC.
    //   O destino é o valor de AGUARDAR[IDX_AGUARDAR], determinado por ISetSubProc
    //   (passo 4, segmento seg045). Valores possíveis (CONTROPC/ISetSubProc):
    //     AgPecas, AgPRJ, AgRecPRJ, AgPRJR, AgRCRaz, AgCRaz, AgPetica.
    //   Apenas AgPecas (AGPECASPC) foi entregue; os outros 6 têm doubles que falham visivelmente.
    //   O registo é verificado no arranque — NAO herda HaltOnBadSubProcess=false do legado TIBCO.

    // -----------------------------------------------------------------------
    // ordem 2 — scriptTask _-bkxKF6JEfGBBLgT-R5iuw — Desativa Subs
    // -----------------------------------------------------------------------
    // RI-script-CONTROPC-DesativaSubs (eRegraDeNegocio=false → Application/Execution).
    // Limpa STATUSSUBPROC após o retorno do subprocesso dinâmico.

    // -----------------------------------------------------------------------
    // ordem 3 — endEvent _-bkw8l6JEfGBBLgT-R5iuw
    // -----------------------------------------------------------------------
    // REGRESSO ao processo pai (POC_EpatProcess / Controlar Intimados).
    // NÃO é encerramento da instância raiz.
    // O método devolve "_-bkw8l6JEfGBBLgT-R5iuw" como sinal de conclusão do subprocesso.

    /// <summary>
    /// Executa o segmento 039 do processo CONTROPC, de 'Aguardar Retorno' ao endEvent.
    ///
    /// <para>
    /// Devolve o identificador do nó terminal alcançado:
    /// <c>"_-bkw8l6JEfGBBLgT-R5iuw"</c> — endEvent (regresso ao chamador, não encerramento).
    /// </para>
    ///
    /// <para>
    /// A resolução do subprocesso dinâmico falha visivelmente com
    /// <see cref="InvalidOperationException"/> se <c>AGUARDAR[IDX_AGUARDAR]</c>
    /// não tiver implementação registada no <c>AGUARDARRegistry</c>.
    /// </para>
    /// </summary>
    /// <param name="caso">Estado mutável do caso AIIM.</param>
    /// <param name="caseRef">Referência imutável ao caso (IdAiim + ProcessId).</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// Identificador do nó terminal: <c>"_-bkw8l6JEfGBBLgT-R5iuw"</c>.
    /// </returns>
    public async Task<string> ExecuteAsync(AiimCase caso, AiimCaseRef caseRef, CancellationToken ct)
    {
        // ordem 1 — callActivity _-bkw-V6JEfGBBLgT-R5iuw — Aguardar Retorno
        // dynamic-subprocess: resolve IAGURETPC pelo valor de AGUARDAR[IDX_AGUARDAR].
        // NOEQ-dynamic-subprocess (interface-registry-validated, 2026-08-06):
        //   falha visível se o destino não estiver registado (NAO silencia como HaltOnBadSubProcess=false).
        var destinationKey = caso.AGUARDAR[caso.IDX_AGUARDAR];
        var subprocess = _resolveAguardar(destinationKey);
        await subprocess.ExecuteAsync(caseRef, ct).ConfigureAwait(false);

        // ordem 2 — scriptTask _-bkxKF6JEfGBBLgT-R5iuw — Desativa Subs
        // RI-script-CONTROPC-DesativaSubs (eRegraDeNegocio=false): envelope técnico.
        // Limpa STATUSSUBPROC após o retorno do subprocesso dinâmico.
        ControlopcSeg039Steps.ExecuteDesativaSubs(caso);

        // ordem 3 — endEvent _-bkw8l6JEfGBBLgT-R5iuw
        // REGRESSO ao processo pai (POC_EpatProcess / Controlar Intimados).
        // NÃO é encerramento da instância raiz — o subprocesso CONTROPC conclui aqui
        // e devolve o controlo ao chamador.
        return "_-bkw8l6JEfGBBLgT-R5iuw";
    }
}
