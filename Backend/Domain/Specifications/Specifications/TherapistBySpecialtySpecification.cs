using System.Linq.Expressions;
using Domain.Entities;

namespace Domain.Specifications.Specifications
{
    public class TherapistBySpecialtySpecification : Specification<Therapist>
    {
        private readonly string _specialtyName;

        public TherapistBySpecialtySpecification(string specialtyName)
        {
            if (string.IsNullOrWhiteSpace(specialtyName))
                throw new ArgumentException("Specialty name cannot be null or empty", nameof(specialtyName));

            _specialtyName = specialtyName;
            AddInclude(t => t.Specialties);
        }

        public override Expression<Func<Therapist, bool>> ToExpression()
        {
            return therapist => therapist.Specialties.Any(s => s.Name == _specialtyName);
        }
    }
}

