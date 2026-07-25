namespace MathVerse.Math.Compiler.Runtime;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Recommends optimizations based on compiled IR analysis and runtime profiling data.
/// Combines static IR analysis (instruction counts, branching patterns, memory operations)
/// with dynamic profiling data to produce actionable optimization advice.
/// </summary>
public sealed class OptimizationAdvisor
{
    private const int HotInstructionThreshold = 200;
    private const int HighBranchRatioThreshold = 40;
    private const int HighMemoryOpsRatioThreshold = 30;

    /// <summary>
    /// Analyzes the IR module and optional profile data to produce optimization recommendations.
    /// </summary>
    /// <param name="module">The IR module to analyze.</param>
    /// <param name="profile">Optional profiling data. If null, advice is based on static analysis only.</param>
    /// <returns>An ordered list of optimization recommendations, highest priority first.</returns>
    public IReadOnlyList<OptimizationAdvice> Advise(IRModule module, ProfileResult? profile)
    {
        ArgumentNullException.ThrowIfNull(module);

        var advices = new List<OptimizationAdvice>();

        foreach (var function in module.Functions)
        {
            var functionProfile = profile != null
                ? TryGetProfile(profile, function.Name)
                : null;

            AnalyzeFunction(function, functionProfile, advices);
        }

        AnalyzeModuleLevel(module, profile, advices);

        advices.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        return advices;
    }

    private static void AnalyzeFunction(IRFunction function, FunctionProfile? profile, List<OptimizationAdvice> advices)
    {
        var totalInstructions = 0;
        var arithmeticOps = 0;
        var memoryOps = 0;
        var branchOps = 0;
        var callOps = 0;

        foreach (var block in function.Blocks)
        {
            foreach (var inst in block.Instructions)
            {
                totalInstructions++;

                switch (inst.OpCode)
                {
                    case IROpCode.Add:
                    case IROpCode.Sub:
                    case IROpCode.Mul:
                    case IROpCode.Div:
                    case IROpCode.Mod:
                    case IROpCode.Fma:
                        arithmeticOps++;
                        break;
                    case IROpCode.Load:
                    case IROpCode.Store:
                    case IROpCode.Alloc:
                        memoryOps++;
                        break;
                    case IROpCode.Branch:
                    case IROpCode.CondBranch:
                    case IROpCode.Return:
                        branchOps++;
                        break;
                    case IROpCode.Call:
                        callOps++;
                        break;
                }
            }
        }

        if (totalInstructions == 0) return;

        var branchRatio = (double)branchOps / totalInstructions * 100.0;
        var memoryRatio = (double)memoryOps / totalInstructions * 100.0;
        var isHot = profile != null && profile.CallCount > 100;
        var priority = isHot ? 100 : 50;

        if (arithmeticOps > HotInstructionThreshold)
        {
            advices.Add(new OptimizationAdvice(
                function.Name,
                isHot ? OptimizationStrategy.AggressiveOptimization : OptimizationStrategy.StandardOptimization,
                $"Function has {arithmeticOps} arithmetic operations. Consider vectorization or SIMD.",
                "compute",
                priority + 10));
        }

        if (branchRatio > HighBranchRatioThreshold)
        {
            advices.Add(new OptimizationAdvice(
                function.Name,
                OptimizationStrategy.StandardOptimization,
                $"High branch ratio ({branchRatio:F1}%). Consider simplifying control flow or branchless techniques.",
                "control-flow",
                priority + 5));
        }

        if (memoryRatio > HighMemoryOpsRatioThreshold)
        {
            advices.Add(new OptimizationAdvice(
                function.Name,
                OptimizationStrategy.StandardOptimization,
                $"High memory operation ratio ({memoryRatio:F1}%). Consider buffer reuse or stack allocation.",
                "memory",
                priority + 7));
        }

        if (function.Blocks.Count > 10)
        {
            advices.Add(new OptimizationAdvice(
                function.Name,
                OptimizationStrategy.AggressiveOptimization,
                $"Function has {function.Blocks.Count} basic blocks. Consider outlining cold paths.",
                "control-flow",
                priority + 3));
        }

        if (callOps > 5)
        {
            advices.Add(new OptimizationAdvice(
                function.Name,
                OptimizationStrategy.StandardOptimization,
                $"Function makes {callOps} calls. Consider inlining small callees.",
                "inlining",
                priority + 4));
        }

        if (function.ComputeTempRegisterCount() > 50)
        {
            advices.Add(new OptimizationAdvice(
                function.Name,
                OptimizationStrategy.BasicOptimization,
                $"High register pressure ({function.ComputeTempRegisterCount()} temporaries). Consider reducing intermediate values.",
                "register",
                priority + 2));
        }

        if (profile != null && profile.MinTime > TimeSpan.Zero)
        {
            var variance = profile.MaxTime.TotalMicroseconds / Math.Max(profile.MinTime.TotalMicroseconds, 1);
            if (variance > 10.0)
            {
                advices.Add(new OptimizationAdvice(
                    function.Name,
                    OptimizationStrategy.AggressiveOptimization,
                    $"High execution time variance ({variance:F1}x). May benefit from specialization or devirtualization.",
                    "specialization",
                    priority + 8));
            }
        }

        if (profile != null && profile.CallCount < 10)
        {
            advices.Add(new OptimizationAdvice(
                function.Name,
                OptimizationStrategy.SkipOptimization,
                "Function called rarely. Optimization resources better spent elsewhere.",
                "cost",
                10));
        }
    }

    private static void AnalyzeModuleLevel(IRModule module, ProfileResult? profile, List<OptimizationAdvice> advices)
    {
        if (module.Functions.Count > 20)
        {
            advices.Add(new OptimizationAdvice(
                "(module)",
                OptimizationStrategy.StandardOptimization,
                $"Module contains {module.Functions.Count} functions. Consider splitting into smaller compilation units.",
                "module",
                30));
        }

        var totalInstructions = module.TotalInstructionCount();
        if (totalInstructions > 10000)
        {
            advices.Add(new OptimizationAdvice(
                "(module)",
                OptimizationStrategy.AggressiveOptimization,
                $"Module has {totalInstructions} total instructions. Multi-pass optimization may yield better results.",
                "module",
                35));
        }

        if (profile != null)
        {
            var hotPaths = profile.HotPaths;
            if (hotPaths.Count > 0)
            {
                var hotFunctionNames = new List<string>();
                for (var i = 0; i < Math.Min(hotPaths.Count, 5); i++)
                {
                    for (var j = 0; j < hotPaths[i].FunctionChain.Count; j++)
                        hotFunctionNames.Add(hotPaths[i].FunctionChain[j]);
                }

                advices.Add(new OptimizationAdvice(
                    "(module)",
                    OptimizationStrategy.FullOptimization,
                    $"Hot path identified through {string.Join(" → ", hotFunctionNames)}. Prioritize optimization of these functions.",
                    "hot-path",
                    90));
            }
        }
    }

    private static FunctionProfile? TryGetProfile(ProfileResult profile, string functionName)
    {
        if (profile.FunctionProfiles.TryGetValue(functionName, out var p))
            return p;
        return null;
    }
}
