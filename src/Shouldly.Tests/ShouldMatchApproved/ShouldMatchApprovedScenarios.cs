using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DiffEngine;

namespace Shouldly.Tests.ShouldMatchApproved;

public class ShouldMatchApprovedScenarios
{
    private readonly Func<string, string> _scrubber = v => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? Regex.Replace(v, @"\w:.+?shouldly\\src", "C:\\PathToCode\\shouldly\\src")
        : Regex.Replace(v, @"\/([U,u]sers|[H,h]ome).+?shouldly\/src", "/PathToCode/shouldly/src");

    [Fact]
    public void Simple()
    {
        "Bar".ShouldMatchApproved();
    }

    [Fact]
    public void MissingApprovedFile()
    {
        var approvalPath = IsWindows()
            ? @"C:\PathToCode\shouldly\src\Shouldly.Tests\ShouldMatchApproved\ShouldMatchApprovedScenarios.MissingApprovedFile"
            : "/PathToCode/shouldly/src/Shouldly.Tests/ShouldMatchApproved/ShouldMatchApprovedScenarios.MissingApprovedFile";

        var cmd = IsWindows()
            ? $"""
               copy /Y "{approvalPath}.received.txt" "{approvalPath}.approved.txt"
               """
            : $"""
               cp "{approvalPath}.received.txt" "{approvalPath}.approved.txt"
               """;

        var errorMsg = $"""
                        To approve the changes run this command:
                        {cmd}
                        ----------------------------

                        Approval file {approvalPath}.approved.txt
                            does not exist
                        """;

        Verify.ShouldFail(() =>
                "Bar".ShouldMatchApproved(c => c.NoDiff()),

            errorWithSource: errorMsg,
            errorWithoutSource: errorMsg,
            messageScrubber: _scrubber);
    }

    [Fact]
    public void BigStringShouldTrim()
    {
        var str = $"Foo{new string('*', 10000)}";

        Exception? exception = null;
        try
        {
            str.ShouldMatchApproved(c =>
            {
                c.NoDiff();
            });
        }
        catch (Exception e)
        {
            exception = e;
        }

        exception.ShouldNotBeNull();
        // text is limited to 5000 char. but then the diff results in 5000*2+some extraneous text
        exception!.Message.Length.ShouldBeLessThan(12000);
    }

    [Fact]
    public void DifferencesUseShouldlyMessages()
    {
        var cmd = IsWindows()
            ? """
              copy /Y "C:\PathToCode\shouldly\src\Shouldly.Tests\ShouldMatchApproved\ShouldMatchApprovedScenarios.DifferencesUseShouldlyMessages.received.txt" "C:\PathToCode\shouldly\src\Shouldly.Tests\ShouldMatchApproved\ShouldMatchApprovedScenarios.DifferencesUseShouldlyMessages.approved.txt"
              """
            : """
              cp "/PathToCode/shouldly/src/Shouldly.Tests/ShouldMatchApproved/ShouldMatchApprovedScenarios.DifferencesUseShouldlyMessages.received.txt" "/PathToCode/shouldly/src/Shouldly.Tests/ShouldMatchApproved/ShouldMatchApprovedScenarios.DifferencesUseShouldlyMessages.approved.txt"
              """;

        var str = "Foo";
        Verify.ShouldFail(() =>
                str.ShouldMatchApproved(c => c.NoDiff()),

            errorWithSource:
            $"""
             To approve the changes run this command:
             {cmd}
             ----------------------------

             str
                 should match approved with options: Ignoring line endings
             "Bar"
                 but was
             "Foo"
                 difference
             Difference     |  |    |    |   
                            | \|/  \|/  \|/  
             Index          | 0    1    2    
             Expected Value | B    a    r    
             Actual Value   | F    o    o    
             Expected Code  | 66   97   114  
             Actual Code    | 70   111  111  
             """,

            errorWithoutSource:
            $"""
             To approve the changes run this command:
             {cmd}
             ----------------------------

             "Foo"
                 should match approved with options: Ignoring line endings
             "Bar"
                 but was not
                 difference
             Difference     |  |    |    |   
                            | \|/  \|/  \|/  
             Index          | 0    1    2    
             Expected Value | B    a    r    
             Actual Value   | F    o    o    
             Expected Code  | 66   97   114  
             Actual Code    | 70   111  111  
             """,

            messageScrubber:
            _scrubber);
    }

    [Fact]
    public void IgnoresLineEndingsByDefault()
    {
        var stacktrace = new StackTrace(true);
        var sourceFileDir = Path.GetDirectoryName(stacktrace.GetFrame(0)!.GetFileName())!;
        var approved = Path.Combine(sourceFileDir, "ShouldMatchApprovedScenarios.IgnoresLineEndingsByDefault.approved.txt");
        File.WriteAllText(approved, "Different\nStyle\nLine\nBreaks");

        try
        {
            "Different\r\nStyle\r\nLine\r\nBreaks".ShouldMatchApproved();
        }
        finally
        {
            File.Delete(approved);
        }
    }

    [Fact]
    public void ChangingInstanceConfigDoesntChangeGlobal()
    {
        Should.Throw<ShouldMatchApprovedException>(() => "".ShouldMatchApproved(c => c.NoDiff()));

        ShouldMatchConfiguration.ShouldMatchApprovedDefaults.Build().PreventDiff.ShouldBe(false);
    }

    [Fact]
    public void CanFindTestAttribute()
    {
        FirstInCallStackToAssert();
    }

    private static void FirstInCallStackToAssert()
    {
        AnotherInCallStack();
    }

    private static void AnotherInCallStack()
    {
        "testAttributes".ShouldMatchApproved(b => b.LocateTestMethodUsingAttribute<FactAttribute>());
    }

    [Fact]
    public async Task CanFindTestAttributeInAsync()
    {
        await Task.Yield();

        "testAttributes".ShouldMatchApproved(b => b.LocateTestMethodUsingAttribute<FactAttribute>());
    }

    [Fact]
    public async Task HandlesAsync()
    {
        await Task.Yield();

        "Foo".ShouldMatchApproved();
    }

    public static bool IsWindows()
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void DiffConfiguresDiffViewer()
    {
        var defaultConfig = ShouldMatchConfiguration.ShouldMatchApprovedDefaults.Build();
        var builder = new ShouldMatchConfigurationBuilder(defaultConfig);
        builder.Diff();
        var newConfig = builder.Build();

        newConfig.ShouldNotBe(defaultConfig);
        defaultConfig.DiffViewer.ShouldBeNull();
        newConfig.DiffViewer.ShouldBeOfType<DiffEngineDiffViewer>();
    }

    [Fact]
    public void DiffViewerIsCarriedThroughBuilder()
    {
        var defaultBuilder = ShouldMatchConfiguration.ShouldMatchApprovedDefaults;
        var defaultConfig = defaultBuilder.Build();
        var builder = new ShouldMatchConfigurationBuilder(defaultConfig);
        builder.Diff();
        var configWithDiff = builder.Build();
        var newBuilder = new ShouldMatchConfigurationBuilder(configWithDiff);
        var newConfig = newBuilder.Build();

        defaultConfig.DiffViewer.ShouldBeNull();
        configWithDiff.DiffViewer.ShouldBeOfType<DiffEngineDiffViewer>();
        newConfig.DiffViewer.ShouldBeOfType<DiffEngineDiffViewer>();
    }
}