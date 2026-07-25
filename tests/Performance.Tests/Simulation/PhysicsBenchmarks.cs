namespace MathVerse.Performance.Tests.Simulation;

using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using MathVerse.Math.Numerics.LinearAlgebra;
using MathVerse.Math.Simulation.Physics;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

[MemoryDiagnoser]
public class PhysicsBenchmarks
{
    private Particle _p1 = null!, _p2 = null!;
    private PhysicsState _state = null!;
    private PhysicsState _largeState = null!;

    [GlobalSetup]
    public void Setup()
    {
        _p1 = Particle.Create("p1", MVVector.ZeroOf(3), MVVector.ZeroOf(3), 1.0);
        _p2 = Particle.Create("p2", new MVVector(new double[] { 1.0, 0.0, 0.0 }), MVVector.ZeroOf(3), 1.0);

        var particles = ImmutableDictionary<string, Particle>.Empty;
        for (int i = 0; i < 10; i++)
        {
            var p = Particle.Create($"p{i}", new MVVector(new double[] { i * 0.1, 0, 0 }), MVVector.ZeroOf(3), 1.0);
            particles = particles.Add(p.Id, p);
        }
        _state = PhysicsState.Create(new MVVector(new double[] { 0, -9.81, 0 })) with { Particles = particles };

        var largeParticles = ImmutableDictionary<string, Particle>.Empty;
        for (int i = 0; i < 100; i++)
        {
            var p = Particle.Create($"p{i}", new MVVector(new double[] { i * 0.1, i * 0.05, 0 }), new MVVector(new double[] { System.Math.Sin(i), System.Math.Cos(i), 0 }), 1.0);
            largeParticles = largeParticles.Add(p.Id, p);
        }
        _largeState = PhysicsState.Create(new MVVector(new double[] { 0, -9.81, 0 })) with { Particles = largeParticles };
    }

    [Benchmark] public PhysicsState PhysicsStep_SingleParticle() => PhysicsEngine.Step(_state with { Particles = ImmutableDictionary<string, Particle>.Empty.Add("p1", _p1) }, 0.01);
    [Benchmark] public PhysicsState PhysicsStep_TenParticles() => PhysicsEngine.Step(_state, 0.01);
    [Benchmark] public PhysicsState PhysicsStep_HundredParticles() => PhysicsEngine.Step(_largeState, 0.01);
    [Benchmark] public MVVector ComputeGravitationalForce_Close() => PhysicsEngine.ComputeGravitationalForce(_p1, _p2);
    [Benchmark] public MVVector ComputeGravitationalForce_Far()
    {
        var far = Particle.Create("far", new MVVector(new double[] { 100.0, 0, 0 }), MVVector.ZeroOf(3), 1.0);
        return PhysicsEngine.ComputeGravitationalForce(_p1, far);
    }
    [Benchmark] public MVVector ComputeSpringForce_Compressed() => PhysicsEngine.ComputeSpringForce(_p1, _p2, 2.0, 100.0);
    [Benchmark] public MVVector ComputeSpringForce_Extended() => PhysicsEngine.ComputeSpringForce(_p1, _p2, 0.5, 100.0);
    [Benchmark] public MVVector ComputeSpringForce_AtRest() => PhysicsEngine.ComputeSpringForce(_p1, _p2, 1.0, 100.0);
    [Benchmark] public double Particle_KineticEnergy()
    {
        var p = Particle.Create("p1", MVVector.ZeroOf(3), new MVVector(new double[] { 3.0, 4.0, 5.0 }), 2.0);
        return p.KineticEnergy;
    }
    [Benchmark] public MVVector Particle_Momentum()
    {
        var p = Particle.Create("p1", MVVector.ZeroOf(3), new MVVector(new double[] { 1.0, 2.0, 3.0 }), 5.0);
        return p.Momentum;
    }
    [Benchmark] public MVVector Particle_NetForce_NoForces() => PhysicsEngine.ComputeNetForce(_p1);
    [Benchmark] public MVVector Particle_NetForce_MultipleForces()
    {
        var p = Particle.Create("p1", MVVector.ZeroOf(3), MVVector.ZeroOf(3), 1.0) with
        {
            Forces = ImmutableArray.Create(
                Force.GravityForce(1.0, -9.81),
                Force.SpringForce(new MVVector(new double[] { 0.5, 0, 0 }), 100.0))
        };
        return PhysicsEngine.ComputeNetForce(p);
    }
    [Benchmark] public Force Force_GravityFactory() => Force.GravityForce(10.0, -9.81);
    [Benchmark] public Force Force_SpringFactory() => Force.SpringForce(new MVVector(new double[] { 0.5 }), 100.0, 1.0);
    [Benchmark] public Force Force_DragFactory() => Force.DragForce(new MVVector(new double[] { 10.0, 0, 0 }), 0.5, 1.0);
    [Benchmark] public Constraint Constraint_Distance() => Constraint.Distance("c1", "p1", "p2", 5.0);
    [Benchmark] public Constraint Constraint_Fixed() => Constraint.Fixed("c1", "p1", MVVector.ZeroOf(3));
    [Benchmark] public PhysicsState PhysicsState_Create() => PhysicsState.Create(new MVVector(new double[] { 0, -9.81, 0 }));
    [Benchmark] public PhysicsState PhysicsState_WithGravity() => _state with { Gravity = new MVVector(new double[] { 0, -10.0, 0 }) };
    [Benchmark] public Particle Particle_Create_Benchmark() => Particle.Create("p1", new MVVector(new double[] { 1, 2, 3 }), new MVVector(new double[] { 4, 5, 6 }), 10.0, 0.5);
    [Benchmark] public PhysicsState LargeScale_PhysicsStep() => PhysicsEngine.Step(_largeState, 0.001);
    [Benchmark] public PhysicsState PhysicsStep_RepeatedSteps()
    {
        var s = _state;
        for (int i = 0; i < 10; i++) s = PhysicsEngine.Step(s, 0.01);
        return s;
    }
}
