using System.Text.Json;
using Murchalka.AgentRuntime.Runtime;

namespace Murchalka.AgentRuntime.Tests;

internal sealed class RejectingDependencyClient : IModuleDependencyClient
{
    public ValueTask<JsonElement> InvokeAsync(
        string requirementId,
        ModuleInvocationContext context,
        JsonElement payload,
        string payloadSchema,
        string? idempotencyKey,
        CancellationToken cancellationToken) =>
        throw new InvalidOperationException("No dependency invocation was expected.");
}

