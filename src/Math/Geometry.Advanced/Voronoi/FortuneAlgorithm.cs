using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.Voronoi;

/// <summary>
/// Represents a Voronoi site (input point).
/// </summary>
public readonly record struct VoronoiSite(double X, double Y, int Id);

/// <summary>
/// Represents a Voronoi edge between two sites.
/// </summary>
public readonly record struct VoronoiEdge(Point2D P1, Point2D P2, int Site1, int Site2);

/// <summary>
/// Represents a Voronoi cell (polygon) for a single site.
/// </summary>
public readonly record struct VoronoiCell(int SiteId, ImmutableArray<Point2D> Vertices);

/// <summary>
/// Computes the Voronoi diagram of a set of 2D points using Fortune's sweep-line algorithm.
/// </summary>
public static class FortuneAlgorithm
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the full Voronoi diagram of a set of 2D sites using Fortune's algorithm.
    /// </summary>
    /// <param name="sites">The input site points.</param>
    /// <returns>
    /// A tuple containing the sites, edges, and cells of the Voronoi diagram.
    /// </returns>
    public static (ImmutableArray<VoronoiSite> Sites, ImmutableArray<VoronoiEdge> Edges, ImmutableArray<VoronoiCell> Cells) Compute(ImmutableArray<Point2D> sites)
    {
        var siteArray = ImmutableArray.CreateBuilder<VoronoiSite>(sites.Length);
        for (int i = 0; i < sites.Length; i++)
            siteArray.Add(new VoronoiSite(sites[i].X, sites[i].Y, i));

        ImmutableArray<VoronoiSite> siteResult = siteArray.ToImmutable();

        if (sites.Length < 2)
        {
            return (siteResult, ImmutableArray<VoronoiEdge>.Empty, ImmutableArray<VoronoiCell>.Empty);
        }

        if (sites.Length == 2)
        {
            Point2D mid = sites[0].Lerp(sites[1], 0.5);
            Vector2D diff = new Vector2D(sites[1].X - sites[0].X, sites[1].Y - sites[0].Y);
            Vector2D perp = diff.Perpendicular().Normalize();
            double extent = System.Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y) * 10;

            var edge = new VoronoiEdge(
                new Point2D(mid.X - perp.X * extent, mid.Y - perp.Y * extent),
                new Point2D(mid.X + perp.X * extent, mid.Y + perp.Y * extent),
                0, 1);

            var cell0 = new VoronoiCell(0, ImmutableArray.Create(
                new Point2D(mid.X - perp.X * extent, mid.Y - perp.Y * extent),
                new Point2D(mid.X + perp.X * extent, mid.Y + perp.Y * extent)));
            var cell1 = new VoronoiCell(1, ImmutableArray.Create(
                new Point2D(mid.X + perp.X * extent, mid.Y + perp.Y * extent),
                new Point2D(mid.X - perp.X * extent, mid.Y - perp.Y * extent)));

            return (siteResult, ImmutableArray.Create(edge), ImmutableArray.Create(cell0, cell1));
        }

        var sweepline = new SweepLineState(siteResult);

        sweepline.Run();

        return sweepline.BuildResult();
    }

    private sealed class Arc
    {
        public int SiteIndex;
        public Arc? Left;
        public Arc? Right;
        public CircleEvent? CircleEvent;
        public double XBreakLeft;
        public double XBreakRight;

        public Arc(int siteIndex)
        {
            SiteIndex = siteIndex;
            XBreakLeft = double.NegativeInfinity;
            XBreakRight = double.PositiveInfinity;
        }
    }

    private sealed class CircleEvent
    {
        public double X;
        public double Y;
        public double SweepY;
        public Arc Arc;
        public bool IsValid = true;

        public CircleEvent(double x, double y, double sweepY, Arc arc)
        {
            X = x;
            Y = y;
            SweepY = sweepY;
            Arc = arc;
        }
    }

    private sealed class SiteEvent
    {
        public int SiteIndex;
        public double X;
        public double Y;

        public SiteEvent(int siteIndex, double x, double y)
        {
            SiteIndex = siteIndex;
            X = x;
            Y = y;
        }
    }

    private sealed class SweepLineState
    {
        private readonly ImmutableArray<VoronoiSite> _sites;
        private readonly SortedSet<SiteEvent> _siteEvents;
        private readonly SortedSet<CircleEvent> _circleEvents;
        private readonly List<VoronoiEdge> _edges;
        private Arc? _beachLine;
        private double _sweepY;

        public SweepLineState(ImmutableArray<VoronoiSite> sites)
        {
            _sites = sites;
            _siteEvents = new SortedSet<SiteEvent>(Comparer<SiteEvent>.Create((a, b) =>
            {
                int cmp = a.Y.CompareTo(b.Y);
                if (cmp != 0) return cmp;
                cmp = a.X.CompareTo(b.X);
                return cmp != 0 ? cmp : a.SiteIndex.CompareTo(b.SiteIndex);
            }));
            _circleEvents = new SortedSet<CircleEvent>(Comparer<CircleEvent>.Create((a, b) =>
            {
                int cmp = a.SweepY.CompareTo(b.SweepY);
                if (cmp != 0) return cmp;
                cmp = a.X.CompareTo(b.X);
                return cmp != 0 ? cmp : a.Y.CompareTo(b.Y);
            }));
            _edges = new List<VoronoiEdge>();
        }

        public void Run()
        {
            for (int i = 0; i < _sites.Length; i++)
                _siteEvents.Add(new SiteEvent(i, _sites[i].X, _sites[i].Y));

            while (_siteEvents.Count > 0 || _circleEvents.Count > 0)
            {
                if (_siteEvents.Count == 0)
                {
                    ProcessCircleEvent();
                }
                else if (_circleEvents.Count == 0)
                {
                    ProcessSiteEvent();
                }
                else
                {
                    SiteEvent nextSite = _siteEvents.Min!;
                    CircleEvent nextCircle = _circleEvents.Min!;
                    if (nextSite.Y <= nextCircle.SweepY + Tolerance)
                        ProcessSiteEvent();
                    else
                        ProcessCircleEvent();
                }
            }
        }

        private void ProcessSiteEvent()
        {
            SiteEvent e = _siteEvents.Min!;
            _siteEvents.Remove(e);
            _sweepY = e.Y;
            InsertArc(e.SiteIndex, e.X);
        }

        private void ProcessCircleEvent()
        {
            CircleEvent? e = null;
            while (_circleEvents.Count > 0)
            {
                e = _circleEvents.Min!;
                _circleEvents.Remove(e);
                if (e.IsValid) break;
                e = null;
            }

            if (e == null) return;

            _sweepY = e.SweepY;
            Arc? arc = e.Arc;

            if (arc == null || arc.Left == null || arc.Right == null) return;

            Point2D vertex = new Point2D(e.X, e.Y);
            int site1 = arc.Left.SiteIndex;
            int site2 = arc.Right.SiteIndex;

            _edges.Add(new VoronoiEdge(
                vertex,
                new Point2D(e.X, e.Y),
                site1, site2));

            RemoveArc(arc);

            CheckCircleEvent(arc.Left!);
            CheckCircleEvent(arc.Right!);
        }

        private void InsertArc(int siteIndex, double x)
        {
            if (_beachLine == null)
            {
                _beachLine = new Arc(siteIndex);
                return;
            }

            Arc? current = _beachLine;
            while (current != null)
            {
                double ix = IntersectX(current, siteIndex);
                if (ix < x + Tolerance)
                {
                    if (current.Left != null)
                    {
                        current = current.Left;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (current.Right != null)
                    {
                        current = current.Right;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Arc? existing = current;
            Arc newArc = new Arc(siteIndex);

            if (existing != null)
            {
                newArc.Left = existing;
                newArc.Right = existing.Right;
                if (existing.Right != null)
                    existing.Right.Left = newArc;
                existing.Right = newArc;
            }
            else
            {
                newArc.Left = _beachLine;
                _beachLine.Right = newArc;
                _beachLine = newArc;
            }

            CheckCircleEvent(newArc.Left!);
            CheckCircleEvent(newArc.Right!);
        }

        private void RemoveArc(Arc arc)
        {
            if (arc.Left != null)
                arc.Left.Right = arc.Right;
            if (arc.Right != null)
                arc.Right.Left = arc.Left;

            if (arc == _beachLine)
                _beachLine = arc.Left ?? arc.Right;
        }

        private void CheckCircleEvent(Arc arc)
        {
            if (arc.CircleEvent != null)
                arc.CircleEvent.IsValid = false;

            if (arc.Left == null || arc.Right == null) return;

            double d = ComputeCircleCenter(
                _sites[arc.Left.SiteIndex], _sites[arc.SiteIndex], _sites[arc.Right.SiteIndex],
                out double cx, out double cy);

            if (d < Tolerance) return;

            double sweepY = cy - System.Math.Sqrt(d);
            if (sweepY < _sweepY - Tolerance) return;

            var ce = new CircleEvent(cx, cy, sweepY, arc);
            arc.CircleEvent = ce;
            _circleEvents.Add(ce);
        }

        private static double ComputeCircleCenter(VoronoiSite a, VoronoiSite b, VoronoiSite c, out double cx, out double cy)
        {
            double d = 2.0 * (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y));
            if (System.Math.Abs(d) < Tolerance)
            {
                cx = 0;
                cy = 0;
                return -1;
            }

            double aSq = a.X * a.X + a.Y * a.Y;
            double bSq = b.X * b.X + b.Y * b.Y;
            double cSq = c.X * c.X + c.Y * c.Y;

            cx = (aSq * (b.Y - c.Y) + bSq * (c.Y - a.Y) + cSq * (a.Y - b.Y)) / d;
            cy = (aSq * (c.X - b.X) + bSq * (a.X - c.X) + cSq * (b.X - a.X)) / d;

            double dx = cx - a.X;
            double dy = cy - a.Y;
            return dx * dx + dy * dy;
        }

        private double IntersectX(Arc arc, int newSiteIndex)
        {
            if (System.Math.Abs(_sites[arc.SiteIndex].Y - _sites[newSiteIndex].Y) < Tolerance)
                return (_sites[arc.SiteIndex].X + _sites[newSiteIndex].X) * 0.5;

            double ax = _sites[arc.SiteIndex].X;
            double ay = _sites[arc.SiteIndex].Y;
            double bx = _sites[newSiteIndex].X;
            double by = _sites[newSiteIndex].Y;

            double midX = (ax + bx) * 0.5;
            double midY = (ay + by) * 0.5;
            double dx = bx - ax;
            double dy = by - ay;

            return midX - dy * (ax * ax + ay * ay - bx * bx - by * by) / (2.0 * dx * dy);
        }

        public (ImmutableArray<VoronoiSite> Sites, ImmutableArray<VoronoiEdge> Edges, ImmutableArray<VoronoiCell> Cells) BuildResult()
        {
            var edgeResult = ImmutableArray.CreateBuilder<VoronoiEdge>(_edges.Count);
            for (int i = 0; i < _edges.Count; i++)
                edgeResult.Add(_edges[i]);

            var cellEdges = new Dictionary<int, List<VoronoiEdge>>();
            for (int i = 0; i < _sites.Length; i++)
                cellEdges[i] = new List<VoronoiEdge>();

            for (int i = 0; i < _edges.Count; i++)
            {
                VoronoiEdge e = _edges[i];
                if (cellEdges.ContainsKey(e.Site1)) cellEdges[e.Site1].Add(e);
                if (cellEdges.ContainsKey(e.Site2)) cellEdges[e.Site2].Add(e);
            }

            var cells = ImmutableArray.CreateBuilder<VoronoiCell>(_sites.Length);
            for (int i = 0; i < _sites.Length; i++)
            {
                if (cellEdges[i].Count == 0)
                {
                    cells.Add(new VoronoiCell(i, ImmutableArray<Point2D>.Empty));
                    continue;
                }

                var vertexSet = new HashSet<(double, double)>();
                var vertices = new List<Point2D>();

                for (int j = 0; j < cellEdges[i].Count; j++)
                {
                    VoronoiEdge e = cellEdges[i][j];
                    Point2D p1 = e.P1;
                    Point2D p2 = e.P2;
                    if (vertexSet.Add((System.Math.Round(p1.X, 10), System.Math.Round(p1.Y, 10))))
                        vertices.Add(p1);
                    if (vertexSet.Add((System.Math.Round(p2.X, 10), System.Math.Round(p2.Y, 10))))
                        vertices.Add(p2);
                }

                double centroidX = _sites[i].X;
                double centroidY = _sites[i].Y;
                vertices.Sort((a, b) =>
                {
                    double angleA = System.Math.Atan2(a.Y - centroidY, a.X - centroidX);
                    double angleB = System.Math.Atan2(b.Y - centroidY, b.X - centroidX);
                    return angleA.CompareTo(angleB);
                });

                cells.Add(new VoronoiCell(i, ImmutableArray.CreateRange(vertices)));
            }

            return (_sites, edgeResult.ToImmutable(), cells.ToImmutable());
        }
    }
}
