using CurrencyConverter.Domain.Ports;
using CurrencyConverter.Infrastructure.Caching;
using CurrencyConverter.Infrastructure.Configuration;
using CurrencyConverter.Infrastructure.Providers;
using CurrencyConverter.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CurrencyConverter.Infrastructure.Exceptions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // 1. Конфигурация
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.Section));
        services.Configure<ProviderOptions>(configuration.GetSection(ProviderOptions.Section));

        // 2. Кэш
        services.AddMemoryCache(options =>
        {
            var cacheOpts = configuration.GetSection(CacheOptions.Section).Get<CacheOptions>() ?? new();
            options.SizeLimit = cacheOpts.SizeLimit;
            options.CompactionPercentage = 0.2;
        });

        // 3. HttpClient с Polly
        services.AddHttpClient<IRatesSource, CbrRatesSource>()
            .AddPolicyHandler(ResiliencePolicies.GetRetryPolicy())
            .AddPolicyHandler(ResiliencePolicies.GetCircuitBreakerPolicy())
            .ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptionsMonitor<ProviderOptions>>().CurrentValue;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("CurrencyConverterMicroservice/1.0");
            });

        // 4. Регистрация порта домена
        services.AddScoped<IRatesRepository, CachedRatesRepository>();

        return services;
    }
}