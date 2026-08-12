#nullable enable
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules.ATZINTPC;

/// <summary>
/// RI-script-ATZINTPC-SetParameters — parte de domínio pura.
/// Calcula PROCESS_ID e o valor por omissão de MAXRETRIES.
/// </summary>
public static class SetParametersRule
{
    public static string ComputeProcessId(long idAiim, FieldValue<int> idProcesso) =>
        ComputeProcessIdCore(idAiim, idProcesso);

    public static string ComputeProcessId(long idAiim, FieldValue<long> idProcesso) =>
        ComputeProcessIdCore(idAiim, idProcesso);

    public static int ComputeMaxRetries(int? currentMaxRetries) =>
        currentMaxRetries ?? 5;

    private static string ComputeProcessIdCore<T>(long idAiim, FieldValue<T> idProcesso) =>
        idProcesso.Match(
            hasValue: value => $"idAiim-{idAiim}idProc-{value}",
            notAvailable: () => $"idAiim-{idAiim}idProc-NA",
            empty: () => $"idAiim-{idAiim}idProc-NA");
}
