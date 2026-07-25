namespace MathVerse.Math.Distributed.GPU
{
    using System;

    /// <summary>
    /// Result of a GPU kernel execution, containing timing information,
    /// output data, and any error details.
    /// </summary>
    public sealed class GPUExecutionResult
    {
        /// <summary>
        /// Gets or sets whether the execution completed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the execution time in milliseconds.
        /// </summary>
        public double ExecutionTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the output data returned by the kernel, if any.
        /// </summary>
        public byte[]? OutputData { get; set; }

        /// <summary>
        /// Gets or sets the error message if the execution failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets the execution time in seconds.
        /// </summary>
        public double ExecutionTimeSeconds => ExecutionTimeMs / 1000.0;

        /// <summary>
        /// Creates a successful execution result with the specified timing and output data.
        /// </summary>
        /// <param name="executionTimeMs">The execution time in milliseconds.</param>
        /// <param name="outputData">The output data from the kernel.</param>
        /// <returns>A new successful <see cref="GPUExecutionResult"/>.</returns>
        public static GPUExecutionResult CreateSuccess(double executionTimeMs, byte[]? outputData = null)
        {
            return new GPUExecutionResult
            {
                Success = true,
                ExecutionTimeMs = executionTimeMs,
                OutputData = outputData
            };
        }

        /// <summary>
        /// Creates a failed execution result with the specified error message.
        /// </summary>
        /// <param name="errorMessage">A description of the error that occurred.</param>
        /// <returns>A new failed <see cref="GPUExecutionResult"/>.</returns>
        public static GPUExecutionResult CreateFailure(string errorMessage)
        {
            return new GPUExecutionResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Returns a string representation of this result.
        /// </summary>
        /// <returns>A string indicating success or failure, along with timing information.</returns>
        public override string ToString()
        {
            if (Success)
                return $"Success - {ExecutionTimeMs:F3} ms";
            else
                return $"Failed - {ErrorMessage ?? "Unknown error"}";
        }
    }
}
