using MathVerse.Core;

namespace MathVerse.Kernel.Tests;

public class ValueObjectTests
{
    private sealed class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    private sealed class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }

        public Address(string street, string city)
        {
            Street = street;
            City = city;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
        }
    }

    [Fact]
    public void ValueObject_Equal_SameComponents()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        money1.Should().Be(money2);
    }

    [Fact]
    public void ValueObject_NotEqual_DifferentComponents()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(200m, "USD");

        money1.Should().NotBe(money2);
    }

    [Fact]
    public void ValueObject_EqualityOperators()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");
        Money? nullMoney = null;

        (money1 == money2).Should().BeTrue();
        (money1 != money2).Should().BeFalse();
        (money1 == nullMoney).Should().BeFalse();
        (nullMoney != money1).Should().BeTrue();
    }

    [Fact]
    public void ValueObject_DifferentTypes_NotEqual()
    {
        var money = new Money(100m, "USD");
        var address = new Address("123 Main St", "Springfield");

        money.Should().NotBe(address);
    }

    [Fact]
    public void ValueObject_GetHashCode_SameComponents_SameHash()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }

    [Fact]
    public void ValueObject_IsImmutable()
    {
        var money = new Money(100m, "USD");

        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void ValueObject_NullComponent_HandledInHash()
    {
        var address1 = new Address("123 Main St", null!);
        var address2 = new Address("123 Main St", null!);

        address1.Should().Be(address2);
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }
}
