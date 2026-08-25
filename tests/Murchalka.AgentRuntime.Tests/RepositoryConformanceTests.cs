using Murchalka.ModuleSdk.Testing;
using Xunit;

namespace Murchalka.AgentRuntime.Tests;

/// <summary>Verifies the canonical repository, manifest, and capability contract.</summary>
public sealed class RepositoryConformanceTests
{
    /// <summary>Verifies schema, dependency, permission, and reference conformance.</summary>
    [Fact]
    public void RepositorySatisfiesModuleSdkConformance()
    {
        var report = new ModuleRepositoryConformance().Validate(RepositoryRootLocator.Find());

        Assert.True(
            report.Passed,
            string.Join(
                Environment.NewLine,
                report.Findings.Select(finding =>
                    $"{finding.Severity}: {finding.Check}/{finding.Code} at {finding.Location}: {finding.Message}")));
    }
}

