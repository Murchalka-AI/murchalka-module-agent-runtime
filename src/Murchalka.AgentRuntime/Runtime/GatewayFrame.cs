using System.Text.Json;

namespace Murchalka.AgentRuntime.Runtime;

internal sealed record GatewayFrame(string Kind, JsonElement Payload);

