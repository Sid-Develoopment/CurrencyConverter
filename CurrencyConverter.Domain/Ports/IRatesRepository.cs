using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models;
using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Ports;

/// <summary>
/// Порт для получения курсов обмена
/// Реализуется в инфраструктурном слое
/// </summary>
public interface IRatesRepository
{
    /// <summary>
    /// Получить актуальные курсы для заданных валют
    /// </summary>
    Task<IReadOnlyCollection<ExchangeRate>> GetRatesAsync(
        CurrencyCode? baseCurrency = null,
        IEnumerable<CurrencyCode>? quoteCurrencies = null,
        DateTimeOffset? asOf = null,
        CancellationToken ct = default);

    /// <summary>
    /// Найти конкретный курс
    /// </summary>
    Task<ExchangeRate?> FindRateAsync(
        CurrencyCode baseCurrency,
        CurrencyCode quoteCurrency,
        DateTimeOffset? asOf = null,
        CancellationToken ct = default);

    /// <summary>
    /// Проверить наличие курсов для пары валют
    /// </summary>
    Task<bool> HasConversionPathAsync(
        CurrencyCode from,
        CurrencyCode to,
        ConversionOptions options,
        CancellationToken ct = default);
}