namespace MathVerse.Math.Interop.ScientificFormats;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

/// <summary>
/// Reads NumPy .npz format files (ZIP archives containing .npy files).
/// </summary>
public sealed class NumPyNpzReader
{
    private const ushort NpyMagic = 0x4E4E;

    /// <summary>
    /// Reads a NumPy .npz file from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the .npz file data.</param>
    /// <returns>A dictionary mapping array names to their data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    public Dictionary<string, Array> Read(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        var result = new Dictionary<string, Array>(StringComparer.OrdinalIgnoreCase);

        using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        foreach (var entry in zipArchive.Entries)
        {
            if (!entry.Name.EndsWith(".npy", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var arrayName = Path.GetFileNameWithoutExtension(entry.Name);
            using var entryStream = entry.Open();
            using var memoryStream = new MemoryStream();
            entryStream.CopyTo(memoryStream);

            var npyReader = new NumPyNpyReader();
            var data = npyReader.Read(memoryStream.ToArray());
            result[arrayName] = data;
        }

        return result;
    }

    /// <summary>
    /// Reads a NumPy .npz file from a byte array.
    /// </summary>
    /// <param name="data">The byte array containing the .npz file data.</param>
    /// <returns>A dictionary mapping array names to their data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public Dictionary<string, Array> Read(byte[] data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        using var stream = new MemoryStream(data);
        return Read(stream);
    }

    /// <summary>
    /// Reads a specific array from a NumPy .npz stream by name.
    /// </summary>
    /// <param name="stream">The stream containing the .npz file data.</param>
    /// <param name="arrayName">The name of the array to read.</param>
    /// <returns>The array data, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> or <paramref name="arrayName"/> is null.</exception>
    public Array? ReadEntry(Stream stream, string arrayName)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (string.IsNullOrWhiteSpace(arrayName))
        {
            return null;
        }

        var targetEntry = arrayName + ".npy";
        using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var entry = zipArchive.GetEntry(targetEntry);
        if (entry is null)
        {
            return null;
        }

        using var entryStream = entry.Open();
        using var memoryStream = new MemoryStream();
        entryStream.CopyTo(memoryStream);

        var npyReader = new NumPyNpyReader();
        return npyReader.Read(memoryStream.ToArray());
    }
}

/// <summary>
/// Writes NumPy .npz format files (ZIP archives containing .npy files).
/// </summary>
public sealed class NumPyNpzWriter
{
    /// <summary>
    /// Writes a dictionary of arrays to a NumPy .npz file byte array.
    /// </summary>
    /// <param name="arrays">The arrays to write, mapping names to data.</param>
    /// <returns>A byte array containing the .npz file data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arrays"/> is null.</exception>
    public byte[] Write(Dictionary<string, Array> arrays)
    {
        if (arrays is null)
        {
            throw new ArgumentNullException(nameof(arrays));
        }

        using var stream = new MemoryStream();
        using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);

        foreach (var kvp in arrays)
        {
            var entryName = kvp.Key + ".npy";
            var entry = zipArchive.CreateEntry(entryName, CompressionLevel.Optimal);

            using var entryStream = entry.Open();
            var npyWriter = new NumPyNpyWriter();
            var npyData = npyWriter.Write(kvp.Value);
            entryStream.Write(npyData, 0, npyData.Length);
        }

        zipArchive.Dispose();
        return stream.ToArray();
    }
}
