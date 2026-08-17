namespace CurrencyConverter.Application.DTOs;

public record ConvertResponseDto
{
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal Result { get; init; }
    public decimal AppliedRate { get; init; }
    public DateTime Timestamp { get; init; }
    public string Strategy { get; init; } = string.Empty;
    public string? Warning { get; init; }
}