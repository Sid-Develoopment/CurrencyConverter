using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Exceptions;
using CurrencyConverter.Domain.Models;
using CurrencyConverter.Domain.Ports;
using CurrencyConverter.Domain.Services;
using CurrencyConverter.Domain.ValueObjects;
using CurrencyConverter.Infrastructure.Configuration;
using CurrencyConverter.Infrastructure.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CurrencyConverter.Infrastructure.Caching;

/// <summary>
/// Репозиторий с прозрачным in-memory кэшированием и graceful degradation
/// </summary>
public class CachedRatesRepository : IRatesRepository
{
    private readonly IRatesSource _source;
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _cacheOptions;
    private readonly ILogger<CachedRatesRepository> _logger;
    private const string CacheKey = "rates:latest";

    public CachedRatesRepository(
        IRatesSource source,
        IMemoryCache cache,
        IOptions<CacheOptions> cacheOptions,
        ILogger<CachedRatesRepository> logger)
    {
        _source = source;
        _cache = cache;
        _cacheOptions = cacheOptions.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<ExchangeRate>> GetRatesAsync(
        CurrencyCode? baseCurrency = null,
        IEnumerable<CurrencyCode>? quoteCurrencies = null,
        DateTimeOffset? asOf = null,
        CancellationToken ct = default)
    {
        var targetAsOf = asOf ?? DateTimeOffset.UtcNow;
        
        // Попытка получить из кэша
        if (_cache.TryGetValue(CacheKey, out IReadOnlyCollection<ExchangeRate>? cached) && 
            cached is not null && cached.Count > 0)
        {
            _logger.LogDebug("Cache HIT for latest rates");
            return FilterRates(cached, baseCurrency, quoteCurrencies, targetAsOf);
        }

        _logger.LogDebug("Cache MISS. Fetching from source...");
        var fresh = await _source.FetchLatestAsync(ct);
        
        if (fresh.Count == 0)
            throw new RatesUnavailableException("No rates returned from source");

        // Кэширование с двойной политикой истечения
        var entry = _cache.CreateEntry(CacheKey);
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheOptions.AbsoluteExpirationMinutes);
        entry.SlidingExpiration = TimeSpan.FromMinutes(_cacheOptions.SlidingExpirationMinutes);
        entry.Priority = CacheItemPriority.High;
        entry.Value = fresh;
        entry.Dispose(); // Записываем в кэш

        _logger.LogInformation("Rates cached successfully. Valid for {Minutes} min", 
            _cacheOptions.AbsoluteExpirationMinutes);
            
        return FilterRates(fresh, baseCurrency, quoteCurrencies, targetAsOf);
    }

    public async Task<ExchangeRate?> FindRateAsync(
        CurrencyCode baseCurrency, 
        CurrencyCode quoteCurrency, 
        DateTimeOffset? asOf = null, 
        CancellationToken ct = default)
    {
        var all = await GetRatesAsync(ct: ct);
        var targetAsOf = asOf ?? DateTimeOffset.UtcNow;
        
        return all.FirstOrDefault(r => 
            r.BaseCurrency == baseCurrency && 
            r.QuoteCurrency == quoteCurrency && 
            r.IsValidAt(targetAsOf));
    }

    public async Task<bool> HasConversionPathAsync(
        CurrencyCode from, 
        CurrencyCode to, 
        ConversionOptions options, 
        CancellationToken ct = default)
    {
        if (from == to) return true;
        
        var rates = await GetRatesAsync(ct: ct);
        var resolver = new ConversionStrategyResolver(new IConversionStrategy[]
        {
            new DirectConversionStrategy(),
            new TriangulationConversionStrategy(CurrencyCode.RUB)
        });

        try
        {
            resolver.Convert(Amount.Create(1m), from, to, rates, options);
            return true;
        }
        catch (ConversionImpossibleException)
        {
            return false;
        }
    }

    private static IReadOnlyCollection<ExchangeRate> FilterRates(
        IEnumerable<ExchangeRate> source,
        CurrencyCode? baseCurrency,
        IEnumerable<CurrencyCode>? quoteCurrencies,
        DateTimeOffset asOf)
    {
        var query = source.Where(r => r.IsValidAt(asOf));
        
        if (baseCurrency is not null)
            query = query.Where(r => r.BaseCurrency == baseCurrency);
            
        if (quoteCurrencies is not null)
        {
            var quoteSet = new HashSet<CurrencyCode>(quoteCurrencies);
            query = query.Where(r => quoteSet.Contains(r.QuoteCurrency));
        }
        
        return query.ToList().AsReadOnly();
    }
}