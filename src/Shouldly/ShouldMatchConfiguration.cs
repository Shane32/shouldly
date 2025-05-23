namespace Shouldly;

public class ShouldMatchConfiguration
{
    public static ShouldMatchConfigurationBuilder ShouldMatchApprovedDefaults { get; } =
        new(new()
        {
            StringCompareOptions = StringCompareShould.IgnoreLineEndings,
            TestMethodFinder = new FirstNonShouldlyMethodFinder(),
            FileExtension = "txt",
            FilenameGenerator = (testMethodInfo, discriminator, type, extension)
                => $"{testMethodInfo.DeclaringTypeName}.{testMethodInfo.MethodName}{discriminator}.{type}.{extension}"
        });

    public ShouldMatchConfiguration()
    {
    }

    public ShouldMatchConfiguration(ShouldMatchConfiguration initialConfig)
    {
        StringCompareOptions = initialConfig.StringCompareOptions;
        FilenameDiscriminator = initialConfig.FilenameDiscriminator;
        PreventDiff = initialConfig.PreventDiff;
        DiffViewer = initialConfig.DiffViewer;
        FileExtension = initialConfig.FileExtension;
        TestMethodFinder = initialConfig.TestMethodFinder;
        ApprovalFileSubFolder = initialConfig.ApprovalFileSubFolder;
        Scrubber = initialConfig.Scrubber;
        FilenameGenerator = initialConfig.FilenameGenerator;
    }

    public StringCompareShould StringCompareOptions { get; set; } = StringCompareShould.IgnoreLineEndings;
    public string? FilenameDiscriminator { get; set; }
    public bool PreventDiff { get; set; } = false;
    public IDiffViewer? DiffViewer { get; set; }

    /// <summary>
    /// File extension without the.
    /// </summary>
    public string FileExtension { get; set; } = "txt";

    public ITestMethodFinder TestMethodFinder { get; set; } = new FirstNonShouldlyMethodFinder();
    public string? ApprovalFileSubFolder { get; set; }

    /// <summary>
    /// Scrubbers allow you to alter the received document before comparing it to approved.
    ///
    /// This is useful for replacing dates or dynamic data with fixed data
    /// </summary>
    public Func<string, string>? Scrubber { get; set; }

    public FilenameGenerator FilenameGenerator { get; set; } =
        (testMethodInfo, discriminator, type, extension)
            => $"{testMethodInfo.DeclaringTypeName}.{testMethodInfo.MethodName}{discriminator}.{type}.{extension}";
}