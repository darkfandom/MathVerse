namespace MathVerse.Math.Distributed.GPU
{
    using System;

    /// <summary>
    /// Represents a compiled GPU program (shader or kernel binary) that can be used
    /// to create and launch kernels on a GPU device.
    /// </summary>
    public sealed class GPUProgram
    {
        /// <summary>
        /// Gets or sets the unique identifier for this program.
        /// </summary>
        public int ProgramId { get; set; }

        /// <summary>
        /// Gets or sets the source code of this program.
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the compiled binary representation of this program.
        /// </summary>
        public byte[] Binary { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets or sets whether this program has been successfully compiled.
        /// </summary>
        public bool IsCompiled { get; set; }

        /// <summary>
        /// Gets the size of the compiled binary in bytes.
        /// </summary>
        public int BinarySize => Binary?.Length ?? 0;

        /// <summary>
        /// Gets the number of source code characters.
        /// </summary>
        public int SourceLength => Source?.Length ?? 0;

        /// <summary>
        /// Returns a string representation of this program.
        /// </summary>
        /// <returns>A string containing the program ID and compilation status.</returns>
        public override string ToString()
        {
            string status = IsCompiled ? "Compiled" : "Not compiled";
            return $"Program[{ProgramId}] - {status} - {BinarySize} bytes";
        }
    }
}
