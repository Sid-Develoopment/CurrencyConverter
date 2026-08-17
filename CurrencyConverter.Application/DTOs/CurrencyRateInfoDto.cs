namespace CurrencyConverter.Application.DTOs;

public record CurrencyRateInfoDto
{
    public string Code { get; init; } = string.Empty;
    public string BaseCurrency { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public int Nominal { get; init; }
    public DateTime Date { get; init; }
    public decimal? PreviousValue { get; init; }
}