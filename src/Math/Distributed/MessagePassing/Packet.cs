namespace MathVerse.Math.Distributed.MessagePassing;

using System;

/// <summary>
/// Represents a network packet with a CRC32 checksum for data integrity verification.
/// </summary>
public sealed class Packet
{
    private static readonly uint[] Crc32Table = GenerateCrc32Table();

    /// <summary>Gets the unique identifier of this packet.</summary>
    public Guid PacketId { get; }

    /// <summary>Gets the sequence number for ordering packets within a stream.</summary>
    public long SequenceNumber { get; }

    /// <summary>Gets the node ID of the packet source.</summary>
    public string SourceNode { get; }

    /// <summary>Gets the node ID of the packet destination.</summary>
    public string DestinationNode { get; }

    /// <summary>Gets the type of message this packet carries.</summary>
    public MessageType MessageType { get; }

    /// <summary>Gets the binary payload carried by this packet.</summary>
    public byte[] Payload { get; }

    /// <summary>Gets the CRC32 checksum of the payload for integrity verification.</summary>
    public uint Checksum { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Packet"/> class, computing the CRC32 checksum.
    /// </summary>
    /// <param name="packetId">Unique packet identifier.</param>
    /// <param name="sequenceNumber">Sequence number for ordering.</param>
    /// <param name="sourceNode">Source node ID.</param>
    /// <param name="destinationNode">Destination node ID.</param>
    /// <param name="messageType">Type of message carried.</param>
    /// <param name="payload">Binary payload data.</param>
    public Packet(
        Guid packetId,
        long sequenceNumber,
        string sourceNode,
        string destinationNode,
        MessageType messageType,
        byte[] payload)
    {
        PacketId = packetId;
        SequenceNumber = sequenceNumber;
        SourceNode = sourceNode ?? throw new ArgumentNullException(nameof(sourceNode));
        DestinationNode = destinationNode ?? throw new ArgumentNullException(nameof(destinationNode));
        MessageType = messageType;
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        Checksum = ComputeChecksum(payload);
    }

    /// <summary>
    /// Initializes a new instance with a pre-computed checksum (for deserialization).
    /// </summary>
    /// <param name="packetId">Unique packet identifier.</param>
    /// <param name="sequenceNumber">Sequence number for ordering.</param>
    /// <param name="sourceNode">Source node ID.</param>
    /// <param name="destinationNode">Destination node ID.</param>
    /// <param name="messageType">Type of message carried.</param>
    /// <param name="payload">Binary payload data.</param>
    /// <param name="checksum">Pre-computed CRC32 checksum.</param>
    public Packet(
        Guid packetId,
        long sequenceNumber,
        string sourceNode,
        string destinationNode,
        MessageType messageType,
        byte[] payload,
        uint checksum)
    {
        PacketId = packetId;
        SequenceNumber = sequenceNumber;
        SourceNode = sourceNode ?? throw new ArgumentNullException(nameof(sourceNode));
        DestinationNode = destinationNode ?? throw new ArgumentNullException(nameof(destinationNode));
        MessageType = messageType;
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        Checksum = checksum;
    }

    /// <summary>
    /// Verifies that the stored CRC32 checksum matches the checksum of the current payload.
    /// </summary>
    /// <returns>True if the checksum is valid; otherwise, false.</returns>
    public bool VerifyChecksum()
    {
        return Checksum == ComputeChecksum(Payload);
    }

    /// <summary>
    /// Computes the CRC32 checksum of the given byte array.
    /// </summary>
    /// <param name="data">The byte array to compute the checksum for.</param>
    /// <returns>The CRC32 checksum value.</returns>
    public static uint ComputeChecksum(byte[] data)
    {
        uint crc = 0xFFFFFFFFu;
        for (int i = 0; i < data.Length; i++)
        {
            crc = (crc >> 8) ^ Crc32Table[(crc ^ data[i]) & 0xFF];
        }
        return crc ^ 0xFFFFFFFFu;
    }

    /// <summary>
    /// Generates the CRC32 lookup table using the standard polynomial 0xEDB88320.
    /// </summary>
    private static uint[] GenerateCrc32Table()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 0; j < 8; j++)
            {
                crc = (crc & 1) != 0
                    ? (crc >> 1) ^ 0xEDB88320u
                    : crc >> 1;
            }
            table[i] = crc;
        }
        return table;
    }
}
