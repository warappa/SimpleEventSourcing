﻿using System;

namespace Shop.Core.Domain.Shared
{
    public class Money
    {
        public string IsoCode { get; private set; }
        public decimal Value { get; private set; }

        public Money(decimal value, string isoCode)
        {
            Value = value;
            IsoCode = isoCode;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Money;
            if (other == null)
            {
                return false;
            }

            return Value == other.Value &&
                IsoCode == other.IsoCode;
        }

        public static Money EUR(decimal value)
        {
            return new Money(value, "EUR");
        }

        public static Money operator *(Money moneyA, decimal factor)
        {
            return new Money(moneyA.Value * factor, moneyA.IsoCode);
        }

        public static Money operator /(Money moneyA, decimal factor)
        {
            return new Money(moneyA.Value / factor, moneyA.IsoCode);
        }

        public static Money operator +(Money moneyA, decimal value)
        {
            return new Money(moneyA.Value + value, moneyA.IsoCode);
        }

        public static Money operator -(Money moneyA, decimal value)
        {
            return moneyA + (0 - value);
        }

        public static Money operator +(Money moneyA, Money moneyB)
        {
            if (moneyA == null ||
                moneyB == null)
            {
                throw new InvalidOperationException($"Cannot add nulls!");
            }

            if (moneyA.IsoCode != moneyB.IsoCode)
            {
                throw new InvalidOperationException($"Cannot add {moneyA.IsoCode} to {moneyB.IsoCode}!");
            }

            return new Money(moneyA.Value + moneyB.Value, moneyA.IsoCode);
        }

        public static Money operator -(Money moneyA, Money moneyB)
        {
            if (moneyA == null ||
                moneyB == null)
            {
                throw new InvalidOperationException($"Cannot add nulls!");
            }

            if (moneyA.IsoCode != moneyB.IsoCode)
            {
                throw new InvalidOperationException($"Cannot add {moneyA.IsoCode} to {moneyB.IsoCode}!");
            }

            return new Money(moneyA.Value - moneyB.Value, moneyA.IsoCode);
        }
    }
}
