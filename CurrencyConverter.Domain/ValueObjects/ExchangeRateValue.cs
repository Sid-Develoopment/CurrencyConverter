using CurrencyConverter.Domain.Exceptions;

namespace CurrencyConverter.Domain.ValueObjects;

public sealed class ExchangeRateValue : IEquatable<ExchangeRateValue>
{
    public decimal Value { get; }
    
    private const int DefaultPrecision = 6;

    private ExchangeRateValue(decimal value) => Value = value;

    public static ExchangeRateValue Create(decimal value, int? precision = null)
    {
        if (value <= 0)
            throw new InvalidAmountException("Exchange rate must be positive");

        var p = precision ?? DefaultPrecision;
        var rounded = Math.Round(value, p, MidpointRounding.ToEven);
        
        return new ExchangeRateValue(rounded);
    }
    
    public ExchangeRateValue Inverse() => 
        Create(1m / Value);
    
    public Amount ApplyTo(Amount amount, int? resultPrecision = null) => 
        amount.Multiply(Value, resultPrecision);
    
    public ExchangeRateValue ComposeWith(ExchangeRateValue other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Create(Value * other.Value);
    }

    public bool Equals(ExchangeRateValue? other) => 
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as ExchangeRateValue);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString("F6");

    public static bool operator ==(ExchangeRateValue? left, ExchangeRateValue? right) => 
        left?.Equals(right) ?? right is null;

    public static bool operator !=(ExchangeRateValue? left, ExchangeRateValue? right) => 
        !(left == right);
}