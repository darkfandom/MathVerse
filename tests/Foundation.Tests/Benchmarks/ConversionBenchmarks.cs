using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Foundation.Conversion;

namespace MathVerse.Foundation.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ConversionBenchmarks
{
    private ConversionGraph _graph = null!;
    private UnitConverter _converter = null!;

    [GlobalSetup]
    public void Setup()
    {
        _graph = ConversionGraph.Instance;
        _converter = UnitConverter.Instance;

        _graph.AddRule(new ConversionRule
        {
            From = "km",
            To = "m",
            Converter = v => v * 1000.0,
            IsExact = true,
            Description = "km to m"
        });

        _graph.AddRule(new ConversionRule
        {
            From = "m",
            To = "cm",
            Converter = v => v * 100.0,
            IsExact = true,
            Description = "m to cm"
        });

        _graph.AddRule(new ConversionRule
        {
            From = "cm",
            To = "mm",
            Converter = v => v * 10.0,
            IsExact = true,
            Description = "cm to mm"
        });

        _graph.AddRule(new ConversionRule
        {
            From = "kg",
            To = "g",
            Converter = v => v * 1000.0,
            IsExact = true,
            Description = "kg to g"
        });

        _graph.AddRule(new ConversionRule
        {
            From = "g",
            To = "mg",
            Converter = v => v * 1000.0,
            IsExact = true,
            Description = "g to mg"
        });

        _graph.AddRule(new ConversionRule
        {
            From = "min",
            To = "s",
            Converter = v => v * 60.0,
            IsExact = true,
            Description = "min to s"
        });

        _graph.AddRule(new ConversionRule
        {
            From = "h",
            To = "min",
            Converter = v => v * 60.0,
            IsExact = true,
            Description = "h to min"
        });

        _graph.AddRule(new ConversionRule
        {
            From = "kWh",
            To = "J",
            Converter = v => v * 3600000.0,
            IsExact = true,
            Description = "kWh to J"
        });

        _graph.AddRule(new ConversionRule
        {
            From = "cal",
            To = "J",
            Converter = v => v * 4.184,
            IsExact = true,
            Description = "cal to J"
        });

        _graph.AddRule(new ConversionRule
        {
            From = "l",
            To = "ml",
            Converter = v => v * 1000.0,
            IsExact = true,
            Description = "l to ml"
        });

        _graph.AddRule(new ConversionRule
        {
            From = "atm",
            To = "Pa",
            Converter = v => v * 101325.0,
            IsExact = true,
            Description = "atm to Pa"
        });

        _graph.AddRule(new ConversionRule
        {
            From = "bar",
            To = "Pa",
            Converter = v => v * 100000.0,
            IsExact = true,
            Description = "bar to Pa"
        });
    }

