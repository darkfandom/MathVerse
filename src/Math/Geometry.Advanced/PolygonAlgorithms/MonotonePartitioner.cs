using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.PolygonAlgorithms;

/// <summary>
/// Provides Y-monotone partitioning and monotone polygon triangulation.
/// A polygon is Y-monotone if any horizontal line intersects it in at most one segment.
/// Partitioning converts any simple polygon into monotone sub-polygons in O(n log n).
/// </summary>
public static class MonotonePartitioner
{
    private const double Tolerance = 1e-10;

    private enum VertexType
    {
        Start,
        End,
        Split,
        Merge,
        Regular
    }

    /// <summary>
    /// Partitions a simple polygon into Y-monotone sub-polygons using the trapezoidal sweep algorithm.
    /// Returns an array of sub-polygons, each defined by its vertices in order.
    /// </summary>
    /// <param name="polygon">The polygon vertices in winding order.</param>
    /// <returns>An immutable array of monotone sub-polygons.</returns>
    public static ImmutableArray<ImmutableArray<Point2D>> Partition(ImmutableArray<Point2D> polygon)
    {
        var result = ImmutableArray.CreateBuilder<ImmutableArray<Point2D>>();
        int n = polygon.Length;
        if (n < 3) return result.ToImmutable();
        if (n == 3) { result.Add(polygon); return result.ToImmutable(); }

        var vertexTypes = new VertexType[n];
        ClassifyVertices(polygon, vertexTypes);

        var diagonals = new List<(int from, int to)>();
        var statusTree = new SortedList<double, (int segIndex, double y)>(new DuplicateKeyComparer());
        var edgeTable = new Dictionary<int, int>();

        var sweepEvents = new List<(double y, int index, VertexType type)>();
        for (int i = 0; i < n; i++)
            sweepEvents.Add((polygon[i].Y, i, vertexTypes[i]));
        sweepEvents.Sort((a, b) =>
        {
            int cmp = b.y.CompareTo(a.y);
            if (cmp != 0) return cmp;
            return a.index.CompareTo(b.index);
        });

        var helper = new Dictionary<int, int>();

        for (int si = 0; si < sweepEvents.Count; si++)
        {
            var (y, vi, vtype) = sweepEvents[si];
            int next = (vi + 1) % n;
            int prev = (vi + n - 1) % n;

            switch (vtype)
            {
                case VertexType.Start:
                    edgeTable[vi] = next;
                    helper[vi] = vi;
                    InsertStatus(statusTree, vi, polygon[vi].Y, polygon[next].Y, polygon[vi].X);
                    break;

                case VertexType.End:
                    if (helper.TryGetValue(next, out int helperNext) && vertexTypes[helperNext] == VertexType.Merge)
                    {
                        diagonals.Add((vi, helperNext));
                    }
                    RemoveStatus(statusTree, next, polygon);
                    edgeTable.Remove(next);
                    break;

                case VertexType.Split:
                {
                    int above = FindAboveEdge(statusTree, polygon[vi].X, polygon[vi].Y);
                    if (above >= 0 && helper.TryGetValue(above, out int helperAbove))
                    {
                        diagonals.Add((vi, helperAbove));
                        helper[above] = vi;
                    }
                    edgeTable[vi] = next;
                    helper[vi] = vi;
                    InsertStatus(statusTree, vi, polygon[vi].Y, polygon[next].Y, polygon[vi].X);
                    break;
                }

                case VertexType.Merge:
                {
                    if (helper.TryGetValue(next, out int helperNext2) && vertexTypes[helperNext2] == VertexType.Merge)
                    {
                        diagonals.Add((vi, helperNext2));
                    }
                    RemoveStatus(statusTree, next, polygon);
                    edgeTable.Remove(next);

                    int above2 = FindAboveEdge(statusTree, polygon[vi].X, polygon[vi].Y);
                    if (above2 >= 0 && helper.TryGetValue(above2, out int helperAbove2) && vertexTypes[helperAbove2] == VertexType.Merge)
                    {
                        diagonals.Add((vi, helperAbove2));
                        helper[above2] = vi;
                    }
                    else if (above2 >= 0)
                    {
                        helper[above2] = vi;
                    }
                    break;
                }

                case VertexType.Regular:
                {
                    bool isLeftSide = polygon[vi].X < polygon[next].X;
                    if (!isLeftSide)
                    {
                        int aboveReg = FindAboveEdge(statusTree, polygon[vi].X, polygon[vi].Y);
                        if (aboveReg >= 0 && helper.TryGetValue(aboveReg, out int helperReg) && vertexTypes[helperReg] == VertexType.Merge)
                        {
                            diagonals.Add((vi, helperReg));
                            helper[aboveReg] = vi;
                        }
                        else if (aboveReg >= 0)
                        {
                            helper[aboveReg] = vi;
                        }
                    }
                    else
                    {
                        edgeTable[vi] = next;
                        helper[vi] = vi;
                        InsertStatus(statusTree, vi, polygon[vi].Y, polygon[next].Y, polygon[vi].X);
                    }
                    break;
                }
            }
        }

        if (diagonals.Count == 0)
        {
            result.Add(polygon);
            return result.ToImmutable();
        }

        return BuildMonotonePolygons(polygon, diagonals);
    }

