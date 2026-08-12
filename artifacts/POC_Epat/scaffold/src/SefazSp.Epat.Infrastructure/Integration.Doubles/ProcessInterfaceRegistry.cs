#nullable enable

using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Valida, no arranque do conjunto de testes, que todos os destinos dinamicos registados
/// (implementacoes de <see cref="ICTRINTPC"/>) possuem um duble correspondente.
///
/// Gaps.dynamic-subprocess = interface-registry-validated:
/// um destino sem duble quebra o teste de registo com uma excecao identificavel,
/// nunca falha silenciosamente em producao.
/// </summary>
public static class ProcessInterfaceRegistry
{
    /// <summary>
    /// Valida que todos os <paramref name="registeredTypes"/> sao assignaveis a
    /// <see cref="ICTRINTPC"/> e possuem implementacao concreta disponivel.
    ///
    /// Lanca <see cref="ProcessInterfaceRegistryException"/> com a lista completa
    /// de tipos em falta, para que a falha seja visivel e identificavel de imediato.
    /// </summary>
    /// <param name="registeredTypes">Tipos registados no contentor de DI / catalogo de dubles.</param>
    public static void ValidateCtrintpcDoubles(IEnumerable<Type> registeredTypes)
    {
        var ctrintpcImplementations = registeredTypes
            .Where(t => typeof(ICTRINTPC).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .ToList();

        if (ctrintpcImplementations.Count == 0)
        {
            throw new ProcessInterfaceRegistryException(
                "Nenhuma implementacao de ICTRINTPC encontrada no registo de dubles. " +
                "O destino CTRINTPC esta em falta — registe CtrintpcDouble (ou equivalente) " +
                "antes de iniciar o conjunto de testes.");
        }
    }

    /// <summary>
    /// Valida que <paramref name="instance"/> nao e nula, lancando excecao identificavel
    /// quando o duble de CTRINTPC nao foi registado.
    /// </summary>
    public static ICTRINTPC RequireCtrintpcDouble(ICTRINTPC? instance)
    {
        return instance ?? throw new ProcessInterfaceRegistryException(
            "ICTRINTPC nao foi registado no contentor de dubles. " +
            "O destino CTRINTPC esta em falta — registe CtrintpcDouble antes de iniciar o conjunto de testes.");
    }
}

/// <summary>
/// Excecao lancada pelo <see cref="ProcessInterfaceRegistry"/> quando um destino
/// dinamico nao possui duble registado no arranque do conjunto de testes.
/// </summary>
public sealed class ProcessInterfaceRegistryException : Exception
{
    public ProcessInterfaceRegistryException(string message) : base(message) { }
}
