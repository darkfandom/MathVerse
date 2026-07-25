using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.SweepLine;

/// <summary>
/// Represents the type of event encountered during the sweep line algorithm.
/// </summary>
public enum SweepEventType
{
    /// <summary>The left endpoint of a segment (lower x-coordinate).</summary>
    LeftEndpoint,

    /// <summary>The right endpoint of a segment (higher x-coordinate).</summary>
    RightEndpoint,

    /// <summary>An intersection point between two segments.</summary>
    Intersection
}

/// <summary>
/// Represents an event in the sweep line algorithm's event queue.
/// Events are ordered by Y coordinate (top to bottom), then by type priority,
/// then by X coordinate for the same priority level.
/// </summary>
/// <param name="Y">The Y coordinate of the event (sweep line position).</param>
/// <param name="Type">The type of event.</param>
/// <param name="SegmentIndex">The index of the primary segment involved in this event.</param>
public readonly record struct SweepEvent(double Y, SweepEventType Type, int SegmentIndex)
{
    /// <summary>
    /// Gets a secondary segment index for intersection events.
    /// Defaults to -1 for endpoint events.
    /// </summary>
    public int SecondarySegmentIndex { get; init; } = -1;

    /// <summary>
    /// Gets the X coordinate of the event point.
    /// For endpoint events, this is the endpoint's X coordinate.
    /// For intersection events, this is computed from the intersecting segments.
    /// </summary>
    public double X { get; init; } = 0;
}

/// <summary>
/// Represents the result of a segment intersection detection.
/// Contains the intersection point and the indices of the two intersecting segments.
/// </summary>
/// <param name="Point">The intersection point.</param>
/// <param name="Segment1Index">The index of the first segment.</param>
/// <param name="Segment2Index">The index of the second segment.</param>
public readonly record struct IntersectionResult(Point2D Point, int Segment1Index, int Segment2Index);

