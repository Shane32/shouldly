namespace Shouldly.Tests.ShouldContain;

public class PredicateObjectScenario
{
    [Fact]
    public void PredicateObjectScenarioShouldFail()
    {
        var a = new object();
        var b = new object();
        var c = new object();

        Verify.ShouldFail(() =>
                new[] { a, b, c }.ShouldContain(o => o.GetType().FullName!.Equals(""), "Some additional context"),

            errorWithSource:
            """
            new[] { a, b, c }
                should contain an element satisfying the condition
            o.GetType().FullName.Equals("")
                but does not

            Additional Info:
                Some additional context
            """,

            errorWithoutSource:
            """
            [System.Object (000000), System.Object (000000), System.Object (000000)]
                should contain an element satisfying the condition
            o.GetType().FullName.Equals("")
                but does not

            Additional Info:
                Some additional context
            """);
    }

    [Fact]
    public void ShouldPass()
    {
        var a = new object();
        var b = new object();
        var c = new object();
        new[] { a, b, c }.ShouldContain(o => o.GetType().FullName!.Equals("System.Object"));
    }
}