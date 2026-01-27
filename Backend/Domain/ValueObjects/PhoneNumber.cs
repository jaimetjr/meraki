using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{
    public class PhoneNumber
    {
        public string Value { get; }

        public PhoneNumber(string value)
        {
            if (!Regex.IsMatch(value, @"^\+?[0-9]{7,15}$"))
                throw new ArgumentException("Invalid phone number");

            Value = value;
        }

        public override bool Equals(object? obj) =>
            obj is PhoneNumber other && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();
    }
}
