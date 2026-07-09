using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Models;

public sealed record ConversionResult(
    Amount OriginalAmount,
    CurrencyCode From,
    CurrencyCode To,
    Amount ConvertedAmount,
    ExchangeRateValue AppliedRate,
    DateTimeOffset RateTimestamp,
    string StrategyUsed,
    bool IsExactMatch = true,
    string? Warning = null);