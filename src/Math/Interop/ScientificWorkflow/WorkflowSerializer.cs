namespace MathVerse.Math.Interop.ScientificWorkflow;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Serializes and deserializes workflow definitions using a manual text format
/// for Native AOT safety without relying on reflection-based serializers.
/// </summary>
public sealed class WorkflowSerializer
{
    private const char FieldSeparator = '|';
    private const char EntrySeparator = ';';

    /// <summary>
    /// Serializes a workflow to a text string.
    /// </summary>
    /// <param name="workflow">The workflow to serialize.</param>
    /// <returns>A text string representing the workflow.</returns>
    public string Serialize(Workflow workflow)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        var sb = new StringBuilder();
        sb.AppendLine("WORKFLOW_V1");
        sb.AppendLine(Escape(workflow.Id));
        sb.AppendLine(Escape(workflow.Name));
        sb.AppendLine(Escape(workflow.Description));

        sb.AppendLine(workflow.Variables.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
        foreach (var kvp in workflow.Variables)
        {
            sb.AppendLine($"{Escape(kvp.Key)}{FieldSeparator}{Escape(kvp.Value)}");
        }

        sb.AppendLine(workflow.Steps.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
        foreach (var step in workflow.Steps)
        {
            sb.AppendLine(Escape(step.StepId));
            sb.AppendLine(Escape(step.Name));
            sb.AppendLine(Escape(step.StepType));

            sb.AppendLine(step.Parameters.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
            foreach (var param in step.Parameters)
            {
                sb.AppendLine($"{Escape(param.Key)}{FieldSeparator}{Escape(param.Value?.ToString() ?? string.Empty)}");
            }

            sb.AppendLine(step.Dependencies.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
            foreach (var dep in step.Dependencies)
            {
                sb.AppendLine(Escape(dep));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Deserializes a workflow from a text string.
    /// </summary>
    /// <param name="json">The text string to deserialize.</param>
    /// <returns>The deserialized workflow.</returns>
    public Workflow Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        var lines = SplitLines(json);
        var pos = 0;

        var header = lines[pos++];
        if (!header.StartsWith("WORKFLOW_V", StringComparison.Ordinal))
        {
            throw new FormatException("Invalid workflow format header.");
        }

        var workflow = new Workflow
        {
            Id = Unescape(lines[pos++]),
            Name = Unescape(lines[pos++]),
            Description = Unescape(lines[pos++])
        };

        var varCount = int.Parse(lines[pos++], System.Globalization.CultureInfo.InvariantCulture);
        for (var i = 0; i < varCount; i++)
        {
            var parts = lines[pos++].Split(FieldSeparator);
            workflow.Variables[Unescape(parts[0])] = Unescape(parts[1]);
        }

        var stepCount = int.Parse(lines[pos++], System.Globalization.CultureInfo.InvariantCulture);
        for (var i = 0; i < stepCount; i++)
        {
            var step = new WorkflowStep
            {
                StepId = Unescape(lines[pos++]),
                Name = Unescape(lines[pos++]),
                StepType = Unescape(lines[pos++])
            };

            var paramCount = int.Parse(lines[pos++], System.Globalization.CultureInfo.InvariantCulture);
            for (var p = 0; p < paramCount; p++)
            {
                var parts = lines[pos++].Split(FieldSeparator);
                step.Parameters[Unescape(parts[0])] = Unescape(parts[1]);
            }

            var depCount = int.Parse(lines[pos++], System.Globalization.CultureInfo.InvariantCulture);
            for (var d = 0; d < depCount; d++)
            {
                step.Dependencies.Add(Unescape(lines[pos++]));
            }

            workflow.AddStep(step);
        }

        return workflow;
    }

    /// <summary>
    /// Serializes a workflow to a binary byte array.
    /// </summary>
    /// <param name="workflow">The workflow to serialize.</param>
    /// <returns>A byte array containing the serialized workflow.</returns>
    public byte[] SerializeBinary(Workflow workflow)
    {
        return Encoding.UTF8.GetBytes(Serialize(workflow));
    }

    /// <summary>
    /// Deserializes a workflow from a binary byte array.
    /// </summary>
    /// <param name="data">The byte array containing the serialized workflow.</param>
    /// <returns>The deserialized workflow.</returns>
    public Workflow DeserializeBinary(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return Deserialize(Encoding.UTF8.GetString(data));
    }

    private static string[] SplitLines(string text)
    {
        return text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace("\\", "\\\\")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace(FieldSeparator.ToString(), $"\\{FieldSeparator}");
    }

    private static string Unescape(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace($"\\{FieldSeparator}", FieldSeparator.ToString())
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\\\", "\\");
    }
}
