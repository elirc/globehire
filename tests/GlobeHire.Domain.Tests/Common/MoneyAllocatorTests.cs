using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.Tests.Common;

public sealed class MoneyAllocatorTests
{
    [Fact]
    public void Allocate_IndivisibleAmount_DistributesRemainderAndReconciles()
    {
        var allocations = MoneyAllocator.Allocate(Money.Of(100, "USD"), 3);

        Assert.Equal([33.34m, 33.33m, 33.33m], allocations.Select(item => item.Amount));
        Assert.Equal(100m, allocations.Sum(item => item.Amount));
        Assert.All(allocations, item => Assert.Equal("USD", item.Currency));
    }

    [Fact]
    public void Allocate_ManyParts_NeverLosesACent()
    {
        var allocations = MoneyAllocator.Allocate(Money.Of(10.01m, "GBP"), 12);

        Assert.Equal(10.01m, allocations.Sum(item => item.Amount));
        Assert.True(allocations.Max(item => item.Amount) - allocations.Min(item => item.Amount) <= 0.01m);
    }
}
