namespace MathVerse.Math.Visualization.Labeling;
using System.Numerics;
using System.Collections.Generic;

/// <summary>Represents the result of label layout computation.</summary>
public sealed class LabelLayoutResult
{
    /// <summary>Gets the repositioned labels.</summary>
    public List<MathLabel> Labels { get; init; } = new();

    /// <summary>Gets the number of overlapping labels that were moved.</summary>
    public int OverlapsResolved { get; init; }

    /// <summary>Gets whether all labels could be placed without overlap.</summary>
    public bool AllPlaced { get; init; }
}

/// <summary>Automatic label placement engine to avoid overlaps.</summary>
public sealed class LabelLayoutEngine
{
    private const double MinSeparation = 4.0;
    private const int MaxIterations = 100;
    private const double RepulsionForce = 0.5;
    private const double AttractionForce = 0.01;

    /// <summary>Computes non-overlapping label positions using a greedy placement algorithm.</summary>
    /// <param name="labels">The input labels to place.</param>
    /// <param name="width">The width of the layout area.</param>
    /// <param name="height">The height of the layout area.</param>
    /// <returns>A layout result with repositioned labels.</returns>
    public static LabelLayoutResult LayoutLabels(List<MathLabel> labels, double width, double height)
    {
        if (labels.Count == 0)
        {
            return new LabelLayoutResult
            {
                Labels = new List<MathLabel>(),
                OverlapsResolved = 0,
                AllPlaced = true
            };
        }

        var placed = new List<MathLabel>();
        var placedBounds = new List<(double x, double y, double w, double h)>();
        int overlapsResolved = 0;

        foreach (var label in labels)
        {
            var bestPosition = FindBestPosition(label, placedBounds, width, height);
            var repositioned = label with { Position = bestPosition };

            placed.Add(repositioned);

            double labelWidth = label.Text.Length * label.FontSize * 0.6;
            double labelHeight = label.FontSize * 1.2;
            placedBounds.Add((bestPosition.X - labelWidth / 2.0, bestPosition.Y - labelHeight / 2.0, labelWidth, labelHeight));

            if (System.Math.Abs(bestPosition.X - label.Position.X) > 0.1 ||
                System.Math.Abs(bestPosition.Y - label.Position.Y) > 0.1)
            {
                overlapsResolved++;
            }
        }

        var refined = RefinePositions(placed, width, height);

        return new LabelLayoutResult
        {
            Labels = refined,
            OverlapsResolved = overlapsResolved,
            AllPlaced = overlapsResolved == 0
        };
    }

    /// <summary>Checks if two rectangular labels overlap.</summary>
    /// <param name="ax">X coordinate of first label center.</param>
    /// <param name="ay">Y coordinate of first label center.</param>
    /// <param name="aw">Width of first label.</param>
    /// <param name="ah">Height of first label.</param>
    /// <param name="bx">X coordinate of second label center.</param>
    /// <param name="by">Y coordinate of second label center.</param>
    /// <param name="bw">Width of second label.</param>
    /// <param name="bh">Height of second label.</param>
    /// <returns>True if the labels overlap.</returns>
    public static bool CheckOverlap(double ax, double ay, double aw, double ah,
        double bx, double by, double bw, double bh)
    {
        double aLeft = ax - aw / 2.0 - MinSeparation;
        double aRight = ax + aw / 2.0 + MinSeparation;
        double aTop = ay - ah / 2.0 - MinSeparation;
        double aBottom = ay + ah / 2.0 + MinSeparation;

        double bLeft = bx - bw / 2.0;
        double bRight = bx + bw / 2.0;
        double bTop = by - bh / 2.0;
        double bBottom = by + bh / 2.0;

        return aLeft < bRight && aRight > bLeft && aTop < bBottom && aBottom > bTop;
    }

