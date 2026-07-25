namespace MathVerse.Math.Compiler.Integration;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MathVerse.Math.Compiler.Scientific;

/// <summary>Bridges the Compiler module with all other MathVerse math modules.
/// Provides static methods to register compilation strategies for each domain.</summary>
public static class CompilerIntegrationBridge
{
    private static readonly ConcurrentDictionary<string, ScientificCompilerBase> _registry = new();
    private static volatile bool _initialized;

    /// <summary>Gets the compiler registry containing all registered domain compilers.</summary>
    public static IReadOnlyDictionary<string, ScientificCompilerBase> CompilerRegistry => _registry;

    /// <summary>Initializes the bridge and registers all scientific compilers.</summary>
    public static void Initialize()
    {
        if (_initialized) return;
        lock (_registry)
        {
            if (_initialized) return;
            RegisterCAS();
            RegisterNumerics();
            RegisterGeometry();
            RegisterSimulation();
            RegisterQuantum();
            RegisterAI();
            RegisterVisualization();
            RegisterDistributed();
            RegisterDataScience();
            RegisterInterop();
            _initialized = true;
        }
    }

    /// <summary>Registers the CAS compiler.</summary>
    public static void RegisterCAS()
    {
        var compiler = new CASCompiler();
        _registry[compiler.DomainName] = compiler;
    }

    /// <summary>Registers the Numerics compiler.</summary>
    public static void RegisterNumerics()
    {
        var compiler = new NumericsCompiler();
        _registry[compiler.DomainName] = compiler;
    }

    /// <summary>Registers the Geometry compiler.</summary>
    public static void RegisterGeometry()
    {
        var compiler = new GeometryCompiler();
        _registry[compiler.DomainName] = compiler;
    }

    /// <summary>Registers the Simulation compiler.</summary>
    public static void RegisterSimulation()
    {
        var compiler = new SimulationCompiler();
        _registry[compiler.DomainName] = compiler;
    }

    /// <summary>Registers the Quantum compiler.</summary>
    public static void RegisterQuantum()
    {
        var compiler = new QuantumCompiler();
        _registry[compiler.DomainName] = compiler;
    }

    /// <summary>Registers the AI compiler.</summary>
    public static void RegisterAI()
    {
        var compiler = new AICompiler();
        _registry[compiler.DomainName] = compiler;
    }

    /// <summary>Registers the Visualization compiler.</summary>
    public static void RegisterVisualization()
    {
        var compiler = new VisualizationCompiler();
        _registry[compiler.DomainName] = compiler;
    }

    /// <summary>Registers the Distributed compiler.</summary>
    public static void RegisterDistributed()
    {
        var compiler = new DistributedCompiler();
        _registry[compiler.DomainName] = compiler;
    }

    /// <summary>Registers the DataScience compiler.</summary>
    public static void RegisterDataScience()
    {
        var compiler = new DataScienceCompiler();
        _registry[compiler.DomainName] = compiler;
    }

    /// <summary>Registers the Interop compiler.</summary>
    public static void RegisterInterop()
    {
        var compiler = new InteropCompiler();
        _registry[compiler.DomainName] = compiler;
    }

    /// <summary>Gets a registered compiler by domain name.</summary>
    /// <param name="domainName">The domain name (e.g., "CAS", "Numerics").</param>
    /// <returns>The registered compiler, or null if not found.</returns>
    public static ScientificCompilerBase? GetCompiler(string domainName)
    {
        if (domainName is null) throw new ArgumentNullException(nameof(domainName));
        _registry.TryGetValue(domainName, out var compiler);
        return compiler;
    }

    /// <summary>Gets whether the bridge has been initialized.</summary>
    public static bool IsInitialized => _initialized;
}
