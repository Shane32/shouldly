public class EnumerableShouldBeUniqueExamples
{
    ITestOutputHelper _testOutputHelper;

    public EnumerableShouldBeUniqueExamples(ITestOutputHelper testOutputHelper) =>
        _testOutputHelper = testOutputHelper;

    [Fact]
    public void ShouldBeUnique()
    {
        DocExampleWriter.Document(
            () =>
            {
                var lisa = new Person { Name = "Lisa" };
                var bart = new Person { Name = "Bart" };
                var maggie = new Person { Name = "Maggie" };
                var simpsonsKids = new List<Person> { bart, lisa, maggie, maggie };

                simpsonsKids.ShouldBeUnique();
            },
            _testOutputHelper);
    }
}