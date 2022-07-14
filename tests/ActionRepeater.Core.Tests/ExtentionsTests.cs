using System.Collections.Generic;
using ActionRepeater.Core.Extentions;

namespace ActionRepeater.Core.Tests;

public class ExtentionsTests
{
    [Theory]
    [InlineData("KeyPress", "Key Press")]
    [InlineData("ThisIsASentence", "This Is A Sentence")]
    [InlineData("Thing_1ThatDoesStuff2", "Thing_1 That Does Stuff2")]
    public void AddSpacesBetweenWords(string input, string expectedResult)
    {
        string result = input.AddSpacesBetweenWords();

        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(10, 10, 20, 15, 10, 10, true)]
    [InlineData(10, 10, 20, 15, 30, 25, true)]
    [InlineData(10, 10, 20, 15, 12, 10, true)]
    [InlineData(10, 10, 20, 15, 12, 25, true)]
    [InlineData(10, 10, 20, 15, 10, 20, true)]
    [InlineData(10, 10, 20, 15, 30, 20, true)]
    [InlineData(10, 10, 20, 15, 15, 20, true)]
    [InlineData(10, 10, 20, 15, 9, 9, false)]
    [InlineData(10, 10, 20, 15, 31, 26, false)]
    [InlineData(10, 10, 20, 15, -8, 7, false)]
    [InlineData(10, 10, 20, 15, 5, -7, false)]
    public void ContainsInclusive(int recX, int recY, int recWidth, int recHeight, int x, int y, bool expectedResult)
    {
        System.Drawing.Rectangle rect = new(recX, recY, recWidth, recHeight);

        bool result = rect.ContainsInclusive(x, y);

        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(new int[0], true)]
    [InlineData(new int[] { 69, 420 }, false)]
    public void IsNullOrEmpty(IEnumerable<int> enumerable, bool expectedResult)
    {
        bool result = enumerable.IsNullOrEmpty();

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void RefIndexOfReverse_ReturnsCorrectIndex()
    {
        TestRecord itemNotInList = new();
        TestRecord itemToFind = new();
        List<TestRecord> list = new() { new TestRecord(), itemToFind, new TestRecord() };
        const int indexOfItemToFind = 1;

        int index = list.RefIndexOfReverse(itemToFind);
        int indexOfNonExistantItem = list.RefIndexOfReverse(itemNotInList);

        Assert.Equal(indexOfItemToFind, index);
        Assert.Equal(-1, indexOfNonExistantItem);
    }

    private record TestRecord(int N = 0);
}
