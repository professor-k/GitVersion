namespace GitVersion.Core.Tests.Helpers;

public static class PathHelper
{
    public static string GetCurrentDirectory() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public static string GetDotNetExecutable() => "dotnet";

    public static string GetExecutableArgs(string args)
    {
        args = $"{Path.Combine(GetExeDirectory(), "gitversion.dll")} {args}";
        return args;
    }

    public static string GetTempPath() => Path.Combine(GetCurrentDirectory(), "TestRepositories", Guid.NewGuid().ToString());

    private static string GetExeDirectory() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)?.Replace("GitVersion.App.Tests", "GitVersion.App");
}
