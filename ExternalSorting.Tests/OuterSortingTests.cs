namespace ExternalSorting.Tests;

public class OuterSortingTests
{
    [Fact]
    public void DirectOuterSortTest()
    {
        var sort = new DirectOuterSort(1);

        sort.Sort();
    }
    [Fact]
    public void NaturalOuterSortTest()
    {
        var sort = new DirectOuterSort(1);

        sort.Sort();
    }

    [Fact]
    public void MultiThreadOuterSortTest()
    {
        var sort = new MultiThreadOuterSort(1);

        sort.Sort();
    }
}