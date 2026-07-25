namespace MathVerse.Math.Visualization.Labeling;
using System.Numerics;
using System.Collections.Generic;

/// <summary>Represents a line segment for path rendering.</summary>
public sealed class LineSegment
{
    /// <summary>Gets the start point.</summary>
    public Vector2 Start { get; init; }

    /// <summary>Gets the end point.</summary>
    public Vector2 End { get; init; }
}

/// <summary>Represents a quadratic Bezier curve segment.</summary>
public sealed class BezierSegment
{
    /// <summary>Gets the start point.</summary>
    public Vector2 Start { get; init; }

    /// <summary>Gets the control point.</summary>
    public Vector2 Control { get; init; }

    /// <summary>Gets the end point.</summary>
    public Vector2 End { get; init; }
}

/// <summary>Renders common mathematical symbols as path data.</summary>
public sealed class MathSymbolRenderer
{
    private const double DefaultStrokeWidth = 1.5;

    /// <summary>Gets the line segments for rendering a mathematical symbol.</summary>
    /// <param name="symbol">The symbol character (π, Σ, ∫, √, ∞, ±, ×, ÷, ≤, ≥, ≠, ≈, ∂, ∇, ∀, ∃, ∈, ⊂, ∪, ∩).</param>
    /// <param name="x">The x coordinate of the symbol center.</param>
    /// <param name="y">The y coordinate of the symbol center.</param>
    /// <param name="size">The size of the symbol.</param>
    /// <returns>A list of line segments for the symbol.</returns>
    public static List<LineSegment> GetSymbolPath(char symbol, double x, double y, double size)
    {
        var segments = new List<LineSegment>();

        switch (symbol)
        {
            case '\u03C0': // π
                RenderPi(segments, x, y, size);
                break;
            case '\u03A3': // Σ
                RenderSigma(segments, x, y, size);
                break;
            case '\u222B': // ∫
                RenderIntegral(segments, x, y, size);
                break;
            case '\u221A': // √
                RenderSquareRoot(segments, x, y, size);
                break;
            case '\u221E': // ∞
                RenderInfinity(segments, x, y, size);
                break;
            case '\u00B1': // ±
                RenderPlusMinus(segments, x, y, size);
                break;
            case '\u00D7': // ×
                RenderMultiply(segments, x, y, size);
                break;
            case '\u00F7': // ÷
                RenderDivide(segments, x, y, size);
                break;
            case '\u2264': // ≤
                RenderLessEqual(segments, x, y, size);
                break;
            case '\u2265': // ≥
                RenderGreaterEqual(segments, x, y, size);
                break;
            case '\u2260': // ≠
                RenderNotEqual(segments, x, y, size);
                break;
            case '\u2248': // ≈
                RenderApproximately(segments, x, y, size);
                break;
            case '\u2202': // ∂
                RenderPartial(segments, x, y, size);
                break;
            case '\u2207': // ∇
                RenderNabla(segments, x, y, size);
                break;
            case '\u2200': // ∀
                RenderForAll(segments, x, y, size);
                break;
            case '\u2203': // ∃
                RenderExists(segments, x, y, size);
                break;
            case '\u2208': // ∈
                RenderElementOf(segments, x, y, size);
                break;
            case '\u2282': // ⊂
                RenderSubset(segments, x, y, size);
                break;
            case '\u222A': // ∪
                RenderUnion(segments, x, y, size);
                break;
            case '\u2229': // ∩
                RenderIntersection(segments, x, y, size);
                break;
            default:
                break;
        }

        return segments;
    }

    /// <summary>Gets a list of commonly available math symbols.</summary>
    /// <returns>Array of supported symbol characters.</returns>
    public static char[] GetAvailableSymbols()
    {
        return new char[]
        {
            '\u03C0', '\u03A3', '\u222B', '\u221A', '\u221E',
            '\u00B1', '\u00D7', '\u00F7', '\u2264', '\u2265',
            '\u2260', '\u2248', '\u2202', '\u2207', '\u2200',
            '\u2203', '\u2208', '\u2282', '\u222A', '\u2229'
        };
    }

