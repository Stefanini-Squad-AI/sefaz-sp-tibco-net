#nullable enable

using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-transition-AGPECASPC-gatewayEvOwVF6eEfGJqLUhfbpFcQ.
/// </summary>
public static class AgpecaspcGatewayDecisionRule
{
    public static bool ShouldEnterWaitBranch(AiimCase aiimCase)
    {
        ArgumentNullException.ThrowIfNull(aiimCase);

        return aiimCase.DATACONTROLE.IsNotAvailable
            || aiimCase.DATACONTROLE.Match(
                date => date != aiimCase.PRAZORECEBIMENT,
                () => true,
                () => true);
    }
}