/// <summary>
/// Provides a full Bentley-Ottmann sweep line implementation for finding all pairwise
/// intersections among a set of line segments. Uses an event queue (priority queue) and
/// a balanced BST status structure ordered by sweep line position.
/// Time complexity: O((n + k) log n) where n is the number of segments and k is the number of intersections.
/// </summary>
public static class SweepLineEngine
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Finds all intersections among a set of line segments using the Bentley-Ottmann sweep line algorithm.
    /// Maintains an event queue sorted by Y (top to bottom), and a balanced status tree of active edges
    /// ordered by their X position at the current sweep line Y coordinate.
    /// </summary>
    /// <param name="segments">The line segments to check for intersections.</param>
    /// <returns>An immutable array of intersection results, each containing the intersection point and segment indices.</returns>
    public static ImmutableArray<IntersectionResult> FindAllIntersections(ImmutableArray<Segment2D> segments)
    {
        var results = ImmutableArray.CreateBuilder<IntersectionResult>();
        int n = segments.Length;
        if (n < 2) return results.ToImmutable();

        var eventQueue = new EventQueue();
        var statusTree = new StatusTree(segments);
        var foundIntersections = new HashSet<(int, int)>();

        for (int i = 0; i < n; i++)
        {
            Point2D left, right;
            if (segments[i].P1.X < segments[i].P2.X ||
                (System.Math.Abs(segments[i].P1.X - segments[i].P2.X) < Tolerance && segments[i].P1.Y < segments[i].P2.Y))
            {
                left = segments[i].P1;
                right = segments[i].P2;
            }
            else
            {
                left = segments[i].P2;
                right = segments[i].P1;
            }

            eventQueue.Enqueue(new SweepEvent(left.Y, SweepEventType.LeftEndpoint, i)
            { X = left.X });
            eventQueue.Enqueue(new SweepEvent(right.Y, SweepEventType.RightEndpoint, i)
            { X = right.X });
        }

        while (eventQueue.HasEvents)
        {
            var evt = eventQueue.Dequeue();

            HandleEvent(evt, segments, statusTree, eventQueue, foundIntersections, results);
        }

        return results.ToImmutable();
    }

    private static void HandleEvent(
        SweepEvent evt,
        ImmutableArray<Segment2D> segments,
        StatusTree statusTree,
        EventQueue eventQueue,
        HashSet<(int, int)> foundIntersections,
        ImmutableArray<IntersectionResult>.Builder results)
    {
        switch (evt.Type)
        {
            case SweepEventType.LeftEndpoint:
                statusTree.Insert(evt.SegmentIndex, evt.Y);
                var neighbors = statusTree.GetNeighbors(evt.SegmentIndex, evt.Y);
                if (neighbors.above >= 0)
                    CheckForIntersection(evt.SegmentIndex, neighbors.above, segments, statusTree, eventQueue, evt.Y, foundIntersections, results);
                if (neighbors.below >= 0)
                    CheckForIntersection(neighbors.below, evt.SegmentIndex, segments, statusTree, eventQueue, evt.Y, foundIntersections, results);
                break;

            case SweepEventType.RightEndpoint:
                var rightNeighbors = statusTree.GetNeighbors(evt.SegmentIndex, evt.Y);
                statusTree.Remove(evt.SegmentIndex, evt.Y);
                if (rightNeighbors.above >= 0 && rightNeighbors.below >= 0)
                    CheckForIntersection(rightNeighbors.below, rightNeighbors.above, segments, statusTree, eventQueue, evt.Y, foundIntersections, results);
                break;

            case SweepEventType.Intersection:
                int segI = evt.SegmentIndex;
                int segJ = evt.SecondarySegmentIndex;

                if (segI < 0 || segJ < 0 || segI >= segments.Length || segJ >= segments.Length) break;

                statusTree.Swap(segI, segJ, evt.Y);

                var swapAboveI = statusTree.GetAbove(segI, evt.Y);
                if (swapAboveI >= 0)
                    CheckForIntersection(segI, swapAboveI, segments, statusTree, eventQueue, evt.Y, foundIntersections, results);

                var swapBelowJ = statusTree.GetBelow(segJ, evt.Y);
                if (swapBelowJ >= 0)
                    CheckForIntersection(swapBelowJ, segJ, segments, statusTree, eventQueue, evt.Y, foundIntersections, results);
                break;
        }
    }

    private static void CheckForIntersection(
        int segI, int segJ,
        ImmutableArray<Segment2D> segments,
        StatusTree statusTree,
        EventQueue eventQueue,
        double sweepY,
        HashSet<(int, int)> foundIntersections,
        ImmutableArray<IntersectionResult>.Builder results)
    {
        if (segI < 0 || segJ < 0 || segI >= segments.Length || segJ >= segments.Length) return;
        if (segI == segJ) return;

        int pairKey = System.Math.Min(segI, segJ);
        int pairVal = System.Math.Max(segI, segJ);

        var hit = segments[segI].Intersect(segments[segJ]);
        if (!hit.hit) return;

        Point2D intersection = hit.point;

        if (intersection.Y > sweepY + Tolerance) return;

        var pair = (pairKey, pairVal);
        if (foundIntersections.Contains(pair)) return;

        foundIntersections.Add(pair);
        results.Add(new IntersectionResult(intersection, segI, segJ));

        var intersectionEvt = new SweepEvent(intersection.Y, SweepEventType.Intersection, segI)
        {
            SecondarySegmentIndex = segJ,
            X = intersection.X
        };
        eventQueue.Enqueue(intersectionEvt);
    }
}

/// <summary>
/// Internal balanced BST status structure for the sweep line algorithm.
/// Maintains active segments ordered by their X position at the current sweep line Y coordinate.
/// Uses a sorted list with binary search for efficient insertion, removal, and neighbor queries.
/// </summary>
internal sealed class StatusTree
{
    private const double Tolerance = 1e-10;

    private readonly ImmutableArray<Segment2D> _segments;
    private readonly List<int> _activeSegments;
    private readonly List<double> _keys;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusTree"/> class.
    /// </summary>
    /// <param name="segments">The segments being processed.</param>
    public StatusTree(ImmutableArray<Segment2D> segments)
    {
        _segments = segments;
        _activeSegments = new List<int>();
        _keys = new List<double>();
    }