    /// <summary>
    /// Triangulates a Y-monotone polygon in O(n) time using a sweep from top to bottom.
    /// The polygon must be Y-monotone for this to produce correct results.
    /// </summary>
    /// <param name="polygon">A Y-monotone polygon with vertices in order.</param>
    /// <returns>An immutable array of triangle vertex indices (groups of 3).</returns>
    public static ImmutableArray<int> TriangulateMonotone(ImmutableArray<Point2D> polygon)
    {
        var builder = ImmutableArray.CreateBuilder<int>();
        int n = polygon.Length;
        if (n < 3) return builder.ToImmutable();

        int topIdx = 0;
        double topY = polygon[0].Y;
        for (int i = 1; i < n; i++)
        {
            if (polygon[i].Y > topY || (System.Math.Abs(polygon[i].Y - topY) < Tolerance && polygon[i].X < polygon[topIdx].X))
            {
                topY = polygon[i].Y;
                topIdx = i;
            }
        }

        var sorted = new List<int>(n);
        for (int i = 0; i < n; i++) sorted.Add(i);
        sorted.Sort((a, b) =>
        {
            int cmp = polygon[b].Y.CompareTo(polygon[a].Y);
            if (cmp != 0) return cmp;
            return polygon[a].X.CompareTo(polygon[b].X);
        });

        var stack = new Stack<int>();
        stack.Push(sorted[0]);
        stack.Push(sorted[1]);

        for (int i = 2; i < n; i++)
        {
            int vi = sorted[i];
            int stackTop = stack.Peek();
            bool sameSide = IsOnSameSide(polygon, stackTop, vi, topIdx);

            if (sameSide)
            {
                int prev = stack.Pop();
                while (stack.Count > 0)
                {
                    int curr = stack.Peek();
                    builder.Add(vi);
                    builder.Add(prev);
                    builder.Add(curr);
                    prev = curr;
                    stack.Pop();
                }
                stack.Push(sorted[i - 1 < 0 ? 0 : System.Math.Max(0, i - 1)]);
                stack.Push(vi);
            }
            else
            {
                int last = stack.Pop();
                builder.Add(vi);
                builder.Add(last);
                builder.Add(stack.Peek());

                while (stack.Count > 1)
                {
                    int curr = stack.Pop();
                    builder.Add(vi);
                    builder.Add(curr);
                    builder.Add(stack.Peek());
                }

                stack.Clear();
                stack.Push(sorted[i - 1 < 0 ? 0 : System.Math.Max(0, i - 1)]);
                stack.Push(vi);
            }
        }

        return builder.ToImmutable();
    }

    private static bool IsOnSameSide(ImmutableArray<Point2D> polygon, int idx1, int idx2, int topIdx)
    {
        int n = polygon.Length;
        int bottomIdx = 0;
        double bottomY = polygon[0].Y;
        for (int i = 1; i < n; i++)
        {
            if (polygon[i].Y < bottomY)
            {
                bottomY = polygon[i].Y;
                bottomIdx = i;
            }
        }

        double midX = (polygon[topIdx].X + polygon[bottomIdx].X) * 0.5;
        return (polygon[idx1].X < midX) == (polygon[idx2].X < midX);
    }

    private static void ClassifyVertices(ImmutableArray<Point2D> polygon, VertexType[] types)
    {
        int n = polygon.Length;
        for (int i = 0; i < n; i++)
        {
            int prev = (i + n - 1) % n;
            int next = (i + 1) % n;
            types[i] = ClassifyVertex(polygon, prev, i, next);
        }
    }

