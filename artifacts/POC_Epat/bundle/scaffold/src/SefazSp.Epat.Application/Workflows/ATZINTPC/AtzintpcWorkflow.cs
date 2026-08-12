#nullable enable

namespace SefazSp.Epat.Application.Workflows.ATZINTPC;

public sealed record ExplicitTransition(string From, string To, string Kind);

/// <summary>
/// Fluxo ATZINTPC: de 'Start Event' a 'Done - Success' (segmento seg043).
/// As transições de descida (_RNdJ2l6PEfGBBLgT-R5iuw → _RNdKFl6PEfGBBLgT-R5iuw)
/// e de regresso (_RNdKF16PEfGBBLgT-R5iuw → _RNdJ2V6PEfGBBLgT-R5iuw)
/// não existem no XPDL e estão aqui escritas explicitamente.
/// </summary>
public sealed class AtzintpcWorkflow
{
    public static IReadOnlyList<ExplicitTransition> ExplicitTransitions { get; } =
    [
        new ExplicitTransition(
            From: "_RNdJ2l6PEfGBBLgT-R5iuw",
            To: "_RNdKFl6PEfGBBLgT-R5iuw",
            Kind: "descida"),
        new ExplicitTransition(
            From: "_RNdKF16PEfGBBLgT-R5iuw",
            To: "_RNdJ2V6PEfGBBLgT-R5iuw",
            Kind: "regresso")
    ];

    /// <summary>
    /// Simula o percurso seg043 e devolve os IDs dos nós visitados em ordem.
    /// Os parâmetros booleanos traduzem as condições dos gateways sem reavaliar o estado:
    /// a verdade do estado vem dos inputs do cenário (fixture), não de re-execução.
    /// </summary>
    public IReadOnlyList<string> RunSegment043(
        bool checkRetriesStillGood,
        bool serviceCallFailed,
        bool isTechError,
        bool isAppError)
    {
        var path = new List<string>();

        path.Add("_RNdJyV6PEfGBBLgT-R5iuw");
        path.Add("_RNdJyl6PEfGBBLgT-R5iuw");
        path.Add("_RNdJzF6PEfGBBLgT-R5iuw");
        path.Add("_RNdJ2l6PEfGBBLgT-R5iuw");

        path.Add("_RNdKFl6PEfGBBLgT-R5iuw");
        path.Add("_RNdKFF6PEfGBBLgT-R5iuw");
        path.Add("_RNdKFV6PEfGBBLgT-R5iuw");

        if (checkRetriesStillGood)
        {
            path.Add("_RNdKHF6PEfGBBLgT-R5iuw");
            path.Add("_RNdKGl6PEfGBBLgT-R5iuw");

            if (serviceCallFailed)
            {
                path.Add("_RNdKGV6PEfGBBLgT-R5iuw");
            }

            path.Add("_RNdKG16PEfGBBLgT-R5iuw");
        }
        else
        {
            path.Add("_RNdKGF6PEfGBBLgT-R5iuw");
        }

        path.Add("_RNdKF16PEfGBBLgT-R5iuw");
        path.Add("_RNdJ2V6PEfGBBLgT-R5iuw");

        if (!isTechError)
        {
            path.Add("_RNdJ2F6PEfGBBLgT-R5iuw");

            if (!isAppError)
            {
                path.Add("_RNdJ116PEfGBBLgT-R5iuw");
            }
        }

        return path.AsReadOnly();
    }
}
