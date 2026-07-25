namespace MathVerse.Math.Quantum.Chemistry;

using System;
using System.Collections.Generic;
using LinearAlgebra;

/// <summary>
/// Builder for molecular Hamiltonians from one-electron and two-electron integrals,
/// producing a qubit Hamiltonian via the Jordan-Wigner transformation.
/// </summary>
public sealed class MolecularHamiltonian
{
    private readonly int _numQubits;
    private readonly Dictionary<(int, int), double> _oneElectronIntegrals = new();
    private readonly Dictionary<(int, int, int, int), double> _twoElectronIntegrals = new();
    private double _nuclearRepulsion;

    /// <summary>Gets the number of qubits (orbitals) for this molecular Hamiltonian.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Creates a molecular Hamiltonian for the specified number of qubits (spatial orbitals).</summary>
    /// <param name="numQubits">The number of spatial orbitals.</param>
    public MolecularHamiltonian(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        _numQubits = numQubits;
    }

    /// <summary>Adds a one-electron integral hᵢⱼ to the molecular Hamiltonian.</summary>
    /// <param name="i">First orbital index.</param>
    /// <param name="j">Second orbital index.</param>
    /// <param name="value">The integral value.</param>
    public void AddOneElectronIntegral(int i, int j, double value)
    {
        if (i < 0 || i >= _numQubits) throw new ArgumentOutOfRangeException(nameof(i));
        if (j < 0 || j >= _numQubits) throw new ArgumentOutOfRangeException(nameof(j));
        _oneElectronIntegrals[(i, j)] = value;
    }

    /// <summary>Adds a two-electron integral (ij|kl) to the molecular Hamiltonian.</summary>
    /// <param name="i">First orbital index.</param>
    /// <param name="j">Second orbital index.</param>
    /// <param name="k">Third orbital index.</param>
    /// <param name="l">Fourth orbital index.</param>
    /// <param name="value">The integral value.</param>
    public void AddTwoElectronIntegral(int i, int j, int k, int l, double value)
    {
        if (i < 0 || i >= _numQubits) throw new ArgumentOutOfRangeException(nameof(i));
        if (j < 0 || j >= _numQubits) throw new ArgumentOutOfRangeException(nameof(j));
        if (k < 0 || k >= _numQubits) throw new ArgumentOutOfRangeException(nameof(k));
        if (l < 0 || l >= _numQubits) throw new ArgumentOutOfRangeException(nameof(l));
        _twoElectronIntegrals[(i, j, k, l)] = value;
    }

    /// <summary>Sets the nuclear repulsion energy.</summary>
    /// <param name="energy">The nuclear repulsion energy value.</param>
    public void SetNuclearRepulsion(double energy)
    {
        _nuclearRepulsion = energy;
    }

    /// <summary>Builds the full qubit Hamiltonian using Jordan-Wigner transformation.</summary>
    /// <returns>A <see cref="Hamiltonian"/> representing the molecular Hamiltonian in the qubit basis.</returns>
    public Hamiltonian BuildHamiltonian()
    {
        var hamiltonian = new Hamiltonian(_numQubits);

        var identityMatrix = ComplexMatrix.Identity(1 << _numQubits);
        double constantTerm = _nuclearRepulsion;

        foreach (var kvp in _oneElectronIntegrals)
        {
            int i = kvp.Key.Item1;
            int j = kvp.Key.Item2;
            double value = kvp.Value;

            ComplexMatrix ni = FermionicOperator.Number(i, _numQubits);
            hamiltonian.AddTerm(value, ni);

            if (i != j)
            {
                ComplexMatrix niNj = ni.Multiply(FermionicOperator.Number(j, _numQubits));
                hamiltonian.AddTerm(-0.25 * value, niNj);
            }
        }

        foreach (var kvp in _twoElectronIntegrals)
        {
            int i = kvp.Key.Item1, j = kvp.Key.Item2, k = kvp.Key.Item3, l = kvp.Key.Item4;
            double value = kvp.Value;

            ComplexMatrix ni = FermionicOperator.Number(i, _numQubits);
            ComplexMatrix nj = FermionicOperator.Number(j, _numQubits);
            ComplexMatrix nk = FermionicOperator.Number(k, _numQubits);
            ComplexMatrix nl = FermionicOperator.Number(l, _numQubits);

            ComplexMatrix term = ni.Multiply(nj).Multiply(nk).Multiply(nl);
            hamiltonian.AddTerm(0.125 * value, term);
        }

        hamiltonian.AddTerm(1.0, identityMatrix.Scale(new System.Numerics.Complex(constantTerm, 0.0)));

        return hamiltonian;
    }

    /// <summary>Computes the classical energy: nuclear repulsion + one-electron contributions.</summary>
    /// <returns>The classical energy value.</returns>
    public double ComputeClassicalEnergy()
    {
        double energy = _nuclearRepulsion;
        foreach (var kvp in _oneElectronIntegrals)
        {
            if (kvp.Key.Item1 == kvp.Key.Item2)
                energy += 0.5 * kvp.Value;
        }
        return energy;
    }
}
