using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Foundation.Dimensions;

namespace MathVerse.Foundation.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class DimensionBenchmarks
{
    private Dimension _length = null!;
    private Dimension _mass = null!;
    private Dimension _time = null!;
    private Dimension _lengthTime = null!;
    private Dimension _velocity = null!;
    private Dimension _force = null!;
    private Dimension _energy = null!;
    private Dimension _area = null!;
    private Dimension _volume = null!;
    private Dimension _dimensionless = null!;

    [GlobalSetup]
    public void Setup()
    {
        _length = Dimension.FromBaseDimensions(length: 1);
        _mass = Dimension.FromBaseDimensions(mass: 1);
        _time = Dimension.FromBaseDimensions(time: 1);
        _lengthTime = Dimension.FromBaseDimensions(length: 1, time: 1);
        _velocity = DerivedDimension.Velocity;
        _force = DerivedDimension.Force;
        _energy = DerivedDimension.Energy;
        _area = DerivedDimension.Area;
        _volume = DerivedDimension.Volume;
        _dimensionless = Dimension.None;
    }

    [BenchmarkCategory("FromBase"), Benchmark(Baseline = true)]
    public Dimension FromBaseDimensions_Length()
    {
        return Dimension.FromBaseDimensions(length: 1);
    }

    [BenchmarkCategory("FromBase"), Benchmark]
    public Dimension FromBaseDimensions_Mass()
    {
        return Dimension.FromBaseDimensions(mass: 1);
    }

    [BenchmarkCategory("FromBase"), Benchmark]
    public Dimension FromBaseDimensions_Time()
    {
        return Dimension.FromBaseDimensions(time: 1);
    }

    [BenchmarkCategory("FromBase"), Benchmark]
    public Dimension FromBaseDimensions_Force()
    {
        return Dimension.FromBaseDimensions(mass: 1, length: 1, time: -2);
    }

    [BenchmarkCategory("FromBase"), Benchmark]
    public Dimension FromBaseDimensions_Voltage()
    {
        return Dimension.FromBaseDimensions(mass: 1, length: 2, time: -3, current: -1);
    }

    [BenchmarkCategory("Builder"), Benchmark]
    public Dimension Builder_Length()
    {
        return new DimensionBuilder().Length().Build();
    }

    [BenchmarkCategory("Builder"), Benchmark]
    public Dimension Builder_Mass_Time()
    {
        return new DimensionBuilder().Mass().Time().Build();
    }

    [BenchmarkCategory("Builder"), Benchmark]
    public Dimension Builder_Length_Mass_Time()
    {
        return new DimensionBuilder().Length().Mass().Time().Build();
    }

    [BenchmarkCategory("Builder"), Benchmark]
    public Dimension Builder_AllBase()
    {
        return new DimensionBuilder()
            .Length(1).Mass(1).Time(-1).Current(1)
            .Temperature(1).Substance(1).Luminous(1)
            .Build();
    }

    [BenchmarkCategory("Arithmetic"), Benchmark(Baseline = true)]
    public Dimension Multiply_Length_Time()
    {
        return _length.Multiply(_time);
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public Dimension Multiply_Mass_Acceleration()
    {
        return _mass.Multiply(DerivedDimension.Acceleration);
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public Dimension Multiply_Velocity_Time()
    {
        return _velocity.Multiply(_time);
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public Dimension Divide_Energy_Length()
    {
        return _energy.Divide(_length);
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public Dimension Divide_Velocity_Time()
    {
        return _velocity.Divide(_time);
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public Dimension Power_Area()
    {
        return _length.Power(2);
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public Dimension Power_Volume()
    {
        return _length.Power(3);
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public Dimension Root_SquareArea()
    {
        return _area.Root(2);
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public Dimension Root_CubeVolume()
    {
        return _volume.Root(3);
    }

    [BenchmarkCategory("Derived"), Benchmark]
    public Dimension Derived_Velocity()
    {
        return DerivedDimension.Velocity;
    }

    [BenchmarkCategory("Derived"), Benchmark]
    public Dimension Derived_Force()
    {
        return DerivedDimension.Force;
    }

    [BenchmarkCategory("Derived"), Benchmark]
    public Dimension Derived_Energy()
    {
        return DerivedDimension.Energy;
    }

    [BenchmarkCategory("Derived"), Benchmark]
    public Dimension Derived_Power()
    {
        return DerivedDimension.Power;
    }

    [BenchmarkCategory("Derived"), Benchmark]
    public Dimension Derived_Pressure()
    {
        return DerivedDimension.Pressure;
    }

    [BenchmarkCategory("Derived"), Benchmark]
    public Dimension Derived_Voltage()
    {
        return DerivedDimension.Voltage;
    }

    [BenchmarkCategory("Derived"), Benchmark]
    public Dimension Derived_Resistance()
    {
        return DerivedDimension.Resistance;
    }

    [BenchmarkCategory("Derived"), Benchmark]
    public Dimension Derived_Capacitance()
    {
        return DerivedDimension.Capacitance;
    }

    [BenchmarkCategory("DerivedStatic"), Benchmark]
    public Dimension Static_Multiply()
    {
        return DerivedDimension.Multiply(_mass, DerivedDimension.Acceleration);
    }

    [BenchmarkCategory("DerivedStatic"), Benchmark]
    public Dimension Static_Divide()
    {
        return DerivedDimension.Divide(_energy, _time);
    }

    [BenchmarkCategory("DerivedStatic"), Benchmark]
    public Dimension Static_Create()
    {
        return DerivedDimension.Create(_length, 3);
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public bool IsDimensionless_DimensionNone()
    {
        return _dimensionless.IsDimensionless;
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public bool IsDimensionless_Length()
    {
        return _length.IsDimensionless;
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public bool IsBaseDimension_Length()
    {
        return _length.IsBaseDimension;
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public bool IsBaseDimension_Velocity()
    {
        return _velocity.IsBaseDimension;
    }

    [BenchmarkCategory("Comparison"), Benchmark(Baseline = true)]
    public bool IsCompatibleWith_Length_Length()
    {
        return _length.IsCompatibleWith(_length);
    }

    [BenchmarkCategory("Comparison"), Benchmark]
    public bool IsCompatibleWith_Length_Mass()
    {
        return _length.IsCompatibleWith(_mass);
    }

    [BenchmarkCategory("Comparison"), Benchmark]
    public bool IsCompatibleWith_Velocity_Velocity()
    {
        return DerivedDimension.Velocity.IsCompatibleWith(DerivedDimension.Velocity);
    }

    [BenchmarkCategory("Comparison"), Benchmark]
    public bool Equals_Length_Length()
    {
        return _length.Equals(Dimension.FromBaseDimensions(length: 1));
    }

    [BenchmarkCategory("Comparison"), Benchmark]
    public bool Equals_Length_Mass()
    {
        return _length.Equals(_mass);
    }

    [BenchmarkCategory("Comparison"), Benchmark]
    public bool Equals_Energy_Energy()
    {
        return _energy.Equals(DerivedDimension.Energy);
    }

    [BenchmarkCategory("ToString"), Benchmark]
    public string ToString_Length()
    {
        return _length.ToString();
    }

    [BenchmarkCategory("ToString"), Benchmark]
    public string ToString_Velocity()
    {
        return _velocity.ToString();
    }

    [BenchmarkCategory("ToString"), Benchmark]
    public string ToString_Force()
    {
        return _force.ToString();
    }

    [BenchmarkCategory("ToString"), Benchmark]
    public string ToString_Dimensionless()
    {
        return _dimensionless.ToString();
    }
}
