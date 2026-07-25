using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.Spatial;

/// <summary>Represents a single entry in an R-tree with a bounding box and associated identifier.</summary>
/// <param name="Bounds">The axis-aligned bounding box of the entry.</param>
/// <param name="Id">The unique identifier associated with this entry.</param>
public readonly record struct RTreeEntry(BoundingBox2D Bounds, int Id);

/// <summary>A 2D R-tree spatial index for bounding box data, using a quadratic-cost split heuristic.</summary>
public class RTree2D
{
    private const double Tolerance = 1e-10;
    private readonly int _maxEntries;
    private RTreeNode _root;
    private int _count;

    /// <summary>Gets the total number of entries stored in the R-tree.</summary>
    public int Count => _count;

    /// <summary>Internal node of the R-tree storing children, leaf entries, and minimum bounding rectangle.</summary>
    internal sealed class RTreeNode
    {
        /// <summary>The minimum bounding rectangle enclosing all children or entries.</summary>
        public BoundingBox2D MBR { get; set; }

        /// <summary>Whether this node is a leaf node.</summary>
        public bool IsLeaf { get; set; }

        /// <summary>Child nodes (internal nodes only).</summary>
        public List<RTreeNode> Children { get; set; } = new();

        /// <summary>Leaf entries (leaf nodes only).</summary>
        public List<RTreeEntry> Entries { get; set; } = new();
    }

    /// <summary>Initializes a new <see cref="RTree2D"/> with the specified maximum entries per node.</summary>
    /// <param name="maxEntries">The maximum number of entries or children per node before splitting.</param>
    public RTree2D(int maxEntries = 16)
    {
        _maxEntries = maxEntries;
        _root = new RTreeNode { IsLeaf = true, MBR = new BoundingBox2D(Point2D.Origin, Point2D.Origin) };
        _count = 0;
    }

    /// <summary>Inserts a bounding box entry with the given identifier into the R-tree.</summary>
    /// <param name="bounds">The axis-aligned bounding box to insert.</param>
    /// <param name="id">The identifier to associate with this entry.</param>
    public void Insert(BoundingBox2D bounds, int id)
    {
        RTreeEntry entry = new RTreeEntry(bounds, id);
        InsertEntry(_root, entry, 0);
        _count++;
    }

