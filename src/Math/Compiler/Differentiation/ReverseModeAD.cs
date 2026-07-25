namespace MathVerse.Math.Compiler.Differentiation;

using System;
using System.Collections.Generic;

/// <summary>Provides reverse-mode automatic differentiation using a tape.</summary>
public sealed class ReverseModeAD
{
    private readonly Tape _tape = new();

    /// <summary>Computes the gradient of a function f: R^n → R using reverse-mode AD.</summary>
    /// <param name="f">The function to differentiate. Takes an array of inputs, uses the tape to record operations, and returns the scalar output.</param>
    /// <param name="x">The evaluation point.</param>
    /// <returns>The gradient vector ∇f(x).</returns>
    public double[] Differentiate(Func<Tape, AdjointValue[], AdjointValue> f, double[] x)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (x is null) throw new ArgumentNullException(nameof(x));

        _tape.Clear();

        var inputs = new AdjointValue[x.Length];
        for (int i = 0; i < x.Length; i++)
            inputs[i] = AdjointValue.CreateLeaf(x[i], $"x[{i}]");

        AdjointValue output = f(_tape, inputs);

        output.AccumulateGradient(1.0);

        _tape.Backward();

        var gradient = new double[x.Length];
        for (int i = 0; i < x.Length; i++)
            gradient[i] = inputs[i].Gradient;

        return gradient;
    }

    /// <summary>Computes the gradient of a function f: R^n → R using the specified tape for recording.</summary>
    public double[] DifferentiateWithTape(
        Func<Tape, AdjointValue[], AdjointValue> f,
        double[] x,
        Tape tape)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (x is null) throw new ArgumentNullException(nameof(x));
        if (tape is null) throw new ArgumentNullException(nameof(tape));

        var inputs = new AdjointValue[x.Length];
        for (int i = 0; i < x.Length; i++)
            inputs[i] = AdjointValue.CreateLeaf(x[i], $"x[{i}]");

        AdjointValue output = f(tape, inputs);
        output.AccumulateGradient(1.0);
        tape.Backward();

        var gradient = new double[x.Length];
        for (int i = 0; i < x.Length; i++)
            gradient[i] = inputs[i].Gradient;

        return gradient;
    }

    /// <summary>Computes the Jacobian of a function f: R^n → R^m using forward-over-reverse AD.</summary>
    public double[,] Jacobian(
        Func<Tape, AdjointValue[], AdjointValue[]> f,
        double[] x,
        int outputDimension)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (x is null) throw new ArgumentNullException(nameof(x));

        var jacobian = new double[outputDimension, x.Length];

        for (int j = 0; j < outputDimension; j++)
        {
            var tape = new Tape();
            var inputs = new AdjointValue[x.Length];
            for (int i = 0; i < x.Length; i++)
                inputs[i] = AdjointValue.CreateLeaf(x[i], $"x[{i}]");

            AdjointValue[] outputs = f(tape, inputs);

            if (j < outputs.Length)
            {
                outputs[j].AccumulateGradient(1.0);
                tape.Backward();

                for (int i = 0; i < x.Length; i++)
                    jacobian[j, i] = inputs[i].Gradient;
            }
        }

        return jacobian;
    }

    /// <summary>Gets the tape used for the most recent differentiation.</summary>
    public Tape Tape => _tape;

    /// <summary>Records and evaluates an expression AST using the tape for reverse-mode AD.</summary>
    public AdjointValue EvaluateExpression(
        Expressions.ExpressionNode root,
        IReadOnlyDictionary<string, AdjointValue> variables)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));
        if (variables is null) throw new ArgumentNullException(nameof(variables));

        return root switch
        {
            Expressions.NumberNode num => AdjointValue.CreateConstant(num.Value),
            Expressions.VariableNode var => variables.TryGetValue(var.Name, out var val)
                ? val
                : AdjointValue.CreateConstant(0),
            Expressions.BinaryOpNode bin => EvaluateBinary(bin, variables),
            Expressions.UnaryOpNode unary => EvaluateUnary(unary, variables),
            Expressions.FunctionNode func => EvaluateFunction(func, variables),
            _ => throw new ArgumentException($"Unknown expression type: {root.GetType().Name}"),
        };
    }

    private AdjointValue EvaluateBinary(Expressions.BinaryOpNode bin, IReadOnlyDictionary<string, AdjointValue> vars)
    {
        AdjointValue left = EvaluateExpression(bin.Left, vars);
        AdjointValue right = EvaluateExpression(bin.Right, vars);

        return bin.Op switch
        {
            Expressions.BinaryOperator.Add => _tape.RecordAdd(left, right),
            Expressions.BinaryOperator.Subtract => _tape.RecordSub(left, right),
            Expressions.BinaryOperator.Multiply => _tape.RecordMul(left, right),
            Expressions.BinaryOperator.Divide => _tape.RecordDiv(left, right),
            Expressions.BinaryOperator.Power => _tape.RecordPow(left, right),
            _ => throw new ArgumentException($"Unknown binary operator: {bin.Op}"),
        };
    }

    private AdjointValue EvaluateUnary(Expressions.UnaryOpNode unary, IReadOnlyDictionary<string, AdjointValue> vars)
    {
        AdjointValue operand = EvaluateExpression(unary.Operand, vars);
        return unary.Op switch
        {
            Expressions.UnaryOperator.Negate => _tape.RecordNeg(operand),
            Expressions.UnaryOperator.Positive => operand,
            _ => throw new ArgumentException($"Unknown unary operator: {unary.Op}"),
        };
    }

    private AdjointValue EvaluateFunction(Expressions.FunctionNode func, IReadOnlyDictionary<string, AdjointValue> vars)
    {
        if (func.Arguments.Count != 1)
            throw new ArgumentException($"Multi-argument functions not supported: {func.FunctionName}");

        AdjointValue arg = EvaluateExpression(func.Arguments[0], vars);

        return func.FunctionName.ToLowerInvariant() switch
        {
            "sin" => _tape.RecordSin(arg),
            "cos" => _tape.RecordCos(arg),
            "tan" => _tape.RecordTan(arg),
            "exp" => _tape.RecordExp(arg),
            "ln" => _tape.RecordLn(arg),
            "sqrt" => _tape.RecordSqrt(arg),
            _ => throw new ArgumentException($"Unsupported function for reverse AD: {func.FunctionName}"),
        };
    }
}
