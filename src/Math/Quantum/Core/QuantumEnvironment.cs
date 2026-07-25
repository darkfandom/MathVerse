namespace MathVerse.Math.Quantum.Core;

/// <summary>
/// Provides information about the current quantum runtime environment.
/// </summary>
public sealed class QuantumEnvironment
{
    /// <summary>
    /// Gets the number of available processors.
    /// </summary>
    public int ProcessorCount { get; }

    /// <summary>
    /// Gets a value indicating whether the environment is a simulation.
    /// </summary>
    public bool IsSimulated { get; }

    /// <summary>
    /// Gets the type of simulator being used, or <c>null</c> if running on hardware.
    /// </summary>
    public string SimulatorType { get; }

    /// <summary>
    /// Gets the available memory in bytes.
    /// </summary>
    public long AvailableMemory { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumEnvironment"/> class.
    /// </summary>
    /// <param name="processorCount">The number of processors.</param>
    /// <param name="isSimulated">Whether this is a simulated environment.</param>
    /// <param name="simulatorType">The simulator type name.</param>
    /// <param name="availableMemory">The available memory in bytes.</param>
    public QuantumEnvironment(int processorCount, bool isSimulated, string simulatorType, long availableMemory)
    {
        ProcessorCount = processorCount;
        IsSimulated = isSimulated;
        SimulatorType = simulatorType ?? string.Empty;
        AvailableMemory = availableMemory;
    }

    /// <summary>
    /// Gets the current quantum runtime environment information.
    /// </summary>
    /// <returns>A <see cref="QuantumEnvironment"/> reflecting the current system.</returns>
    public static QuantumEnvironment GetCurrent()
    {
        return new QuantumEnvironment(
            processorCount: Environment.ProcessorCount,
            isSimulated: true,
            simulatorType: "StateVectorSimulator",
            availableMemory: GC.GetGCMemoryInfo().TotalAvailableMemoryBytes);
    }
}
