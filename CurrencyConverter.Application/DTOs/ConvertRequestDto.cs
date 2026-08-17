namespace CurrencyConverter.Application.DTOs;

public record ConvertRequestDto
{
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int? Precision { get; init; } = 4;
}