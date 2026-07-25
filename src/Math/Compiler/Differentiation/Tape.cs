namespace MathVerse.Math.Compiler.Differentiation;

using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>Enumerates the kinds of operations recorded on the AD tape.</summary>
public enum TapeOperation
{
    /// <summary>Addition: out = a + b.</summary>
    Add,

    /// <summary>Subtraction: out = a - b.</summary>
    Sub,

    /// <summary>Multiplication: out = a * b.</summary>
    Mul,

    /// <summary>Division: out = a / b.</summary>
    Div,

    /// <summary>Power: out = a ^ b.</summary>
    Pow,

    /// <summary>Negation: out = -a.</summary>
    Neg,

    /// <summary>Sine: out = sin(a).</summary>
    Sin,

    /// <summary>Cosine: out = cos(a).</summary>
    Cos,

    /// <summary>Tangent: out = tan(a).</summary>
    Tan,

    /// <summary>Exponential: out = exp(a).</summary>
    Exp,

    /// <summary>Natural logarithm: out = ln(a).</summary>
    Ln,

    /// <summary>Square root: out = sqrt(a).</summary>
    Sqrt,
}

/// <summary>A single entry on the tape recording an operation and its operands.</summary>
/// <param name="Operation">The operation performed.</param>
/// <param name="Inputs">Input adjoint values.</param>
/// <param name="Output">Output adjoint value.</param>
public sealed record TapeEntry(TapeOperation Operation, IReadOnlyList<AdjointValue> Inputs, AdjointValue Output);

/// <summary>Records operations for reverse-mode automatic differentiation.</summary>
public sealed class Tape
{
    private readonly List<TapeEntry> _entries = [];
    private readonly object _lock = new();

    /// <summary>The number of recorded entries.</summary>
    public int Count
    {
        get { lock (_lock) return _entries.Count; }
    }

    /// <summary>All recorded tape entries (in forward order).</summary>
    public IReadOnlyList<TapeEntry> Entries
    {
        get { lock (_lock) return [.. _entries]; }
    }

