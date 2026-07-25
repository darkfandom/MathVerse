namespace MathVerse.Math.Geometry.Advanced.PolygonAlgorithms;

/// <summary>
/// Provides polygon clipping algorithms for computing intersections and unions of polygons.
/// Supports both Sutherland-Hodgman (convex clip) and Weiler-Atherton (general) approaches.
/// </summary>
public static class PolygonClipper
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Clips a subject polygon against a convex clip polygon using the Sutherland-Hodgman algorithm.
    /// The clip polygon must be convex for correct results. The subject polygon may be concave.
    /// </summary>
    /// <param name="subject">The polygon to be clipped.</param>
    /// <param name="clip">The convex clipping polygon.</param>
    /// <returns>The clipped polygon as an immutable array of vertices.</returns>
    public static ImmutableArray<Point2D> SutherlandHodgman(ImmutableArray<Point2D> subject, ImmutableArray<Point2D> clip)
    {
        if (subject.Length < 3 || clip.Length < 3)
            return ImmutableArray<Point2D>.Empty;

        var output = subject;

        for (int i = 0; i < clip.Length; i++)
        {
            if (output.Length < 3) return ImmutableArray<Point2D>.Empty;

            int j = (i + 1) % clip.Length;
            Point2D edgeStart = clip[i];
            Point2D edgeEnd = clip[j];

            var input = output;
            var clipped = ImmutableArray.CreateBuilder<Point2D>();

            for (int k = 0; k < input.Length; k++)
            {
                int m = (k + 1) % input.Length;
                Point2D current = input[k];
                Point2D next = input[m];

                bool currInside = IsInsideEdge(current, edgeStart, edgeEnd);
                bool nextInside = IsInsideEdge(next, edgeStart, edgeEnd);

                if (currInside && nextInside)
                {
                    clipped.Add(next);
                }
                else if (currInside && !nextInside)
                {
                    Point2D intersection = LineLineIntersection(edgeStart, edgeEnd, current, next);
                    double dx = intersection.X - current.X;
                    double dy = intersection.Y - current.Y;
                    if (dx * dx + dy * dy > Tolerance * Tolerance)
                        clipped.Add(intersection);
                }
                else if (!currInside && nextInside)
                {
                    Point2D intersection = LineLineIntersection(edgeStart, edgeEnd, current, next);
                    double dx = intersection.X - next.X;
                    double dy = intersection.Y - next.Y;
                    if (dx * dx + dy * dy > Tolerance * Tolerance)
                    {
                        clipped.Add(intersection);
                        clipped.Add(next);
                    }
                }
            }

            output = clipped.ToImmutable();
        }

        return output;
    }

    /// <summary>
    /// Clips two polygons using the Weiler-Atherton algorithm.
    /// Supports concave polygons and handles polygon intersection, union, and difference
    /// by following the intersection and original polygon edges appropriately.
    /// Returns the polygon region that represents the overlap of subject and clip.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clipping polygon.</param>
    /// <returns>The clipped region as an immutable array of vertices.</returns>
    public static ImmutableArray<Point2D> WeilerAtherton(ImmutableArray<Point2D> subject, ImmutableArray<Point2D> clip)
    {
        if (subject.Length < 3 || clip.Length < 3)
            return ImmutableArray<Point2D>.Empty;

        var intersectionPoints = new List<IntersectionData>();
        int sN = subject.Length;
        int cN = clip.Length;

        for (int i = 0; i < sN; i++)
        {
            int si = (i + 1) % sN;
            Point2D sA = subject[i];
            Point2D sB = subject[si];

            for (int j = 0; j < cN; j++)
            {
                int ci = (j + 1) % cN;
                Point2D cA = clip[j];
                Point2D cB = clip[ci];

                var result = SegmentIntersection(sA, sB, cA, cB);
                if (result.hit)
                {
                    double sParam = 0;
                    double sLenSq = (sB.X - sA.X) * (sB.X - sA.X) + (sB.Y - sA.Y) * (sB.Y - sA.Y);
                    if (sLenSq > Tolerance * Tolerance)
                    {
                        double dx = result.point.X - sA.X;
                        double dy = result.point.Y - sA.Y;
                        sParam = (dx * (sB.X - sA.X) + dy * (sB.Y - sA.Y)) / sLenSq;
                    }

                    double cParam = 0;
                    double cLenSq = (cB.X - cA.X) * (cB.X - cA.X) + (cB.Y - cA.Y) * (cB.Y - cA.Y);
                    if (cLenSq > Tolerance * Tolerance)
                    {
                        double dx = result.point.X - cA.X;
                        double dy = result.point.Y - cA.Y;
                        cParam = (dx * (cB.X - cA.X) + dy * (cB.Y - cA.Y)) / cLenSq;
                    }

                    intersectionPoints.Add(new IntersectionData(
                        result.point, i, j, sParam, cParam));
                }
            }
        }

        if (intersectionPoints.Count == 0)
        {
            if (IsPointInPolygon(subject[0], clip))
                return subject;
            if (IsPointInPolygon(clip[0], subject))
                return clip;
            return ImmutableArray<Point2D>.Empty;
        }

        var sortedBySubject = new List<IntersectionData>(intersectionPoints);
        sortedBySubject.Sort((a, b) =>
        {
            int cmp = a.SubjectEdge.CompareTo(b.SubjectEdge);
            if (cmp != 0) return cmp;
            return a.SParam.CompareTo(b.SParam);
        });

        var sortedByClip = new List<IntersectionData>(intersectionPoints);
        sortedByClip.Sort((a, b) =>
        {
            int cmp = a.ClipEdge.CompareTo(b.ClipEdge);
            if (cmp != 0) return cmp;
            return a.CParam.CompareTo(b.CParam);
        });

        var subjectIPTree = new List<List<IntersectionData>>();
        var clipIPTree = new List<List<IntersectionData>>();
        for (int i = 0; i < sN; i++) subjectIPTree.Add(new List<IntersectionData>());
        for (int j = 0; j < cN; j++) clipIPTree.Add(new List<IntersectionData>());

        foreach (var ip in sortedBySubject)
            subjectIPTree[ip.SubjectEdge].Add(ip);
        foreach (var ip in sortedByClip)
            clipIPTree[ip.ClipEdge].Add(ip);

        var visited = new HashSet<(int, int)>();
        var resultPoints = ImmutableArray.CreateBuilder<Point2D>();

        for (int edgeIdx = 0; edgeIdx < sN; edgeIdx++)
        {
            if (subjectIPTree[edgeIdx].Count == 0) continue;

            foreach (var startIP in subjectIPTree[edgeIdx])
            {
                var key = (startIP.SubjectEdge, startIP.ClipEdge);
                if (visited.Contains(key)) continue;

                var path = new List<Point2D>();
                int currentSEdge = startIP.SubjectEdge;
                int currentCEdge = startIP.ClipEdge;
                Point2D currentPoint = startIP.Point;
                bool onSubject = true;
                int maxSteps = (sN + cN) * 4;
                int steps = 0;

                do
                {
                    path.Add(currentPoint);
                    visited.Add((currentSEdge, currentCEdge));

                    if (onSubject)
                    {
                        int nextSEdge = (currentSEdge + 1) % sN;
                        Point2D endVertex = subject[nextSEdge];

                        bool reachedEnd = System.Math.Abs(currentPoint.X - endVertex.X) < Tolerance &&
                                          System.Math.Abs(currentPoint.Y - endVertex.Y) < Tolerance;

                        bool foundNextIP = false;
                        if (!reachedEnd)
                        {
                            foreach (var ip in subjectIPTree[nextSEdge])
                            {
                                if (!visited.Contains((nextSEdge, ip.ClipEdge)))
                                {
                                    path.Add(endVertex);
                                    currentPoint = ip.Point;
                                    currentSEdge = nextSEdge;
                                    currentCEdge = ip.ClipEdge;
                                    onSubject = false;
                                    foundNextIP = true;
                                    break;
                                }
                            }
                        }

                        if (!foundNextIP)
                        {
                            if (reachedEnd)
                            {
                                if (!IsPointInPolygon(endVertex, clip))
                                {
                                    onSubject = false;
                                }
                                else
                                {
                                    currentPoint = endVertex;
                                    currentSEdge = nextSEdge;
                                }
                            }
                            else
                            {
                                path.Add(endVertex);
                                currentPoint = endVertex;
                                currentSEdge = nextSEdge;
                            }
                        }
                    }
                    else
                    {
                        int nextCEdge = (currentCEdge + 1) % cN;
                        Point2D endVertex = clip[nextCEdge];

                        bool reachedEnd = System.Math.Abs(currentPoint.X - endVertex.X) < Tolerance &&
                                          System.Math.Abs(currentPoint.Y - endVertex.Y) < Tolerance;

                        bool foundNextIP = false;
                        if (!reachedEnd)
                        {
                            foreach (var ip in clipIPTree[nextCEdge])
                            {
                                if (!visited.Contains((ip.SubjectEdge, nextCEdge)))
                                {
                                    path.Add(endVertex);
                                    currentPoint = ip.Point;
                                    currentSEdge = ip.SubjectEdge;
                                    currentCEdge = nextCEdge;
                                    onSubject = true;
                                    foundNextIP = true;
                                    break;
                                }
                            }
                        }

                        if (!foundNextIP)
                        {
                            if (reachedEnd)
                            {
                                if (!IsPointInPolygon(endVertex, subject))
                                {
                                    onSubject = true;
                                }
                                else
                                {
                                    currentPoint = endVertex;
                                    currentCEdge = nextCEdge;
                                }
                            }
                            else
                            {
                                path.Add(endVertex);
                                currentPoint = endVertex;
                                currentCEdge = nextCEdge;
                            }
                        }
                    }

                    steps++;
                } while (steps < maxSteps && (
                    System.Math.Abs(currentPoint.X - startIP.Point.X) > Tolerance ||
                    System.Math.Abs(currentPoint.Y - startIP.Point.Y) > Tolerance));

                if (path.Count >= 3)
                {
                    for (int i = 0; i < path.Count; i++)
                        resultPoints.Add(path[i]);
                }
            }
        }

        if (resultPoints.Count >= 3)
            return resultPoints.ToImmutable();

        if (IsPointInPolygon(subject[0], clip))
            return subject;
        if (IsPointInPolygon(clip[0], subject))
            return clip;

        return ImmutableArray<Point2D>.Empty;
    }

    /// <summary>
    /// Determines whether a point lies inside a polygon using the ray casting algorithm.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="polygon">The polygon to test against.</param>
    /// <returns><c>true</c> if the point is inside the polygon; otherwise, <c>false</c>.</returns>
    public static bool IsPointInPolygon(Point2D point, ImmutableArray<Point2D> polygon)
    {
        bool inside = false;
        int n = polygon.Length;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if ((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y) &&
                point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                inside = !inside;
        }
        return inside;
    }

    private static bool IsInsideEdge(Point2D point, Point2D edgeStart, Point2D edgeEnd)
    {
        double cross = (edgeEnd.X - edgeStart.X) * (point.Y - edgeStart.Y)
                     - (edgeEnd.Y - edgeStart.Y) * (point.X - edgeStart.X);
        return cross >= -Tolerance;
    }

    private static Point2D LineLineIntersection(Point2D a1, Point2D a2, Point2D b1, Point2D b2)
    {
        double d1x = a2.X - a1.X, d1y = a2.Y - a1.Y;
        double d2x = b2.X - b1.X, d2y = b2.Y - b1.Y;
        double cross = d1x * d2y - d1y * d2x;
        if (System.Math.Abs(cross) < Tolerance)
            return a1;
        double t = ((b1.X - a1.X) * d2y - (b1.Y - a1.Y) * d2x) / cross;
        return new Point2D(a1.X + t * d1x, a1.Y + t * d1y);
    }

    private static (bool hit, Point2D point) SegmentIntersection(Point2D a1, Point2D a2, Point2D b1, Point2D b2)
    {
        double d1x = a2.X - a1.X, d1y = a2.Y - a1.Y;
        double d2x = b2.X - b1.X, d2y = b2.Y - b1.Y;
        double cross = d1x * d2y - d1y * d2x;
        if (System.Math.Abs(cross) < Tolerance) return (false, Point2D.Origin);

        double t = ((b1.X - a1.X) * d2y - (b1.Y - a1.Y) * d2x) / cross;
        double u = ((b1.X - a1.X) * d1y - (b1.Y - a1.Y) * d1x) / cross;

        if (t >= -Tolerance && t <= 1.0 + Tolerance && u >= -Tolerance && u <= 1.0 + Tolerance)
            return (true, new Point2D(a1.X + t * d1x, a1.Y + t * d1y));

        return (false, Point2D.Origin);
    }

    private readonly record struct IntersectionData(
        Point2D Point, int SubjectEdge, int ClipEdge, double SParam, double CParam);
}
