namespace MathVerse.Math.Compiler.Configuration;

using System;

public enum OptimizationLevel
{
    None,
    Basic,
    Aggressive
}

public enum CompilationTargetType
{
    Generic,
    Numerics,
    Geometry,
    Simulation,
    AI,
    Quantum,
    Visualization,
    DataPipeline,
    CAS
}
