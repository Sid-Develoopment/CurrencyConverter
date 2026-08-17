using System.Text.Json;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Infrastructure.Configuration;
using CurrencyConverter.Infrastructure.Extensions;
using CurrencyConverter.Infrastructure.Providers.Mapping;

namespace CurrencyConverter.Infrastructure.Providers;

public class CbrRatesSource : IRatesSource
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CbrRatesSource> _logger;
    private const string Endpoint = "daily_json.js";
    
    public string ProviderId => "cbr";

    public CbrRatesSource(
        HttpClient httpClient, 
        ILogger<CbrRatesSource> logger,
        IOptionsMonitor<ProviderOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        var opts = options.CurrentValue;
        if (string.IsNullOrWhiteSpace(opts.BaseUrl))
            throw new InvalidOperationException("CBR BaseUrl is not configured");
            
        _httpClient.BaseAddress = new Uri(opts.BaseUrl);
    }

    public async Task<IReadOnlyCollection<ExchangeRate>> FetchLatestAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching latest rates from {Provider}...", ProviderId);
        
        try
        {
            var json = await _httpClient.GetStringAsync(Endpoint, ct);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };
            
            var raw = JsonSerializer.Deserialize<CbrJsonResponse>(json, options);
            
            if (raw is null)
                throw new InfrastructureException("Received null response from CBR API");

            var rates = CbrRatesMapper.MapToDomain(raw);
            _logger.LogInformation("Fetched {Count} valid rates from CBR", rates.Count);
            
            return rates;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching CBR rates");
            throw new InfrastructureException("Failed to connect to CBR rates API", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON from CBR API");
            throw new InfrastructureException("Failed to parse CBR rates response", ex);
        }
    }
}