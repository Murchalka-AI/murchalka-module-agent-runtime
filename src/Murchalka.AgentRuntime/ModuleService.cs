using System.Text.Json;
using Murchalka.AgentRuntime.Runtime;
using Murchalka.ModuleProtocol.Contracts;

namespace Murchalka.AgentRuntime;

/// <summary>Executes one authorized text turn by composing conversation, context, model, and audit modules.</summary>
public sealed class ModuleService : IModuleService
{
    /// <summary>Creates the module-composed agent runtime service.</summary>
    public ModuleService()
    {
    }

    /// <inheritdoc />
    public async ValueTask<JsonElement> HandleAsync(
        ModuleInvocationContext context,
        JsonElement request,
        IModuleDependencyClient dependencies,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!string.Equals(RequestReader.RequiredString(request, "operation", 32), "turn", StringComparison.Ordinal))
        {
            throw RequestReader.Invalid("Unknown agent runtime operation.");
        }

        if (string.IsNullOrWhiteSpace(context.IdempotencyKey))
        {
            throw new ModuleOperationException("idempotency-key-required", ErrorCategory.InvalidRequest, "Agent turns require an idempotency key.");
        }

        var conversationId = RequestReader.RequiredString(request, "conversationId", 64);
        var input = RequestReader.RequiredString(request, "text", 32768);
        var subject = context.ActorReference;
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ModuleOperationException("authentication-required", ErrorCategory.PermissionDenied, "Agent turns require an authenticated actor.");
        }

        var authorization = await dependencies.InvokeAsync(
            "authorization",
            context,
            JsonSerializer.SerializeToElement(new
            {
                operation = "evaluate",
                subject,
                action = "agent.turn",
                resource = $"conversation:{conversationId}"
            }),
            "authorization.evaluate.request@1",
            null,
            cancellationToken).ConfigureAwait(false);
        var personId = authorization.GetProperty("personId").GetString();
        if (string.IsNullOrWhiteSpace(personId))
        {
            throw new ModuleOperationException("authorization-response-invalid", ErrorCategory.Internal, "Authorization response has no canonical person identifier.");
        }

        if (!authorization.GetProperty("allowed").GetBoolean())
        {
            await AuditAsync(context, dependencies, personId, conversationId, "denied", cancellationToken).ConfigureAwait(false);
            throw new ModuleOperationException("agent-turn-denied", ErrorCategory.PermissionDenied, "The actor is not authorized to execute an agent turn.");
        }

        var userMessageId = $"message-{Guid.NewGuid():N}";
        await dependencies.InvokeAsync(
            "conversations",
            context,
            JsonSerializer.SerializeToElement(new
            {
                operation = "append",
                conversationId,
                messageId = userMessageId,
                authorPersonId = personId,
                role = "user",
                content = input
            }),
            "conversations.history.request@1",
            $"{context.IdempotencyKey}:user",
            cancellationToken).ConfigureAwait(false);

        var assembled = await dependencies.InvokeAsync(
            "context",
            context,
            JsonSerializer.SerializeToElement(new
            {
                operation = "assemble",
                conversationId,
                characterId = RequestReader.OptionalString(request, "characterId", 64) ?? "default"
            }),
            "context.assemble.request@1",
            null,
            cancellationToken).ConfigureAwait(false);

        var model = await dependencies.InvokeAsync(
            "model",
            context,
            JsonSerializer.SerializeToElement(new
            {
                operation = "chat",
                system = assembled.GetProperty("system").GetString(),
                messages = assembled.GetProperty("messages")
            }),
            "model.chat.request@1",
            null,
            cancellationToken).ConfigureAwait(false);
        var assistantContent = model.GetProperty("message").GetProperty("content").GetString()
            ?? throw new ModuleOperationException("model-response-invalid", ErrorCategory.Internal, "Model response has no assistant content.");

        var assistantMessageId = $"message-{Guid.NewGuid():N}";
        await dependencies.InvokeAsync(
            "conversations",
            context,
            JsonSerializer.SerializeToElement(new
            {
                operation = "append",
                conversationId,
                messageId = assistantMessageId,
                authorPersonId = (string?)null,
                role = "assistant",
                content = assistantContent
            }),
            "conversations.history.request@1",
            $"{context.IdempotencyKey}:assistant",
            cancellationToken).ConfigureAwait(false);
        await AuditAsync(context, dependencies, personId, conversationId, "succeeded", cancellationToken).ConfigureAwait(false);

        return JsonSerializer.SerializeToElement(new
        {
            conversationId,
            userMessageId,
            assistantMessageId,
            message = new { role = "assistant", content = assistantContent },
            model = model.TryGetProperty("model", out var modelName) ? modelName.GetString() : null
        });
    }

    private static async ValueTask AuditAsync(
        ModuleInvocationContext context,
        IModuleDependencyClient dependencies,
        string personId,
        string conversationId,
        string outcome,
        CancellationToken cancellationToken)
    {
        await dependencies.InvokeAsync(
            "audit",
            context,
            JsonSerializer.SerializeToElement(new
            {
                operation = "append",
                kind = "agent.turn",
                actor = context.ActorReference,
                subject = $"conversation:{conversationId}",
                outcome,
                summary = $"Agent turn {outcome}.",
                dataClassification = "internal"
            }),
            "audit.store.request@1",
            $"{context.IdempotencyKey}:audit:{outcome}",
            cancellationToken).ConfigureAwait(false);
    }
}
