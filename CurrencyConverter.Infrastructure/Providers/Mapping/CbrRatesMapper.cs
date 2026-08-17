using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Infrastructure.Providers.Mapping;

internal record CbrJsonResponse
{
public DateTime Date { get; init; }
public Dictionary<string, CbrCurrencyEntry>? Valute { get; init; }
}

internal record CbrCurrencyEntry
{
    public string ID { get; init; } = string.Empty;
    public string NumCode { get; init; } = string.Empty;
    public string CharCode { get; init; } = string.Empty;
    public int Nominal { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public decimal Previous { get; init; }
}

internal static class CbrRatesMapper
{
    public static IReadOnlyCollection<ExchangeRate> MapToDomain(CbrJsonResponse raw)
    {
        if (raw.Valute is null || raw.Valute.Count == 0)
            return Array.Empty<ExchangeRate>();

        var validAt = DateTimeOffset.SpecifyKind(raw.Date, DateTimeKind.Utc);
        var rates = new List<ExchangeRate>(raw.Valute.Count);

        foreach (var entry in raw.Valute.Values)
        {
            try
            {
                var baseCurrency = CurrencyCode.Create(entry.CharCode);
                // ЦБ РФ даёт курсы к RUB: 1 USD = X RUB
                var rateValue = ExchangeRateValue.Create(entry.Value / entry.Nominal);
                
                rates.Add(ExchangeRate.Create(
                    baseCurrency: baseCurrency,
                    quoteCurrency: CurrencyCode.RUB,
                    rate: rateValue,
                    validAt: validAt,
                    providerId: "cbr",
                    metadata: new Dictionary<string, object>
                    {
                        { "CbrId", entry.ID },
                        { "NumCode", entry.NumCode },
                        { "Nominal", entry.Nominal }
                    }));
            }
            catch (Exception)
            {
                // Пропускаем невалидные записи, логируем на уровне источника
            }
        }

        return rates;
    }
}
