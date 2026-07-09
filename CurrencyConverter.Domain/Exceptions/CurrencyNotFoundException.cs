using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Exceptions;

public sealed class CurrencyNotFoundException : DomainException
{
    public CurrencyCode MissingCurrency { get; }
    
    public CurrencyNotFoundException(CurrencyCode currency) 
        : base($"Currency not found: {currency}")
    {
        MissingCurrency = currency;
    }
}