    /// <summary>Records an addition operation.</summary>
    public AdjointValue RecordAdd(AdjointValue a, AdjointValue b)
    {
        var output = AdjointValue.CreateIntermediate(a.Value + b.Value);
        var entry = new TapeEntry(TapeOperation.Add, [a, b], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Records a subtraction operation.</summary>
    public AdjointValue RecordSub(AdjointValue a, AdjointValue b)
    {
        var output = AdjointValue.CreateIntermediate(a.Value - b.Value);
        var entry = new TapeEntry(TapeOperation.Sub, [a, b], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Records a multiplication operation.</summary>
    public AdjointValue RecordMul(AdjointValue a, AdjointValue b)
    {
        var output = AdjointValue.CreateIntermediate(a.Value * b.Value);
        var entry = new TapeEntry(TapeOperation.Mul, [a, b], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Records a division operation.</summary>
    public AdjointValue RecordDiv(AdjointValue a, AdjointValue b)
    {
        var output = AdjointValue.CreateIntermediate(a.Value / b.Value);
        var entry = new TapeEntry(TapeOperation.Div, [a, b], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Records a power operation.</summary>
    public AdjointValue RecordPow(AdjointValue a, AdjointValue b)
    {
        var output = AdjointValue.CreateIntermediate(Math.Pow(a.Value, b.Value));
        var entry = new TapeEntry(TapeOperation.Pow, [a, b], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Records a negation operation.</summary>
    public AdjointValue RecordNeg(AdjointValue a)
    {
        var output = AdjointValue.CreateIntermediate(-a.Value);
        var entry = new TapeEntry(TapeOperation.Neg, [a], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Records a sine operation.</summary>
    public AdjointValue RecordSin(AdjointValue a)
    {
        var output = AdjointValue.CreateIntermediate(Math.Sin(a.Value));
        var entry = new TapeEntry(TapeOperation.Sin, [a], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Records a cosine operation.</summary>
    public AdjointValue RecordCos(AdjointValue a)
    {
        var output = AdjointValue.CreateIntermediate(Math.Cos(a.Value));
        var entry = new TapeEntry(TapeOperation.Cos, [a], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Records a tangent operation.</summary>
    public AdjointValue RecordTan(AdjointValue a)
    {
        var output = AdjointValue.CreateIntermediate(Math.Tan(a.Value));
        var entry = new TapeEntry(TapeOperation.Tan, [a], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Records an exponential operation.</summary>
    public AdjointValue RecordExp(AdjointValue a)
    {
        var output = AdjointValue.CreateIntermediate(Math.Exp(a.Value));
        var entry = new TapeEntry(TapeOperation.Exp, [a], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Records a natural logarithm operation.</summary>
    public AdjointValue RecordLn(AdjointValue a)
    {
        var output = AdjointValue.CreateIntermediate(Math.Log(a.Value));
        var entry = new TapeEntry(TapeOperation.Ln, [a], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Records a square root operation.</summary>
    public AdjointValue RecordSqrt(AdjointValue a)
    {
        var output = AdjointValue.CreateIntermediate(Math.Sqrt(a.Value));
        var entry = new TapeEntry(TapeOperation.Sqrt, [a], output);
        lock (_lock) _entries.Add(entry);
        return output;
    }

    /// <summary>Replays the tape in reverse to propagate gradients.</summary>
    public void Backward()
    {
        List<TapeEntry> snapshot;
        lock (_lock)
            snapshot = [.. _entries];

        for (int i = snapshot.Count - 1; i >= 0; i--)
            BackwardEntry(snapshot[i]);
    }

    /// <summary>Clears all tape entries.</summary>
    public void Clear()
    {
        lock (_lock) _entries.Clear();
    }

    /// <summary>Returns a snapshot of entries in reverse order.</summary>
    public IReadOnlyList<TapeEntry> GetReversedEntries()
    {
        lock (_lock)
        {
            var reversed = new List<TapeEntry>(_entries);
            reversed.Reverse();
            return reversed;
        }
    }

    private static void BackwardEntry(TapeEntry entry)
    {
        double outGrad = entry.Output.Gradient;
        if (outGrad == 0) return;

        switch (entry.Operation)
        {
            case TapeOperation.Add:
                entry.Inputs[0].AccumulateGradient(outGrad);
                entry.Inputs[1].AccumulateGradient(outGrad);
                break;

            case TapeOperation.Sub:
                entry.Inputs[0].AccumulateGradient(outGrad);
                entry.Inputs[1].AccumulateGradient(-outGrad);
                break;

            case TapeOperation.Mul:
                entry.Inputs[0].AccumulateGradient(outGrad * entry.Inputs[1].Value);
                entry.Inputs[1].AccumulateGradient(outGrad * entry.Inputs[0].Value);
                break;

            case TapeOperation.Div:
                {
                    double a = entry.Inputs[0].Value;
                    double b = entry.Inputs[1].Value;
                    double bSq = b * b;
                    entry.Inputs[0].AccumulateGradient(outGrad / b);
                    entry.Inputs[1].AccumulateGradient(-outGrad * a / bSq);
                }
                break;

            case TapeOperation.Pow:
                {
                    double a = entry.Inputs[0].Value;
                    double b = entry.Inputs[1].Value;
                    if (a > 0)
                    {
                        entry.Inputs[0].AccumulateGradient(outGrad * b * Math.Pow(a, b - 1));
                        entry.Inputs[1].AccumulateGradient(outGrad * Math.Pow(a, b) * Math.Log(a));
                    }
                }
                break;

            case TapeOperation.Neg:
                entry.Inputs[0].AccumulateGradient(-outGrad);
                break;

            case TapeOperation.Sin:
                entry.Inputs[0].AccumulateGradient(outGrad * Math.Cos(entry.Inputs[0].Value));
                break;

            case TapeOperation.Cos:
                entry.Inputs[0].AccumulateGradient(-outGrad * Math.Sin(entry.Inputs[0].Value));
                break;

            case TapeOperation.Tan:
                {
                    double cosA = Math.Cos(entry.Inputs[0].Value);
                    entry.Inputs[0].AccumulateGradient(outGrad / (cosA * cosA));
                }
                break;

            case TapeOperation.Exp:
                entry.Inputs[0].AccumulateGradient(outGrad * Math.Exp(entry.Inputs[0].Value));
                break;

            case TapeOperation.Ln:
                entry.Inputs[0].AccumulateGradient(outGrad / entry.Inputs[0].Value);
                break;

            case TapeOperation.Sqrt:
                {
                    double sqrtVal = Math.Sqrt(entry.Inputs[0].Value);
                    entry.Inputs[0].AccumulateGradient(outGrad / (2.0 * sqrtVal));
                }
                break;
        }
    }
}
