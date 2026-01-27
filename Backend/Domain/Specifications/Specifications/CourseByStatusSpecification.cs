using System.Linq.Expressions;
using Domain.Entities;
using Domain.Enums;

namespace Domain.Specifications.Specifications
{
    public class CourseByStatusSpecification : Specification<Course>
    {
        private readonly CourseStatus _status;

        public CourseByStatusSpecification(CourseStatus status)
        {
            _status = status;
        }

        public override Expression<Func<Course, bool>> ToExpression()
        {
            return course => course.Status == _status;
        }
    }
}

