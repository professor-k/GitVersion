using GitVersion.BuildAgents;
using GitVersion.Core.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace GitVersion.Core.Tests.BuildAgents;

[TestFixture]
public class AzurePipelinesTests : TestBase
{
    private const string key = "BUILD_BUILDNUMBER";
    private const string logPrefix = "##vso[build.updatebuildnumber]";

    private IEnvironment environment;
    private AzurePipelines buildServer;

    [SetUp]
    public void SetEnvironmentVariableForTest()
    {
        var sp = ConfigureServices(services => services.AddSingleton<AzurePipelines>());
        this.environment = sp.GetRequiredService<IEnvironment>();
        this.buildServer = sp.GetRequiredService<AzurePipelines>();

        this.environment.SetEnvironmentVariable(key, "Some Build_Value $(GitVersion_FullSemVer) 20151310.3 $(UnknownVar) Release");
    }

    [TearDown]
    public void ClearEnvironmentVariableForTest() => this.environment.SetEnvironmentVariable(key, null);

    [Test]
    public void DevelopBranch()
    {
        var vars = new TestableVersionVariables(fullSemVer: "0.0.0-Unstable4");
        var vsVersion = this.buildServer.GenerateSetVersionMessage(vars);

        vsVersion.ShouldBe("##vso[build.updatebuildnumber]Some Build_Value 0.0.0-Unstable4 20151310.3 $(UnknownVar) Release");
    }

    [Test]
    public void EscapeValues()
    {
        var vsVersion = this.buildServer.GenerateSetParameterMessage("Foo", "0.8.0-unstable568 Branch:'develop' Sha:'ee69bff1087ebc95c6b43aa2124bd58f5722e0cb'");

        vsVersion.ShouldContain("##vso[task.setvariable variable=GitVersion.Foo]0.8.0-unstable568 Branch:'develop' Sha:'ee69bff1087ebc95c6b43aa2124bd58f5722e0cb'");
        vsVersion.ShouldContain("##vso[task.setvariable variable=GitVersion.Foo;isOutput=true]0.8.0-unstable568 Branch:'develop' Sha:'ee69bff1087ebc95c6b43aa2124bd58f5722e0cb'");
    }

    [Test]
    public void MissingEnvShouldNotBlowUp()
    {
        this.environment.SetEnvironmentVariable(key, null);

        const string semver = "0.0.0-Unstable4";
        var vars = new TestableVersionVariables(fullSemVer: semver);
        var vsVersion = this.buildServer.GenerateSetVersionMessage(vars);
        vsVersion.ShouldBe(semver);
    }

    [TestCase("$(GitVersion.FullSemVer)", "1.0.0", "1.0.0")]
    [TestCase("$(GITVERSION_FULLSEMVER)", "1.0.0", "1.0.0")]
    [TestCase("$(GitVersion.FullSemVer)-Build.1234", "1.0.0", "1.0.0-Build.1234")]
    [TestCase("$(GITVERSION_FULLSEMVER)-Build.1234", "1.0.0", "1.0.0-Build.1234")]
    public void AzurePipelinesBuildNumberWithFullSemVer(string buildNumberFormat, string myFullSemVer, string expectedBuildNumber)
    {
        this.environment.SetEnvironmentVariable(key, buildNumberFormat);
        var vars = new TestableVersionVariables(fullSemVer: myFullSemVer);
        var logMessage = this.buildServer.GenerateSetVersionMessage(vars);
        logMessage.ShouldBe(logPrefix + expectedBuildNumber);
    }

    [TestCase("$(GitVersion.SemVer)", "1.0.0", "1.0.0")]
    [TestCase("$(GITVERSION_SEMVER)", "1.0.0", "1.0.0")]
    [TestCase("$(GitVersion.SemVer)-Build.1234", "1.0.0", "1.0.0-Build.1234")]
    [TestCase("$(GITVERSION_SEMVER)-Build.1234", "1.0.0", "1.0.0-Build.1234")]
    public void AzurePipelinesBuildNumberWithSemVer(string buildNumberFormat, string mySemVer, string expectedBuildNumber)
    {
        this.environment.SetEnvironmentVariable(key, buildNumberFormat);
        var vars = new TestableVersionVariables(semVer: mySemVer);
        var logMessage = this.buildServer.GenerateSetVersionMessage(vars);
        logMessage.ShouldBe(logPrefix + expectedBuildNumber);
    }
}