    private static void RenderPi(List<LineSegment> segs, double x, double y, double size)
    {
        double h = size * 0.5;
        double w = size * 0.45;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - w), (float)(y + h * 0.3)),
            End = new Vector2((float)(x + w), (float)(y + h * 0.3))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - w * 0.35), (float)(y + h * 0.3)),
            End = new Vector2((float)(x - w * 0.35), (float)(y - h))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + w * 0.35), (float)(y + h * 0.3)),
            End = new Vector2((float)(x + w * 0.35), (float)(y - h))
        });
    }

    private static void RenderSigma(List<LineSegment> segs, double x, double y, double size)
    {
        double h = size * 0.5;
        double w = size * 0.4;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + w), (float)(y + h)),
            End = new Vector2((float)(x - w), (float)(y + h))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - w), (float)(y + h)),
            End = new Vector2((float)(x), (float)(y))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x), (float)(y)),
            End = new Vector2((float)(x - w), (float)(y - h))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - w), (float)(y - h)),
            End = new Vector2((float)(x + w), (float)(y - h))
        });
    }

    private static void RenderIntegral(List<LineSegment> segs, double x, double y, double size)
    {
        double h = size * 0.55;
        double w = size * 0.15;

        int numSegments = 20;
        for (int i = 0; i < numSegments; i++)
        {
            double t0 = (double)i / numSegments;
            double t1 = (double)(i + 1) / numSegments;

            double y0 = y + h - t0 * 2.0 * h;
            double y1 = y + h - t1 * 2.0 * h;

            double x0 = x + w * System.Math.Sin(t0 * System.Math.PI);
            double x1 = x + w * System.Math.Sin(t1 * System.Math.PI);

            segs.Add(new LineSegment
            {
                Start = new Vector2((float)x0, (float)y0),
                End = new Vector2((float)x1, (float)y1)
            });
        }
    }

    private static void RenderSquareRoot(List<LineSegment> segs, double x, double y, double size)
    {
        double h = size * 0.35;
        double w = size * 0.5;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - w * 0.6), (float)(y + h * 0.5)),
            End = new Vector2((float)(x - w * 0.3), (float)(y + h * 0.5))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - w * 0.3), (float)(y + h * 0.5)),
            End = new Vector2((float)(x - w * 0.15), (float)(y - h * 0.5))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - w * 0.15), (float)(y - h * 0.5)),
            End = new Vector2((float)(x + w), (float)(y - h * 0.5))
        });
    }

    private static void RenderInfinity(List<LineSegment> segs, double x, double y, double size)
    {
        double r = size * 0.22;
        int numSegments = 24;

        for (int i = 0; i < numSegments; i++)
        {
            double t0 = (double)i / numSegments * 2.0 * System.Math.PI;
            double t1 = (double)(i + 1) / numSegments * 2.0 * System.Math.PI;

            double lx0 = x - r + r * System.Math.Cos(t0);
            double ly0 = y + r * System.Math.Sin(t0);
            double lx1 = x - r + r * System.Math.Cos(t1);
            double ly1 = y + r * System.Math.Sin(t1);

            segs.Add(new LineSegment
            {
                Start = new Vector2((float)lx0, (float)ly0),
                End = new Vector2((float)lx1, (float)ly1)
            });

            double rx0 = x + r + r * System.Math.Cos(-t0);
            double ry0 = y + r * System.Math.Sin(-t0);
            double rx1 = x + r + r * System.Math.Cos(-t1);
            double ry1 = y + r * System.Math.Sin(-t1);

            segs.Add(new LineSegment
            {
                Start = new Vector2((float)rx0, (float)ry0),
                End = new Vector2((float)rx1, (float)ry1)
            });
        }
    }

    private static void RenderPlusMinus(List<LineSegment> segs, double x, double y, double size)
    {
        double h = size * 0.35;
        double w = size * 0.35;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - w), (float)(y + h * 0.5)),
            End = new Vector2((float)(x + w), (float)(y + h * 0.5))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)x, (float)(y + h * 0.5 - w)),
            End = new Vector2((float)x, (float)(y + h * 0.5 + w))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - w), (float)(y - h * 0.5)),
            End = new Vector2((float)(x + w), (float)(y - h * 0.5))
        });
    }

    private static void RenderMultiply(List<LineSegment> segs, double x, double y, double size)
    {
        double d = size * 0.35;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - d), (float)(y - d)),
            End = new Vector2((float)(x + d), (float)(y + d))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + d), (float)(y - d)),
            End = new Vector2((float)(x - d), (float)(y + d))
        });
    }

    private static void RenderDivide(List<LineSegment> segs, double x, double y, double size)
    {
        double w = size * 0.25;
        double h = size * 0.35;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - w), (float)y),
            End = new Vector2((float)(x + w), (float)y)
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)x, (float)(y - h)),
            End = new Vector2((float)x, (float)(y - h))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)x, (float)(y + h)),
            End = new Vector2((float)x, (float)(y + h))
        });
    }

    private static void RenderLessEqual(List<LineSegment> segs, double x, double y, double size)
    {
        double d = size * 0.3;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + d), (float)(y - d)),
            End = new Vector2((float)(x - d), (float)y)
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - d), (float)y),
            End = new Vector2((float)(x + d), (float)(y + d))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - d), (float)(y - d * 0.3)),
            End = new Vector2((float)(x + d), (float)(y + d * 0.3))
        });
    }

    private static void RenderGreaterEqual(List<LineSegment> segs, double x, double y, double size)
    {
        double d = size * 0.3;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - d), (float)(y - d)),
            End = new Vector2((float)(x + d), (float)y)
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + d), (float)y),
            End = new Vector2((float)(x - d), (float)(y + d))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - d), (float)(y + d * 0.3)),
            End = new Vector2((float)(x + d), (float)(y - d * 0.3))
        });
    }

    private static void RenderNotEqual(List<LineSegment> segs, double x, double y, double size)
    {
        double d = size * 0.3;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - d), (float)(y - d)),
            End = new Vector2((float)(x + d), (float)(y + d))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + d), (float)(y - d)),
            End = new Vector2((float)(x - d), (float)(y + d))
        });
    }

    private static void RenderApproximately(List<LineSegment> segs, double x, double y, double size)
    {
        double w = size * 0.35;
        double h = size * 0.12;

        int numSegments = 12;
        for (int i = 0; i < numSegments; i++)
        {
            double t0 = (double)i / numSegments;
            double t1 = (double)(i + 1) / numSegments;

            double x0 = x - w + t0 * 2.0 * w;
            double x1 = x - w + t1 * 2.0 * w;
            double y0 = y - h + h * System.Math.Sin(t0 * 4.0 * System.Math.PI);
            double y1 = y - h + h * System.Math.Sin(t1 * 4.0 * System.Math.PI);

            segs.Add(new LineSegment
            {
                Start = new Vector2((float)x0, (float)y0),
                End = new Vector2((float)x1, (float)y1)
            });
        }

        for (int i = 0; i < numSegments; i++)
        {
            double t0 = (double)i / numSegments;
            double t1 = (double)(i + 1) / numSegments;

            double x0 = x - w + t0 * 2.0 * w;
            double x1 = x - w + t1 * 2.0 * w;
            double y0 = y + h + h * System.Math.Sin(t0 * 4.0 * System.Math.PI);
            double y1 = y + h + h * System.Math.Sin(t1 * 4.0 * System.Math.PI);

            segs.Add(new LineSegment
            {
                Start = new Vector2((float)x0, (float)y0),
                End = new Vector2((float)x1, (float)y1)
            });
        }
    }

    private static void RenderPartial(List<LineSegment> segs, double x, double y, double size)
    {
        double r = size * 0.3;
        int numSegments = 16;

        for (int i = 0; i < numSegments / 2; i++)
        {
            double t0 = (double)i / (numSegments / 2) * System.Math.PI;
            double t1 = (double)(i + 1) / (numSegments / 2) * System.Math.PI;

            double x0 = x + r * System.Math.Cos(t0);
            double y0 = y + r * System.Math.Sin(t0);
            double x1 = x + r * System.Math.Cos(t1);
            double y1 = y + r * System.Math.Sin(t1);

            segs.Add(new LineSegment
            {
                Start = new Vector2((float)x0, (float)y0),
                End = new Vector2((float)x1, (float)y1)
            });
        }

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + r), (float)y),
            End = new Vector2((float)(x + r), (float)(y + r * 1.5))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + r), (float)(y + r * 1.5)),
            End = new Vector2((float)(x - r * 0.3), (float)(y + r * 1.5))
        });
    }

    private static void RenderNabla(List<LineSegment> segs, double x, double y, double size)
    {
        double d = size * 0.35;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - d), (float)(y + d)),
            End = new Vector2((float)(x + d), (float)(y + d))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + d), (float)(y + d)),
            End = new Vector2((float)x, (float)(y - d))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)x, (float)(y - d)),
            End = new Vector2((float)(x - d), (float)(y + d))
        });
    }

    private static void RenderForAll(List<LineSegment> segs, double x, double y, double size)
    {
        double d = size * 0.35;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - d), (float)(y + d)),
            End = new Vector2((float)(x + d), (float)(y + d))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + d), (float)(y + d)),
            End = new Vector2((float)x, (float)(y - d))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)x, (float)(y - d)),
            End = new Vector2((float)(x - d), (float)(y + d))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - d * 0.6), (float)(y - d * 0.3)),
            End = new Vector2((float)(x + d * 0.6), (float)(y - d * 0.3))
        });
    }

    private static void RenderExists(List<LineSegment> segs, double x, double y, double size)
    {
        double d = size * 0.35;

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - d), (float)(y - d)),
            End = new Vector2((float)(x + d), (float)(y - d))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + d), (float)(y - d)),
            End = new Vector2((float)(x - d), (float)(y + d))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - d), (float)(y + d)),
            End = new Vector2((float)(x + d), (float)(y + d))
        });
    }

    private static void RenderElementOf(List<LineSegment> segs, double x, double y, double size)
    {
        double r = size * 0.3;
        int numSegments = 16;

        for (int i = 0; i < numSegments; i++)
        {
            double t0 = (double)i / numSegments * System.Math.PI;
            double t1 = (double)(i + 1) / numSegments * System.Math.PI;

            double x0 = x - r * 0.5 + r * System.Math.Cos(t0);
            double y0 = y + r * System.Math.Sin(t0);
            double x1 = x - r * 0.5 + r * System.Math.Cos(t1);
            double y1 = y + r * System.Math.Sin(t1);

            segs.Add(new LineSegment
            {
                Start = new Vector2((float)x0, (float)y0),
                End = new Vector2((float)x1, (float)y1)
            });
        }

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - r * 0.5), (float)y),
            End = new Vector2((float)(x + r), (float)y)
        });
    }

    private static void RenderSubset(List<LineSegment> segs, double x, double y, double size)
    {
        double r = size * 0.3;
        int numSegments = 16;

        for (int i = 0; i < numSegments; i++)
        {
            double t0 = (double)i / numSegments * System.Math.PI;
            double t1 = (double)(i + 1) / numSegments * System.Math.PI;

            double x0 = x - r * 0.5 + r * System.Math.Cos(t0);
            double y0 = y + r * System.Math.Sin(t0);
            double x1 = x - r * 0.5 + r * System.Math.Cos(t1);
            double y1 = y + r * System.Math.Sin(t1);

            segs.Add(new LineSegment
            {
                Start = new Vector2((float)x0, (float)y0),
                End = new Vector2((float)x1, (float)y1)
            });
        }

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x + r), (float)(y - r * 0.5)),
            End = new Vector2((float)(x + r), (float)(y + r * 0.5))
        });
    }

    private static void RenderUnion(List<LineSegment> segs, double x, double y, double size)
    {
        double r = size * 0.22;
        int numSegments = 12;

        for (int i = 0; i < numSegments; i++)
        {
            double t0 = (double)i / numSegments * System.Math.PI;
            double t1 = (double)(i + 1) / numSegments * System.Math.PI;

            double x0 = x + r * System.Math.Cos(t0);
            double y0 = y - r * 0.5 + r * System.Math.Sin(t0);
            double x1 = x + r * System.Math.Cos(t1);
            double y1 = y - r * 0.5 + r * System.Math.Sin(t1);

            segs.Add(new LineSegment
            {
                Start = new Vector2((float)x0, (float)y0),
                End = new Vector2((float)x1, (float)y1)
            });
        }

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - r), (float)(y + r * 0.3)),
            End = new Vector2((float)x, (float)(y + r))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)x, (float)(y + r)),
            End = new Vector2((float)(x + r), (float)(y + r * 0.3))
        });
    }

    private static void RenderIntersection(List<LineSegment> segs, double x, double y, double size)
    {
        double r = size * 0.22;
        int numSegments = 12;

        for (int i = 0; i < numSegments; i++)
        {
            double t0 = (double)i / numSegments * System.Math.PI;
            double t1 = (double)(i + 1) / numSegments * System.Math.PI;

            double x0 = x + r * System.Math.Cos(t0 + System.Math.PI);
            double y0 = y + r * 0.5 + r * System.Math.Sin(t0 + System.Math.PI);
            double x1 = x + r * System.Math.Cos(t1 + System.Math.PI);
            double y1 = y + r * 0.5 + r * System.Math.Sin(t1 + System.Math.PI);

            segs.Add(new LineSegment
            {
                Start = new Vector2((float)x0, (float)y0),
                End = new Vector2((float)x1, (float)y1)
            });
        }

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)(x - r), (float)(y - r * 0.3)),
            End = new Vector2((float)x, (float)(y - r))
        });

        segs.Add(new LineSegment
        {
            Start = new Vector2((float)x, (float)(y - r)),
            End = new Vector2((float)(x + r), (float)(y - r * 0.3))
        });
    }
}
