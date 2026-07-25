namespace MathVerse.Math.Numerics.Sparse;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public enum SparseSolverMethod
{
    LU,
    CG,
    BiCGSTAB,
    Auto
}

public static class SparseLinearSolver
{
    public static Vector SolveLU(SparseMatrix A, Vector b)
    {
        var lu = SparseLUDecomposition.Compute(A);
        return lu.Solve(b);
    }

    public static Vector SolveCG(SparseMatrix A, Vector b, double tol = 1e-12, int maxIter = 1000)
    {
        if (!A.IsSquare) throw new ArgumentException("Matrix must be square");
        int n = A.Rows;
        var x = new double[n];
        var r = new double[n];
        var p = new double[n];
        var Ap = new double[n];

        for (int i = 0; i < n; i++) r[i] = b[i];
        Array.Copy(r, p, n);

        double rsold = 0;
        for (int i = 0; i < n; i++) rsold += r[i] * r[i];

        for (int iter = 0; iter < maxIter; iter++)
        {
            var pVec = new Vector(p);
            var ApVec = A.Multiply(pVec);
            ApVec._values.CopyTo(Ap, 0);

            double pAp = 0;
            for (int i = 0; i < n; i++) pAp += p[i] * Ap[i];

            if (System.Math.Abs(pAp) < 1e-15) break;

            double alpha = rsold / pAp;
            for (int i = 0; i < n; i++)
            {
                x[i] += alpha * p[i];
                r[i] -= alpha * Ap[i];
            }

            double rsnew = 0;
            for (int i = 0; i < n; i++) rsnew += r[i] * r[i];

            if (System.Math.Sqrt(rsnew) < tol) break;

            double beta = rsnew / rsold;
            for (int i = 0; i < n; i++) p[i] = r[i] + beta * p[i];
            rsold = rsnew;
        }

        return new Vector(x.ToImmutableArray());
    }

    public static Vector SolveBiCGSTAB(SparseMatrix A, Vector b, double tol = 1e-12, int maxIter = 1000)
    {
        if (!A.IsSquare) throw new ArgumentException("Matrix must be square");
        int n = A.Rows;
        var x = new double[n];
        var r = new double[n];
        var r0 = new double[n];
        var p = new double[n];
        var v = new double[n];
        var s = new double[n];
        var t = new double[n];

        for (int i = 0; i < n; i++) r[i] = r0[i] = b[i];

        double rho = 1, alpha = 1, omega = 1;

        for (int iter = 0; iter < maxIter; iter++)
        {
            double rhoNew = 0;
            for (int i = 0; i < n; i++) rhoNew += r0[i] * r[i];

            if (System.Math.Abs(rhoNew) < 1e-15) break;

            double beta = (rhoNew / rho) * (alpha / omega);
            rho = rhoNew;

            for (int i = 0; i < n; i++) p[i] = r[i] + beta * (p[i] - omega * v[i]);

            var pVec = new Vector(p);
            var vVec = A.Multiply(pVec);
            vVec._values.CopyTo(v, 0);

            double r0v = 0;
            for (int i = 0; i < n; i++) r0v += r0[i] * v[i];
            if (System.Math.Abs(r0v) < 1e-15) break;

            alpha = rho / r0v;

            for (int i = 0; i < n; i++) s[i] = r[i] - alpha * v[i];

            var sVec = new Vector(s);
            var tVec = A.Multiply(sVec);
            tVec._values.CopyTo(t, 0);

            double st = 0, tt = 0;
            for (int i = 0; i < n; i++) { st += s[i] * t[i]; tt += t[i] * t[i]; }
            if (System.Math.Abs(tt) < 1e-15) { omega = 0; }
            else { omega = st / tt; }

            for (int i = 0; i < n; i++)
            {
                x[i] += alpha * p[i] + omega * s[i];
                r[i] = s[i] - omega * t[i];
            }

            double normR = 0;
            for (int i = 0; i < n; i++) normR += r[i] * r[i];
            if (System.Math.Sqrt(normR) < tol) break;
        }

        return new Vector(x.ToImmutableArray());
    }

    public static Vector Solve(SparseMatrix A, Vector b, SparseSolverMethod method)
    {
        return method switch
        {
            SparseSolverMethod.LU => SolveLU(A, b),
            SparseSolverMethod.CG => SolveCG(A, b),
            SparseSolverMethod.BiCGSTAB => SolveBiCGSTAB(A, b),
            SparseSolverMethod.Auto => A.IsSquare && A.Rows < 1000 ? SolveLU(A, b) : SolveCG(A, b),
            _ => throw new ArgumentException("Unknown solver method")
        };
    }
}