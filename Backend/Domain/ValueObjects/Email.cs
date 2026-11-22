using Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public class Email
    {
        public string Value { get; }

        public Email(string value)
        {
            if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new DomainException("Formato de e-mail inválido.");

            Value = value;
        }

        public override bool Equals(object? obj) =>
            obj is Email other && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();
    }
}
