#nullable enable

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-script-ATZINTPC-SetParameters
/// Regra de domínio pura do passo SetParameters do processo ATZINTPC.
///
/// Expressão legada: if (MAXRETRIES == null) MAXRETRIES = 5
/// Consequência: escreve MAXRETRIES (default 5) quando ainda não inicializado.
///
/// NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06).
/// SW_NA é mapeado via <see cref="Domain.ValueObjects.FieldValue{T}"/> — NUNCA para null.
///
/// Invariante: identificador do nó _RNdJyl6PEfGBBLgT-R5iuw não deve ser renomeado.
/// Card: BUILD-ATZINTPC-seg041 · AC1
/// </summary>
public static class AtzintpcSetParametersRule
{
    /// <summary>
    /// Valor padrão de MAXRETRIES quando não foi ainda inicializado.
    /// Fonte: glossário POC_Epat.yaml — "if (MAXRETRIES == null) MAXRETRIES = 5",
    /// confirmado em 2026-08-06.
    /// </summary>
    public const int DefaultMaxRetries = 5;

    /// <summary>
    /// Avalia se o passo SetParameters deve inicializar o contexto.
    /// </summary>
    /// <param name="maxRetries">Valor atual de MAXRETRIES (null = ainda não inicializado).</param>
    /// <returns>
    ///   <c>true</c> → escrever MAXRETRIES no contexto de execução.
    ///   <c>false</c> → nenhuma escrita necessária.
    /// </returns>
    public static bool ShouldInitialize(int? maxRetries) =>
        maxRetries is null;

    /// <summary>
    /// Devolve o valor efectivo de MAXRETRIES: o que foi fixado no caso, ou o default.
    /// </summary>
    public static int ResolveMaxRetries(int? maxRetries) =>
        maxRetries ?? DefaultMaxRetries;
}
