using System.Text.Json;
using Murchalka.ModuleProtocol.Contracts;

namespace Murchalka.AgentRuntime.Runtime;

internal sealed class ModuleDependencyClient : IModuleDependencyClient
{
    private readonly Stream _stream;
    private readonly ModuleId _moduleId;
    private DependencyEndpointsSnapshot _snapshot;

    public ModuleDependencyClient(Stream stream, ModuleId moduleId, DependencyEndpointsSnapshot snapshot)
    {
        _stream = stream;
        _moduleId = moduleId;
        _snapshot = snapshot;
    }

    public void Update(DependencyEndpointsSnapshot snapshot) =>
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));

    public async ValueTask<JsonElement> InvokeAsync(
        string requirementId,
        ModuleInvocationContext context,
        JsonElement payload,
        string payloadSchema,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var endpoint = _snapshot.Endpoints.SingleOrDefault(
            value => string.Equals(value.RequirementId, requirementId, StringComparison.Ordinal))
            ?? throw new ModuleOperationException(
                "dependency-not-granted",
                ErrorCategory.Unavailable,
                $"Required dependency '{requirementId}' is not granted.",
                retryable: true);

        var invocation = new InvocationEnvelope(
            Guid.NewGuid(),
            endpoint.Capability,
            endpoint.CapabilityVersion,
            endpoint.ProviderInstance,
            _moduleId,
            context.ActorReference,
            context.Scope,
            context.Purpose,
            endpoint.AuthorizationReference,
            context.CorrelationId,
            context.CorrelationId,
            null,
            context.Deadline,
            idempotencyKey,
            payloadSchema,
            payload,
            null);

        await GatewayFrameCodec.WriteAsync(_stream, "capabilityInvocation", invocation, cancellationToken).ConfigureAwait(false);
        var frame = await GatewayFrameCodec.ReadAsync(_stream, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(frame.Kind, "capabilityResult", StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Expected 'capabilityResult', received '{frame.Kind}'.");
        }

        var result = GatewayFrameCodec.PayloadAs<ResultEnvelope>(frame);
        if (result.InvocationId != invocation.InvocationId)
        {
            throw new InvalidDataException("Dependency result invocation id does not match.");
        }

        if (result.Status == InvocationStatus.Succeeded && result.Payload is { } response)
        {
            return response;
        }

        var error = result.Error;
        throw new ModuleOperationException(
            error?.Code ?? "dependency-failed",
            error?.Category ?? ErrorCategory.Unavailable,
            error?.Message ?? "A granted dependency invocation failed.",
            error?.Retryable ?? true);
    }
}

