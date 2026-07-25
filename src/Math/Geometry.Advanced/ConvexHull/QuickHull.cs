using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.ConvexHull;

/// <summary>
/// Computes the convex hull of a set of 2D points using the QuickHull divide-and-conquer algorithm.
/// </summary>
public static class QuickHull
{
    private const double Tolerance = 1e-10;
    private const int IterativeThreshold = 256;

    /// <summary>
    /// Computes the convex hull of a set of 2D points using the QuickHull algorithm.
    /// </summary>
    /// <param name="points">The input point set.</param>
    /// <returns>
    /// An immutable array of points forming the convex hull in counter-clockwise order.
    /// Returns an empty array if fewer than 3 non-collinear points exist.
    /// </returns>
    public static ImmutableArray<Point2D> Compute(ImmutableArray<Point2D> points)
    {
        if (points.Length <= 2)
            return ImmutableArray.CreateRange(points);

        Point2D[] pts = points.ToArray();

        int minIdx = 0, maxIdx = 0;
        for (int i = 1; i < pts.Length; i++)
        {
            if (pts[i].X < pts[minIdx].X || (System.Math.Abs(pts[i].X - pts[minIdx].X) < Tolerance && pts[i].Y < pts[minIdx].Y))
                minIdx = i;
            if (pts[i].X > pts[maxIdx].X || (System.Math.Abs(pts[i].X - pts[maxIdx].X) < Tolerance && pts[i].Y > pts[maxIdx].Y))
                maxIdx = i;
        }

        if (minIdx == maxIdx)
            return ImmutableArray.Create(pts[0]);

        Point2D left = pts[minIdx];
        Point2D right = pts[maxIdx];

        var above = new List<int>();
        var below = new List<int>();

        for (int i = 0; i < pts.Length; i++)
        {
            if (i == minIdx || i == maxIdx) continue;
            double cross = Cross2D(left, right, pts[i]);
            if (cross > Tolerance)
                above.Add(i);
            else if (cross < -Tolerance)
                below.Add(i);
        }

        var hullSet = new HashSet<int>();
        hullSet.Add(minIdx);
        hullSet.Add(maxIdx);

        var upperStack = new Stack<(int left, int right, List<int> points)>();
        upperStack.Push((minIdx, maxIdx, above));

        while (upperStack.Count > 0)
        {
            (int lIdx, int rIdx, List<int> ptsList) = upperStack.Pop();
            if (ptsList.Count == 0)
            {
                hullSet.Add(lIdx);
                hullSet.Add(rIdx);
                continue;
            }

            Point2D lp = pts[lIdx];
            Point2D rp = pts[rIdx];
            int farthestIdx = FindFarthest(pts, ptsList, lp, rp);

            var leftSet = new List<int>();
            var rightSet = new List<int>();
            Point2D fp = pts[farthestIdx];

            for (int i = 0; i < ptsList.Count; i++)
            {
                int idx = ptsList[i];
                if (idx == farthestIdx) continue;
                double crossL = Cross2D(lp, fp, pts[idx]);
                double crossR = Cross2D(fp, rp, pts[idx]);
                if (crossL > Tolerance)
                    leftSet.Add(idx);
                else if (crossR > Tolerance)
                    rightSet.Add(idx);
            }

            upperStack.Push((lIdx, farthestIdx, leftSet));
            upperStack.Push((farthestIdx, rIdx, rightSet));
        }

        var lowerStack = new Stack<(int left, int right, List<int> points)>();
        lowerStack.Push((minIdx, maxIdx, below));

        while (lowerStack.Count > 0)
        {
            (int lIdx, int rIdx, List<int> ptsList) = lowerStack.Pop();
            if (ptsList.Count == 0)
            {
                hullSet.Add(lIdx);
                hullSet.Add(rIdx);
                continue;
            }

            Point2D lp = pts[lIdx];
            Point2D rp = pts[rIdx];
            int farthestIdx = FindFarthest(pts, ptsList, lp, rp);

            var leftSet = new List<int>();
            var rightSet = new List<int>();
            Point2D fp = pts[farthestIdx];

            for (int i = 0; i < ptsList.Count; i++)
            {
                int idx = ptsList[i];
                if (idx == farthestIdx) continue;
                double crossL = Cross2D(lp, fp, pts[idx]);
                double crossR = Cross2D(fp, rp, pts[idx]);
                if (crossL < -Tolerance)
                    leftSet.Add(idx);
                else if (crossR < -Tolerance)
                    rightSet.Add(idx);
            }

            lowerStack.Push((lIdx, farthestIdx, leftSet));
            lowerStack.Push((farthestIdx, rIdx, rightSet));
        }

        int[] hullIndices = hullSet.ToArray();
        System.Array.Sort(hullIndices, (a, b) =>
        {
            Point2D pa = pts[a], pb = pts[b];
            double angleA = System.Math.Atan2(pa.Y - pts[hullIndices[0]].Y, pa.X - pts[hullIndices[0]].X);
            double angleB = System.Math.Atan2(pb.Y - pts[hullIndices[0]].Y, pb.X - pts[hullIndices[0]].X);
            return angleA.CompareTo(angleB);
        });

        var result = ImmutableArray.CreateBuilder<Point2D>(hullIndices.Length);
        for (int i = 0; i < hullIndices.Length; i++)
            result.Add(pts[hullIndices[i]]);

        return result.ToImmutable();
    }

    private static int FindFarthest(Point2D[] pts, List<int> indices, Point2D a, Point2D b)
    {
        int best = indices[0];
        double bestDist = 0;

        for (int i = 0; i < indices.Count; i++)
        {
            int idx = indices[i];
            double dist = System.Math.Abs((b.X - a.X) * (a.Y - pts[idx].Y) - (a.X - pts[idx].X) * (b.Y - a.Y));
            if (dist > bestDist + Tolerance)
            {
                bestDist = dist;
                best = idx;
            }
        }

        return best;
    }

    private static double Cross2D(Point2D o, Point2D a, Point2D b)
    {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }
}
