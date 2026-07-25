namespace MathVerse.Math.Geometry.Curves;

using Geometry2D;

/// <summary>Represents an implicit 2D curve defined by the equation F(x, y) = 0.</summary>
public sealed class ImplicitCurve2D
{
    /// <summary>Initializes a new instance of the <see cref="ImplicitCurve2D"/> class.</summary>
    /// <param name="f">The implicit function F(x, y) where F = 0 defines the curve.</param>
    public ImplicitCurve2D(Func<double, double, double> f)
    {
        F = f ?? throw new ArgumentNullException(nameof(f));
    }

    /// <summary>Gets the implicit function F(x, y).</summary>
    public Func<double, double, double> F { get; }

    /// <summary>Evaluates the implicit function at (x, y).</summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The value of F(x, y).</returns>
    public double Evaluate(double x, double y) => F(x, y);

    /// <summary>Extracts contour segments where F(x, y) = 0 using marching squares.</summary>
    /// <param name="xMin">The minimum x bound.</param>
    /// <param name="xMax">The maximum x bound.</param>
    /// <param name="yMin">The minimum y bound.</param>
    /// <param name="yMax">The maximum y bound.</param>
    /// <param name="resolution">The number of subdivisions along each axis.</param>
    /// <returns>An immutable array of segments approximating the contour.</returns>
    public ImmutableArray<Segment2D> Contour(double xMin, double xMax, double yMin, double yMax, int resolution)
    {
        var segments = ImmutableArray.CreateBuilder<Segment2D>();
        double dx = (xMax - xMin) / resolution;
        double dy = (yMax - yMin) / resolution;

        double[,] values = new double[resolution + 1, resolution + 1];
        for (int j = 0; j <= resolution; j++)
        {
            for (int i = 0; i <= resolution; i++)
            {
                values[i, j] = F(xMin + i * dx, yMin + j * dy);
            }
        }

        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                double v00 = values[i, j];
                double v10 = values[i + 1, j];
                double v11 = values[i + 1, j + 1];
                double v01 = values[i, j + 1];

                double x0 = xMin + i * dx;
                double y0 = yMin + j * dy;
                double x1 = x0 + dx;
                double y1 = y0 + dy;

                int caseIndex = 0;
                if (v00 > 0) caseIndex |= 1;
                if (v10 > 0) caseIndex |= 2;
                if (v11 > 0) caseIndex |= 4;
                if (v01 > 0) caseIndex |= 8;

                if (caseIndex == 0 || caseIndex == 15) continue;

                Point2D bottom = LerpEdge(x0, y0, x1, y0, v00, v10);
                Point2D right = LerpEdge(x1, y0, x1, y1, v10, v11);
                Point2D top = LerpEdge(x0, y1, x1, y1, v01, v11);
                Point2D left = LerpEdge(x0, y0, x0, y1, v00, v01);

                switch (caseIndex)
                {
                    case 1: case 14: segments.Add(new Segment2D(bottom, left)); break;
                    case 2: case 13: segments.Add(new Segment2D(right, bottom)); break;
                    case 3: case 12: segments.Add(new Segment2D(right, left)); break;
                    case 4: case 11: segments.Add(new Segment2D(top, right)); break;
                    case 5:
                        segments.Add(new Segment2D(bottom, left));
                        segments.Add(new Segment2D(top, right));
                        break;
                    case 6: case 9: segments.Add(new Segment2D(top, bottom)); break;
                    case 7: case 8: segments.Add(new Segment2D(top, left)); break;
                    case 10:
                        segments.Add(new Segment2D(bottom, right));
                        segments.Add(new Segment2D(top, left));
                        break;
                }
            }
        }

        return segments.ToImmutable();
    }

    private static Point2D LerpEdge(double x0, double y0, double x1, double y1, double v0, double v1)
    {
        double denom = v0 - v1;
        if (System.Math.Abs(denom) < 1e-15)
            return new Point2D((x0 + x1) * 0.5, (y0 + y1) * 0.5);
        double t = v0 / denom;
        return new Point2D(x0 + t * (x1 - x0), y0 + t * (y1 - y0));
    }
}
