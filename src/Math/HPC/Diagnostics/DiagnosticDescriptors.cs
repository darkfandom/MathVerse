namespace MathVerse.Math.HPC.Diagnostics;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor HPC001_UnreachableCode = new(
        "HPC001", "Unreachable code detected",
        "Code at this location is unreachable",
        DiagnosticSeverity.Warning, "Optimization", true);

    public static readonly DiagnosticDescriptor HPC002_DeadStore = new(
        "HPC002", "Dead store detected",
        "Value stored to '{0}' is never read",
        DiagnosticSeverity.Warning, "Optimization", true);

    public static readonly DiagnosticDescriptor HPC003_RedundantComputation = new(
        "HPC003", "Redundant computation detected",
        "Expression '{0}' computes the same value as previous computation",
        DiagnosticSeverity.Warning, "Optimization", true);

    public static readonly DiagnosticDescriptor HPC004_LoopInvariantCodeMotion = new(
        "HPC004", "Loop-invariant code motion opportunity",
        "Expression '{0}' is loop-invariant and can be hoisted",
        DiagnosticSeverity.Info, "Optimization", true);

    public static readonly DiagnosticDescriptor HPC005_VectorizationFailed = new(
        "HPC005", "Vectorization failed",
        "Loop could not be vectorized: {0}",
        DiagnosticSeverity.Warning, "Vectorization", true);

    public static readonly DiagnosticDescriptor HPC006_VectorizationPossible = new(
        "HPC006", "Vectorization opportunity",
        "Loop can be vectorized with {0}x speedup",
        DiagnosticSeverity.Info, "Vectorization", true);

    public static readonly DiagnosticDescriptor HPC007_MemoryAlignment = new(
        "HPC007", "Memory alignment opportunity",
        "Buffer '{0}' could be aligned to {1} bytes for better performance",
        DiagnosticSeverity.Info, "Memory", true);

    public static readonly DiagnosticDescriptor HPC008_CacheMiss = new(
        "HPC008", "Potential cache miss pattern",
        "Access pattern on '{0}' may cause cache misses",
        DiagnosticSeverity.Warning, "Memory", true);

    public static readonly DiagnosticDescriptor HPC009_RegisterPressure = new(
        "HPC009", "High register pressure",
        "Function uses {0} registers, consider splitting",
        DiagnosticSeverity.Warning, "RegisterAllocation", true);

    public static readonly DiagnosticDescriptor HPC010_UnusedVariable = new(
        "HPC010", "Unused variable",
        "Variable '{0}' is declared but never used",
        DiagnosticSeverity.Warning, "Analysis", true);

    public static readonly DiagnosticDescriptor HPC011_UninitializedVariable = new(
        "HPC011", "Possibly uninitialized variable",
        "Variable '{0}' may be used before assignment",
        DiagnosticSeverity.Error, "Analysis", true);

    public static readonly DiagnosticDescriptor HPC012_IntegerOverflow = new(
        "HPC012", "Potential integer overflow",
        "Operation '{0}' may overflow",
        DiagnosticSeverity.Warning, "Analysis", true);

    public static readonly DiagnosticDescriptor HPC013_DivisionByZero = new(
        "HPC013", "Potential division by zero",
        "Division by '{0}' may be zero",
        DiagnosticSeverity.Error, "Analysis", true);

    public static readonly DiagnosticDescriptor HPC014_ArrayBounds = new(
        "HPC014", "Potential array bounds violation",
        "Index '{0}' may be out of bounds for array '{1}'",
        DiagnosticSeverity.Error, "Analysis", true);

    public static readonly DiagnosticDescriptor HPC015_NullDereference = new(
        "HPC015", "Potential null dereference",
        "Variable '{0}' may be null when dereferenced",
        DiagnosticSeverity.Error, "Analysis", true);

    public static readonly DiagnosticDescriptor HPC016_LoopUnrolling = new(
        "HPC016", "Loop unrolling opportunity",
        "Loop with {0} iterations can be unrolled by factor {1}",
        DiagnosticSeverity.Info, "Optimization", true);

    public static readonly DiagnosticDescriptor HPC017_FunctionInlining = new(
        "HPC017", "Function inlining opportunity",
        "Function '{0}' is small and hot, consider inlining",
        DiagnosticSeverity.Info, "Optimization", true);

    public static readonly DiagnosticDescriptor HPC018_ConstantFolding = new(
        "HPC018", "Constant folding opportunity",
        "Expression '{0}' can be evaluated at compile time",
        DiagnosticSeverity.Info, "Optimization", true);

    public static readonly DiagnosticDescriptor HPC019_CommonSubexpression = new(
        "HPC019", "Common subexpression elimination",
        "Expression '{0}' appears multiple times",
        DiagnosticSeverity.Info, "Optimization", true);

    public static readonly DiagnosticDescriptor HPC020_StrengthReduction = new(
        "HPC020", "Strength reduction opportunity",
        "Operation '{0}' can be replaced with cheaper '{1}'",
        DiagnosticSeverity.Info, "Optimization", true);

    public static readonly DiagnosticDescriptor HPC021_ParallelizationOpportunity = new(
        "HPC021", "Parallelization opportunity",
        "Loop over '{0}' can be parallelized with {1}x speedup",
        DiagnosticSeverity.Info, "Parallelization", true);

    public static readonly DiagnosticDescriptor HPC022_RaceCondition = new(
        "HPC022", "Potential race condition",
        "Variable '{0}' accessed concurrently without synchronization",
        DiagnosticSeverity.Error, "Concurrency", true);

    public static readonly DiagnosticDescriptor HPC023_DeadlockRisk = new(
        "HPC023", "Potential deadlock",
        "Lock ordering violation detected on '{0}'",
        DiagnosticSeverity.Warning, "Concurrency", true);

    public static readonly DiagnosticDescriptor HPC024_FalseSharing = new(
        "HPC024", "False sharing detected",
        "Variables '{0}' and '{1}' share cache line",
        DiagnosticSeverity.Warning, "Concurrency", true);

    public static readonly DiagnosticDescriptor HPC025_MemoryLeak = new(
        "HPC025", "Potential memory leak",
        "Allocation at '{0}' may not be freed",
        DiagnosticSeverity.Warning, "Memory", true);

    public static readonly DiagnosticDescriptor HPC026_BufferOverrun = new(
        "HPC026", "Potential buffer overrun",
        "Write to '{0}' may exceed allocated size",
        DiagnosticSeverity.Error, "Memory", true);

    public static readonly DiagnosticDescriptor HPC027_UseAfterFree = new(
        "HPC027", "Use after free",
        "Memory '{0}' accessed after deallocation",
        DiagnosticSeverity.Error, "Memory", true);

    public static readonly DiagnosticDescriptor HPC028_DoubleFree = new(
        "HPC028", "Double free detected",
        "Memory '{0}' freed multiple times",
        DiagnosticSeverity.Error, "Memory", true);

    public static readonly DiagnosticDescriptor HPC029_AlignmentRequirement = new(
        "HPC029", "Alignment requirement not met",
        "Type '{0}' requires {1}-byte alignment",
        DiagnosticSeverity.Error, "Memory", true);

    public static readonly DiagnosticDescriptor HPC030_StackOverflow = new(
        "HPC030", "Potential stack overflow",
        "Recursive function '{0}' may exceed stack limit",
        DiagnosticSeverity.Warning, "Analysis", true);

    public static readonly DiagnosticDescriptor HPC031_TailCallOptimization = new(
        "HPC031", "Tail call optimization opportunity",
        "Function '{0}' ends with tail call to '{1}'",
        DiagnosticSeverity.Info, "Optimization", true);

    public static readonly DiagnosticDescriptor HPC032_RegisterSpilling = new(
        "HPC032", "Register spilling detected",
        "Function spills {0} registers to stack",
        DiagnosticSeverity.Warning, "RegisterAllocation", true);

    public static readonly DiagnosticDescriptor HPC033_SpillCost = new(
        "HPC033", "High spill cost",
        "Spilling register '{0}' costs {1} cycles",
        DiagnosticSeverity.Warning, "RegisterAllocation", true);

    public static readonly DiagnosticDescriptor HPC034_CoalescingOpportunity = new(
        "HPC034", "Register coalescing opportunity",
        "Registers '{0}' and '{1}' can be coalesced",
        DiagnosticSeverity.Info, "RegisterAllocation", true);

    public static readonly DiagnosticDescriptor HPC035_BankConflict = new(
        "HPC035", "Shared memory bank conflict",
        "Access pattern on shared memory causes {0}-way bank conflicts",
        DiagnosticSeverity.Warning, "GPU", true);

    public static readonly DiagnosticDescriptor HPC036_WarpDivergence = new(
        "HPC036", "Warp divergence detected",
        "Branch divergence in warp: {0}% threads take different path",
        DiagnosticSeverity.Warning, "GPU", true);

    public static readonly DiagnosticDescriptor HPC037_OccupancyLimited = new(
        "HPC037", "Occupancy limited by registers",
        "Kernel uses {0} registers/thread, limiting occupancy to {1}%",
        DiagnosticSeverity.Warning, "GPU", true);

    public static readonly DiagnosticDescriptor HPC038_SharedMemoryPressure = new(
        "HPC038", "Shared memory pressure",
        "Kernel uses {0} bytes shared memory, limiting occupancy",
        DiagnosticSeverity.Warning, "GPU", true);

    public static readonly DiagnosticDescriptor HPC039_UncoalescedMemoryAccess = new(
        "HPC039", "Uncoalesced global memory access",
        "Memory access pattern in '{0}' is not coalesced",
        DiagnosticSeverity.Warning, "GPU", true);

    public static readonly DiagnosticDescriptor HPC040_AsyncCopyOpportunity = new(
        "HPC040", "Async copy opportunity",
        "Data transfer in '{0}' can use async copy",
        DiagnosticSeverity.Info, "GPU", true);

    public static readonly IReadOnlyList<DiagnosticDescriptor> All = new[]
    {
        HPC001_UnreachableCode, HPC002_DeadStore, HPC003_RedundantComputation,
        HPC004_LoopInvariantCodeMotion, HPC005_VectorizationFailed, HPC006_VectorizationPossible,
        HPC007_MemoryAlignment, HPC008_CacheMiss, HPC009_RegisterPressure,
        HPC010_UnusedVariable, HPC011_UninitializedVariable, HPC012_IntegerOverflow,
        HPC013_DivisionByZero, HPC014_ArrayBounds, HPC015_NullDereference,
        HPC016_LoopUnrolling, HPC017_FunctionInlining, HPC018_ConstantFolding,
        HPC019_CommonSubexpression, HPC020_StrengthReduction, HPC021_ParallelizationOpportunity,
        HPC022_RaceCondition, HPC023_DeadlockRisk, HPC024_FalseSharing,
        HPC025_MemoryLeak, HPC026_BufferOverrun, HPC027_UseAfterFree, HPC028_DoubleFree,
        HPC029_AlignmentRequirement, HPC030_StackOverflow, HPC031_TailCallOptimization,
        HPC032_RegisterSpilling, HPC033_SpillCost, HPC034_CoalescingOpportunity,
        HPC035_BankConflict, HPC036_WarpDivergence, HPC037_OccupancyLimited,
        HPC038_SharedMemoryPressure, HPC039_UncoalescedMemoryAccess, HPC040_AsyncCopyOpportunity
    };
}