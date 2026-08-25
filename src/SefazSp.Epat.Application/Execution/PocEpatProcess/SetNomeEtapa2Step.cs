#nullable enable

// Card: BUILD-POCEPATPROCESS-seg023
// AC2 — scriptTask 'Set Nome Etapa 2' (_XWivF1qTEfG5K7mY0I3I6w, entrouPor=fluxo)
//
// Classificacao (rule-catalogue.json · RI-script-POC_EpatProcess-SetNomeEtapa2):
//   eRegraDeNegocio=false · efeito=tecnico
//   "nao le nenhum campo do caso; so envelope tecnico ou estado da pagina"
//   → logica de envelope tecnico fica em Application/Execution (nao em Domain/Rules).
//
// Script legado (processo POC_EpatProcess, linha 2291 do XPDL):
//   NOMEETAPA = "CQ";
//   DAYSOVER = 0;
//   DTFIMCQ = IPESystemValues.SW_DATE;
//   HRFIMCQ = IPEDateTimeUtil.CALCTIME('23:59', 0, 0, DAYSOVER);
//
// Decisao NOEQ-iprocess-builtin → shim-tri-state (2026-08-06):
//   SW_NA e um terceiro estado distinto de null e de vazio — o compilador exige
//   pattern matching exaustivo. Este passo nao usa SW_NA nem valores sentinela;
//   usa SW_DATE (data de hoje do motor) e CALCTIME (hora calculada pelo builtin).
//
// CALCTIME: semantica pendente de confirmacao da documentacao TIBCO
//   (builtin-contract.json · semanticsStatus=unconfirmed).
//   O corpo sera completado quando IProcessBuiltins.CALCTIME estiver definido em
//   src/SefazSp.Epat.Infrastructure/Legacy.
//   Expressao observada: CALCTIME('23:59', 0, 0, DAYSOVER).

using SefazSp.Epat.Application.Abstractions.Legacy;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Execution.PocEpatProcess;

/// <summary>
/// Passo de envelope tecnico do scriptTask 'Set Nome Etapa 2'
/// (<c>_XWivF1qTEfG5K7mY0I3I6w</c>) do processo POC_EpatProcess.
///
/// Inicializa os campos da etapa CQ no caso:
/// <list type="bullet">
///   <item><c>NOMEETAPA = "CQ"</c> — nome da etapa corrente.</item>
///   <item><c>DAYSOVER = 0</c> — contador de dias extra, zerado ao iniciar a etapa.</item>
///   <item><c>DTFIMCQ = SW_DATE</c> — data de fim da etapa CQ, fixada na data de hoje
///         (fornecida pelo motor via <see cref="IClock"/>).</item>
///   <item><c>HRFIMCQ = CALCTIME('23:59', 0, 0, DAYSOVER)</c> — hora de fim da etapa CQ,
///         calculada por <c>IPEDateTimeUtil.CALCTIME</c> (PENDENTE: semantica nao confirmada).</item>
/// </list>
///
/// Classificacao: eRegraDeNegocio=false, efeito=tecnico.
/// Nunca usa <see cref="DateTime.Now"/>: o relogio e sempre <see cref="IClock"/> injectado.
/// </summary>
public sealed class SetNomeEtapa2Step
{
    private readonly IClock _clock;
    private readonly IProcessBuiltins _builtins;

    /// <param name="clock">Relogio injectavel — nunca <see cref="DateTime.Now"/>.</param>
    /// <param name="builtins">Camada anticorrupção que reproduz CALCTIME.</param>
    public SetNomeEtapa2Step(IClock clock, IProcessBuiltins builtins)
    {
        _clock = clock;
        _builtins = builtins;
    }

    /// <summary>
    /// Executa o script 'Set Nome Etapa 2' sobre <paramref name="aiimCase"/>.
    ///
    /// Reproduce o comportamento do legado:
    /// <code>
    ///   NOMEETAPA = "CQ";
    ///   DAYSOVER  = 0;
    ///   DTFIMCQ   = IPESystemValues.SW_DATE;
    ///   HRFIMCQ   = IPEDateTimeUtil.CALCTIME('23:59', 0, 0, DAYSOVER);
    /// </code>
    /// </summary>
    /// <param name="aiimCase">Estado de negocio mutavel do caso.</param>
    public void Execute(AiimCase aiimCase)
    {
        // NOMEETAPA = "CQ" — nome da etapa corrente; literal do legado.
        aiimCase.NOMEETAPA = "CQ";

        // DAYSOVER = 0 — contador de dias extra; zerado ao iniciar a etapa.
        aiimCase.DAYSOVER = 0;

        // DTFIMCQ = IPESystemValues.SW_DATE — data de hoje fornecida pelo motor.
        // Em .NET, SW_DATE mapeia para a data local fornecida pelo relogio injectavel.
        aiimCase.DTFIMCQ = DateOnly.FromDateTime(_clock.Now.DateTime);

        // HRFIMCQ = IPEDateTimeUtil.CALCTIME('23:59', 0, 0, DAYSOVER)
        // Reproduzido pela camada anticorrupção (base-1). Com 0h/0m somados, o resultado
        // é a própria hora-base 23:59, independentemente de DAYSOVER.
        aiimCase.HRFIMCQ = _builtins.CalcTime(new TimeOnly(23, 59), 0, 0, aiimCase.DAYSOVER);
    }
}
