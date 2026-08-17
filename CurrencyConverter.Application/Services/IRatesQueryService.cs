using CurrencyConverter.Application.DTOs;

namespace CurrencyConverter.Application.Services;

public interface IRatesQueryService
{
    Task<RatesSummaryDto> GetLatestRatesAsync(CancellationToken ct = default);
}