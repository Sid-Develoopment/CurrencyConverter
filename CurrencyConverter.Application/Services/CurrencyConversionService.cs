using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Application.Exceptions;
using CurrencyConverter.Application.Mapping;
using CurrencyConverter.Domain.Models;
using CurrencyConverter.Domain.Ports;
using CurrencyConverter.Domain.Services;
using CurrencyConverter.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Application.Services;

public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly IRatesRepository _ratesRepository;
    private readonly ConversionStrategyResolver _strategyResolver;
    private readonly ILogger<CurrencyConversionService> _logger;

    public CurrencyConversionService(
        IRatesRepository ratesRepository,
        ConversionStrategyResolver strategyResolver,
        ILogger<CurrencyConversionService> logger)
    {
        _ratesRepository = ratesRepository;
        _strategyResolver = strategyResolver;
        _logger = logger;
    }

    public async Task<ConvertResponseDto> ConvertAsync(ConvertRequestDto request, CancellationToken ct = default)
    {
        // 1. Валидация через доменные примитивы (Fail Fast)
        var from = CurrencyCode.Create(request.From);
        var to = CurrencyCode.Create(request.To);
        var amount = Amount.Create(request.Amount);
        var precision = request.Precision ?? 4;

        // 2. Получение курсов
        var rates = await _ratesRepository.GetRatesAsync(
            baseCurrency: null, // Загружаем все доступные для поддержки триангуляции
            asOf: DateTimeOffset.UtcNow,
            ct: ct);

        if (rates.Count == 0)
        {
            _logger.LogWarning("No exchange rates available from repository");
            throw new RatesUnavailableException("Currency rates are currently unavailable. Please try again later.");
        }

        // 3. Настройка опций конвертации
        var options = new ConversionOptions(
            ResultPrecision: precision,
            AllowTriangulation: true,
            MaxChainLength: 3);

        // 4. Выполнение конвертации
        var result = _strategyResolver.Convert(amount, from, to, rates, options);

        // 5. Логирование и возврат DTO
        _logger.LogInformation(
            "Converted {From} → {To}: {Amount} = {Result} (Rate: {Rate}, Strategy: {Strategy})",
            from, to, amount.Value, result.ConvertedAmount.Value, 
            result.AppliedRate.Value, result.StrategyUsed);

        return result.ToDto();
    }
}