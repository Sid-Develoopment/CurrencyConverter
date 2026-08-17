using System.Text.RegularExpressions;
using CurrencyConverter.Domain.Exceptions;

namespace CurrencyConverter.Domain.ValueObjects;

public sealed class CurrencyCode : IEquatable<CurrencyCode>
{
    private static readonly Regex ValidPattern = new("^[A-Z]{3}$", RegexOptions.Compiled);
    
    public string Value { get; }
    
    private CurrencyCode(string value) => Value = value;

    public static CurrencyCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidCurrencyCodeException("Currency code cannot be empty");

        var normalized = code.Trim().ToUpperInvariant();
        
        if (!ValidPattern.IsMatch(normalized))
            throw new InvalidCurrencyCodeException(
                $"Invalid currency code format: '{code}'. Expected ISO 4217 format (e.g., USD, EUR)");

        return new CurrencyCode(normalized);
    }
    
    public static CurrencyCode RUB => new("RUB");
    public static CurrencyCode USD => new("USD");
    public static CurrencyCode EUR => new("EUR");
    
    public bool Equals(CurrencyCode? other) => 
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) => 
        Equals(obj as CurrencyCode);

    public override int GetHashCode() => 
        Value.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Value;

    public static bool operator ==(CurrencyCode? left, CurrencyCode? right) => 
        left?.Equals(right) ?? right is null;

    public static bool operator !=(CurrencyCode left, CurrencyCode right) => 
        !(left == right);
}
