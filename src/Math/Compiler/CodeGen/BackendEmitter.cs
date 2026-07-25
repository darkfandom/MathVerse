namespace MathVerse.Math.Compiler.CodeGen;

using System;
using System.Buffers;
using System.Text;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Abstract base class for backend code emitters. Uses StringBuilder backed by ArrayPool for efficient string construction.
/// </summary>
public abstract class BackendEmitter : IDisposable
{
    private char[] _buffer;
    private StringBuilder _sb;
    private bool _disposed;

    /// <summary>
    /// Initializes a new backend emitter with a pooled character buffer.
    /// </summary>
    protected BackendEmitter()
    {
        _buffer = ArrayPool<char>.Shared.Rent(4096);
        _sb = new StringBuilder(_buffer.Length);
    }

    /// <summary>
    /// Emits the function prologue (opening braces, parameter declarations, local variable setup).
    /// </summary>
    /// <param name="function">The IR function being emitted.</param>
    public abstract void EmitPrologue(IRFunction function);

    /// <summary>
    /// Emits a single IR instruction into the target representation.
    /// </summary>
    /// <param name="instruction">The IR instruction to emit.</param>
    public abstract void EmitInstruction(IRInstruction instruction);

    /// <summary>
    /// Emits the function epilogue (closing braces, return handling, cleanup).
    /// </summary>
    /// <param name="function">The IR function being emitted.</param>
    public abstract void EmitEpilogue(IRFunction function);

    /// <summary>
    /// Appends raw text to the emission buffer.
    /// </summary>
    /// <param name="text">The text to append.</param>
    protected void Append(string text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _sb.Append(text);
    }

    /// <summary>
    /// Appends a single character to the emission buffer.
    /// </summary>
    /// <param name="c">The character to append.</param>
    protected void Append(char c)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _sb.Append(c);
    }

    /// <summary>
    /// Appends a line terminator to the emission buffer.
    /// </summary>
    protected void AppendLine()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _sb.AppendLine();
    }

    /// <summary>
    /// Appends text followed by a line terminator to the emission buffer.
    /// </summary>
    /// <param name="text">The text to append.</param>
    protected void AppendLine(string text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _sb.AppendLine(text);
    }

    /// <summary>
    /// Gets the accumulated result as a string.
    /// </summary>
    /// <returns>The emitted code string.</returns>
    public string GetResult()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _sb.ToString();
    }

    /// <summary>
    /// Clears the emission buffer for reuse.
    /// </summary>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _sb.Clear();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _sb.Clear();
            _sb = null!;
            if (_buffer != null)
            {
                ArrayPool<char>.Shared.Return(_buffer);
                _buffer = null!;
            }
        }
    }
}
