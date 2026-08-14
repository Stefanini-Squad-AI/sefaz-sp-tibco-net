#nullable enable

using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-script-PRPINTPC-SetParameters
/// Regra de domínio pura do passo SetParameters do processo PRPINTPC.
///
/// Expressão legada: if (MAXRETRIES == null) MAXRETRIES = 5
/// Consequência: escreve MAXRETRIES (default 5) quando ainda não inicializado.
///
/// Não usa SW_NA nesta regra; MAXRETRIES é um inteiro inicializado no contexto técnico.
/// Decisão NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06).
/// </summary>
public static class PrpintpcSetParametersRule
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
