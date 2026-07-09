using CurrencyConverter.Domain.Exceptions;

namespace CurrencyConverter.Domain.ValueObjects;

public sealed class Amount : IEquatable<Amount>, IComparable<Amount>
{
    public decimal Value { get; }
    public int Precision { get; }

    private Amount(decimal value, int precision)
    {
        Value = value;
        Precision = precision;
    }

    public static Amount Create(decimal value, int precision = 4)
    {
        if (precision < 0 || precision > 28)
            throw new InvalidAmountException("Precision must be between 0 and 28");

        if (value < 0)
            throw new InvalidAmountException("Amount cannot be negative");
        
        var rounded = Math.Round(value, precision, MidpointRounding.ToEven);
        
        return new Amount(rounded, precision);
    }

    public Amount Add(Amount other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        
        var commonPrecision = Math.Max(Precision, other.Precision);
        var result = Math.Round(Value + other.Value, commonPrecision, MidpointRounding.ToEven);
        
        return Create(result, commonPrecision);
    }

    public Amount Multiply(decimal factor, int? targetPrecision = null)
    {
        if (factor < 0)
            throw new InvalidAmountException("Multiplication factor cannot be negative");

        var precision = targetPrecision ?? Precision;
        var result = Math.Round(Value * factor, precision, MidpointRounding.ToEven);
        
        return Create(result, precision);
    }

    public bool IsZero() => Value == 0m;
    public bool IsGreaterThan(Amount? other) => other is not null && Value > other.Value;
    
    public Amount WithPrecision(int newPrecision) => 
        Create(Value, newPrecision);

    public bool Equals(Amount? other) => 
        other is not null && Value == other.Value && Precision == other.Precision;

    public override bool Equals(object? obj) => Equals(obj as Amount);
    public override int GetHashCode() => HashCode.Combine(Value, Precision);
    public int CompareTo(Amount? other) => other is null ? 1 : Value.CompareTo(other.Value);
    
    public override string ToString() => Value.ToString($"F{Precision}");

    public static bool operator ==(Amount? left, Amount? right) => 
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Amount? left, Amount? right) => 
        !(left == right);

    public static bool operator >(Amount left, Amount right) => 
        left.CompareTo(right) > 0;

    public static bool operator <(Amount left, Amount right) => 
        left.CompareTo(right) < 0;
}