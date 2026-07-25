namespace MathVerse.Math.Distributed.MessagePassing;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

/// <summary>
/// A typed communication channel wrapping <see cref="System.Threading.Channels.Channel{T}"/>
/// for point-to-point message exchange between nodes.
/// </summary>
public sealed class Channel
{
    private readonly System.Threading.Channels.Channel<Message> _channel;

    /// <summary>Gets the unique identifier of this channel.</summary>
    public string ChannelId { get; }

    /// <summary>
    /// Initializes a new unbounded channel with the specified identifier.
    /// </summary>
    /// <param name="channelId">Unique identifier for this channel.</param>
    public Channel(string channelId)
    {
        ChannelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
        _channel = System.Threading.Channels.Channel.CreateUnbounded<Message>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });
    }

    /// <summary>
    /// Initializes a bounded channel with the specified capacity and identifier.
    /// </summary>
    /// <param name="channelId">Unique identifier for this channel.</param>
    /// <param name="capacity">Maximum number of messages the channel can hold.</param>
    public Channel(string channelId, int capacity)
    {
        ChannelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
        _channel = System.Threading.Channels.Channel.CreateBounded<Message>(
            new BoundedChannelOptions(capacity)
            {
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
                FullMode = BoundedChannelFullMode.Wait
            });
    }

    /// <summary>
    /// Asynchronously sends a message into the channel.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>A <see cref="ValueTask"/> that completes when the message has been written.</returns>
    public ValueTask SendMessage(Message message)
    {
        return _channel.Writer.WriteAsync(message);
    }

    /// <summary>
    /// Asynchronously receives a message from the channel.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the receive operation.</param>
    /// <returns>The received message.</returns>
    public ValueTask<Message> ReceiveMessage(CancellationToken ct)
    {
        return _channel.Reader.ReadAsync(ct);
    }

    /// <summary>
    /// Attempts to read a message from the channel without blocking.
    /// </summary>
    /// <param name="message">The message read, or null if no message is available.</param>
    /// <returns>True if a message was read; otherwise, false.</returns>
    public bool TryRead(out Message? message)
    {
        return _channel.Reader.TryRead(out message);
    }

    /// <summary>
    /// Gets the number of messages currently buffered in the channel.
    /// </summary>
    public int Count => _channel.Reader.Count;

    /// <summary>
    /// Marks the channel as complete for writing, preventing further messages from being sent.
    /// </summary>
    /// <param name="error">Optional exception that caused the completion.</param>
    public void Complete(Exception? error = null)
    {
        _channel.Writer.Complete(error);
    }
}
