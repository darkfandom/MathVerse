namespace MathVerse.Math.Interop.GeometryExchange;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Interface for reading IGES format geometry files.
/// </summary>
public interface IIGESReader
{
    /// <summary>
    /// Reads an IGES file from the provided stream.
    /// </summary>
    /// <param name="stream">The stream containing IGES data.</param>
    /// <returns>An <see cref="IGESFile"/> containing the parsed entities.</returns>
    IGESFile Read(System.IO.Stream stream);

    /// <summary>
    /// Reads an IGES file from the provided string content.
    /// </summary>
    /// <param name="content">The IGES file content string.</param>
    /// <returns>An <see cref="IGESFile"/> containing the parsed entities.</returns>
    IGESFile Read(string content);
}

/// <summary>
/// Interface for writing IGES format geometry files.
/// </summary>
public interface IIGESWriter
{
    /// <summary>
    /// Writes an IGES file to a string.
    /// </summary>
    /// <param name="igesFile">The IGES file to write.</param>
    /// <returns>A string containing the IGES format data.</returns>
    string Write(IGESFile igesFile);

    /// <summary>
    /// Writes an IGES file to a stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="igesFile">The IGES file to write.</param>
    void Write(System.IO.Stream stream, IGESFile igesFile);
}

/// <summary>
/// In-memory representation of an IGES file storing entities as a dictionary of properties.
/// </summary>
public sealed class IGESFile
{
    /// <summary>
    /// Gets or sets the IGES file description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender name.
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the originating system identifier.
    /// </summary>
    public string OriginatingSystem { get; set; } = "MathVerse IGES Writer";

    /// <summary>
    /// Gets the dictionary of entities stored by sequence number.
    /// </summary>
    public Dictionary<int, IGESDirectoryEntry> DirectoryEntries { get; } = new();

    /// <summary>
    /// Gets the parameter data stored by entity sequence number.
    /// </summary>
    public Dictionary<int, string> ParameterData { get; } = new();

    /// <summary>
    /// Gets the list of entity sequence numbers in file order.
    /// </summary>
    public List<int> EntityOrder { get; } = new();

    /// <summary>
    /// Adds an entity to the IGES file.
    /// </summary>
    /// <param name="sequenceNumber">The entity sequence number.</param>
    /// <param name="entry">The directory entry.</param>
    /// <param name="parameters">The parameter data string.</param>
    public void AddEntity(int sequenceNumber, IGESDirectoryEntry entry, string parameters)
    {
        DirectoryEntries[sequenceNumber] = entry ?? throw new ArgumentNullException(nameof(entry));
        ParameterData[sequenceNumber] = parameters ?? string.Empty;
        EntityOrder.Add(sequenceNumber);
    }
}

/// <summary>
/// Represents an IGES directory entry for an entity.
/// </summary>
public sealed class IGESDirectoryEntry
{
    /// <summary>Gets or sets the entity type number.</summary>
    public int EntityTypeNumber { get; set; }

    /// <summary>Gets or sets the parameter data sequence number.</summary>
    public int ParameterDataSequenceNumber { get; set; }

    /// <summary>Gets or sets the line font pattern.</summary>
    public int LineFontPattern { get; set; }

    /// <summary>Gets or sets the level.</summary>
    public int Level { get; set; }

    /// <summary>Gets or sets the view number.</summary>
    public int ViewNumber { get; set; }

    /// <summary>Gets or sets the transformation matrix pointer.</summary>
    public int TransformationMatrixPointer { get; set; }

    /// <summary>Gets or sets the label display association.</summary>
    public int LabelDisplayAssociation { get; set; }

    /// <summary>Gets or sets the status number.</summary>
    public string StatusNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity type label.</summary>
    public string EntityTypeLabel { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity subtype number.</summary>
    public int EntitySubscriptNumber { get; set; }
}

/// <summary>
/// Default IGES format reader with in-memory entity storage.
/// </summary>
public sealed class DefaultIGESReader : IIGESReader
{
    /// <inheritdoc/>
    public IGESFile Read(System.IO.Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new System.IO.StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return Read(reader.ReadToEnd());
    }

    /// <inheritdoc/>
    public IGESFile Read(string content)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        var igesFile = new IGESFile();
        var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        int lineNum = 0;