    private static VertexType ClassifyVertex(ImmutableArray<Point2D> polygon, int prev, int curr, int next)
    {
        double yPrev = polygon[prev].Y;
        double yCurr = polygon[curr].Y;
        double yNext = polygon[next].Y;

        bool isLocalMax = yCurr > yPrev && yCurr > yNext;
        bool isLocalMin = yCurr < yPrev && yCurr < yNext;

        double cross = (polygon[curr].X - polygon[prev].X) * (polygon[next].Y - polygon[curr].Y)
                     - (polygon[curr].Y - polygon[prev].Y) * (polygon[next].X - polygon[curr].X);

        if (isLocalMax)
            return cross > 0 ? VertexType.Start : VertexType.End;
        if (isLocalMin)
            return cross > 0 ? VertexType.End : VertexType.Start;

        if (yCurr > yPrev && yCurr < yNext)
            return cross > 0 ? VertexType.Split : VertexType.Merge;
        if (yCurr < yPrev && yCurr > yNext)
            return cross > 0 ? VertexType.Merge : VertexType.Split;

        return VertexType.Regular;
    }

    private static void InsertStatus(SortedList<double, (int segIndex, double y)> tree, int segIndex, double yTop, double yBot, double x)
    {
        double key = x + (double)segIndex * 1e-15;
        int attempts = 0;
        while (tree.ContainsKey(key) && attempts < 100)
        {
            key += 1e-14;
            attempts++;
        }
        tree[key] = (segIndex, yTop);
    }

    private static void RemoveStatus(SortedList<double, (int segIndex, double y)> tree, int segIndex, ImmutableArray<Point2D> polygon)
    {
        for (int i = tree.Count - 1; i >= 0; i--)
        {
            if (tree.Values[i].segIndex == segIndex)
            {
                tree.RemoveAt(i);
                break;
            }
        }
    }

    private static int FindAboveEdge(SortedList<double, (int segIndex, double y)> tree, double x, double y)
    {
        int bestIdx = -1;
        for (int i = 0; i < tree.Count; i++)
        {
            int segIdx = tree.Values[i].segIndex;
            bestIdx = segIdx;
        }
        return bestIdx;
    }

    private static ImmutableArray<ImmutableArray<Point2D>> BuildMonotonePolygons(
        ImmutableArray<Point2D> polygon, List<(int from, int to)> diagonals)
    {
        int n = polygon.Length;
        var used = new bool[n];
        var result = ImmutableArray.CreateBuilder<ImmutableArray<Point2D>>();

        var adj = new List<int>[n];
        for (int i = 0; i < n; i++) adj[i] = new List<int>();

        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;
            adj[i].Add(next);
            adj[next].Add(i);
        }

        foreach (var (from, to) in diagonals)
        {
            if (from >= 0 && from < n && to >= 0 && to < n)
            {
                adj[from].Add(to);
                adj[to].Add(from);
            }
        }

        var visited = new bool[n];
        for (int start = 0; start < n; start++)
        {
            if (visited[start]) continue;

            var chain = new List<int>();
            int current = start;
            int prevVertex = -1;

            while (current >= 0 && current < n && !visited[current])
            {
                visited[current] = true;
                chain.Add(current);

                int nextVertex = -1;

                foreach (int neighbor in adj[current])
                {
                    if (neighbor == prevVertex) continue;

                    int chainNext = (current + 1) % n;
                    int chainPrev = (current + n - 1) % n;

                    if (neighbor == chainNext || neighbor == chainPrev)
                    {
                        nextVertex = neighbor;
                        break;
                    }
                }

                if (nextVertex == -1)
                {
                    foreach (int neighbor in adj[current])
                    {
                        if (neighbor == prevVertex) continue;
                        if (!visited[neighbor])
                        {
                            nextVertex = neighbor;
                            break;
                        }
                    }
                }

                if (nextVertex == -1) break;
                prevVertex = current;
                current = nextVertex;
            }

            if (chain.Count >= 3)
            {
                var subPoly = ImmutableArray.CreateBuilder<Point2D>(chain.Count);
                foreach (int idx in chain) subPoly.Add(polygon[idx]);
                result.Add(subPoly.ToImmutable());
            }
        }

        return result.ToImmutable();
    }

    private sealed class DuplicateKeyComparer : IComparer<double>
    {
        public int Compare(double x, double y)
        {
            int result = x.CompareTo(y);
            return result == 0 ? 1 : result;
        }
    }
}
