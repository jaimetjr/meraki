using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "BRL")
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative");
            Amount = amount;
            Currency = currency;
        }

        public override bool Equals(object? obj) =>
            obj is Money other && Amount == other.Amount && Currency == other.Currency;

        public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    }
}
