using System.Text.Json;
using Murchalka.ModuleProtocol.Contracts;

namespace Murchalka.AgentRuntime.Runtime;

internal static class RequestReader
{
    public static string RequiredString(JsonElement value, string property, int maximumLength = 4096)
    {
        if (!value.TryGetProperty(property, out var item) ||
            item.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(item.GetString()))
        {
            throw Invalid($"Property '{property}' is required.");
        }

        var result = item.GetString()!;
        if (result.Length > maximumLength)
        {
            throw Invalid($"Property '{property}' exceeds {maximumLength} characters.");
        }

        return result;
    }

    public static string? OptionalString(JsonElement value, string property, int maximumLength = 4096)
    {
        if (!value.TryGetProperty(property, out var item) || item.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (item.ValueKind != JsonValueKind.String)
        {
            throw Invalid($"Property '{property}' must be a string.");
        }

        var result = item.GetString();
        if (result?.Length > maximumLength)
        {
            throw Invalid($"Property '{property}' exceeds {maximumLength} characters.");
        }

        return result;
    }

    public static ModuleOperationException Invalid(string message) =>
        new("request-invalid", ErrorCategory.InvalidRequest, message);
}

