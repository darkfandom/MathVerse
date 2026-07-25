namespace MathVerse.Math.Distributed.IncrementalComputation;

using System.Collections.Concurrent;

/// <summary>Tracks which computation nodes are dirty and need recomputation.</summary>
public sealed class DirtyNodeTracker
{
    private readonly ConcurrentDictionary<string, bool> _dirtyNodes = new();
    private long _totalMarks;

    /// <summary>Gets the number of currently dirty nodes.</summary>
    public int DirtyCount => _dirtyNodes.Count;

    /// <summary>Gets the total number of times a node has been marked dirty since creation.</summary>
    public long TotalMarks => Interlocked.Read(ref _totalMarks);

    /// <summary>Marks the specified node as dirty.</summary>
    /// <param name="nodeId">The string identifier of the node to mark dirty.</param>
    public void MarkDirty(string nodeId)
    {
        _dirtyNodes[nodeId] = true;
        Interlocked.Increment(ref _totalMarks);
    }

    /// <summary>Marks multiple nodes as dirty.</summary>
    /// <param name="nodeIds">The node identifiers to mark dirty.</param>
    public void MarkDirtyBatch(IEnumerable<string> nodeIds)
    {
        foreach (var nodeId in nodeIds)
        {
            _dirtyNodes[nodeId] = true;
            Interlocked.Increment(ref _totalMarks);
        }
    }

    /// <summary>Returns whether the specified node is dirty.</summary>
    /// <param name="nodeId">The node identifier to check.</param>
    /// <returns>True if the node is dirty; false otherwise.</returns>
    public bool IsDirty(string nodeId)
    {
        return _dirtyNodes.ContainsKey(nodeId);
    }

    /// <summary>Returns all currently dirty node identifiers.</summary>
    /// <returns>An array of dirty node identifiers.</returns>
    public string[] GetDirtyNodes()
    {
        return _dirtyNodes.Keys.ToArray();
    }

    /// <summary>Returns the dirty node identifiers matching a given prefix.</summary>
    /// <param name="prefix">The prefix to filter by.</param>
    /// <returns>An array of matching dirty node identifiers.</returns>
    public string[] GetDirtyNodes(string prefix)
    {
        return _dirtyNodes.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .ToArray();
    }

    /// <summary>Clears the dirty state of a specific node.</summary>
    /// <param name="nodeId">The node identifier to clear.</param>
    /// <returns>True if the node was dirty and is now cleared; false if it was not dirty.</returns>
    public bool ClearNode(string nodeId)
    {
        return _dirtyNodes.TryRemove(nodeId, out _);
    }

    /// <summary>Clears the dirty state of all tracked nodes.</summary>
    public void Clear()
    {
        _dirtyNodes.Clear();
    }

    /// <summary>Returns whether any nodes are currently dirty.</summary>
    /// <returns>True if at least one node is dirty.</returns>
    public bool HasDirtyNodes()
    {
        return !_dirtyNodes.IsEmpty;
    }

    /// <summary>Returns a snapshot of the current dirty count.</summary>
    /// <returns>An approximation of the dirty node count.</returns>
    public int GetSnapshotCount()
    {
        return _dirtyNodes.Count;
    }
}
