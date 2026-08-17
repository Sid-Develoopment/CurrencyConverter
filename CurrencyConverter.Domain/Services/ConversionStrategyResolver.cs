using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Exceptions;
using CurrencyConverter.Domain.Models;
using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Services;

/// <summary>
/// Резолвер: выбирает первую применимую стратегию из списка
/// </summary>
public sealed class ConversionStrategyResolver
{
    private readonly List<IConversionStrategy> _strategies;

    public ConversionStrategyResolver(IReadOnlyList<IConversionStrategy> strategies)
    {
        _strategies = strategies.ToList() ?? throw new ArgumentNullException(nameof(strategies));
        
        if (_strategies.Count == 0)
            throw new ArgumentException("At least one conversion strategy must be provided");
    }

    public ConversionResult Convert(
        Amount amount,
        CurrencyCode from,
        CurrencyCode to,
        IReadOnlyCollection<ExchangeRate> availableRates,
        ConversionOptions options)
    {
        if (from == to)
            return new ConversionResult(
                amount, from, to, amount,
                ExchangeRateValue.Create(1m),
                DateTimeOffset.UtcNow,
                "identity");

        foreach (var strategy in _strategies)
        {
            var result = strategy.TryConvert(amount, from, to, availableRates.ToList(), options);
            if (result is not null)
                return result;
        }

        throw new ConversionImpossibleException(from, to,
            $"Cannot convert {from} to {to}: no applicable strategy found. " +
            $"Available rates: {string.Join(", ", availableRates.Select(r => $"{r.BaseCurrency}->{r.QuoteCurrency}"))}");
    }

    /// <summary>
    /// Для тестов: получить список доступных стратегий
    /// </summary>
    public IReadOnlyList<string> GetAvailableStrategyIds() => 
        _strategies.Select(s => s.StrategyId).ToList();
}