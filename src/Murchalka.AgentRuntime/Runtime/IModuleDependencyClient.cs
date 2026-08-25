using System.Text.Json;

namespace Murchalka.AgentRuntime.Runtime;

/// <summary>Invokes only capabilities present in the Runtime-issued dependency snapshot.</summary>
public interface IModuleDependencyClient
{
    /// <summary>Invokes a granted dependency by requirement identifier.</summary>
    /// <param name="requirementId">The manifest requirement identifier.</param>
    /// <param name="context">The current trusted invocation context.</param>
    /// <param name="payload">The dependency request payload.</param>
    /// <param name="payloadSchema">The request schema identifier.</param>
    /// <param name="idempotencyKey">The dependency idempotency key, when required.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The successful dependency response payload.</returns>
    ValueTask<JsonElement> InvokeAsync(
        string requirementId,
        ModuleInvocationContext context,
        JsonElement payload,
        string payloadSchema,
        string? idempotencyKey,
        CancellationToken cancellationToken);
}

