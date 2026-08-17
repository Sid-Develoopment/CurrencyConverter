using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models;
using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Services;

public interface IConversionStrategy
{
    string StrategyId { get; }

    ConversionResult? TryConvert(
        Amount amount,
        CurrencyCode from,
        CurrencyCode to,
        List<ExchangeRate> availableRates,
        ConversionOptions options);
    
    bool CanHandle(CurrencyCode from, CurrencyCode to, List<ExchangeRate> availableRates);
}