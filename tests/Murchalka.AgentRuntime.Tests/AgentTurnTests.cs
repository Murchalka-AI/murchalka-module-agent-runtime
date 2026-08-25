using System.Text.Json;
using Murchalka.AgentRuntime.Runtime;
using Murchalka.ModuleProtocol.Contracts;
using Xunit;

namespace Murchalka.AgentRuntime.Tests;

/// <summary>Verifies the complete module-composed text turn.</summary>
public sealed class AgentTurnTests
{
    /// <summary>Verifies authorization, persistence, context, model, and audit ordering.</summary>
    [Fact]
    public async Task TurnIsAssembledOnlyThroughGrantedModules()
    {
        var dependencies = new RecordingDependencyClient();
        var service = new ModuleService();
        var context = new ModuleInvocationContext(
            "agent.turn",
            "dev.murchalka.client-realtime",
            "local:test",
            new InvocationScope(null, null, "person-test", null, null, null),
            "interactive-agent-turn",
            "correlation-test",
            DateTimeOffset.UtcNow.AddMinutes(1),
            "turn-test",
            JsonSerializer.SerializeToElement(new { }));

        var result = await service.HandleAsync(
            context,
            JsonSerializer.SerializeToElement(new
            {
                operation = "turn",
                conversationId = "conversation-test",
                text = "Hello"
            }),
            dependencies,
            TestContext.Current.CancellationToken);

        Assert.Equal("Hello back.", result.GetProperty("message").GetProperty("content").GetString());
        Assert.Equal(
            ["authorization", "conversations", "context", "model", "conversations", "audit"],
            dependencies.Calls);
    }
}
