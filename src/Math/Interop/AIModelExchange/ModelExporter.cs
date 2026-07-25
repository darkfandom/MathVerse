namespace MathVerse.Math.Interop.AIModelExchange;

using System;
using System.Text;

/// <summary>
/// Exports AI model descriptors to various external formats.
/// </summary>
public sealed class ModelExporter
{
    /// <summary>
    /// Exports a model descriptor to ONNX binary format.
    /// </summary>
    /// <param name="model">The model descriptor to export.</param>
    /// <returns>A byte array containing the model in ONNX-compatible format.</returns>
    public byte[] ExportToONNX(ModelDescriptor model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (model.Weights.Length > 0)
        {
            return model.Weights;
        }

        return CreateMinimalONNXPackage(model);
    }

    /// <summary>
    /// Exports a model descriptor to PMML XML format.
    /// </summary>
    /// <param name="model">The model descriptor to export.</param>
    /// <returns>A string containing the model in PMML XML format.</returns>
    public string ExportToPMML(ModelDescriptor model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<PMML xmlns=\"http://www.dmg.org/PMML-4_4\" version=\"4.4\">");
        sb.AppendLine("  <Header>");
        sb.AppendFormat("    <Application name=\"MathVerse.Interop\" version=\"1.0\" />{0}", Environment.NewLine);
        sb.AppendFormat("    <Timestamp>{0}</Timestamp>{1}", model.Metadata.Created.ToString("o"), Environment.NewLine);
        sb.AppendLine("  </Header>");
        sb.AppendLine("  <DataDictionary>");
        sb.AppendFormat("    <DataField name=\"ModelName\" optype=\"categorical\" dataType=\"string\" />{0}", Environment.NewLine);
        sb.AppendLine("  </DataDictionary>");
        sb.AppendFormat("  <Regression modelName=\"{0}\">{1}", EscapeXml(model.Name), Environment.NewLine);
        sb.AppendFormat("    <MiningSchema>{0}{1}      <MiningField name=\"ModelName\" />{2}{1}    </MiningSchema>{1}",
            Environment.NewLine, Environment.NewLine, Environment.NewLine);
        sb.AppendLine("  </Regression>");
        sb.AppendLine("</PMML>");

        return sb.ToString();
    }

    /// <summary>
    /// Exports a model descriptor to a byte array in the specified format.
    /// </summary>
    /// <param name="model">The model descriptor to export.</param>
    /// <param name="format">The target format identifier (e.g., "onnx", "pmml", "raw").</param>
    /// <returns>A byte array containing the model in the target format.</returns>
    public byte[] ExportToBytes(ModelDescriptor model, string format)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrEmpty(format);

        return format.ToLowerInvariant() switch
        {
            "onnx" => ExportToONNX(model),
            "pmml" => Encoding.UTF8.GetBytes(ExportToPMML(model)),
            "raw" => model.Weights.Length > 0
                ? model.Weights
                : Array.Empty<byte>(),
            _ => throw new NotSupportedException($"Format '{format}' is not supported for export.")
        };
    }

    private static byte[] CreateMinimalONNXPackage(ModelDescriptor model)
    {
        var header = Encoding.UTF8.GetBytes(
            $"{{\"format\":\"onnx\",\"name\":\"{EscapeJson(model.Name)}\",\"architecture\":\"{EscapeJson(model.Architecture)}\"}}");
        var result = new byte[header.Length];
        Buffer.BlockCopy(header, 0, result, 0, header.Length);
        return result;
    }

    private static string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }

    private static string EscapeXml(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
