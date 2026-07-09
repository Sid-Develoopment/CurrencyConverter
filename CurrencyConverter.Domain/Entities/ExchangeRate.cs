using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Entities;

public sealed class ExchangeRate : IEquatable<ExchangeRate>
{
    public CurrencyCode BaseCurrency { get; }
    public CurrencyCode QuoteCurrency { get; }
    public DateRange ValidPeriod { get; }
    

    public ExchangeRateValue Rate { get; }
    public string ProviderId { get; }
    public DateTimeOffset FetchedAt { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }

    private ExchangeRate(
        CurrencyCode baseCurrency,
        CurrencyCode quoteCurrency,
        DateRange validPeriod,
        ExchangeRateValue rate,
        string providerId,
        DateTimeOffset fetchedAt,
        IReadOnlyDictionary<string, object>? metadata)
    {
        if (baseCurrency == quoteCurrency)
            throw new ArgumentException("Base and quote currencies must differ");

        BaseCurrency = baseCurrency;
        QuoteCurrency = quoteCurrency;
        ValidPeriod = validPeriod;
        Rate = rate;
        ProviderId = providerId ?? throw new ArgumentNullException(nameof(providerId));
        FetchedAt = fetchedAt;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    public static ExchangeRate Create(
        CurrencyCode baseCurrency,
        CurrencyCode quoteCurrency,
        ExchangeRateValue rate,
        DateTimeOffset validAt,
        string providerId,
        DateTimeOffset? fetchedAt = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new ExchangeRate(
            baseCurrency,
            quoteCurrency,
            DateRange.At(validAt),
            rate,
            providerId,
            fetchedAt ?? DateTimeOffset.UtcNow,
            metadata);
    }
    
    public static ExchangeRate CreateWithPeriod(
        CurrencyCode baseCurrency,
        CurrencyCode quoteCurrency,
        ExchangeRateValue rate,
        DateTimeOffset start,
        DateTimeOffset end,
        string providerId,
        DateTimeOffset? fetchedAt = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new ExchangeRate(
            baseCurrency,
            quoteCurrency,
            DateRange.Between(start, end),
            rate,
            providerId,
            fetchedAt ?? DateTimeOffset.UtcNow,
            metadata);
    }

    public bool IsValidAt(DateTimeOffset pointInTime) => 
        ValidPeriod.Contains(pointInTime);

    public ExchangeRate Inverse() => 
        new ExchangeRate(
            QuoteCurrency,
            BaseCurrency,
            ValidPeriod,
            Rate.Inverse(),
            ProviderId,
            FetchedAt,
            Metadata);

    public Amount Convert(Amount amount, int? resultPrecision = null) => 
        Rate.ApplyTo(amount, resultPrecision);

    public string GetCacheKey() => 
        $"rate:{BaseCurrency}:{QuoteCurrency}:{ValidPeriod.Start:yyyy-MM-dd}";
    
    public bool Equals(ExchangeRate? other)
    {
        if (other is null) 
            return false;
        if (ReferenceEquals(this, other)) 
            return true;
        
        return BaseCurrency == other.BaseCurrency
            && QuoteCurrency == other.QuoteCurrency
            && ValidPeriod.Equals(other.ValidPeriod)
            && Rate == other.Rate;
    }

    public override bool Equals(object? obj) => Equals(obj as ExchangeRate);
    public override int GetHashCode() => 
        HashCode.Combine(BaseCurrency, QuoteCurrency, ValidPeriod, Rate);
}