    /// <summary>
    /// Inserts a segment into the active status at the given sweep line Y coordinate.
    /// </summary>
    /// <param name="segIndex">The segment index.</param>
    /// <param name="sweepY">The current sweep line Y coordinate.</param>
    public void Insert(int segIndex, double sweepY)
    {
        double key = GetKey(segIndex, sweepY);
        int pos = FindInsertionPosition(key);
        _activeSegments.Insert(pos, segIndex);
        _keys.Insert(pos, key);
    }

    /// <summary>
    /// Removes a segment from the active status.
    /// </summary>
    /// <param name="segIndex">The segment index to remove.</param>
    /// <param name="sweepY">The current sweep line Y coordinate.</param>
    public void Remove(int segIndex, double sweepY)
    {
        for (int i = 0; i < _activeSegments.Count; i++)
        {
            if (_activeSegments[i] == segIndex)
            {
                _activeSegments.RemoveAt(i);
                _keys.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// Gets the neighboring segments (above and below) of a given segment in the status tree.
    /// </summary>
    /// <param name="segIndex">The segment index.</param>
    /// <param name="sweepY">The current sweep line Y coordinate.</param>
    /// <returns>A tuple with the above and below segment indices (-1 if none).</returns>
    public (int above, int below) GetNeighbors(int segIndex, double sweepY)
    {
        int pos = _activeSegments.IndexOf(segIndex);
        if (pos < 0) return (-1, -1);

        int above = pos + 1 < _activeSegments.Count ? _activeSegments[pos + 1] : -1;
        int below = pos - 1 >= 0 ? _activeSegments[pos - 1] : -1;
        return (above, below);
    }

    /// <summary>
    /// Gets the segment immediately above a given segment.
    /// </summary>
    /// <param name="segIndex">The segment index.</param>
    /// <param name="sweepY">The current sweep line Y coordinate.</param>
    /// <returns>The segment index above, or -1 if at the top.</returns>
    public int GetAbove(int segIndex, double sweepY)
    {
        int pos = _activeSegments.IndexOf(segIndex);
        if (pos < 0 || pos + 1 >= _activeSegments.Count) return -1;
        return _activeSegments[pos + 1];
    }

    /// <summary>
    /// Gets the segment immediately below a given segment.
    /// </summary>
    /// <param name="segIndex">The segment index.</param>
    /// <param name="sweepY">The current sweep line Y coordinate.</param>
    /// <returns>The segment index below, or -1 if at the bottom.</returns>
    public int GetBelow(int segIndex, double sweepY)
    {
        int pos = _activeSegments.IndexOf(segIndex);
        if (pos <= 0) return -1;
        return _activeSegments[pos - 1];
    }

    /// <summary>
    /// Swaps two adjacent segments in the status tree (used when an intersection is found).
    /// </summary>
    /// <param name="segI">The first segment index.</param>
    /// <param name="segJ">The second segment index.</param>
    /// <param name="sweepY">The current sweep line Y coordinate.</param>
    public void Swap(int segI, int segJ, double sweepY)
    {
        int posI = _activeSegments.IndexOf(segI);
        int posJ = _activeSegments.IndexOf(segJ);
        if (posI < 0 || posJ < 0) return;

        if (System.Math.Abs(posI - posJ) != 1) return;

        if (posI > posJ)
        {
            int temp = posI;
            posI = posJ;
            posJ = temp;
        }

        _activeSegments[posI] = segJ;
        _activeSegments[posJ] = segI;
        _keys[posI] = GetKey(segJ, sweepY);
        _keys[posJ] = GetKey(segI, sweepY);
    }

    private double GetKey(int segIndex, double sweepY)
    {
        Segment2D seg = _segments[segIndex];
        double dx = seg.P2.X - seg.P1.X;
        double dy = seg.P2.Y - seg.P1.Y;

        if (System.Math.Abs(dy) < Tolerance)
            return (seg.P1.X + seg.P2.X) * 0.5;

        double t = (sweepY - seg.P1.Y) / dy;
        return seg.P1.X + t * dx;
    }

    private int FindInsertionPosition(double key)
    {
        int lo = 0, hi = _keys.Count;
        while (lo < hi)
        {
            int mid = (lo + hi) / 2;
            if (_keys[mid] < key) lo = mid + 1;
            else hi = mid;
        }
        return lo;
    }
}
