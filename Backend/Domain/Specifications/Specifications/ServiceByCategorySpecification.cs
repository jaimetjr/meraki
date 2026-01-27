using System.Linq.Expressions;
using Domain.Entities;

namespace Domain.Specifications.Specifications
{
    public class ServiceByCategorySpecification : Specification<Service>
    {
        private readonly string _categoryName;

        public ServiceByCategorySpecification(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                throw new ArgumentException("Category name cannot be null or empty", nameof(categoryName));

            _categoryName = categoryName;
            AddInclude(s => s.Category);
        }

        public override Expression<Func<Service, bool>> ToExpression()
        {
            return service => service.Category != null && service.Category.Name == _categoryName;
        }
    }
}

