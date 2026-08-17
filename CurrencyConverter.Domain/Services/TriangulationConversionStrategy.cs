using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models;
using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Services;

public sealed class TriangulationConversionStrategy : IConversionStrategy
{
    private readonly CurrencyCode _triangulationBase;

    public TriangulationConversionStrategy(CurrencyCode triangulationBase)
    {
        _triangulationBase = triangulationBase;
    }

    public string StrategyId => $"triangulation:{_triangulationBase}";

    public bool CanHandle(CurrencyCode from, CurrencyCode to, List<ExchangeRate> availableRates)
    {
        if (from == to) return true;
        if (from == _triangulationBase || to == _triangulationBase) return true;
        
        var asOf = DateTimeOffset.UtcNow;
        var hasFromToBase = availableRates.Any(r => 
            r.BaseCurrency == from && r.QuoteCurrency == _triangulationBase && r.IsValidAt(asOf));
        var hasBaseToTo = availableRates.Any(r => 
            r.BaseCurrency == _triangulationBase && r.QuoteCurrency == to && r.IsValidAt(asOf));
            
        return hasFromToBase && hasBaseToTo;
    }

    public ConversionResult? TryConvert(
        Amount amount,
        CurrencyCode from,
        CurrencyCode to,
        List<ExchangeRate> availableRates,
        ConversionOptions options)
    {
        if (from == to)
            return new ConversionResult(
                amount, from, to, amount,
                ExchangeRateValue.Create(1m),
                DateTimeOffset.UtcNow,
                StrategyId);

        if (!CanHandle(from, to, availableRates))
            return null;

        var asOf = options.AsOf ?? DateTimeOffset.UtcNow;
        
        var rateFromToBase = availableRates.First(r => 
            r.BaseCurrency == from && r.QuoteCurrency == _triangulationBase && r.IsValidAt(asOf));
        
        var rateBaseToTo = availableRates.First(r => 
            r.BaseCurrency == _triangulationBase && r.QuoteCurrency == to && r.IsValidAt(asOf));

        // Композиция курсов: (From→Base) * (Base→To) = From→To
        var composedRate = rateFromToBase.Rate.ComposeWith(rateBaseToTo.Rate);
        var result = composedRate.ApplyTo(amount, options.ResultPrecision);

        // Предупреждение, если курсы из разных источников/времени
        var warning = BuildWarning(rateFromToBase, rateBaseToTo);

        return new ConversionResult(
            OriginalAmount: amount,
            From: from,
            To: to,
            ConvertedAmount: result,
            AppliedRate: composedRate,
            RateTimestamp: asOf,
            StrategyUsed: StrategyId,
            IsExactMatch: rateFromToBase.ProviderId == rateBaseToTo.ProviderId,
            Warning: warning);
    }

    private static string? BuildWarning(ExchangeRate r1, ExchangeRate r2)
    {
        if (r1.ProviderId != r2.ProviderId)
            return $"Rates from different providers: {r1.ProviderId}, {r2.ProviderId}";
        
        var timeDiff = Math.Abs((r1.ValidPeriod.Start - r2.ValidPeriod.Start).TotalHours);
        if (timeDiff > 1)
            return $"Rates timestamp difference: {timeDiff:F1} hours";
            
        return null;
    }
}