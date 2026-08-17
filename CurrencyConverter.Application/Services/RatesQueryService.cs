using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Application.Exceptions;
using CurrencyConverter.Application.Mapping;
using CurrencyConverter.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Application.Services;

public class RatesQueryService : IRatesQueryService
{
    private readonly IRatesRepository _repository;
    private readonly ILogger<RatesQueryService> _logger;

    public RatesQueryService(IRatesRepository repository, ILogger<RatesQueryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<RatesSummaryDto> GetLatestRatesAsync(CancellationToken ct = default)
    {
        var rates = await _repository.GetRatesAsync(ct: ct);
        
        if (rates.Count == 0)
            throw new RatesUnavailableException("No rates available to query.");

        // Берём провайдер из первого курса (в реальном приложении можно агрегировать)
        var providerId = rates.FirstOrDefault()?.ProviderId ?? "unknown";
        var fetchedAt = DateTimeOffset.UtcNow;

        _logger.LogDebug("Returning {Count} currency rates from provider {Provider}", 
            rates.Count, providerId);

        return ApplicationMapper.ToRatesDto(rates, fetchedAt, providerId);
    }
}