    [BenchmarkCategory("Convert"), Benchmark(Baseline = true)]
    public ConversionResult Convert_km_to_m()
    {
        return _graph.Convert(1.0, "km", "m");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_m_to_cm()
    {
        return _graph.Convert(1.0, "m", "cm");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_cm_to_mm()
    {
        return _graph.Convert(1.0, "cm", "mm");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_km_to_cm()
    {
        return _graph.Convert(1.0, "km", "cm");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_km_to_mm()
    {
        return _graph.Convert(1.0, "km", "mm");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_kg_to_g()
    {
        return _graph.Convert(1.0, "kg", "g");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_g_to_mg()
    {
        return _graph.Convert(1.0, "g", "mg");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_kg_to_mg()
    {
        return _graph.Convert(1.0, "kg", "mg");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_min_to_s()
    {
        return _graph.Convert(1.0, "min", "s");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_h_to_s()
    {
        return _graph.Convert(1.0, "h", "s");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_kWh_to_J()
    {
        return _graph.Convert(1.0, "kWh", "J");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_cal_to_J()
    {
        return _graph.Convert(1.0, "cal", "J");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_atm_to_Pa()
    {
        return _graph.Convert(1.0, "atm", "Pa");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_bar_to_Pa()
    {
        return _graph.Convert(1.0, "bar", "Pa");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_SameUnit()
    {
        return _graph.Convert(42.0, "m", "m");
    }

    [BenchmarkCategory("Convert"), Benchmark]
    public ConversionResult Convert_NotFound()
    {
        return _graph.Convert(1.0, "m", "kg");
    }

    [BenchmarkCategory("FindPath"), Benchmark(Baseline = true)]
    public ConversionPath? FindPath_km_m()
    {
        return _graph.FindPath("km", "m");
    }

    [BenchmarkCategory("FindPath"), Benchmark]
    public ConversionPath? FindPath_km_cm()
    {
        return _graph.FindPath("km", "cm");
    }

    [BenchmarkCategory("FindPath"), Benchmark]
    public ConversionPath? FindPath_km_mm()
    {
        return _graph.FindPath("km", "mm");
    }

    [BenchmarkCategory("FindPath"), Benchmark]
    public ConversionPath? FindPath_h_s()
    {
        return _graph.FindPath("h", "s");
    }

    [BenchmarkCategory("FindPath"), Benchmark]
    public ConversionPath? FindPath_kg_mg()
    {
        return _graph.FindPath("kg", "mg");
    }

    [BenchmarkCategory("FindPath"), Benchmark]
    public ConversionPath? FindPath_NotFound()
    {
        return _graph.FindPath("m", "kg");
    }

    [BenchmarkCategory("CanConvert"), Benchmark]
    public bool CanConvert_km_m()
    {
        return _graph.CanConvert("km", "m");
    }

    [BenchmarkCategory("CanConvert"), Benchmark]
    public bool CanConvert_km_mm()
    {
        return _graph.CanConvert("km", "mm");
    }

    [BenchmarkCategory("CanConvert"), Benchmark]
    public bool CanConvert_NotFound()
    {
        return _graph.CanConvert("m", "kg");
    }

    [BenchmarkCategory("UnitConverter"), Benchmark(Baseline = true)]
    public ConversionResult Converter_km_m()
    {
        return _converter.Convert(1.0, "km", "m");
    }

    [BenchmarkCategory("UnitConverter"), Benchmark]
    public ConversionResult Converter_kg_g()
    {
        return _converter.Convert(1.0, "kg", "g");
    }

    [BenchmarkCategory("UnitConverter"), Benchmark]
    public ConversionResult Converter_h_s()
    {
        return _converter.Convert(1.0, "h", "s");
    }

    [BenchmarkCategory("UnitConverter"), Benchmark]
    public bool TryConvert_km_m()
    {
        return _converter.TryConvert(1.0, "km", "m", out _);
    }

    [BenchmarkCategory("UnitConverter"), Benchmark]
    public bool TryConvert_kg_mg()
    {
        return _converter.TryConvert(1.0, "kg", "mg", out _);
    }

    [BenchmarkCategory("UnitConverter"), Benchmark]
    public bool TryConvert_NotFound()
    {
        return _converter.TryConvert(1.0, "m", "kg", out _);
    }

    [BenchmarkCategory("UnitConverter"), Benchmark]
    public bool ConverterCanConvert_km_m()
    {
        return _converter.CanConvert("km", "m");
    }

    [BenchmarkCategory("UnitConverter"), Benchmark]
    public bool ConverterCanConvert_NotFound()
    {
        return _converter.CanConvert("m", "kg");
    }

    [BenchmarkCategory("ConversionPath"), Benchmark]
    public double Path_Convert_Direct()
    {
        var path = _graph.FindPath("km", "m");
        return path!.Convert(1.0);
    }

    [BenchmarkCategory("ConversionPath"), Benchmark]
    public double Path_Convert_TwoStep()
    {
        var path = _graph.FindPath("km", "mm");
        return path!.Convert(1.0);
    }

    [BenchmarkCategory("ConversionPath"), Benchmark]
    public int Path_StepCount()
    {
        var path = _graph.FindPath("km", "mm");
        return path!.StepCount;
    }

    [BenchmarkCategory("ConversionPath"), Benchmark]
    public bool Path_IsDirect()
    {
        var path = _graph.FindPath("km", "m");
        return path!.IsDirect;
    }

    [BenchmarkCategory("LargeValue"), Benchmark]
    public ConversionResult Convert_LargeValue_km_m()
    {
        return _graph.Convert(1e6, "km", "m");
    }

    [BenchmarkCategory("LargeValue"), Benchmark]
    public ConversionResult Convert_SmallValue_cm_mm()
    {
        return _graph.Convert(1e-10, "cm", "mm");
    }
}
