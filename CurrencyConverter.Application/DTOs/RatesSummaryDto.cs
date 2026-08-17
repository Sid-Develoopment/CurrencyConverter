namespace CurrencyConverter.Application.DTOs;

public record RatesSummaryDto
{
    public IReadOnlyDictionary<string, CurrencyRateInfoDto> Rates { get; init; } = 
        new Dictionary<string, CurrencyRateInfoDto>();
    public DateTime FetchedAt { get; init; }
    public string ProviderId { get; init; } = string.Empty;
}