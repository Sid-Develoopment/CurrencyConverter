using CurrencyConverter.Domain.Entities;

namespace CurrencyConverter.Infrastructure.Providers;

/// <summary>
/// Абстракция источника курсов для инфраструктурного слоя
/// </summary>
public interface IRatesSource
{
    string ProviderId { get; }
    Task<IReadOnlyCollection<ExchangeRate>> FetchLatestAsync(CancellationToken ct = default);
}