namespace CurrencyConverter.Application.Exceptions;

public class RatesUnavailableException : ApplicationException
{
    public RatesUnavailableException(string message) : base(message) { }
    public RatesUnavailableException(string message, Exception inner) : base(message, inner) { }
}