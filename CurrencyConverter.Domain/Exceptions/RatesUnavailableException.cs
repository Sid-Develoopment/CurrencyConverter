namespace CurrencyConverter.Domain.Exceptions;

public class RatesUnavailableException : DomainException
{
    public RatesUnavailableException(string message) : base(message) { }
}