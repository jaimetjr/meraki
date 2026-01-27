using System.Linq.Expressions;
using Domain.Entities;

namespace Domain.Specifications.Specifications
{
    public class SpecialtyByNameSpecification : Specification<Specialty>
    {
        private readonly string _name;

        public SpecialtyByNameSpecification(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            _name = name;
        }

        public override Expression<Func<Specialty, bool>> ToExpression()
        {
            return specialty => specialty.Name == _name;
        }
    }
}

