namespace CurrencyConverter.Domain.Models;

public sealed record ConversionOptions(
    int ResultPrecision = 4,
    bool AllowTriangulation = true,
    int MaxChainLength = 3,
    DateTimeOffset? AsOf = null,
    string? PreferredProvider = null);