    /// <summary>Searches the R-tree for all entries whose bounding boxes intersect the query box.</summary>
    /// <param name="query">The query bounding box.</param>
    /// <returns>An immutable array of identifiers of matching entries.</returns>
    public ImmutableArray<int> Search(BoundingBox2D query)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        SearchRecursive(_root, query, result);
        return result.ToImmutable();
    }

    /// <summary>Removes the entry with the specified identifier from the R-tree.</summary>
    /// <param name="id">The identifier of the entry to remove.</param>
    /// <returns><c>true</c> if the entry was found and removed; otherwise, <c>false</c>.</returns>
    public bool Remove(int id)
    {
        bool removed = RemoveRecursive(_root, id);
        if (removed) _count--;
        return removed;
    }

    private void InsertEntry(RTreeNode node, RTreeEntry entry, int depth)
    {
        if (node.IsLeaf)
        {
            node.Entries.Add(entry);
            node.MBR = node.MBR.Union(entry.Bounds);
            if (node.Entries.Count > _maxEntries && depth < 100)
                SplitLeaf(node, depth);
            return;
        }

        RTreeNode bestChild = ChooseBestChild(node, entry.Bounds);
        InsertEntry(bestChild, entry, depth + 1);
        node.MBR = ComputeMBR(node.Children);
    }

    private void SplitLeaf(RTreeNode node, int depth)
    {
        List<RTreeEntry> entries = node.Entries;
        node.Entries = new List<RTreeEntry>();
        node.IsLeaf = false;

        int mid = entries.Count / 2;
        (List<RTreeEntry> group1, List<RTreeEntry> group2) = QuadraticSplit(entries);
        node.Children.Clear();

        RTreeNode child1 = new RTreeNode { IsLeaf = true, Entries = group1, MBR = ComputeMBR(group1) };
        RTreeNode child2 = new RTreeNode { IsLeaf = true, Entries = group2, MBR = ComputeMBR(group2) };
        node.Children.Add(child1);
        node.Children.Add(child2);
        node.MBR = child1.MBR.Union(child2.MBR);
    }

    private (List<RTreeEntry> Group1, List<RTreeEntry> Group2) QuadraticSplit(List<RTreeEntry> entries)
    {
        int n = entries.Count;
        int m = n / 2;
        var group1 = new List<RTreeEntry>(m);
        var group2 = new List<RTreeEntry>(n - m);
        bool[] assigned = new bool[n];

        int seed1 = 0, seed2 = 1;
        double maxArea = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                double area = entries[i].Bounds.Union(entries[j].Bounds).Area - entries[i].Bounds.Area - entries[j].Bounds.Area;
                if (area > maxArea)
                {
                    maxArea = area;
                    seed1 = i;
                    seed2 = j;
                }
            }
        }

        group1.Add(entries[seed1]);
        group2.Add(entries[seed2]);
        assigned[seed1] = true;
        assigned[seed2] = true;

        BoundingBox2D mbr1 = entries[seed1].Bounds;
        BoundingBox2D mbr2 = entries[seed2].Bounds;

        while (group1.Count < m && group1.Count + (n - group1.Count - group2.Count) > m)
        {
            double bestDiff = double.MinValue;
            int bestIdx = -1;
            bool assignToGroup1 = true;
            for (int i = 0; i < n; i++)
            {
                if (assigned[i]) continue;
                double area1 = mbr1.Union(entries[i].Bounds).Area;
                double area2 = mbr2.Union(entries[i].Bounds).Area;
                double increase1 = area1 - mbr1.Area;
                double increase2 = area2 - mbr2.Area;
                double diff = System.Math.Abs(increase1 - increase2);
                if (diff > bestDiff)
                {
                    bestDiff = diff;
                    bestIdx = i;
                    assignToGroup1 = increase1 <= increase2;
                }
            }
            if (bestIdx < 0) break;
            if (assignToGroup1)
            {
                group1.Add(entries[bestIdx]);
                mbr1 = mbr1.Union(entries[bestIdx].Bounds);
            }
            else
            {
                group2.Add(entries[bestIdx]);
                mbr2 = mbr2.Union(entries[bestIdx].Bounds);
            }
            assigned[bestIdx] = true;
        }

        for (int i = 0; i < n; i++)
        {
            if (!assigned[i])
                group2.Add(entries[i]);
        }

        return (group1, group2);
    }

    private void SearchRecursive(RTreeNode node, BoundingBox2D query, ImmutableArray<int>.Builder result)
    {
        if (!node.MBR.Intersects(query))
            return;
        if (node.IsLeaf)
        {
            foreach (RTreeEntry entry in node.Entries)
            {
                if (entry.Bounds.Intersects(query))
                    result.Add(entry.Id);
            }
            return;
        }
        foreach (RTreeNode child in node.Children)
            SearchRecursive(child, query, result);
    }

    private bool RemoveRecursive(RTreeNode node, int id)
    {
        if (node.IsLeaf)
        {
            int before = node.Entries.Count;
            node.Entries.RemoveAll(e => e.Id == id);
            if (node.Entries.Count < before)
            {
                node.MBR = node.Entries.Count > 0 ? ComputeMBR(node.Entries) : new BoundingBox2D(Point2D.Origin, Point2D.Origin);
                return true;
            }
            return false;
        }
        foreach (RTreeNode child in node.Children)
        {
            if (RemoveRecursive(child, id))
            {
                node.MBR = ComputeMBR(node.Children);
                return true;
            }
        }
        return false;
    }

    private static RTreeNode ChooseBestChild(RTreeNode node, BoundingBox2D bounds)
    {
        RTreeNode best = node.Children[0];
        double bestArea = best.MBR.Union(bounds).Area - best.MBR.Area;
        for (int i = 1; i < node.Children.Count; i++)
        {
            double area = node.Children[i].MBR.Union(bounds).Area - node.Children[i].MBR.Area;
            if (area < bestArea)
            {
                bestArea = area;
                best = node.Children[i];
            }
        }
        return best;
    }

    private static BoundingBox2D ComputeMBR(List<RTreeEntry> entries)
    {
        if (entries.Count == 0)
            return new BoundingBox2D(Point2D.Origin, Point2D.Origin);
        BoundingBox2D mbr = entries[0].Bounds;
        for (int i = 1; i < entries.Count; i++)
            mbr = mbr.Union(entries[i].Bounds);
        return mbr;
    }

    private static BoundingBox2D ComputeMBR(List<RTreeNode> children)
    {
        if (children.Count == 0)
            return new BoundingBox2D(Point2D.Origin, Point2D.Origin);
        BoundingBox2D mbr = children[0].MBR;
        for (int i = 1; i < children.Count; i++)
            mbr = mbr.Union(children[i].MBR);
        return mbr;
    }
}
