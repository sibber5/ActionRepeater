using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActionRepeater.Core.Utilities;

namespace ActionRepeater.Core.Tests;

public class UtilitiesTests
{
    [Theory]
    [InlineData(new int[] { 1, 2, 3, 4, 5 })]
    [InlineData(new int[] { 420 })]
    public void ObservableCollectionEx_AddRange_IncludesAllNewItemsInOrder(IEnumerable<int> range)
    {
        ObservableCollectionEx<int> collectionToTest = new() { 11, 12, 13 };
        ObservableCollectionEx<int> collectionWithRange = new() { 11, 12, 13 };
        collectionWithRange.SuppressNotifications = true;
        foreach (var item in range)
        {
            collectionWithRange.Add(item);
        }

        collectionToTest.AddRange(range);

        Assert.True(Enumerable.SequenceEqual(collectionToTest, collectionWithRange));
    }

    [Fact]
    public async Task TimeConsistencyChecker_UpdateAndCheckIfConsistent_ReturnsTrueIfConsistent()
    {
        const int maxTimeDelta = 1000;
        const int consistencyMargin = 150;
        const int waitTime = 50;
        PeriodicTimer timer = new(TimeSpan.FromMilliseconds(waitTime));
        TimeConsistencyChecker tcc = new(useLastTimeAsBaseline: false, maxTimeDelta: maxTimeDelta, consistencyMargin: consistencyMargin);

        for (int i = 0; i < 4 && await timer.WaitForNextTickAsync(); i++)
        {
            if (i % 2 != 0) await Task.Delay((consistencyMargin - waitTime) / 2);
            Assert.True(tcc.UpdateAndCheckIfConsistent());
        }
    }

    [Fact]
    public async Task TimeConsistencyChecker_UpdateAndCheckIfConsistent_ReturnsFalseIfInconsistent()
    {
        const int maxTimeDelta = 1000;
        const int consistencyMargin = 150;
        const int waitTime = 50;
        PeriodicTimer timer = new(TimeSpan.FromMilliseconds(waitTime));
        TimeConsistencyChecker tcc = new(useLastTimeAsBaseline: false, maxTimeDelta: maxTimeDelta, consistencyMargin: consistencyMargin);

        for (int i = 0; i < 3 && await timer.WaitForNextTickAsync(); i++)
        {
            if (i == 2)
            {
                await Task.Delay(consistencyMargin + waitTime);
                Assert.False(tcc.UpdateAndCheckIfConsistent());
            }
            else
            {
                tcc.UpdateAndCheckIfConsistent();
            }
        }
    }
}
