#nullable enable

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
    /// Devolve o valor efectivo de MAXRETRIES: o que foi fixado no caso, ou o default.
    /// </summary>
    public static int ResolveMaxRetries(int? maxRetries) =>
        maxRetries ?? DefaultMaxRetries;
}
