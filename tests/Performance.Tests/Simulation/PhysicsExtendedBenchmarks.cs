namespace MathVerse.Performance.Tests.Simulation;

using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using MathVerse.Math.Numerics.LinearAlgebra;
using MathVerse.Math.Simulation.Physics;
using MathVerse.Math.Simulation.Chemistry;
using MathVerse.Math.Simulation.Biology;
using MathVerse.Math.Simulation.Finance;
using MathVerse.Math.Simulation.Thermodynamics;
using MathVerse.Math.Simulation.Electromagnetics;
using MathVerse.Math.Simulation.FluidDynamics;
using MathVerse.Math.Simulation.Visualization;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

[MemoryDiagnoser]
public class PhysicsExtendedBenchmarks
{
    private Particle _p1 = null!;
    private Particle _p2 = null!;
    private Particle _heavyP1 = null!;
    private Particle _heavyP2 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _p1 = Particle.Create("p1", MVVector.ZeroOf(3), MVVector.ZeroOf(3), 1.0);
        _p2 = Particle.Create("p2", new MVVector(new double[] { 1.0, 0, 0 }), MVVector.ZeroOf(3), 1.0);
        _heavyP1 = Particle.Create("h1", MVVector.ZeroOf(3), MVVector.ZeroOf(3), 1e10);
        _heavyP2 = Particle.Create("h2", new MVVector(new double[] { 1.0, 0, 0 }), MVVector.ZeroOf(3), 1e10);
    }

    [Benchmark] public PhysicsState Step_MassiveParticles()
    {
        var p = PhysicsState.Create(new MVVector(new double[] { 0, -9.81, 0 })) with
        {
            Particles = ImmutableDictionary<string, Particle>.Empty.Add("h1", _heavyP1).Add("h2", _heavyP2)
        };
        return PhysicsEngine.Step(p, 0.001);
    }
    [Benchmark] public MVVector Gravity_MassiveBodies() => PhysicsEngine.ComputeGravitationalForce(_heavyP1, _heavyP2);
    [Benchmark] public MVVector Gravity_IdenticalBodies() => PhysicsEngine.ComputeGravitationalForce(_p1, _p2);
    [Benchmark] public MVVector Spring_HighStiffness() => PhysicsEngine.ComputeSpringForce(_p1, _p2, 1.0, 10000.0);
    [Benchmark] public MVVector Spring_LowStiffness() => PhysicsEngine.ComputeSpringForce(_p1, _p2, 1.0, 0.01);
    [Benchmark] public MVVector Gravity_SamePosition() => PhysicsEngine.ComputeGravitationalForce(_p1, _p1);
    [Benchmark] public double Particle_KineticEnergy_Zero() => _p1.KineticEnergy;
    [Benchmark] public MVVector Particle_Momentum_Zero() => _p1.Momentum;
    [Benchmark] public Particle Particle_Create_WithForces()
    {
        return Particle.Create("pf", new MVVector(new double[] { 1, 2, 3 }), MVVector.ZeroOf(3), 5.0) with
        {
            Forces = ImmutableArray.Create(Force.GravityForce(5.0, -9.81))
        };
    }
    [Benchmark] public MVVector NetForce_SingleForce()
    {
        var p = Particle.Create("p", MVVector.ZeroOf(3), MVVector.ZeroOf(3), 1.0) with
        {
            Forces = ImmutableArray.Create(Force.GravityForce(1.0, -9.81))
        };
        return PhysicsEngine.ComputeNetForce(p);
    }
    [Benchmark] public Force Drag_HighVelocity() => Force.DragForce(new MVVector(new double[] { 100.0, 0, 0 }), 0.5, 1.0);
    [Benchmark] public Force Drag_LowVelocity() => Force.DragForce(new MVVector(new double[] { 0.01, 0, 0 }), 0.5, 1.0);
    [Benchmark] public Force Spring_Displacement() => Force.SpringForce(new MVVector(new double[] { 2.0 }), 500.0, 1.0);
    [Benchmark] public Constraint Hinge()
    {
        return Constraint.Hinge("h1", "p1", "p2",
            MVVector.ZeroOf(3), new MVVector(new double[] { 1.0, 0, 0 }), new MVVector(new double[] { 0, 0, 1 }));
    }
    [Benchmark] public Constraint Slider()
    {
        return new Constraint { Id = "s1", Type = ConstraintType.Slider, ParticleIds = ImmutableArray.Create("p1", "p2"), RestLength = 1.0 };
    }
    [Benchmark] public RigidBody RigidBody_Create_Full()
    {
        return new RigidBody
        {
            Id = "rb1",
            Position = new MVVector(new double[] { 1, 2, 3 }),
            Velocity = new MVVector(new double[] { 0.1, 0.2, 0.3 }),
            AngularVelocity = new MVVector(new double[] { 0, 0, 1 }),
            Mass = 10.0
        };
    }
    [Benchmark] public Torque Torque_Create()
    {
        return new Torque { Vector = new MVVector(new double[] { 0, 0, 5.0 }), Source = "motor" };
    }
    [Benchmark] public PhysicsState Step_SmallTimestep()
    {
        var p = PhysicsState.Create(new MVVector(new double[] { 0, -9.81, 0 })) with
        {
            Particles = ImmutableDictionary<string, Particle>.Empty.Add("p1", _p1)
        };
        return PhysicsEngine.Step(p, 1e-6);
    }
    [Benchmark] public PhysicsState Step_LargeTimestep()
    {
        var p = PhysicsState.Create(new MVVector(new double[] { 0, -9.81, 0 })) with
        {
            Particles = ImmutableDictionary<string, Particle>.Empty.Add("p1", _p1)
        };
        return PhysicsEngine.Step(p, 1.0);
    }
    [Benchmark] public PhysicsState Step_WithGravity()
    {
        var p = PhysicsState.Create(new MVVector(new double[] { 0, -9.81, 0 })) with
        {
            Particles = ImmutableDictionary<string, Particle>.Empty.Add("p1", _p1)
        };
        return PhysicsEngine.Step(p, 0.01);
    }
    [Benchmark] public PhysicsState Step_NoGravity()
    {
        var p = PhysicsState.Create(MVVector.ZeroOf(3)) with
        {
            Particles = ImmutableDictionary<string, Particle>.Empty.Add("p1", _p1)
        };
        return PhysicsEngine.Step(p, 0.01);
    }
    [Benchmark] public bool Particle_IsFixed()
    {
        var p = Particle.Create("p1", MVVector.ZeroOf(3), MVVector.ZeroOf(3), 1.0) with { IsFixed = true };
        return p.IsFixed;
    }
    [Benchmark] public double Particle_Charge() => _p1.Charge;
    [Benchmark] public double Particle_Radius() => _p1.Radius;
    [Benchmark] public int ConstraintType_Count() => Enum.GetValues<ConstraintType>().Length;
    [Benchmark] public int ForceType_Count() => Enum.GetValues<ForceType>().Length;
}
