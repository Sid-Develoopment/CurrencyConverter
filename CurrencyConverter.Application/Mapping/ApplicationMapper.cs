using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models;

namespace CurrencyConverter.Application.Mapping;

public static class ApplicationMapper
{
    public static ConvertResponseDto ToDto(this ConversionResult result) => new()
    {
        From = result.From.ToString(),
        To = result.To.ToString(),
        Amount = result.OriginalAmount.Value,
        Result = result.ConvertedAmount.Value,
        AppliedRate = result.AppliedRate.Value,
        Timestamp = result.RateTimestamp.UtcDateTime,
        Strategy = result.StrategyUsed,
        Warning = result.Warning
    };

    public static RatesSummaryDto ToRatesDto(
        IReadOnlyCollection<ExchangeRate> rates, 
        DateTimeOffset fetchedAt, 
        string providerId)
    {
        var dict = rates
            .Where(r => r.BaseCurrency != r.QuoteCurrency) // Исключаем само-курсы если есть
            .GroupBy(r => r.QuoteCurrency.ToString()) // Группируем по целевой валюте
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var latest = g.OrderByDescending(r => r.ValidPeriod.Start).First();
                    return new CurrencyRateInfoDto
                    {
                        Code = latest.QuoteCurrency.ToString(),
                        BaseCurrency = latest.BaseCurrency.ToString(),
                        Value = latest.Rate.Value,
                        Nominal = 1, // Нормализовано в домене
                        Date = latest.ValidPeriod.Start.UtcDateTime,
                        PreviousValue = null // Можно расширить при наличии исторических данных
                    };
                },
                StringComparer.OrdinalIgnoreCase);

        return new RatesSummaryDto
        {
            Rates = dict,
            FetchedAt = fetchedAt.UtcDateTime,
            ProviderId = providerId
        };
    }
}