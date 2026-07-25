namespace MathVerse.Math.Visualization.SimulationVisualization;

using System.Collections.Generic;

/// <summary>Represents a vertex of the oriented bounding box.</summary>
public sealed record OBBVertex
{
    /// <summary>X coordinate.</summary>
    public required double X { get; init; }

    /// <summary>Y coordinate.</summary>
    public required double Y { get; init; }

    /// <summary>Z coordinate.</summary>
    public double Z { get; init; }
}

/// <summary>Represents an edge of the oriented bounding box.</summary>
public sealed record OBBEdge
{
    /// <summary>Index of the first vertex.</summary>
    public required int From { get; init; }

    /// <summary>Index of the second vertex.</summary>
    public required int To { get; init; }
}

/// <summary>Complete data for rigid body visualization.</summary>
public sealed record RigidBodyVisualizationData
{
    /// <summary>Vertices of the oriented bounding box.</summary>
    public required IReadOnlyList<OBBVertex> Vertices { get; init; }

    /// <summary>Edges connecting vertices.</summary>
    public required IReadOnlyList<OBBEdge> Edges { get; init; }

    /// <summary>Center position of the body.</summary>
    public required (double X, double Y, double Z) Position { get; init; }

    /// <summary>Forward direction vector.</summary>
    public required (double X, double Y, double Z) Forward { get; init; }

    /// <summary>Up direction vector.</summary>
    public required (double X, double Y, double Z) Up { get; init; }

    /// <summary>Right direction vector.</summary>
    public required (double X, double Y, double Z) Right { get; init; }
}

/// <summary>Visualizes rigid body transforms as an oriented bounding box.</summary>
public sealed class RigidBodyVisualizer
{
    /// <summary>
    /// Creates an oriented bounding box visualization for a rigid body.
    /// </summary>
    /// <param name="position">Body center position [x, y, z].</param>
    /// <param name="orientation">Orientation as a flattened 3x3 rotation matrix (row-major) or quaternion [x, y, z, w].</param>
    /// <param name="extents">Half-extents [hx, hy, hz]. Defaults to [1, 1, 1].</param>
    /// <returns>Oriented bounding box vertices, edges, and direction vectors.</returns>
    public RigidBodyVisualizationData Create(double[] position, double[] orientation, double[]? extents = null)
    {
        double hx = extents != null && extents.Length > 0 ? extents[0] : 1.0;
        double hy = extents != null && extents.Length > 1 ? extents[1] : 1.0;
        double hz = extents != null && extents.Length > 2 ? extents[2] : 1.0;

        double px = position.Length > 0 ? position[0] : 0.0;
        double py = position.Length > 1 ? position[1] : 0.0;
        double pz = position.Length > 2 ? position[2] : 0.0;

        double[,] rot = ExtractRotationMatrix(orientation);

        double fx = rot[0, 0];
        double fy = rot[1, 0];
        double fz = rot[2, 0];
        double ux = rot[0, 1];
        double uy = rot[1, 1];
        double uz = rot[2, 1];
        double rx = rot[0, 2];
        double ry = rot[1, 2];
        double rz = rot[2, 2];

        double[] cornersX = new double[8];
        double[] cornersY = new double[8];
        double[] cornersZ = new double[8];

        for (int i = 0; i < 8; i++)
        {
            double sx = (i & 1) == 0 ? -1.0 : 1.0;
            double sy = (i & 2) == 0 ? -1.0 : 1.0;
            double sz = (i & 4) == 0 ? -1.0 : 1.0;

            cornersX[i] = px + sx * hx * fx + sy * hy * ux + sz * hz * rx;
            cornersY[i] = py + sx * hx * fy + sy * hy * uy + sz * hz * ry;
            cornersZ[i] = pz + sx * hx * fz + sy * hy * uz + sz * hz * rz;
        }

        var vertices = new List<OBBVertex>();
        for (int i = 0; i < 8; i++)
        {
            vertices.Add(new OBBVertex
            {
                X = cornersX[i],
                Y = cornersY[i],
                Z = cornersZ[i]
            });
        }

        var edges = new List<OBBEdge>
        {
            new OBBEdge { From = 0, To = 1 },
            new OBBEdge { From = 0, To = 2 },
            new OBBEdge { From = 0, To = 4 },
            new OBBEdge { From = 1, To = 3 },
            new OBBEdge { From = 1, To = 5 },
            new OBBEdge { From = 2, To = 3 },
            new OBBEdge { From = 2, To = 6 },
            new OBBEdge { From = 3, To = 7 },
            new OBBEdge { From = 4, To = 5 },
            new OBBEdge { From = 4, To = 6 },
            new OBBEdge { From = 5, To = 7 },
            new OBBEdge { From = 6, To = 7 }
        };

        return new RigidBodyVisualizationData
        {
            Vertices = vertices,
            Edges = edges,
            Position = (px, py, pz),
            Forward = (fx, fy, fz),
            Up = (ux, uy, uz),
            Right = (rx, ry, rz)
        };
    }

    private static double[,] ExtractRotationMatrix(double[] orientation)
    {
        double[,] rot = new double[3, 3];

        if (orientation.Length >= 9)
        {
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    rot[r, c] = orientation[r * 3 + c];
        }
        else if (orientation.Length >= 4)
        {
            double qx = orientation[0];
            double qy = orientation[1];
            double qz = orientation[2];
            double qw = orientation[3];

            rot[0, 0] = 1.0 - 2.0 * (qy * qy + qz * qz);
            rot[0, 1] = 2.0 * (qx * qy - qz * qw);
            rot[0, 2] = 2.0 * (qx * qz + qy * qw);
            rot[1, 0] = 2.0 * (qx * qy + qz * qw);
            rot[1, 1] = 1.0 - 2.0 * (qx * qx + qz * qz);
            rot[1, 2] = 2.0 * (qy * qz - qx * qw);
            rot[2, 0] = 2.0 * (qx * qz - qy * qw);
            rot[2, 1] = 2.0 * (qy * qz + qx * qw);
            rot[2, 2] = 1.0 - 2.0 * (qx * qx + qy * qy);
        }
        else
        {
            rot[0, 0] = 1.0;
            rot[1, 1] = 1.0;
            rot[2, 2] = 1.0;
        }

        return rot;
    }
}
