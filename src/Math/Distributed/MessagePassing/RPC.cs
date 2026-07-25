namespace MathVerse.Math.Distributed.MessagePassing;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides a remote procedure call (RPC) mechanism over the <see cref="MessageBus"/>.
/// Sends a request message to a target node and awaits an acknowledgment with the result.
/// </summary>
public sealed class RPC
{
    private readonly MessageBus _bus;
    private readonly Serializer _serializer;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>> _pendingCalls = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RPC"/> class.
    /// </summary>
    /// <param name="bus">The message bus used for communication.</param>
    /// <param name="serializer">The serializer used for argument/result encoding.</param>
    public RPC(MessageBus bus, Serializer serializer)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Asynchronously calls a remote procedure on the specified node and waits for the result.
    /// </summary>
    /// <param name="nodeId">The target node ID.</param>
    /// <param name="procedure">The name of the remote procedure to invoke.</param>
    /// <param name="args">Serialized arguments for the procedure.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The serialized result from the remote procedure.</returns>
    /// <exception cref="TimeoutException">Thrown when the call times out before a response is received.</exception>
    public async ValueTask<byte[]> CallAsync(string nodeId, string procedure, byte[] args, CancellationToken ct)
    {
        var callId = Guid.NewGuid();
        var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingCalls[callId] = tcs;

        ct.Register(() =>
        {
            if (_pendingCalls.TryRemove(callId, out var pending))
                pending.TrySetCanceled(ct);
        });

        var rpcPayload = new RpcRequest
        {
            CallId = callId,
            Procedure = procedure,
            Args = args
        };

        var payloadBytes = _serializer.Serialize(rpcPayload);

        var message = new Message(
            callId,
            MessageType.Control,
            "caller",
            nodeId,
            payloadBytes,
            DateTime.UtcNow,
            10);

        await _bus.Publish(message).ConfigureAwait(false);

        try
        {
            var result = await tcs.Task.ConfigureAwait(false);
            return result;
        }
        finally
        {
            _pendingCalls.TryRemove(callId, out _);
        }
    }

    /// <summary>
    /// Completes a pending RPC call with the given result.
    /// </summary>
    /// <param name="callId">The ID of the RPC call to complete.</param>
    /// <param name="result">The serialized result to return.</param>
    /// <returns>True if the pending call was found and completed; otherwise, false.</returns>
    public bool CompleteCall(Guid callId, byte[] result)
    {
        if (_pendingCalls.TryRemove(callId, out var tcs))
        {
            return tcs.TrySetResult(result);
        }
        return false;
    }

    /// <summary>
    /// Fails a pending RPC call with an error.
    /// </summary>
    /// <param name="callId">The ID of the RPC call to fail.</param>
    /// <param name="error">The error message.</param>
    /// <returns>True if the pending call was found and failed; otherwise, false.</returns>
    public bool FailCall(Guid callId, string error)
    {
        if (_pendingCalls.TryRemove(callId, out var tcs))
        {
            return tcs.TrySetException(new InvalidOperationException(error));
        }
        return false;
    }

    /// <summary>
    /// Represents a serialized RPC request payload.
    /// </summary>
    public sealed class RpcRequest
    {
        /// <summary>Gets or sets the unique call identifier.</summary>
        public Guid CallId { get; set; }

        /// <summary>Gets or sets the name of the procedure to invoke.</summary>
        public string Procedure { get; set; } = string.Empty;

        /// <summary>Gets or sets the serialized arguments.</summary>
        public byte[] Args { get; set; } = Array.Empty<byte>();
    }
}
