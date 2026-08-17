#nullable enable

// BUILD-DEAT0050-seg012 — Start Event → Controlar Data
// Processo: DEAT0050 · Chamado de: POC_EpatProcess/Aguardar evento de Notificacao do AIIM
// Topologia transcrita do XPDL — identificadores imutaveis.

namespace SefazSp.Epat.Application.Workflows.Deat0050;

/// <summary>
/// Topologia do segmento 1 do processo DEAT0050: Start Event → Controlar Data.
/// Cada constante e o ID XPDL do no correspondente — nunca renomear.
/// </summary>
public static class Deat0050WorkflowTopology
{
    // Checklist ordem 1 — startEvent (nó de arranque, sem transição de entrada)
    public const string StartEvent             = "_ppKXcFqjEfG5K7mY0I3I6w";

    // Checklist ordem 2 — callActivity → CALCPRPC (entrouPor=fluxo)
    public const string CalculaPrazo          = "_lrer3lqhEfG5K7mY0I3I6w";

    // Checklist ordem 3 — scriptTask HoraFimSC (entrouPor=fluxo)
    public const string HoraFimSC             = "_lrer3VqhEfG5K7mY0I3I6w";

    // Checklist ordem 4 — gateway "Ja se esperou pelo prazo em vigor?" (entrouPor=fluxo)
    // Regra: RI-transition-DEAT0050-gatewaylrerVqhEfG5K7mY0I3I6w
    public const string GatewayJaSeEsperouPeloPrazo = "_lrer_VqhEfG5K7mY0I3I6w";

    // Checklist ordem 5 — timerEvent Aguarda Defesa (entrouPor=fluxo)
    // Regra: RI-deadline-DEAT0050-AguardaDefesa
    public const string AguardaDefesa         = "_lrer2lqhEfG5K7mY0I3I6w";

    // Checklist ordem 6 — scriptTask Controlar Data (entrouPor=fluxo)
    public const string ControlarData         = "_lrer_lqhEfG5K7mY0I3I6w";

    /// <summary>
    /// Processo filho invocado pelo callActivity CalculaPrazo.
    /// ICALCPRPC deve estar registado em src/SefazSp.Epat.Application/Abstractions/Processes.
    /// dinamica=false: destino estatico, sem interface-registry.
    /// </summary>
    public const string CalculaPrazoDestino   = "CALCPRPC";
}
