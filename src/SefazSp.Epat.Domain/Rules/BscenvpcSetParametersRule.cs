#nullable enable

using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-script-BSCENVPC-SetParameters
/// Regra de domínio pura do passo SetParameters do processo BSCENVPC.
///
/// Expressão legada: IDPROCESSO != IPESystemValues.SW_NA | MAXRETRIES==null
/// Consequência: escreve MAXRETRIES (default 5) e PROCESS_ID.
///
/// IDPROCESSO é comparado com SW_NA — um TERCEIRO estado distinto de null e de vazio.
/// Usa <see cref="FieldValue{T}"/> (shim-tri-state, NOEQ-iprocess-builtin, ratificado 2026-08-06).
/// SW_NA NUNCA é mapeado para null.
/// </summary>
public static class BscenvpcSetParametersRule
{
    /// <summary>
    /// Valor padrão de MAXRETRIES quando não foi ainda inicializado.
    /// Fonte: glossário POC_Epat.yaml — "if (MAXRETRIES == null) MAXRETRIES = 5",
    /// confirmado em 2026-08-06.
    /// </summary>
    public const int DefaultMaxRetries = 5;

    /// <summary>
    /// Avalia a condição legada:
    ///   IDPROCESSO != IPESystemValues.SW_NA  OU  MAXRETRIES==null
    /// Retorna verdadeiro quando há um IDPROCESSO disponível no caso
    /// ou quando MAXRETRIES ainda não foi inicializado (qualquer um pede escrita).
    /// </summary>
    /// <param name="idProcesso">
    ///   Campo tri-estado: HasValue = preenchido; IsNotAvailable = SW_NA; Empty = não declarado.
    /// </param>
    /// <param name="maxRetries">Valor atual de MAXRETRIES (null = ainda não inicializado).</param>
    public static bool ShouldInitialize(FieldValue<long> idProcesso, int? maxRetries) =>
        !idProcesso.IsNotAvailable || maxRetries is null;

    /// <summary>
    /// Devolve o valor efectivo de MAXRETRIES: o que foi fixado no caso, ou o default.
    /// </summary>
    public static int ResolveMaxRetries(int? maxRetries) =>
        maxRetries ?? DefaultMaxRetries;
}
