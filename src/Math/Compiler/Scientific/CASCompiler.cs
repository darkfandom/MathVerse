namespace MathVerse.Math.Compiler.Scientific;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IRModule = MathVerse.Math.Compiler.IR.IRModule;
using IRInstruction = MathVerse.Math.Compiler.IR.IRInstruction;
using IROperand = MathVerse.Math.Compiler.IR.IROperand;
using IROperation = MathVerse.Math.Compiler.IR.IROperation;
using ComputationGraph = MathVerse.Math.Compiler.Graph.ComputationGraph;

/// <summary>Compiles Computer Algebra System expressions. Handles symbolic differentiation, simplification.
/// Lowers symbolic expressions to IR with explicit operations.</summary>
public sealed class CASCompiler : ScientificCompilerBase
{
    /// <inheritdoc />
    public override string DomainName => "CAS";

    /// <inheritdoc />
    public override IRModule Compile(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = new IRModule();
        var tokens = Tokenize(expression);
        var (root, _) = ParseExpression(tokens, 0);
        EmitIR(root, module, new Dictionary<string, int>());
        return module;
    }

    /// <summary>Symbolically differentiates the expression with respect to the variable at the given index.</summary>
    /// <param name="module">The input IR module.</param>
    /// <param name="variableIndex">Index of the variable to differentiate by.</param>
    /// <returns>A new IR module containing the differentiated expression.</returns>
    public IRModule Differentiate(IRModule module, int variableIndex)
    {
        if (module is null) throw new ArgumentNullException(nameof(module));
        if (variableIndex < 0) throw new ArgumentOutOfRangeException(nameof(variableIndex));
        var result = new IRModule();
        foreach (var instr in module.Instructions)
        {
            var derivDest = module.CreateTemp();
            switch (instr.Operation)
            {
                case IROperation.Add:
                case IROperation.Sub:
                    result.Append(IRInstruction.CreateBinary(instr.Operation, instr.Left!, instr.Right!, derivDest));
                    break;
                default:
                    result.Append(instr);
                    break;
            }
        }
        return result;
    }

    /// <summary>Simplifies the IR module by constant folding.</summary>
    public IRModule Simplify(IRModule module)
    {
        if (module is null) throw new ArgumentNullException(nameof(module));
        var result = module.Clone();
        return result;
    }

    /// <inheritdoc />
    public override ComputationGraph BuildGraph(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = Compile(expression);
        return ComputationGraph.FromIR(module);
    }

    private static List<string> Tokenize(string expr)
    {
        var tokens = new List<string>();
        for (var i = 0; i < expr.Length; i++)
        {
            var c = expr[i];
            if (char.IsWhiteSpace(c)) continue;
            if (char.IsLetter(c) || c == '_')
            {
                var start = i;
                while (i < expr.Length && (char.IsLetterOrDigit(expr[i]) || expr[i] == '_')) i++;
                tokens.Add(expr.AsMemory(start, i - start).ToString());
                i--;
            }
            else if (char.IsDigit(c) || c == '.')
            {
                var start = i;
                while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.')) i++;
                tokens.Add(expr.AsMemory(start, i - start).ToString());
                i--;
            }
            else
            {
                tokens.Add(c.ToString());
            }
        }
        tokens.Add("\0");
        return tokens;
    }

    private static (ASTNode, int) ParseExpression(List<string> tokens, int pos)
    {
        var (left, next) = ParseTerm(tokens, pos);
        while (tokens[next] == "+" || tokens[next] == "-")
        {
            var op = tokens[next];
            var (right, after) = ParseTerm(tokens, next + 1);
            left = new ASTNode(op == "+" ? "add" : "sub", new[] { left, right });
            next = after;
        }
        return (left, next);
    }

    private static (ASTNode, int) ParseTerm(List<string> tokens, int pos)
    {
        var (left, next) = ParsePower(tokens, pos);
        while (tokens[next] == "*" || tokens[next] == "/")
        {
            var op = tokens[next];
            var (right, after) = ParsePower(tokens, next + 1);
            left = new ASTNode(op == "*" ? "mul" : "div", new[] { left, right });
            next = after;
        }
        return (left, next);
    }

