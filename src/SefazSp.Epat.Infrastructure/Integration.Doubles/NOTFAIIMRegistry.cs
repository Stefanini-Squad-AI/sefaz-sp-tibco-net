#nullable enable

using SefazSp.Epat.Application.Abstractions.Processes;

namespace SefazSp.Epat.Infrastructure.Integration.Doubles;

/// <summary>
/// Registo de destinos da interface de processo NOTFAIIM.
/// Validado no arranque da aplicacao: um destino sem implementacao concreta do duble
/// lanca <see cref="InvalidOperationException"/> de forma imediata e visivel.
/// NAO herda HaltOnBadSubProcess=false do legado TIBCO.
///
/// Origem da chamada:
///   callActivity <c>_0XWagVqNEfG5K7mY0I3I6w</c> "Aguardar evento de Notificacao do AIIM"
///   no processo POC_EpatProcess — card BUILD-POCEPATPROCESS-seg001, etapas 1 e 2.
///   O callee e resolvido em runtime pelo campo de caso DEAT0050.
///
/// Destinos registados: DEAT0050 — processo que implementa INOTFAIIM, registado com chave "DEAT0050".
/// </summary>
public sealed class NOTFAIIMRegistry
{
    private readonly IReadOnlyDictionary<string, INOTFAIIM> _destinations;

    public NOTFAIIMRegistry(IReadOnlyDictionary<string, INOTFAIIM> destinations)
    {
        if (destinations is null || destinations.Count == 0)
            throw new InvalidOperationException(
                "NOTFAIIMRegistry: nenhum destino registado. " +
                "Cada destino de NOTFAIIM requer uma implementacao concreta do duble (pacote DEAT0050). " +
                "Destino sem implementacao falha visivelmente no arranque, nao em silencio.");

        _destinations = destinations;
    }

    /// <summary>
    /// Resolve o duble para o <paramref name="destination"/> indicado.
    /// Lanca <see cref="InvalidOperationException"/> se o destino nao estiver registado.
    /// </summary>
    public INOTFAIIM Resolve(string destination)
    {
        if (!_destinations.TryGetValue(destination, out var impl))
            throw new InvalidOperationException(
                $"NOTFAIIMRegistry: destino '{destination}' nao tem duble registado. " +
                "Registe a implementacao antes de iniciar a aplicacao (pacote DEAT0050).");

        return impl;
    }

    /// <summary>Destinos actualmente registados (para diagnostico e testes).</summary>
    public IEnumerable<string> RegisteredDestinations => _destinations.Keys;
}
