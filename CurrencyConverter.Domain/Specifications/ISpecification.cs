using System.Linq.Expressions;

namespace CurrencyConverter.Domain.Specifications;

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T candidate);
    Expression<Func<T, bool>> ToExpression();
}