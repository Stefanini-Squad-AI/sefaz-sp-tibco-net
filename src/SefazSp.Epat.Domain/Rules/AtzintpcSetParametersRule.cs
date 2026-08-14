#nullable enable

using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-script-ATZINTPC-SetParameters
/// Regra de domínio pura do passo SetParameters do processo ATZINTPC.
///
/// Expressão legada: if (MAXRETRIES == null) MAXRETRIES = 5
/// Consequência: escreve MAXRETRIES (default 5) quando ainda não inicializado.
///
/// Decisão NOEQ-iprocess-builtin (shim-tri-state, ratificado): IDPROCESSO é comparado
/// com SW_NA — um TERCEIRO estado distinto de null e de vazio.
/// Usa <see cref="FieldValue{T}"/> (shim-tri-state). SW_NA NUNCA é mapeado para null.
///
/// Invariante: identificadores ATZINTPC, SetParameters, MAXRETRIES, SW_QRETRYCOUNT
/// não devem ser renomeados.
/// Card: BUILD-ATZINTPC-seg046 · AC1 · Nó _RNdJyl6PEfGBBLgT-R5iuw
/// </summary>
public static class AtzintpcSetParametersRule
{
    /// <summary>
    /// Valor padrão de MAXRETRIES quando não foi ainda inicializado.
    /// Fonte: glossário POC_Epat — "if (MAXRETRIES == null) MAXRETRIES = 5".
    /// </summary>
    public const int DefaultMaxRetries = 5;

    /// <summary>
    /// Avalia se a inicialização é necessária.
    /// Retorna verdadeiro quando há um IDPROCESSO disponível (não SW_NA) ou
    /// quando MAXRETRIES ainda não foi inicializado.
    /// </summary>
    /// <param name="idProcesso">
    ///   Campo tri-estado: HasValue = preenchido; IsNotAvailable = SW_NA; Empty = não declarado.
    ///   SW_NA significa "não preenchido" — terceiro estado, nunca null.
    /// </param>
    /// <param name="maxRetries">Valor atual de MAXRETRIES (null = ainda não inicializado).</param>
    /// <returns>
    ///   <c>true</c> → escrever MAXRETRIES no contexto de execução.
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
