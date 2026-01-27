using System.Linq.Expressions;

namespace Domain.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> ToExpression();
        bool IsSatisfiedBy(T entity);
        IReadOnlyList<Expression<Func<T, object?>>> Includes { get; }
    }
}

