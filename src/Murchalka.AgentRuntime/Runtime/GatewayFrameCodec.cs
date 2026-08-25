using System.Text.Json;
using Murchalka.ModuleProtocol.Json;

namespace Murchalka.AgentRuntime.Runtime;

internal static class GatewayFrameCodec
{
    public static async Task WriteAsync<T>(Stream stream, string kind, T payload, CancellationToken cancellationToken) =>
        await LengthPrefixedJson.WriteAsync(
            stream,
            new GatewayFrame(kind, JsonSerializer.SerializeToElement(payload, ProtocolJson.Options)),
            cancellationToken: cancellationToken).ConfigureAwait(false);

    public static async Task<GatewayFrame> ReadAsync(Stream stream, CancellationToken cancellationToken) =>
        await LengthPrefixedJson.ReadAsync<GatewayFrame>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

    public static T PayloadAs<T>(GatewayFrame frame) =>
        frame.Payload.Deserialize<T>(ProtocolJson.Options)
        ?? throw new InvalidDataException($"Frame '{frame.Kind}' payload is invalid.");
}

