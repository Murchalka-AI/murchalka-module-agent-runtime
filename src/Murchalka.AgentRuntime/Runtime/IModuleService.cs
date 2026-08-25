using System.Text.Json;

namespace Murchalka.AgentRuntime.Runtime;

/// <summary>Defines the transport-independent behavior exposed by this module.</summary>
public interface IModuleService
{
    /// <summary>Handles one schema-validated capability request.</summary>
    /// <param name="context">The trusted invocation context.</param>
    /// <param name="request">The request payload.</param>
    /// <param name="dependencies">The granted dependency client.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The response payload.</returns>
    ValueTask<JsonElement> HandleAsync(
        ModuleInvocationContext context,
        JsonElement request,
        IModuleDependencyClient dependencies,
        CancellationToken cancellationToken);
}

