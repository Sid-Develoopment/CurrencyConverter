using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models;
using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Services;

/// <summary>
/// Прямая конвертация при наличии курса From→To
/// </summary>
public sealed class DirectConversionStrategy : IConversionStrategy
{
    public string StrategyId => "direct";

    public bool CanHandle(CurrencyCode from, CurrencyCode to, List<ExchangeRate> availableRates) =>
        FindDirectRate(from, to, availableRates) is not null;

    public ConversionResult? TryConvert(
        Amount amount,
        CurrencyCode from,
        CurrencyCode to,
        List<ExchangeRate> availableRates,
        ConversionOptions options)
    {
        if (!CanHandle(from, to, availableRates))
            return null;

        var rate = FindDirectRate(from, to, availableRates)!;
        var result = rate.Convert(amount, options.ResultPrecision);

        return new ConversionResult(
            OriginalAmount: amount,
            From: from,
            To: to,
            ConvertedAmount: result,
            AppliedRate: rate.Rate,
            RateTimestamp: rate.ValidPeriod.Start,
            StrategyUsed: StrategyId,
            IsExactMatch: true);
    }

    private static ExchangeRate? FindDirectRate(
        CurrencyCode from,
        CurrencyCode to,
        IEnumerable<ExchangeRate> rates)
    {
        var asOf = DateTimeOffset.UtcNow;
        
        return rates.FirstOrDefault(r => 
            r.BaseCurrency == from 
            && r.QuoteCurrency == to 
            && r.IsValidAt(asOf));
    }
}