using System.Text.Json;
using Murchalka.AgentRuntime.Runtime;

namespace Murchalka.AgentRuntime.Tests;

internal sealed class RecordingDependencyClient : IModuleDependencyClient
{
    public List<string> Calls { get; } = [];

    public ValueTask<JsonElement> InvokeAsync(
        string requirementId,
        ModuleInvocationContext context,
        JsonElement payload,
        string payloadSchema,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls.Add(requirementId);
        return ValueTask.FromResult(requirementId switch
        {
            "authorization" => JsonSerializer.SerializeToElement(new { allowed = true, personId = "person-test" }),
            "sessions" => JsonSerializer.SerializeToElement(new
            {
                session = new
                {
                    sessionId = "session-test",
                    conversationId = "conversation-test",
                    personId = "person-test",
                    state = "open"
                },
                version = 1
            }),
            "conversations" => JsonSerializer.SerializeToElement(new { accepted = true }),
            "context" => JsonSerializer.SerializeToElement(new
            {
                system = "You are Murchalka.",
                messages = new[] { new { role = "user", content = "Hello" } }
            }),
            "model" => JsonSerializer.SerializeToElement(new
            {
                message = new { role = "assistant", content = "Hello back." },
                model = "test-model"
            }),
            "audit" => JsonSerializer.SerializeToElement(new { accepted = true }),
            "observability" => JsonSerializer.SerializeToElement(new { recorded = true }),
            _ => throw new InvalidOperationException($"Unexpected dependency '{requirementId}'.")
        });
    }
}
