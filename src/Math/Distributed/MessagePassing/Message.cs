namespace MathVerse.Math.Distributed.MessagePassing;

using System;

/// <summary>
/// Defines the type of a message on the message bus.
/// </summary>
public enum MessageType
{
    /// <summary>Carries application data.</summary>
    Data,

    /// <summary>Carries control commands (start, stop, configure).</summary>
    Control,

    /// <summary>Periodic health-check message.</summary>
    Heartbeat,

    /// <summary>Acknowledgment of a previously sent message.</summary>
    Ack,

    /// <summary>Reports an error condition.</summary>
    Error
}

/// <summary>
/// Represents an envelope for messages passed between nodes in the distributed system.
/// </summary>
public sealed class Message
{
    /// <summary>Gets the unique identifier of this message.</summary>
    public Guid MessageId { get; }

    /// <summary>Gets the type of this message.</summary>
    public MessageType Type { get; }

    /// <summary>Gets the node ID of the sender.</summary>
    public string SenderId { get; }

    /// <summary>Gets the node ID of the intended receiver.</summary>
    public string ReceiverId { get; }

    /// <summary>Gets the binary payload carried by this message.</summary>
    public byte[] Payload { get; }

    /// <summary>Gets the UTC timestamp when this message was created.</summary>
    public DateTime Timestamp { get; }

    /// <summary>Gets the priority of this message (higher values indicate higher priority).</summary>
    public int Priority { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// </summary>
    /// <param name="messageId">Unique message identifier.</param>
    /// <param name="type">Type of the message.</param>
    /// <param name="senderId">Sender node ID.</param>
    /// <param name="receiverId">Receiver node ID.</param>
    /// <param name="payload">Binary payload.</param>
    /// <param name="timestamp">UTC creation timestamp.</param>
    /// <param name="priority">Message priority (higher is more important).</param>
    public Message(
        Guid messageId,
        MessageType type,
        string senderId,
        string receiverId,
        byte[] payload,
        DateTime timestamp,
        int priority)
    {
        MessageId = messageId;
        Type = type;
        SenderId = senderId ?? throw new ArgumentNullException(nameof(senderId));
        ReceiverId = receiverId ?? throw new ArgumentNullException(nameof(receiverId));
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        Timestamp = timestamp;
        Priority = priority;
    }
}
