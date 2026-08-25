using System.Text.Json;
using Murchalka.ModuleProtocol.Contracts;

namespace Murchalka.AgentRuntime.Runtime;

/// <summary>Contains the trusted metadata supplied with one module invocation.</summary>
/// <param name="Capability">The invoked capability identifier.</param>
/// <param name="Consumer">The authenticated consumer module identifier.</param>
/// <param name="ActorReference">The authenticated actor reference, when present.</param>
/// <param name="Scope">The invocation scope.</param>
/// <param name="Purpose">The declared invocation purpose.</param>
/// <param name="CorrelationId">The distributed correlation identifier.</param>
/// <param name="Deadline">The invocation deadline.</param>
/// <param name="IdempotencyKey">The idempotency key, when supplied.</param>
/// <param name="Configuration">The validated module configuration.</param>
public sealed record ModuleInvocationContext(
    string Capability,
    string Consumer,
    string? ActorReference,
    InvocationScope Scope,
    string Purpose,
    string CorrelationId,
    DateTimeOffset Deadline,
    string? IdempotencyKey,
    JsonElement Configuration);

