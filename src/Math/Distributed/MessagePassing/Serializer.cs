namespace MathVerse.Math.Distributed.MessagePassing;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides binary serialization and deserialization using System.Text.Json.
/// Configure with source-generated <see cref="JsonSerializerOptions"/> for Native AOT compatibility.
/// </summary>
public sealed class Serializer
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="Serializer"/> class with default options.
    /// </summary>
    public Serializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Serializer"/> class with custom options.
    /// </summary>
    /// <param name="options">Custom JSON serializer options. For AOT, provide a source-generated resolver.</param>
    public Serializer(JsonSerializerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Serializes an object to a UTF-8 byte array.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A UTF-8 encoded byte array containing the serialized data.</returns>
    public byte[] Serialize<T>(T obj)
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj, _options);
    }

    /// <summary>
    /// Deserializes a UTF-8 byte array to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize into.</typeparam>
    /// <param name="data">The UTF-8 encoded byte array to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public T Deserialize<T>(byte[] data)
    {
        return JsonSerializer.Deserialize<T>(data, _options)!;
    }
}
