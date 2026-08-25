using System.Text.Json;
using Murchalka.AgentRuntime.Runtime;
using Xunit;

namespace Murchalka.AgentRuntime.Tests;

/// <summary>Verifies fail-closed and cancellation behavior at the module boundary.</summary>
public sealed class ModuleServiceTests
{
    /// <summary>Verifies that a pre-cancelled operation performs no work.</summary>
    [Fact]
    public async Task PreCancelledInvocationIsRejected()
    {
        var service = new ModuleService();
        var context = CreateContext();
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await service.HandleAsync(
                context,
                JsonSerializer.SerializeToElement(new { operation = "unknown" }),
                new RejectingDependencyClient(),
                source.Token));
    }

    /// <summary>Verifies that unknown operations fail closed with a stable error.</summary>
    [Fact]
    public async Task UnknownOperationFailsClosed()
    {
        var service = new ModuleService();

        var exception = await Assert.ThrowsAsync<ModuleOperationException>(async () =>
            await service.HandleAsync(
                CreateContext(),
                JsonSerializer.SerializeToElement(new { operation = "unknown" }),
                new RejectingDependencyClient(),
                TestContext.Current.CancellationToken));

        Assert.Equal("request-invalid", exception.Code);
    }

    private static ModuleInvocationContext CreateContext() =>
        new(
            "agent.turn",
            "dev.murchalka.tests",
            "test:actor",
            new Murchalka.ModuleProtocol.Contracts.InvocationScope("test", null, "person-test", null, "session-test", null),
            "test",
            "test-correlation",
            DateTimeOffset.UtcNow.AddMinutes(1),
            "test-idempotency",
            JsonSerializer.SerializeToElement(new { }));

}
