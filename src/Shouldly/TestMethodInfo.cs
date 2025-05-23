using Shouldly.Internals;

namespace Shouldly;

public class TestMethodInfo
{
    public TestMethodInfo(StackFrame callingFrame)
    {
        var fileName = callingFrame.GetFileName();
        fileName = DeterministicBuildHelpers.ResolveDeterministicPaths(fileName);
        SourceFileDirectory = Path.GetDirectoryName(fileName);

        var method = callingFrame.GetMethod();
        var originalMethodInfo = GetOriginalMethodInfoForStateMachineMethod(method);

        MethodName = originalMethodInfo?.MethodName ?? method?.Name;
        DeclaringTypeName = (originalMethodInfo?.DeclaringType ?? method?.DeclaringType)?.Name;
    }

    private readonly struct OriginalMethodInfo
    {
        public OriginalMethodInfo(string methodName, Type declaringType)
        {
            MethodName = methodName;
            DeclaringType = declaringType;
        }

        public string MethodName { get; }
        public Type DeclaringType { get; }
    }

    private static OriginalMethodInfo? GetOriginalMethodInfoForStateMachineMethod(MethodBase? method)
    {
        if (method?.DeclaringType is { IsByRef: false } declaringType
            && declaringType.DeclaringType is { } originalMethodDeclaringType
            && ContainsAttribute(declaringType, "System.Runtime.CompilerServices.CompilerGeneratedAttribute")
            && declaringType.GetInterface("System.Runtime.CompilerServices.IAsyncStateMachine") is object)
        {
            var stateMachineTypeName = declaringType.Name;
            var openingAngleBracket = stateMachineTypeName.IndexOf('<');
            if (openingAngleBracket != -1)
            {
                var closingAngleBracket = stateMachineTypeName.IndexOf('>', openingAngleBracket + 1);
                if (closingAngleBracket != -1)
                {
                    var originalMethodName = stateMachineTypeName.Substring(openingAngleBracket + 1, closingAngleBracket - (openingAngleBracket + 1));

                    return new OriginalMethodInfo(originalMethodName, originalMethodDeclaringType);
                }
            }
        }

        return null;
    }

    private static bool ContainsAttribute(MemberInfo member, string attributeName) =>
        member.CustomAttributes.Any(a =>
            a.AttributeType.FullName?.StartsWith(attributeName, StringComparison.Ordinal) ?? false);

    public string? SourceFileDirectory { get; }
    public string? MethodName { get; }
    public string? DeclaringTypeName { get; }
}