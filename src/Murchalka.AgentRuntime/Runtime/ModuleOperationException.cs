using Murchalka.ModuleProtocol.Contracts;

namespace Murchalka.AgentRuntime.Runtime;

/// <summary>Represents a safe structured module failure.</summary>
public sealed class ModuleOperationException : Exception
{
    /// <summary>Creates a structured module failure.</summary>
    /// <param name="code">The stable machine-readable error code.</param>
    /// <param name="category">The protocol error category.</param>
    /// <param name="message">The non-sensitive failure message.</param>
    /// <param name="retryable">Whether retrying may succeed.</param>
    public ModuleOperationException(string code, ErrorCategory category, string message, bool retryable = false)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
        Category = category;
        Retryable = retryable;
    }

    /// <summary>Gets the stable machine-readable error code.</summary>
    public string Code { get; }

    /// <summary>Gets the protocol error category.</summary>
    public ErrorCategory Category { get; }

    /// <summary>Gets whether retrying may succeed.</summary>
    public bool Retryable { get; }
}

