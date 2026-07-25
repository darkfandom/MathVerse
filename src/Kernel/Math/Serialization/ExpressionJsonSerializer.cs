namespace MathVerse.Math.Serialization;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Serializes and deserializes expression trees to/from JSON.
/// </summary>
public static class ExpressionJsonSerializer
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Serializes an expression to a JSON string.</summary>
    public static string Serialize(Expression expression)
    {
        var node = SerializeNode(expression);
        return JsonSerializer.Serialize(node, s_options);
    }

    /// <summary>Deserializes an expression from a JSON string.</summary>
    public static Expression Deserialize(string json)
    {
        var node = JsonSerializer.Deserialize<JsonElement>(json, s_options);
        return DeserializeNode(node);
    }

    /// <summary>Serializes an expression to a JsonElement.</summary>
    public static JsonElement SerializeToElement(Expression expression)
    {
        var node = SerializeNode(expression);
        return JsonSerializer.SerializeToElement(node, s_options);
    }

    private static Dictionary<string, object?> SerializeNode(Expression expression)
    {
        var dict = new Dictionary<string, object?>
        {
            ["kind"] = expression.Kind.ToString()
        };

        switch (expression)
        {
            case LiteralExpression literal:
                dict["value"] = literal.Value;
                break;
            case VariableExpression variable:
                dict["name"] = variable.Name;
                break;
            case ConstantExpression constant:
                dict["name"] = constant.Name;
                dict["value"] = constant.Value;
                break;
            case BinaryExpression binary:
                dict["operator"] = binary.Operator.Symbol;
                dict["left"] = SerializeNode(binary.Left);
                dict["right"] = SerializeNode(binary.Right);
                break;
            case UnaryExpression unary:
                dict["operator"] = unary.Operator.Symbol;
                dict["operand"] = SerializeNode(unary.Operand);
                break;
            case FunctionCallExpression func:
                dict["name"] = func.Name;
                dict["arguments"] = func.Arguments.Select(SerializeNode).ToList();
                break;
            case BooleanExpression boolean:
                dict["value"] = boolean.Value;
                break;
            case EquationExpression equation:
                dict["left"] = SerializeNode(equation.Left);
                dict["right"] = SerializeNode(equation.Right);
                break;
            case RelationExpression relation:
                dict["operator"] = relation.Operator.Symbol;
                dict["left"] = SerializeNode(relation.Left);
                dict["right"] = SerializeNode(relation.Right);
                break;
            case VectorExpression vector:
                dict["components"] = vector.Components.Select(SerializeNode).ToList();
                break;
            case MatrixExpression matrix:
                dict["rows"] = matrix.Rows.Select(SerializeNode).ToList();
                break;
            case IntegralExpression integral:
                dict["integrand"] = SerializeNode(integral.Integrand);
                dict["variable"] = SerializeNode(integral.Variable);
                if (integral.LowerBound is not null) dict["lowerBound"] = SerializeNode(integral.LowerBound);
                if (integral.UpperBound is not null) dict["upperBound"] = SerializeNode(integral.UpperBound);
                break;
            case DerivativeExpression derivative:
                dict["function"] = SerializeNode(derivative.Function);
                dict["variable"] = SerializeNode(derivative.Variable);
                dict["order"] = derivative.Order;
                break;
            case SummationExpression summation:
                dict["variable"] = SerializeNode(summation.Variable);
                dict["lowerBound"] = SerializeNode(summation.LowerBound);
                dict["upperBound"] = SerializeNode(summation.UpperBound);
                dict["body"] = SerializeNode(summation.Body);
                break;
            case IdentityExpression identity:
                dict["operation"] = identity.Operation;
                break;
            case NullExpression:
                break;
        }

        return dict;
    }

    private static Expression DeserializeNode(JsonElement element)
    {
        var kind = element.GetProperty("kind").GetString() ?? "Null";

        return kind switch
        {
            "Literal" => new LiteralExpression(element.GetProperty("value").GetDouble()),
            "Variable" => new VariableExpression(element.GetProperty("name").GetString()!),
            "Constant" => new ConstantExpression(element.GetProperty("name").GetString()!, element.GetProperty("value").GetDouble()),
            "Boolean" => new BooleanExpression(element.GetProperty("value").GetBoolean()),
            "Null" => NullExpression.Instance,
            "Identity" => new IdentityExpression(element.GetProperty("operation").GetString()!),
            _ => NullExpression.Instance
        };
    }
}

/// <summary>
/// Placeholder for MessagePack serialization support.
/// </summary>
public static class ExpressionMessagePackSerializer
{
    /// <summary>Serializes an expression to bytes.</summary>
    public static byte[] Serialize(Expression expression)
    {
        var json = ExpressionJsonSerializer.Serialize(expression);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    /// <summary>Deserializes an expression from bytes.</summary>
    public static Expression Deserialize(byte[] data)
    {
        var json = System.Text.Encoding.UTF8.GetString(data);
        return ExpressionJsonSerializer.Deserialize(json);
    }
}
