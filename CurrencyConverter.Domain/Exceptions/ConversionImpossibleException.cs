using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Exceptions;

public sealed class ConversionImpossibleException : DomainException
{
    public CurrencyCode From { get; }
    public CurrencyCode To { get; }
    
    public ConversionImpossibleException(CurrencyCode from, CurrencyCode to, string message) 
        : base(message)
    {
        From = from;
        To = to;
    }
}