    private static Vector2 FindBestPosition(MathLabel label, List<(double x, double y, double w, double h)> placedBounds,
        double width, double height)
    {
        double labelWidth = label.Text.Length * label.FontSize * 0.6;
        double labelHeight = label.FontSize * 1.2;

        Vector2 original = label.Position;

        if (!HasAnyOverlap(original.X, original.Y, labelWidth, labelHeight, placedBounds))
            return original;

        double[] angles = { 0, System.Math.PI / 4, System.Math.PI / 2, 3 * System.Math.PI / 4,
                           System.Math.PI, 5 * System.Math.PI / 4, 3 * System.Math.PI / 2, 7 * System.Math.PI / 4 };
        double[] distances = { 10, 20, 30, 50, 70, 100 };

        double bestDist = double.MaxValue;
        Vector2 bestPos = original;

        foreach (double dist in distances)
        {
            foreach (double angle in angles)
            {
                double testX = original.X + dist * System.Math.Cos(angle);
                double testY = original.Y + dist * System.Math.Sin(angle);

                if (testX - labelWidth / 2.0 < 0 || testX + labelWidth / 2.0 > width ||
                    testY - labelHeight / 2.0 < 0 || testY + labelHeight / 2.0 > height)
                    continue;

                if (!HasAnyOverlap(testX, testY, labelWidth, labelHeight, placedBounds))
                {
                    double d = System.Math.Sqrt(
                        (testX - original.X) * (testX - original.X) +
                        (testY - original.Y) * (testY - original.Y));

                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestPos = new Vector2((float)testX, (float)testY);
                    }
                }
            }
        }

        return bestPos;
    }

    private static bool HasAnyOverlap(double x, double y, double w, double h,
        List<(double x, double y, double w, double h)> placedBounds)
    {
        foreach (var (px, py, pw, ph) in placedBounds)
        {
            if (CheckOverlap(x, y, w, h, px, py, pw, ph))
                return true;
        }
        return false;
    }

    private static List<MathLabel> RefinePositions(List<MathLabel> labels, double width, double height)
    {
        var positions = new Vector2[labels.Count];
        var sizes = new (double w, double h)[labels.Count];

        for (int i = 0; i < labels.Count; i++)
        {
            positions[i] = labels[i].Position;
            sizes[i] = (labels[i].Text.Length * labels[i].FontSize * 0.6, labels[i].FontSize * 1.2);
        }

        for (int iter = 0; iter < MaxIterations; iter++)
        {
            bool anyMoved = false;

            for (int i = 0; i < labels.Count; i++)
            {
                float fx = 0, fy = 0;

                for (int j = 0; j < labels.Count; j++)
                {
                    if (i == j) continue;

                    float dx = positions[i].X - positions[j].X;
                    float dy = positions[i].Y - positions[j].Y;
                    float dist = System.Math.Max(0.1f, (float)System.Math.Sqrt(dx * dx + dy * dy));

                    float minDist = (float)((sizes[i].w + sizes[j].w) / 2.0 + MinSeparation);

                    if (dist < minDist)
                    {
                        float force = (minDist - dist) / dist * (float)RepulsionForce;
                        fx += dx * force;
                        fy += dy * force;
                    }
                    else
                    {
                        float pullForce = (dist - minDist) / dist * (float)AttractionForce;
                        fx -= dx * pullForce;
                        fy -= dy * pullForce;
                    }
                }

                float newX = positions[i].X + fx;
                float newY = positions[i].Y + fy;

                newX = System.Math.Max((float)(sizes[i].w / 2.0), System.Math.Min((float)(width - sizes[i].w / 2.0), newX));
                newY = System.Math.Max((float)(sizes[i].h / 2.0), System.Math.Min((float)(height - sizes[i].h / 2.0), newY));

                if (System.Math.Abs(newX - positions[i].X) > 0.01f || System.Math.Abs(newY - positions[i].Y) > 0.01f)
                {
                    positions[i] = new Vector2(newX, newY);
                    anyMoved = true;
                }
            }

            if (!anyMoved)
                break;
        }

        var result = new List<MathLabel>();
        for (int i = 0; i < labels.Count; i++)
        {
            result.Add(labels[i] with { Position = positions[i] });
        }

        return result;
    }
}
