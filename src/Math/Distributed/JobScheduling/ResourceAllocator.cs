namespace MathVerse.Math.Distributed.JobScheduling;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Allocates and tracks CPU core and memory resources for distributed tasks.
/// Thread-safe via <see cref="SemaphoreSlim"/>.
/// </summary>
public sealed class ResourceAllocator
{
    private readonly int _totalCores;
    private readonly long _totalMemoryBytes;
    private int _allocatedCores;
    private long _allocatedMemoryBytes;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceAllocator"/> class.
    /// </summary>
    /// <param name="totalCores">Total number of CPU cores available.</param>
    /// <param name="totalMemoryBytes">Total memory in bytes available.</param>
    public ResourceAllocator(int totalCores, long totalMemoryBytes)
    {
        _totalCores = totalCores;
        _totalMemoryBytes = totalMemoryBytes;
    }

    /// <summary>
    /// Asynchronously allocates the specified number of cores and bytes of memory.
    /// </summary>
    /// <param name="cores">Number of CPU cores to allocate.</param>
    /// <param name="memoryBytes">Number of bytes of memory to allocate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the allocation succeeded; false if insufficient resources.</returns>
    public async ValueTask<bool> AllocateAsync(int cores, long memoryBytes, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            int availableCores = _totalCores - _allocatedCores;
            long availableMemory = _totalMemoryBytes - _allocatedMemoryBytes;

            if (cores > availableCores || memoryBytes > availableMemory)
                return false;

            _allocatedCores += cores;
            _allocatedMemoryBytes += memoryBytes;
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Allocates the specified number of cores and bytes of memory synchronously.
    /// </summary>
    /// <param name="cores">Number of CPU cores to allocate.</param>
    /// <param name="memoryBytes">Number of bytes of memory to allocate.</param>
    /// <returns>True if the allocation succeeded; false if insufficient resources.</returns>
    public bool Allocate(int cores, long memoryBytes)
    {
        _lock.Wait();
        try
        {
            int availableCores = _totalCores - _allocatedCores;
            long availableMemory = _totalMemoryBytes - _allocatedMemoryBytes;

            if (cores > availableCores || memoryBytes > availableMemory)
                return false;

            _allocatedCores += cores;
            _allocatedMemoryBytes += memoryBytes;
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Releases previously allocated cores and memory.
    /// </summary>
    /// <param name="cores">Number of CPU cores to release.</param>
    /// <param name="memoryBytes">Number of bytes of memory to release.</param>
    public void Release(int cores, long memoryBytes)
    {
        _lock.Wait();
        try
        {
            _allocatedCores = System.Math.Max(0, _allocatedCores - cores);
            _allocatedMemoryBytes = System.Math.Max(0L, _allocatedMemoryBytes - memoryBytes);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gets the currently available resources.
    /// </summary>
    /// <returns>A tuple of available cores and available memory in bytes.</returns>
    public (int AvailableCores, long AvailableMemoryBytes) GetAvailableResources()
    {
        _lock.Wait();
        try
        {
            return (_totalCores - _allocatedCores, _totalMemoryBytes - _allocatedMemoryBytes);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gets the total resources managed by this allocator.
    /// </summary>
    /// <returns>A tuple of total cores and total memory in bytes.</returns>
    public (int TotalCores, long TotalMemoryBytes) GetTotalResources()
    {
        return (_totalCores, _totalMemoryBytes);
    }
}
