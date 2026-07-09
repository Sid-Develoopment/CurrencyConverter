using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Entities;

public sealed class Currency : IEquatable<Currency>
{
    public CurrencyCode Code { get; }
    public string Name { get; }
    public string? Symbol { get; }
    public int? MinorUnits { get; }
    public string? Country { get; }

    private Currency(CurrencyCode code, string name, string? symbol, int? minorUnits, string? country)
    {
        Code = code;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Symbol = symbol;
        MinorUnits = minorUnits;
        Country = country;
    }

    public static Currency Create(CurrencyCode code, string name, string? symbol = null, int? minorUnits = null, string? country = null) =>
        new(code, name, symbol, minorUnits, country);

    public bool Equals(Currency? other) => 
        other is not null && Code == other.Code;

    public override bool Equals(object? obj) => Equals(obj as Currency);
    public override int GetHashCode() => Code.GetHashCode();
    public override string ToString() => $"{Code} - {Name}";
}