using System.Linq.Expressions;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models;
using CurrencyConverter.Domain.Services;
using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Specifications;

public class CanConvertSpecification : ISpecification<(CurrencyCode, CurrencyCode, IEnumerable<ExchangeRate>)>
{
    private readonly ConversionOptions _options;
    private readonly IEnumerable<IConversionStrategy> _strategies;

    public CanConvertSpecification(ConversionOptions options, IEnumerable<IConversionStrategy> strategies)
    {
        _options = options;
        _strategies = strategies ?? Enumerable.Empty<IConversionStrategy>();
    }

    public bool IsSatisfiedBy((CurrencyCode, CurrencyCode, IEnumerable<ExchangeRate>) candidate)
    {
        var (from, to, rates) = candidate;
        
        if (from == to) return true;
        
        return _strategies.Any(s => s.CanHandle(from, to, rates));
    }
    
    public Expression<Func<(CurrencyCode, CurrencyCode, IEnumerable<ExchangeRate>), bool>> ToExpression() =>
        candidate => IsSatisfiedBy(candidate);
}