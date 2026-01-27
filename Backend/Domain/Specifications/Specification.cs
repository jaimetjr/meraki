using System.Linq.Expressions;

namespace Domain.Specifications
{
    public abstract class Specification<T> : ISpecification<T>
    {
        protected readonly List<Expression<Func<T, object?>>> _includes = new();

        public IReadOnlyList<Expression<Func<T, object?>>> Includes => _includes.AsReadOnly();

        public abstract Expression<Func<T, bool>> ToExpression();

        public virtual bool IsSatisfiedBy(T entity)
        {
            var expression = ToExpression();
            var compiled = expression.Compile();
            return compiled(entity);
        }

        public Specification<T> And(ISpecification<T> other)
        {
            return new AndSpecification<T>(this, other);
        }

        public Specification<T> Or(ISpecification<T> other)
        {
            return new OrSpecification<T>(this, other);
        }

        public Specification<T> Not()
        {
            return new NotSpecification<T>(this);
        }

        protected void AddInclude(Expression<Func<T, object?>> includeExpression)
        {
            _includes.Add(includeExpression);
        }
    }

    internal class AndSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = _left.ToExpression();
            var rightExpression = _right.ToExpression();
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.AndAlso(
                Expression.Invoke(leftExpression, parameter),
                Expression.Invoke(rightExpression, parameter)
            );
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }

    internal class OrSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = _left.ToExpression();
            var rightExpression = _right.ToExpression();
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.OrElse(
                Expression.Invoke(leftExpression, parameter),
                Expression.Invoke(rightExpression, parameter)
            );
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }

    internal class NotSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _specification;

        public NotSpecification(ISpecification<T> specification)
        {
            _specification = specification;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var expression = _specification.ToExpression();
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.Not(Expression.Invoke(expression, parameter));
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}

