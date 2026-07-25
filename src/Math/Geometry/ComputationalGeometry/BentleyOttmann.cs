using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.ComputationalGeometry;

/// <summary>Implements the Bentley-Ottmann sweep-line algorithm for finding all intersections among a set of segments.</summary>
public static class BentleyOttmann
{
    /// <summary>Finds all intersection points among the given line segments.</summary>
    /// <param name="segments">The input segments.</param>
    /// <returns>An immutable array of intersection points.</returns>
    public static ImmutableArray<Point2D> FindIntersections(IReadOnlyList<Segment2D> segments)
    {
        if (segments.Count < 2) return ImmutableArray<Point2D>.Empty;

        var events = new SortedSet< SweepEvent>(SweepEvent.Comparer);
        var status = new List<int>();
        var intersections = new HashSet<(int, int)>();
        var result = ImmutableArray.CreateBuilder<Point2D>();

        for (int i = 0; i < segments.Count; i++)
        {
            Point2D left = segments[i].P1.X <= segments[i].P2.X ? segments[i].P1 : segments[i].P2;
            Point2D right = segments[i].P1.X <= segments[i].P2.X ? segments[i].P2 : segments[i].P1;
            events.Add(new SweepEvent(left.X, left.Y, i, EventType.Left));
            events.Add(new SweepEvent(right.X, right.Y, i, EventType.Right));
        }

        foreach (SweepEvent evt in events)
        {
            if (evt.Type == EventType.Left)
            {
                int segIdx = evt.SegmentIndex;
                status.Add(segIdx);
                status.Sort((a, b) => GetYAtX(segments[a], evt.X).CompareTo(GetYAtX(segments[b], evt.X)));

                int pos = status.IndexOf(segIdx);
                if (pos > 0) CheckIntersection(segments, segIdx, status[pos - 1], evt.X, intersections, result);
                if (pos < status.Count - 1) CheckIntersection(segments, segIdx, status[pos + 1], evt.X, intersections, result);
            }
            else
            {
                int segIdx = evt.SegmentIndex;
                int pos = status.IndexOf(segIdx);
                if (pos >= 0)
                {
                    if (pos > 0 && pos < status.Count - 1)
                        CheckIntersection(segments, status[pos - 1], status[pos + 1], evt.X, intersections, result);
                    status.RemoveAt(pos);
                }
            }
        }

        return result.ToImmutable();
    }

    private static void CheckIntersection(IReadOnlyList<Segment2D> segments, int idxA, int idxB,
        double sweepX, HashSet<(int, int)> seen, ImmutableArray<Point2D>.Builder result)
    {
        if (idxA == idxB) return;
        (int, int) key = idxA < idxB ? (idxA, idxB) : (idxB, idxA);
        if (!seen.Add(key)) return;

        Segment2D a = segments[idxA], b = segments[idxB];
        var (hit, point) = a.Intersect(b);
        if (hit && point.X >= sweepX - 1e-10)
            result.Add(point);
    }

    private static double GetYAtX(Segment2D seg, double x)
    {
        double dx = seg.P2.X - seg.P1.X;
        if (System.Math.Abs(dx) < 1e-15) return (seg.P1.Y + seg.P2.Y) * 0.5;
        double t = (x - seg.P1.X) / dx;
        return seg.P1.Y + t * (seg.P2.Y - seg.P1.Y);
    }

    private enum EventType { Left, Right }

    private readonly record struct SweepEvent(double X, double Y, int SegmentIndex, EventType Type)
    {
        public static readonly IComparer<SweepEvent> Comparer = Comparer<SweepEvent>.Create((a, b) =>
        {
            int cmp = a.X.CompareTo(b.X);
            return cmp != 0 ? cmp : a.Y.CompareTo(b.Y);
        });
    }
}
