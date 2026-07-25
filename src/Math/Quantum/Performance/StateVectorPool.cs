using System.Buffers;
using System.Numerics;

namespace MathVerse.Math.Quantum.Performance;

/// <summary>
/// Provides pooled state vectors sized for quantum register dimensions.
/// </summary>
public sealed class StateVectorPool
{
    private readonly ArrayPool<Complex> _pool;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateVectorPool"/> class.
    /// </summary>
    public StateVectorPool()
    {
        _pool = ArrayPool<Complex>.Shared;
    }

    /// <summary>
    /// Rents a state vector with capacity for the specified number of qubits.
    /// </summary>
    /// <param name="numQubits">The number of qubits (vector size will be 2^numQubits).</param>
    /// <returns>A complex array of length 2^numQubits.</returns>
    public Complex[] RentStateVector(int numQubits)
    {
        int size = 1 << numQubits;
        return _pool.Rent(size);
    }

    /// <summary>
    /// Returns a previously rented state vector to the pool.
    /// </summary>
    /// <param name="stateVector">The state vector to return.</param>
    public void ReturnStateVector(Complex[] stateVector)
    {
        if (stateVector != null)
        {
            _pool.Return(stateVector, clearArray: true);
        }
    }
}
