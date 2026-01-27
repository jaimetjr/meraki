using System.Linq.Expressions;
using Domain.Entities;

namespace Domain.Specifications.Specifications
{
    public class BenefitByKeywordSpecification : Specification<Benefit>
    {
        private readonly string _keyword;

        public BenefitByKeywordSpecification(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentException("Keyword cannot be null or empty", nameof(keyword));

            _keyword = keyword;
        }

        public override Expression<Func<Benefit, bool>> ToExpression()
        {
            return benefit => benefit.Title.Contains(_keyword) || 
                             (benefit.Description != null && benefit.Description.Contains(_keyword));
        }
    }
}

