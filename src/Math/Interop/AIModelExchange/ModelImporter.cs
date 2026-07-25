namespace MathVerse.Math.Interop.AIModelExchange;

using System;
using System.Text;
using MathVerse.Math.Interop.Core;

/// <summary>
/// Imports AI models from various external formats into model descriptors.
/// Uses an adapter pattern where raw data is stored in the descriptor
/// for later processing by format-specific handlers.
/// </summary>
public sealed class ModelImporter
{
    /// <summary>
    /// Imports a model from ONNX binary data.
    /// </summary>
    /// <param name="onnxData">The raw ONNX model bytes.</param>
    /// <returns>An InteropResult containing the imported model descriptor.</returns>
    public InteropResult<ModelDescriptor> ImportFromONNX(byte[] onnxData)
    {
        if (onnxData == null || onnxData.Length == 0)
        {
            return InteropResult<ModelDescriptor>.Failure("ONNX data is null or empty.");
        }

        var model = new ModelDescriptor
        {
            Name = "ImportedONNXModel",
            Version = "1.0",
            Architecture = "onnx",
            Weights = onnxData,
            Metadata =
            {
                Author = string.Empty,
                Description = "Model imported from ONNX format",
                Created = DateTimeOffset.UtcNow
            }
        };
        model.Metadata.Properties["SourceFormat"] = "onnx";
        model.Metadata.Properties["OriginalSize"] = onnxData.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return InteropResult<ModelDescriptor>.Success(model);
    }

    /// <summary>
    /// Imports a model from PMML XML data.
    /// </summary>
    /// <param name="pmmlData">The raw PMML XML string.</param>
    /// <returns>An InteropResult containing the imported model descriptor.</returns>
    public InteropResult<ModelDescriptor> ImportFromPMML(string pmmlData)
    {
        if (string.IsNullOrEmpty(pmmlData))
        {
            return InteropResult<ModelDescriptor>.Failure("PMML data is null or empty.");
        }

        var dataBytes = Encoding.UTF8.GetBytes(pmmlData);
        var model = new ModelDescriptor
        {
            Name = "ImportedPMMLModel",
            Version = "1.0",
            Architecture = "pmml",
            Weights = dataBytes,
            Metadata =
            {
                Author = string.Empty,
                Description = "Model imported from PMML format",
                Created = DateTimeOffset.UtcNow
            }
        };
        model.Metadata.Properties["SourceFormat"] = "pmml";
        model.Metadata.Properties["OriginalSize"] = dataBytes.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return InteropResult<ModelDescriptor>.Success(model);
    }

    /// <summary>
    /// Imports a model from raw bytes with a specified format identifier.
    /// </summary>
    /// <param name="data">The raw model data.</param>
    /// <param name="format">The format identifier (e.g., "onnx", "pmml", "pb", "h5").</param>
    /// <returns>An InteropResult containing the imported model descriptor.</returns>
    public InteropResult<ModelDescriptor> ImportFromBytes(byte[] data, string format)
    {
        if (data == null || data.Length == 0)
        {
            return InteropResult<ModelDescriptor>.Failure("Data is null or empty.");
        }

        if (string.IsNullOrEmpty(format))
        {
            return InteropResult<ModelDescriptor>.Failure("Format identifier is required.");
        }

        var model = new ModelDescriptor
        {
            Name = "ImportedModel",
            Version = "1.0",
            Architecture = format.ToLowerInvariant(),
            Weights = data,
            Metadata =
            {
                Author = string.Empty,
                Description = $"Model imported from {format} format",
                Created = DateTimeOffset.UtcNow
            }
        };
        model.Metadata.Properties["SourceFormat"] = format.ToLowerInvariant();
        model.Metadata.Properties["OriginalSize"] = data.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return InteropResult<ModelDescriptor>.Success(model);
    }
}
