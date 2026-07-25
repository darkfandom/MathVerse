namespace MathVerse.Math.HPC.GpuBackend;

public enum GpuArchitecture
{
    Unknown,
    Cuda,
    Rocm,
    Metal,
    Vulkan,
    DirectCompute,
    OpenCl,
    NvidiaAmpere,
    NvidiaHopper,
    NvidiaBlackwell,
    AmdRdna2,
    AmdRdna3,
    AmdCdna2,
    AmdCdna3,
    IntelXe,
    IntelXe2,
    AppleM1,
    AppleM2,
    AppleM3,
    AppleM4
}

public static class GpuArchitectureExtensions
{
    public static string ToVendorString(this GpuArchitecture arch) => arch switch
    {
        GpuArchitecture.Cuda or
        GpuArchitecture.NvidiaAmpere or
        GpuArchitecture.NvidiaHopper or
        GpuArchitecture.NvidiaBlackwell => "NVIDIA",
        
        GpuArchitecture.Rocm or
        GpuArchitecture.AmdRdna2 or
        GpuArchitecture.AmdRdna3 or
        GpuArchitecture.AmdCdna2 or
        GpuArchitecture.AmdCdna3 => "AMD",
        
        GpuArchitecture.Metal or
        GpuArchitecture.AppleM1 or
        GpuArchitecture.AppleM2 or
        GpuArchitecture.AppleM3 or
        GpuArchitecture.AppleM4 => "Apple",
        
        GpuArchitecture.IntelXe or
        GpuArchitecture.IntelXe2 => "Intel",
        
        GpuArchitecture.Vulkan or
        GpuArchitecture.OpenCl or
        GpuArchitecture.DirectCompute => "Multi-vendor",
        
        _ => "Unknown"
    };
    
    public static bool IsNvidia(this GpuArchitecture arch) => arch.ToVendorString() == "NVIDIA";
    public static bool IsAmd(this GpuArchitecture arch) => arch.ToVendorString() == "AMD";
    public static bool IsApple(this GpuArchitecture arch) => arch.ToVendorString() == "Apple";
    public static bool IsIntel(this GpuArchitecture arch) => arch.ToVendorString() == "Intel";
    public static bool SupportsRayTracing(this GpuArchitecture arch) => arch is GpuArchitecture.NvidiaAmpere or GpuArchitecture.NvidiaHopper or GpuArchitecture.NvidiaBlackwell or GpuArchitecture.AmdRdna2 or GpuArchitecture.AmdRdna3 or GpuArchitecture.IntelXe2;
    public static bool SupportsTensorCores(this GpuArchitecture arch) => arch is GpuArchitecture.NvidiaAmpere or GpuArchitecture.NvidiaHopper or GpuArchitecture.NvidiaBlackwell or GpuArchitecture.AmdCdna2 or GpuArchitecture.AmdCdna3 or GpuArchitecture.IntelXe or GpuArchitecture.IntelXe2;
}