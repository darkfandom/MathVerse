namespace MathVerse.Math.Quantum.Simulation;

using System;
using System.Text;
using States;

/// <summary>
/// Clifford-circuit simulator using the Gottesman-Knill stabilizer tableau formalism.
/// Supports only Pauli-X, Pauli-Z, Hadamard, and CNOT gates, which can be efficiently
/// simulated classically in O(n²) time per gate.
/// </summary>
public sealed class StabilizerSimulator
{
    private readonly int _numQubits;
    private readonly byte[,] _xTable;
    private readonly byte[,] _zTable;
    private readonly byte[] _phases;

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Creates a stabilizer simulator initialized to the |0...0⟩ state.</summary>
    public StabilizerSimulator(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        _numQubits = numQubits;
        _xTable = new byte[numQubits, numQubits];
        _zTable = new byte[numQubits, numQubits];
        _phases = new byte[numQubits];

        for (int i = 0; i < numQubits; i++)
            _zTable[i, i] = 1;
    }

    /// <summary>
    /// Applies the Pauli-X gate to the specified qubit.
    /// </summary>
    public void ApplyPauliX(int qubit)
    {
        ValidateQubit(qubit);
        for (int i = 0; i < _numQubits; i++)
        {
            if (_zTable[i, qubit] == 1)
                _phases[i] ^= 1;
        }
    }

    /// <summary>
    /// Applies the Pauli-Z gate to the specified qubit.
    /// </summary>
    public void ApplyPauliZ(int qubit)
    {
        ValidateQubit(qubit);
        for (int i = 0; i < _numQubits; i++)
        {
            if (_xTable[i, qubit] == 1)
                _phases[i] ^= 1;
        }
    }

    /// <summary>
    /// Applies the Hadamard gate to the specified qubit.
    /// </summary>
    public void ApplyHadamard(int qubit)
    {
        ValidateQubit(qubit);
        for (int i = 0; i < _numQubits; i++)
        {
            byte oldX = _xTable[i, qubit];
            byte oldZ = _zTable[i, qubit];
            _xTable[i, qubit] = oldZ;
            _zTable[i, qubit] = oldX;
            if (oldX == 1 && oldZ == 1)
                _phases[i] ^= 1;
        }
    }

    /// <summary>
    /// Applies the CNOT gate with the specified control and target qubits.
    /// </summary>
    public void ApplyCNOT(int control, int target)
    {
        ValidateQubit(control);
        ValidateQubit(target);
        for (int i = 0; i < _numQubits; i++)
        {
            _xTable[i, target] ^= _xTable[i, control];
            _zTable[i, control] ^= _zTable[i, target];
        }
    }

    /// <summary>
    /// Gets the current stabilizer state.
    /// </summary>
    public StabilizerState GetState()
    {
        var generators = new string[_numQubits];
        for (int i = 0; i < _numQubits; i++)
        {
            var sb = new StringBuilder(_numQubits + 1);
            sb.Append(_phases[i] == 0 ? '+' : '-');
            for (int j = 0; j < _numQubits; j++)
            {
                bool x = _xTable[i, j] == 1;
                bool z = _zTable[i, j] == 1;
                sb.Append((x, z) switch
                {
                    (false, false) => 'I',
                    (true, false) => 'X',
                    (false, true) => 'Z',
                    (true, true) => 'Y'
                });
            }
            generators[i] = sb.ToString();
        }
        return new StabilizerState(generators);
    }

    /// <summary>
    /// Samples a deterministic or random bit string from the current stabilizer state.
    /// </summary>
    public bool[] Sample()
    {
        var result = new bool[_numQubits];
        var rng = new Random(42);

        var xCopy = new byte[_numQubits, _numQubits];
        var zCopy = new byte[_numQubits, _numQubits];
        var phaseCopy = new byte[_numQubits];
        Array.Copy(_xTable, xCopy, _xTable.Length);
        Array.Copy(_zTable, zCopy, _zTable.Length);
        Array.Copy(_phases, phaseCopy, _phases.Length);

        for (int q = 0; q < _numQubits; q++)
        {
            int antiRow = -1;
            for (int i = 0; i < _numQubits; i++)
            {
                if (xCopy[i, q] == 1)
                {
                    antiRow = i;
                    break;
                }
            }

            bool bit;
            if (antiRow == -1)
            {
                bit = false;
                for (int i = 0; i < _numQubits; i++)
                {
                    if (zCopy[i, q] == 1)
                    {
                        bit = phaseCopy[i] == 1;
                        break;
                    }
                }
            }
            else
            {
                bit = rng.Next(2) == 1;
                int zRow = -1;
                for (int i = 0; i < _numQubits; i++)
                {
                    if (zCopy[i, q] == 1 && i != antiRow)
                    {
                        zRow = i;
                        break;
                    }
                }

                if (zRow != -1)
                {
                    for (int j = 0; j < _numQubits; j++)
                    {
                        (xCopy[antiRow, j], xCopy[zRow, j]) = (xCopy[zRow, j], xCopy[antiRow, j]);
                        (zCopy[antiRow, j], zCopy[zRow, j]) = (zCopy[zRow, j], zCopy[antiRow, j]);
                    }
                    (phaseCopy[antiRow], phaseCopy[zRow]) = (phaseCopy[zRow], phaseCopy[antiRow]);
                }

                for (int j = 0; j < _numQubits; j++)
                {
                    xCopy[antiRow, j] = 0;
                    zCopy[antiRow, j] = 0;
                }
                zCopy[antiRow, q] = 1;
                phaseCopy[antiRow] = bit ? (byte)1 : (byte)0;
            }

            result[q] = bit;
        }

        return result;
    }

    private void ValidateQubit(int qubit)
    {
        if (qubit < 0 || qubit >= _numQubits)
            throw new ArgumentOutOfRangeException(nameof(qubit));
    }
}
