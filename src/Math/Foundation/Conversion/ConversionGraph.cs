using System.Collections.Concurrent;

namespace MathVerse.Math.Foundation.Conversion;

public sealed class ConversionGraph
{
    private static readonly Lazy<ConversionGraph> LazyInstance = new(() => new ConversionGraph());

    public static ConversionGraph Instance => LazyInstance.Value;

    private readonly ConcurrentDictionary<string, List<ConversionRule>> _rulesFrom = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<(string From, string To), ConversionRule> _directRules = new();

    private ConversionGraph()
    {
    }

    public void AddRule(ConversionRule rule)
    {
        if (rule is null) throw new ArgumentNullException(nameof(rule));
        if (!_rulesFrom.ContainsKey(rule.From))
            _rulesFrom[rule.From] = new List<ConversionRule>();
        _rulesFrom[rule.From].Add(rule);
        _directRules[(rule.From, rule.To)] = rule;

        var reverse = new ConversionRule
        {
            From = rule.To,
            To = rule.From,
            Converter = v =>
            {
                var x = rule.Converter(0);
                if (System.Math.Abs(x) < 1e-15)
                    return 1.0 / rule.Converter(1.0 / v);
                return v / rule.Converter(1.0);
            },
            IsExact = rule.IsExact,
            Description = $"Reverse of {rule.Description}"
        };
        if (!_rulesFrom.ContainsKey(reverse.From))
            _rulesFrom[reverse.From] = new List<ConversionRule>();
        _rulesFrom[reverse.From].Add(reverse);
        _directRules[(reverse.From, reverse.To)] = reverse;
    }

    public ConversionResult Convert(double value, string fromUnit, string toUnit)
    {
        if (string.Equals(fromUnit, toUnit, StringComparison.OrdinalIgnoreCase))
            return ConversionResult.Succeeded(value, new ConversionPath { From = fromUnit, To = toUnit });

        var path = FindPath(fromUnit, toUnit);
        if (path is null)
            return ConversionResult.Failed($"No conversion path from '{fromUnit}' to '{toUnit}'");

        return ConversionResult.Succeeded(path.Convert(value), path);
    }

    public ConversionPath? FindPath(string fromUnit, string toUnit)
    {
        if (_directRules.TryGetValue((fromUnit, toUnit), out var direct))
        {
            return new ConversionPath
            {
                Steps = ImmutableArray.Create(direct),
                From = fromUnit,
                To = toUnit
            };
        }

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<(string Current, List<ConversionRule> Steps)>();
        queue.Enqueue((fromUnit, new List<ConversionRule>()));
        visited.Add(fromUnit);

        while (queue.Count > 0)
        {
            var (current, steps) = queue.Dequeue();
            if (_rulesFrom.TryGetValue(current, out var rules))
            {
                foreach (var rule in rules)
                {
                    if (string.Equals(rule.To, toUnit, StringComparison.OrdinalIgnoreCase))
                    {
                        var allSteps = new List<ConversionRule>(steps) { rule };
                        return new ConversionPath
                        {
                            Steps = allSteps.ToImmutableArray(),
                            From = fromUnit,
                            To = toUnit
                        };
                    }
                    if (visited.Add(rule.To))
                    {
                        var newSteps = new List<ConversionRule>(steps) { rule };
                        queue.Enqueue((rule.To, newSteps));
                    }
                }
            }
        }

        return null;
    }

    public bool CanConvert(string fromUnit, string toUnit)
    {
        return FindPath(fromUnit, toUnit) is not null;
    }
}
