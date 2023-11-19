namespace ExternalSorting.Tests;

public class DirectOuterSortTests
{
    [Fact]
    public void Test1()
    {
        var sort = new DirectOuterSort(1);

        sort.Sort();
    }
}