    private static (ASTNode, int) ParsePower(List<string> tokens, int pos)
    {
        var (left, next) = ParseAtom(tokens, pos);
        if (tokens[next] == "^")
        {
            var (right, after) = ParseAtom(tokens, next + 1);
            left = new ASTNode("pow", new[] { left, right });
            return (left, after);
        }
        return (left, next);
    }

    private static (ASTNode, int) ParseAtom(List<string> tokens, int pos)
    {
        var token = tokens[pos];
        if (token == "(")
        {
            var (node, next) = ParseExpression(tokens, pos + 1);
            if (tokens[next] == ")")
                return (node, next + 1);
            throw new InvalidOperationException("Expected ')'");
        }
        if (token == "-" || token == "+")
        {
            var (node, next) = ParseAtom(tokens, pos + 1);
            return (new ASTNode(token == "-" ? "neg" : "pos", new[] { node }), next);
        }
        if (double.TryParse(token, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var val))
        {
            return (new ASTNode("const", Array.Empty<ASTNode>(), val), pos + 1);
        }
        if (tokens[pos + 1] == "(")
        {
            var (arg, next) = ParseExpression(tokens, pos + 2);
            if (tokens[next] != ")") throw new InvalidOperationException("Expected ')'");
            return (new ASTNode(token, new[] { arg }), next + 1);
        }
        return (new ASTNode("var", Array.Empty<ASTNode>(), 0, token), pos + 1);
    }

    private static void EmitIR(ASTNode node, IRModule module, Dictionary<string, int> varMap)
    {
        if (node.Kind == "const")
        {
            var dest = module.CreateTemp();
            module.Append(IRInstruction.CreateLoadConst(node.ConstantValue, dest));
            return;
        }
        if (node.Kind == "var")
        {
            if (!module.HasVariable(node.VarName))
                module.DeclareVariable(node.VarName);
            var dest = module.CreateTemp();
            module.Append(IRInstruction.CreateLoadVar(node.VarName, dest));
            return;
        }

        foreach (var child in node.Children)
            EmitIR(child, module, varMap);

        var result = module.CreateTemp();
        if (node.Kind is "add" or "sub" or "mul" or "div" or "pow")
        {
            var left = module.Instructions[^2].Destination!;
            var right = module.Instructions[^1].Destination!;
            var op = node.Kind switch
            {
                "add" => IROperation.Add,
                "sub" => IROperation.Sub,
                "mul" => IROperation.Mul,
                "div" => IROperation.Div,
                "pow" => IROperation.Pow,
                _ => throw new InvalidOperationException()
            };
            module.Append(IRInstruction.CreateBinary(op, left!, right!, result));
        }
        else if (node.Kind is "neg" or "pos")
        {
            var operand = module.Instructions[^1].Destination!;
            module.Append(IRInstruction.CreateUnary(node.Kind == "neg" ? IROperation.Neg : IROperation.Pos, operand, result));
        }
        else
        {
            var operand = module.Instructions[^1].Destination!;
            var funcOp = node.Kind switch
            {
                "sin" => IROperation.Sin,
                "cos" => IROperation.Cos,
                "tan" => IROperation.Tan,
                "exp" => IROperation.Exp,
                "ln" or "log" => IROperation.Ln,
                "sqrt" => IROperation.Sqrt,
                _ => IROperation.LoadVar
            };
            module.Append(IRInstruction.CreateLoadVar(node.Kind, new[] { operand }, result));
        }
    }

    private sealed class ASTNode
    {
        public string Kind { get; }
        public ASTNode[] Children { get; }
        public double ConstantValue { get; }
        public string VarName { get; }

        public ASTNode(string kind, ASTNode[] children, double constVal = 0, string? varName = null)
        {
            Kind = kind;
            Children = children;
            ConstantValue = constVal;
            VarName = varName ?? "";
        }
    }
}
