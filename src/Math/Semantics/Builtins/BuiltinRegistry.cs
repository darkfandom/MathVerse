namespace MathVerse.Math.Semantics.Builtins;

/// <summary>
/// Registry of all built-in mathematical constants and functions.
/// Populates the global symbol table with well-known symbols.
/// </summary>
public static class BuiltinRegistry
{
    /// <summary>Registers all built-in symbols into the symbol table.</summary>
    public static void RegisterAll(SymbolTable table)
    {
        RegisterConstants(table);
        RegisterSingleArgFunctions(table);
        RegisterMultiArgFunctions(table);
        RegisterNamespaceAliases(table);
    }

    private static void RegisterConstants(SymbolTable table)
    {
        var constants = new (string Name, double Value)[]
        {
            ("π", System.Math.PI),
            ("pi", System.Math.PI),
            ("τ", System.Math.Tau),
            ("tau", System.Math.Tau),
            ("e", System.Math.E),
            ("φ", 1.6180339887498949),
            ("phi", 1.6180339887498949),
            ("inf", double.PositiveInfinity),
            ("∞", double.PositiveInfinity),
            ("NaN", double.NaN),
        };

        foreach (var (name, value) in constants)
            table.Declare(new ConstantSymbol(name, value));
    }

    private static void RegisterSingleArgFunctions(SymbolTable table)
    {
        var funcs = new string[]
        {
            "abs", "sqrt", "cbrt", "ln", "log", "log2", "log10",
            "exp", "sin", "cos", "tan", "sec", "csc", "cot",
            "asin", "acos", "atan", "sinh", "cosh", "tanh",
            "asinh", "acosh", "atanh", "floor", "ceil", "round",
            "sign", "factorial", "gamma", "erf", "erfc",
            "deg2rad", "rad2deg", "degrees", "radians",
            "real", "imag", "conj",
        };

        foreach (var name in funcs)
            table.Declare(new FunctionSymbol(name, [new ParameterSymbol("x", 0)]));
    }

    private static void RegisterMultiArgFunctions(SymbolTable table)
    {
        table.Declare(new FunctionSymbol("logbase",
            [new ParameterSymbol("x", 0), new ParameterSymbol("base", 1)]));
        table.Declare(new FunctionSymbol("atan2",
            [new ParameterSymbol("y", 0), new ParameterSymbol("x", 1)]));
        table.Declare(new FunctionSymbol("pow",
            [new ParameterSymbol("base", 0), new ParameterSymbol("exp", 1)]));
        table.Declare(new FunctionSymbol("min",
            [new ParameterSymbol("a", 0), new ParameterSymbol("b", 1)]));
        table.Declare(new FunctionSymbol("max",
            [new ParameterSymbol("a", 0), new ParameterSymbol("b", 1)]));
        table.Declare(new FunctionSymbol("gcd",
            [new ParameterSymbol("a", 0), new ParameterSymbol("b", 1)]));
        table.Declare(new FunctionSymbol("lcm",
            [new ParameterSymbol("a", 0), new ParameterSymbol("b", 1)]));
        table.Declare(new FunctionSymbol("hypot",
            [new ParameterSymbol("x", 0), new ParameterSymbol("y", 1)]));
        table.Declare(new FunctionSymbol("mod",
            [new ParameterSymbol("x", 0), new ParameterSymbol("y", 1)]));
        table.Declare(new FunctionSymbol("clamp",
            [new ParameterSymbol("val", 0), new ParameterSymbol("min", 1), new ParameterSymbol("max", 2)]));

        table.Declare(new FunctionSymbol("beta",
            [new ParameterSymbol("a", 0), new ParameterSymbol("b", 1)]));
        table.Declare(new FunctionSymbol("besselj",
            [new ParameterSymbol("n", 0), new ParameterSymbol("x", 1)]));
        table.Declare(new FunctionSymbol("bessely",
            [new ParameterSymbol("n", 0), new ParameterSymbol("x", 1)]));
        table.Declare(new FunctionSymbol("legendre",
            [new ParameterSymbol("l", 0), new ParameterSymbol("x", 1)]));
        table.Declare(new FunctionSymbol("hermite",
            [new ParameterSymbol("n", 0), new ParameterSymbol("x", 1)]));
    }

    private static void RegisterNamespaceAliases(SymbolTable table)
    {
        var ns = new NamespaceSymbol("std");
        ns.Declare(new ConstantSymbol("g", 9.80665));
        ns.Declare(new ConstantSymbol("c", 299792458.0));
        ns.Declare(new ConstantSymbol("h", 6.62607015e-34));
        ns.Declare(new ConstantSymbol("kb", 1.380649e-23));
        ns.Declare(new ConstantSymbol("NA", 6.02214076e23));
        ns.Declare(new ConstantSymbol("R", 8.31446261815324));
        ns.Declare(new ConstantSymbol("epsilon0", 8.8541878128e-12));
        ns.Declare(new ConstantSymbol("mu0", 1.25663706212e-6));
        table.Declare(ns);
    }
}
