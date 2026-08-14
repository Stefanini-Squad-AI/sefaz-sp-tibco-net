#nullable enable

using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-script-ATZINTPC-SetParameters
/// Regra de domínio pura do passo SetParameters do processo ATZINTPC.
///
/// Expressão legada: IDPROCESSO != IPESystemValues.SW_NA | MAXRETRIES==null
/// Consequência: escreve MAXRETRIES (default 5) e PROCESS_ID quando necessário.
///
/// IDPROCESSO é comparado com SW_NA — um TERCEIRO estado distinto de null e de vazio.
/// Usa <see cref="FieldValue{T}"/> (shim-tri-state, NOEQ-iprocess-builtin, ratificado 2026-08-06).
/// SW_NA NUNCA é mapeado para null.
///
/// Invariante: identificador do nó _RNdJyl6PEfGBBLgT-R5iuw não deve ser renomeado.
/// Card: BUILD-ATZINTPC-seg043 · AC2
/// </summary>
public static class AtzintpcSetParametersRule
{
    /// <summary>
    /// Valor padrão de MAXRETRIES quando não foi ainda inicializado.
    /// Fonte: glossário POC_Epat.yaml — "if (MAXRETRIES == null) MAXRETRIES = 5".
    /// </summary>
    public const int DefaultMaxRetries = 5;

    /// <summary>
    /// Avalia a condição legada:
    ///   IDPROCESSO != IPESystemValues.SW_NA  OU  MAXRETRIES==null
    /// Retorna verdadeiro quando há um IDPROCESSO disponível no caso
    /// ou quando MAXRETRIES ainda não foi inicializado.
    /// </summary>
    /// <param name="idProcesso">
    ///   Campo tri-estado: HasValue = preenchido; IsNotAvailable = SW_NA; Empty = não declarado.
    ///   SW_NA significa "não preenchido" — terceiro estado, nunca null.
    /// </param>
    /// <param name="maxRetries">Valor atual de MAXRETRIES (null = ainda não inicializado).</param>
    /// <returns>
    ///   <c>true</c> → escrever MAXRETRIES e PROCESS_ID no contexto de execução.
    ///   <c>false</c> → nenhuma escrita necessária.
    /// </returns>
    public static bool ShouldInitialize(FieldValue<long> idProcesso, int? maxRetries) =>
        !idProcesso.IsNotAvailable || maxRetries is null;

    /// <summary>
    /// Devolve o valor efectivo de MAXRETRIES: o que foi fixado no caso, ou o default.
    /// </summary>
    public static int ResolveMaxRetries(int? maxRetries) =>
        maxRetries ?? DefaultMaxRetries;
}
