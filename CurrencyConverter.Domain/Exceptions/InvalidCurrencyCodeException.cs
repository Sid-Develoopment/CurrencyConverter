namespace CurrencyConverter.Domain.Exceptions;

public sealed class InvalidCurrencyCodeException : DomainException
{
    public InvalidCurrencyCodeException(string message) : base(message) { }
}