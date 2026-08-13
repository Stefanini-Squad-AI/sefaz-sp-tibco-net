#nullable enable

using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-script-CALCPRPC-SetParameters
/// Regra de domínio pura do passo SetParameters do processo CALCPRPC.
///
/// Expressão legada: IDPROCESSO != IPESystemValues.SW_NA | MAXRETRIES==null
/// Consequência: escreve MAXRETRIES (default 5), NUMAPPRETRIES (default 0) e OUTCOME (default OK).
///
/// IDPROCESSO é comparado com SW_NA — um TERCEIRO estado distinto de null e de vazio.
/// Usa <see cref="FieldValue{T}"/> (shim-tri-state, NOEQ-iprocess-builtin, ratificado 2026-08-06).
/// SW_NA NUNCA é mapeado para null.
/// </summary>
public static class CalcprpcSetParametersRule
{
    public const int DefaultMaxRetries = 5;
    public const int DefaultNumAppRetries = 0;
    public const string DefaultOutcome = "OK";

    /// <summary>
    /// Avalia a condição legada:
    ///   IDPROCESSO != IPESystemValues.SW_NA  OU  MAXRETRIES==null
    /// </summary>
    public static bool ShouldInitialize(FieldValue<long> idProcesso, int? maxRetries) =>
        !idProcesso.IsNotAvailable || maxRetries is null;

    public static int ResolveMaxRetries(int? maxRetries) =>
        maxRetries ?? DefaultMaxRetries;

    public static int ResolveNumAppRetries(int? numAppRetries) =>
        numAppRetries ?? DefaultNumAppRetries;

    public static string ResolveOutcome(string? outcome) =>
        outcome ?? DefaultOutcome;
}
