using CurrencyConverter.Application.DTOs;

namespace CurrencyConverter.Application.Services;

public interface ICurrencyConversionService
{
    Task<ConvertResponseDto> ConvertAsync(ConvertRequestDto request, CancellationToken ct = default);
}