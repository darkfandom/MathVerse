namespace MathVerse.Math.HPC.Configuration;

using System;

/// <summary>
/// Configuration for SIMD (Single Instruction, Multiple Data) operations.
/// </summary>
/// <param name="Enabled">Whether SIMD operations are enabled.</param>
/// <param name="PreferredWidth">Preferred SIMD vector width in bits (128, 256, 512).</param>
/// <param name="EnableAutoVectorization">Whether to enable automatic vectorization.</param>
/// <param name="EnableExplicitVectorization">Whether to enable explicit vectorization.</param>
/// <param name="EnableFma">Whether to enable Fused Multiply-Add instructions.</param>
/// <param name="EnableAvx512">Whether to enable AVX-512 instructions.</param>
/// <param name="EnableAvx2">Whether to enable AVX2 instructions.</param>
/// <param name="EnableSse42">Whether to enable SSE4.2 instructions.</param>
/// <param name="EnableNeon">Whether to enable ARM NEON instructions.</param>
/// <param name="EnableSve">Whether to enable ARM SVE instructions.</param>
/// <param name="PreferredVectorTypes">Preferred vector types for vectorization.</param>
/// <param name="MaxVectorLength">Maximum vector length for variable-length vectors (SVE).</param>
/// <param name="EnableMaskedOperations">Whether to enable masked vector operations.</param>
/// <param name="EnableGatherScatter">Whether to enable gather/scatter operations.</param>
/// <param name="AlignmentRequirement">Required memory alignment in bytes.</param>
public sealed record SimdConfiguration(
    bool Enabled = true,
    int PreferredWidth = 256,
    bool EnableAutoVectorization = true,
    bool EnableExplicitVectorization = true,
    bool EnableFma = true,
    bool EnableAvx512 = true,
    bool EnableAvx2 = true,
    bool EnableSse42 = true,
    bool EnableNeon = true,
    bool EnableSve = false,
    Type[]? PreferredVectorTypes = null,
    int MaxVectorLength = 2048,
    bool EnableMaskedOperations = true,
    bool EnableGatherScatter = true,
    int AlignmentRequirement = 32)
{
    /// <summary>
    /// Default SIMD configuration with all features enabled.
    /// </summary>
    public static SimdConfiguration Default { get; } = new();

    /// <summary>
    /// SIMD configuration optimized for AVX2.
    /// </summary>
    public static SimdConfiguration Avx2Optimized { get; } = new(
        PreferredWidth: 256,
        EnableAvx512: false,
        EnableAvx2: true,
        EnableSse42: true,
        EnableNeon: false,
        EnableSve: false);

    /// <summary>
    /// SIMD configuration optimized for AVX-512.
    /// </summary>
    public static SimdConfiguration Avx512Optimized { get; } = new(
        PreferredWidth: 512,
        EnableAvx512: true,
        EnableAvx2: true,
        EnableSse42: true,
        EnableNeon: false,
        EnableSve: false);

    /// <summary>
    /// SIMD configuration optimized for ARM NEON.
    /// </summary>
    public static SimdConfiguration NeonOptimized { get; } = new(
        PreferredWidth: 128,
        EnableAvx512: false,
        EnableAvx2: false,
        EnableSse42: false,
        EnableNeon: true,
        EnableSve: false);

    /// <summary>
    /// SIMD configuration optimized for ARM SVE.
    /// </summary>
    public static SimdConfiguration SveOptimized { get; } = new(
        PreferredWidth: 512,
        EnableAvx512: false,
        EnableAvx2: false,
        EnableSse42: false,
        EnableNeon: true,
        EnableSve: true,
        MaxVectorLength: 2048);

    /// <summary>
    /// SIMD configuration with minimal features (SSE2 only).
    /// </summary>
    public static SimdConfiguration Minimal { get; } = new(
        Enabled: true,
        PreferredWidth: 128,
        EnableAutoVectorization: false,
        EnableExplicitVectorization: false,
        EnableFma: false,
        EnableAvx512: false,
        EnableAvx2: false,
        EnableSse42: false,
        EnableNeon: false,
        EnableSve: false,
        EnableMaskedOperations: false,
        EnableGatherScatter: false,
        AlignmentRequirement: 16);

    /// <summary>
    /// Creates a SIMD configuration with only the specified instruction set enabled.
    /// </summary>
    public static SimdConfiguration WithInstructionSet(bool avx512 = false, bool avx2 = false, bool sse42 = false, bool neon = false, bool sve = false) =>
        new(
            Enabled: avx512 || avx2 || sse42 || neon || sve,
            PreferredWidth: avx512 ? 512 : (avx2 ? 256 : 128),
            EnableAvx512: avx512,
            EnableAvx2: avx2,
            EnableSse42: sse42,
            EnableNeon: neon,
            EnableSve: sve);
}