        foreach (var rawLine in lines)
        {
            string line = rawLine.Length >= 80 ? rawLine.Substring(0, 80) : rawLine.PadRight(80);
            char section = line[72];

            if (section == 'S')
            {
                string start = line.Substring(0, Math.Min(72, line.Length)).Trim();
                if (!string.IsNullOrEmpty(start))
                    igesFile.Description = start;
            }
            else if (section == 'G')
            {
                string data = line.Substring(0, 72).Trim();
                if (lineNum % 2 == 0 && !string.IsNullOrEmpty(data))
                {
                    // Parse global parameters as comma-separated
                }
                lineNum++;
            }
            else if (section == 'D')
            {
                int entryIdx = lineNum / 2;
                int seqNum = entryIdx + 1;
                string data = line.Substring(0, 72).TrimEnd();

                if (lineNum % 2 == 0 && data.Length >= 8)
                {
                    var entry = new IGESDirectoryEntry();
                    if (int.TryParse(data.Substring(0, 8).Trim(), out int etype))
                        entry.EntityTypeNumber = etype;
                    if (data.Length >= 16 && int.TryParse(data.Substring(8, 8).Trim(), out int pdSeq))
                        entry.ParameterDataSequenceNumber = pdSeq;
                    if (data.Length >= 24 && int.TryParse(data.Substring(16, 8).Trim(), out int lfp))
                        entry.LineFontPattern = lfp;
                    if (data.Length >= 32 && int.TryParse(data.Substring(24, 8).Trim(), out int lvl))
                        entry.Level = lvl;
                    igesFile.DirectoryEntries[seqNum] = entry;
                }

                lineNum++;
            }
            else if (section == 'P')
            {
                int eqIdx = line.IndexOf('=');
                if (eqIdx >= 0 && int.TryParse(line.Substring(0, eqIdx).Trim(), out int seqNum))
                {
                    string paramStr = line.Substring(eqIdx + 1, Math.Min(64, line.Length - eqIdx - 1)).Trim().TrimEnd(',');
                    igesFile.ParameterData[seqNum] = paramStr;
                    if (!igesFile.EntityOrder.Contains(seqNum))
                        igesFile.EntityOrder.Add(seqNum);
                }
            }
        }

        return igesFile;
    }
}

/// <summary>
/// Default IGES format writer from in-memory entity storage.
/// </summary>
public sealed class DefaultIGESWriter : IIGESWriter
{
    private const int LineLength = 80;

    /// <inheritdoc/>
    public string Write(IGESFile igesFile)
    {
        if (igesFile is null)
            throw new ArgumentNullException(nameof(igesFile));

        var sb = new StringBuilder();

        // Start section
        string startLine = $"1H{igesFile.Description},1H,";
        sb.AppendLine(PadLine(startLine, 'S', 1));

        // Global section
        string globalLine = $"1H{igesFile.OriginatingSystem},";
        sb.AppendLine(PadLine(globalLine, 'G', 1));

        // Directory section
        int dirLine = 1;
        foreach (int seqNum in igesFile.EntityOrder)
        {
            if (!igesFile.DirectoryEntries.TryGetValue(seqNum, out var entry)) continue;
            string d1 = $"{entry.EntityTypeNumber,8}{entry.ParameterDataSequenceNumber,8}{entry.LineFontPattern,8}{entry.Level,8}{entry.ViewNumber,8}{entry.TransformationMatrixPointer,8}{entry.LabelDisplayAssociation,8}{entry.StatusNumber,8}";
            string d2 = $"{entry.EntityTypeLabel,8}{entry.EntitySubscriptNumber,8}                                        ";
            sb.AppendLine(PadLine(d1, 'D', dirLine++));
            sb.AppendLine(PadLine(d2, 'D', dirLine++));
        }

        // Parameter section
        int paramLine = 1;
        foreach (int seqNum in igesFile.EntityOrder)
        {
            if (!igesFile.ParameterData.TryGetValue(seqNum, out string? paramData)) continue;
            string pLine = $"{seqNum},{paramData},";
            sb.AppendLine(PadLine(pLine, 'P', paramLine++));
        }

        // Terminate section
        sb.AppendLine(PadLine("", 'T', dirLine));

        return sb.ToString();
    }

    /// <inheritdoc/>
    public void Write(System.IO.Stream stream, IGESFile igesFile)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        string content = Write(igesFile);
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string PadLine(string data, char section, int lineNum)
    {
        string padded = data.Length >= 72 ? data.Substring(0, 72) : data.PadRight(72);
        string suffix = $"{lineNum,7}{section}";
        return padded + suffix;
    }
}
