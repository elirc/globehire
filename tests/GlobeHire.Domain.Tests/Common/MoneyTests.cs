using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.Tests.Common;

public sealed class MoneyTests
{
    [Fact]
    public void Of_RoundsMidpointsAwayFromZero()
    {
        var money = Money.Of(10.125m, "usd");

        Assert.Equal(10.13m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Add_DifferentCurrencies_RejectsAmbiguousArithmetic()
    {
        var usd = Money.Of(10, "USD");
        var eur = Money.Of(10, "EUR");

        var exception = Assert.Throws<BusinessRuleViolationException>(() => usd.Add(eur));

        Assert.Equal("money.currency_mismatch", exception.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("US1")]
    public void Of_InvalidCurrency_UsesStableBusinessError(string currency)
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(() => Money.Of(10, currency));

        Assert.Equal("money.currency_invalid", exception.Code);
    }
}
