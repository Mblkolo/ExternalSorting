namespace ExternalSorting.Tests;

public class OuterSortingTests
{
    private readonly string SampleFilePath = Path.Combine("Samples", "A.csv");
    private readonly string SortingFilePath = "A.csv";
    private static string ExpectedFilePath(string fileName) => Path.Combine("Samples", fileName);


    [Theory]
    [InlineData(0, "Sort0.csv")]
    [InlineData(1, "Sort1.csv")]
    [InlineData(2, "Sort2.csv")]
    public void DirectOuterSortTest(int fieldNo, string expectedFileName)
    {
        File.Copy(SampleFilePath, SortingFilePath, true);
        var sort = new DirectOuterSort(fieldNo);

        //Act
        sort.Sort();

        //Assert
        var sortedLines = File.ReadAllLines(SortingFilePath);
        var expectedLines = File.ReadAllLines(ExpectedFilePath(expectedFileName));

        Assert.Equal(expectedLines, sortedLines);
    }

    [Theory]
    [InlineData(0, "Sort0.csv")]
    [InlineData(1, "Sort1.csv")]
    [InlineData(2, "Sort2.csv")]
    public void NaturalOuterSortTest(int fieldNo, string expectedFileName)
    {
        File.Copy(SampleFilePath, SortingFilePath, true);
        var sort = new DirectOuterSort(fieldNo);

        //Act
        sort.Sort();

        //Assert
        var sortedLines = File.ReadAllLines(SortingFilePath);
        var expectedLines = File.ReadAllLines(ExpectedFilePath(expectedFileName));

        Assert.Equal(expectedLines, sortedLines);
    }

    [Theory]
    [InlineData(0, "Sort0.csv")]
    [InlineData(1, "Sort1.csv")]
    [InlineData(2, "Sort2.csv")]
    public void MultiThreadOuterSortTest(int fieldNo, string expectedFileName)
    {
        File.Copy(SampleFilePath, SortingFilePath, true);
        var sort = new MultiThreadOuterSort(fieldNo);

        //Act
        sort.Sort();

        //Assert
        var sortedLines = File.ReadAllLines(SortingFilePath);
        var expectedLines = File.ReadAllLines(ExpectedFilePath(expectedFileName));

        Assert.Equal(expectedLines, sortedLines);
    }
}