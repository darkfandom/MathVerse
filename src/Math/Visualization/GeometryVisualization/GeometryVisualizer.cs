namespace MathVerse.Math.Visualization.GeometryVisualization;

using System.Collections.Immutable;
using MathVerse.Math.Visualization._3DPlotting;

/// <summary>Main facade for geometry visualization. Takes geometry objects and produces <see cref="Plot3DResult"/> instances.</summary>
public sealed class GeometryVisualizer
{

    /// <summary>Visualizes a single point in 3D space.</summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="z">Z coordinate.</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the point.</returns>
    public Plot3DResult VisualizePoint(double x, double y, double z)
    {
        var result = new Plot3DResult();
        result.Points.Add(new Point3DSeries
        {
            Name = "Point",
            X = ImmutableArray.Create(x),
            Y = ImmutableArray.Create(y),
            Z = ImmutableArray.Create(z),
            Color = "#E74C3C"
        });
        UpdateBounds(result, x, y, z);
        return result;
    }

    /// <summary>Visualizes a line segment between two points.</summary>
    /// <param name="x1">Start X coordinate.</param>
    /// <param name="y1">Start Y coordinate.</param>
    /// <param name="z1">Start Z coordinate.</param>
    /// <param name="x2">End X coordinate.</param>
    /// <param name="y2">End Y coordinate.</param>
    /// <param name="z2">End Z coordinate.</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the line.</returns>
    public Plot3DResult VisualizeLine(double x1, double y1, double z1, double x2, double y2, double z2)
    {
        var result = new Plot3DResult();
        result.Lines.Add(new Line3DSeries
        {
            Name = "Line",
            X = ImmutableArray.Create(x1, x2),
            Y = ImmutableArray.Create(y1, y2),
            Z = ImmutableArray.Create(z1, z2),
            Color = "#007ACC"
        });
        UpdateBounds(result, x1, y1, z1);
        UpdateBounds(result, x2, y2, z2);
        return result;
    }

    /// <summary>Visualizes a ray starting at the origin and extending in the given direction.</summary>
    /// <param name="ox">Origin X coordinate.</param>
    /// <param name="oy">Origin Y coordinate.</param>
    /// <param name="oz">Origin Z coordinate.</param>
    /// <param name="dx">Direction X component.</param>
    /// <param name="dy">Direction Y component.</param>
    /// <param name="dz">Direction Z component.</param>
    /// <param name="length">Length of the ray visualization (default 10.0).</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the ray.</returns>
    public Plot3DResult VisualizeRay(double ox, double oy, double oz, double dx, double dy, double dz, double length = 10.0)
    {
        double dirLen = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        if (dirLen < 1e-15) return VisualizePoint(ox, oy, oz);

        double nx = dx / dirLen;
        double ny = dy / dirLen;
        double nz = dz / dirLen;

        double ex = ox + nx * length;
        double ey = oy + ny * length;
        double ez = oz + nz * length;

        var result = new Plot3DResult();
        result.Lines.Add(new Line3DSeries
        {
            Name = "Ray",
            X = ImmutableArray.Create(ox, ex),
            Y = ImmutableArray.Create(oy, ey),
            Z = ImmutableArray.Create(oz, ez),
            Color = "#8E44AD"
        });
        UpdateBounds(result, ox, oy, oz);
        UpdateBounds(result, ex, ey, ez);
        return result;
    }

    /// <summary>Visualizes a line segment between two points.</summary>
    /// <param name="x1">Start X coordinate.</param>
    /// <param name="y1">Start Y coordinate.</param>
    /// <param name="z1">Start Z coordinate.</param>
    /// <param name="x2">End X coordinate.</param>
    /// <param name="y2">End Y coordinate.</param>
    /// <param name="z2">End Z coordinate.</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the segment.</returns>
    public Plot3DResult VisualizeSegment(double x1, double y1, double z1, double x2, double y2, double z2)
    {
        var result = new Plot3DResult();
        result.Lines.Add(new Line3DSeries
        {
            Name = "Segment",
            X = ImmutableArray.Create(x1, x2),
            Y = ImmutableArray.Create(y1, y2),
            Z = ImmutableArray.Create(z1, z2),
            Color = "#2ECC71"
        });
        result.Points.Add(new Point3DSeries
        {
            Name = "Endpoints",
            X = ImmutableArray.Create(x1, x2),
            Y = ImmutableArray.Create(y1, y2),
            Z = ImmutableArray.Create(z1, z2),
            Color = "#E74C3C",
            PointSize = 6.0
        });
        UpdateBounds(result, x1, y1, z1);
        UpdateBounds(result, x2, y2, z2);
        return result;
    }

    /// <summary>Visualizes a circle in 3D space.</summary>
    /// <param name="cx">Center X coordinate.</param>
    /// <param name="cy">Center Y coordinate.</param>
    /// <param name="cz">Center Z coordinate.</param>
    /// <param name="radius">The circle radius.</param>
    /// <param name="segments">Number of segments (default 64).</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the circle.</returns>
    public Plot3DResult VisualizeCircle(double cx, double cy, double cz, double radius, int segments = 64)
    {
        var (x, y, z) = GeometryRenderHelper.GenerateCirclePoints(cx, cy, cz, radius, segments);
        var result = new Plot3DResult();
        result.Lines.Add(new Line3DSeries
        {
            Name = "Circle",
            X = x,
            Y = y,
            Z = z,
            Color = "#007ACC"
        });
        foreach (var xi in x) foreach (var yi in y) foreach (var zi in z)
        {
            UpdateBounds(result, xi, yi, zi);
            break;
        }
        // Simpler bounds update
        for (int i = 0; i < x.Length; i++)
            UpdateBounds(result, x[i], y[i], z[i]);
        return result;
    }

    /// <summary>Visualizes an ellipse in 3D space (XY plane).</summary>
    /// <param name="cx">Center X coordinate.</param>
    /// <param name="cy">Center Y coordinate.</param>
    /// <param name="cz">Center Z coordinate.</param>
    /// <param name="rx">X-axis radius.</param>
    /// <param name="ry">Y-axis radius.</param>
    /// <param name="segments">Number of segments (default 64).</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the ellipse.</returns>
    public Plot3DResult VisualizeEllipse(double cx, double cy, double cz, double rx, double ry, int segments = 64)
    {
        var xBuilder = ImmutableArray.CreateBuilder<double>(segments + 1);
        var yBuilder = ImmutableArray.CreateBuilder<double>(segments + 1);
        var zBuilder = ImmutableArray.CreateBuilder<double>(segments + 1);

        for (int i = 0; i <= segments; i++)
        {
            double angle = 2.0 * System.Math.PI * i / segments;
            xBuilder.Add(cx + rx * System.Math.Cos(angle));
            yBuilder.Add(cy + ry * System.Math.Sin(angle));
            zBuilder.Add(cz);
        }

        var result = new Plot3DResult();
        result.Lines.Add(new Line3DSeries
        {
            Name = "Ellipse",
            X = xBuilder.ToImmutable(),
            Y = yBuilder.ToImmutable(),
            Z = zBuilder.ToImmutable(),
            Color = "#F39C12"
        });

        for (int i = 0; i <= segments; i++)
            UpdateBounds(result, xBuilder[i], yBuilder[i], zBuilder[i]);
        return result;
    }

    /// <summary>Visualizes a polygon by triangulating and rendering as a filled mesh.</summary>
    /// <param name="vertices">Array of vertex positions (each [x, y] or [x, y, z]).</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the triangulated polygon.</returns>
    public Plot3DResult VisualizePolygon(double[][] vertices)
    {
        var result = new Plot3DResult();
        if (vertices.Length < 2) return result;

        bool is3D = vertices[0].Length >= 3;
        double defaultZ = 0.0;

        var xPts = ImmutableArray.CreateBuilder<double>(vertices.Length);
        var yPts = ImmutableArray.CreateBuilder<double>(vertices.Length);
        var zPts = ImmutableArray.CreateBuilder<double>(vertices.Length);

        foreach (var v in vertices)
        {
            xPts.Add(v[0]);
            yPts.Add(v[1]);
            zPts.Add(v.Length >= 3 ? v[2] : defaultZ);
        }

        var xArr = xPts.ToImmutable();
        var yArr = yPts.ToImmutable();
        var zArr = zPts.ToImmutable();

        // Outline
        var outlineX = ImmutableArray.CreateBuilder<double>(vertices.Length + 1);
        var outlineY = ImmutableArray.CreateBuilder<double>(vertices.Length + 1);
        var outlineZ = ImmutableArray.CreateBuilder<double>(vertices.Length + 1);

        for (int i = 0; i < vertices.Length; i++)
        {
            outlineX.Add(xArr[i]);
            outlineY.Add(yArr[i]);
            outlineZ.Add(zArr[i]);
        }
        outlineX.Add(xArr[0]);
        outlineY.Add(yArr[0]);
        outlineZ.Add(zArr[0]);

        result.Lines.Add(new Line3DSeries
        {
            Name = "Polygon Outline",
            X = outlineX.ToImmutable(),
            Y = outlineY.ToImmutable(),
            Z = outlineZ.ToImmutable(),
            Color = "#007ACC"
        });

        // Triangulate
        var triangles = GeometryRenderHelper.TriangulatePolygon(vertices);

        var faceIndices = ImmutableArray.CreateBuilder<int>(triangles.Count * 3);
        foreach (var (a, b, c) in triangles)
        {
            faceIndices.Add(a);
            faceIndices.Add(b);
            faceIndices.Add(c);
        }

        result.Meshes.Add(new Mesh3DSeries
        {
            Name = "Polygon Fill",
            VertexX = xArr,
            VertexY = yArr,
            VertexZ = zArr,
            FaceIndices = faceIndices.ToImmutable(),
            Color = "#3498DB",
            Opacity = 0.5
        });

        foreach (var v in vertices)
            UpdateBounds(result, v[0], v[1], v.Length >= 3 ? v[2] : 0.0);
        return result;
    }

    /// <summary>Visualizes a polyhedron defined by vertices and face indices.</summary>
    /// <param name="vertices">Array of vertex positions [x, y, z].</param>
    /// <param name="faces">Array of face index triplets [i0, i1, i2].</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the polyhedron mesh and edges.</returns>
    public Plot3DResult VisualizePolyhedron(double[][] vertices, int[][] faces)
    {
        return VisualizeMesh(vertices, faces);
    }

    /// <summary>Computes and visualizes the convex hull of a set of points using the gift-wrapping algorithm.</summary>
    /// <param name="points">Array of point positions [x, y] or [x, y, z].</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the convex hull.</returns>
    public Plot3DResult VisualizeConvexHull(double[][] points)
    {
        var result = new Plot3DResult();
        if (points.Length < 3) return result;

        bool is3D = points[0].Length >= 3;

        if (is3D)
        {
            var hullVerts = ComputeConvexHull3D(points);
            var faceIndices = new List<int>();

            // Generate triangle fan from hull vertices for surface
            for (int i = 1; i < hullVerts.Count - 1; i++)
            {
                faceIndices.Add(0);
                faceIndices.Add(i);
                faceIndices.Add(i + 1);
            }

            var vx = ImmutableArray.CreateBuilder<double>(hullVerts.Count);
            var vy = ImmutableArray.CreateBuilder<double>(hullVerts.Count);
            var vz = ImmutableArray.CreateBuilder<double>(hullVerts.Count);
            foreach (var v in hullVerts)
            {
                vx.Add(v[0]);
                vy.Add(v[1]);
                vz.Add(v[2]);
            }

            if (faceIndices.Count > 0)
            {
                result.Meshes.Add(new Mesh3DSeries
                {
                    Name = "Convex Hull",
                    VertexX = vx.ToImmutable(),
                    VertexY = vy.ToImmutable(),
                    VertexZ = vz.ToImmutable(),
                    FaceIndices = ImmutableArray.Create(faceIndices.ToArray()),
                    Color = "#2ECC71",
                    Opacity = 0.4,
                    Wireframe = false
                });
            }

            // Outline edges
            var edgeX1 = ImmutableArray.CreateBuilder<double>(hullVerts.Count);
            var edgeY1 = ImmutableArray.CreateBuilder<double>(hullVerts.Count);
            var edgeZ1 = ImmutableArray.CreateBuilder<double>(hullVerts.Count);
            var edgeX2 = ImmutableArray.CreateBuilder<double>(hullVerts.Count);
            var edgeY2 = ImmutableArray.CreateBuilder<double>(hullVerts.Count);
            var edgeZ2 = ImmutableArray.CreateBuilder<double>(hullVerts.Count);

            for (int i = 0; i < hullVerts.Count; i++)
            {
                int j = (i + 1) % hullVerts.Count;
                edgeX1.Add(hullVerts[i][0]);
                edgeY1.Add(hullVerts[i][1]);
                edgeZ1.Add(hullVerts[i][2]);
                edgeX2.Add(hullVerts[j][0]);
                edgeY2.Add(hullVerts[j][1]);
                edgeZ2.Add(hullVerts[j][2]);
            }

            result.Edges.Add(new Edge3DSeries
            {
                Name = "Hull Edges",
                X1 = edgeX1.ToImmutable(),
                Y1 = edgeY1.ToImmutable(),
                Z1 = edgeZ1.ToImmutable(),
                X2 = edgeX2.ToImmutable(),
                Y2 = edgeY2.ToImmutable(),
                Z2 = edgeZ2.ToImmutable(),
                Color = "#27AE60"
            });

            foreach (var v in hullVerts)
                UpdateBounds(result, v[0], v[1], v[2]);
        }
        else
        {
            var hull2D = ComputeConvexHull2D(points);
            var xPts = ImmutableArray.CreateBuilder<double>(hull2D.Count + 1);
            var yPts = ImmutableArray.CreateBuilder<double>(hull2D.Count + 1);
            var zPts = ImmutableArray.CreateBuilder<double>(hull2D.Count + 1);

            foreach (var p in hull2D)
            {
                xPts.Add(p[0]);
                yPts.Add(p[1]);
                zPts.Add(0.0);
            }
            xPts.Add(hull2D[0][0]);
            yPts.Add(hull2D[0][1]);
            zPts.Add(0.0);

            result.Lines.Add(new Line3DSeries
            {
                Name = "Convex Hull",
                X = xPts.ToImmutable(),
                Y = yPts.ToImmutable(),
                Z = zPts.ToImmutable(),
                Color = "#2ECC71"
            });

            result.Points.Add(new Point3DSeries
            {
                Name = "Hull Vertices",
                X = ImmutableArray.Create(hull2D.Select(p => p[0]).ToArray()),
                Y = ImmutableArray.Create(hull2D.Select(p => p[1]).ToArray()),
                Z = ImmutableArray.CreateRange(Enumerable.Repeat(0.0, hull2D.Count)),
                Color = "#27AE60"
            });

            foreach (var p in hull2D)
                UpdateBounds(result, p[0], p[1], 0.0);
        }

        return result;
    }

    /// <summary>Visualizes a Voronoi diagram clipped to a bounding box using Fortune's sweep-line concept.</summary>
    /// <param name="sites">Array of site positions [x, y].</param>
    /// <param name="xMin">Bounding box minimum X.</param>
    /// <param name="xMax">Bounding box maximum X.</param>
    /// <param name="yMin">Bounding box minimum Y.</param>
    /// <param name="yMax">Bounding box maximum Y.</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the Voronoi cells and sites.</returns>
    public Plot3DResult VisualizeVoronoi(double[][] sites, double xMin, double xMax, double yMin, double yMax)
    {
        var result = new Plot3DResult();
        if (sites.Length < 2) return result;

        // Compute Voronoi via Fortune-like approach: for each site, compute its cell
        var cells = ComputeVoronoiCells(sites, xMin, xMax, yMin, yMax);

        for (int c = 0; c < cells.Count; c++)
        {
            var cell = cells[c];
            if (cell.Length < 2) continue;

            var cx = ImmutableArray.CreateBuilder<double>(cell.Length + 1);
            var cy = ImmutableArray.CreateBuilder<double>(cell.Length + 1);
            var cz = ImmutableArray.CreateBuilder<double>(cell.Length + 1);

            foreach (var v in cell)
            {
                cx.Add(v[0]);
                cy.Add(v[1]);
                cz.Add(0.0);
            }
            cx.Add(cell[0][0]);
            cy.Add(cell[0][1]);
            cz.Add(0.0);

            result.Lines.Add(new Line3DSeries
            {
                Name = $"Voronoi Cell {c}",
                X = cx.ToImmutable(),
                Y = cy.ToImmutable(),
                Z = cz.ToImmutable(),
                Color = "#9B59B6"
            });
        }

        // Plot sites
        var siteX = ImmutableArray.Create(sites.Select(s => s[0]).ToArray());
        var siteY = ImmutableArray.Create(sites.Select(s => s[1]).ToArray());
        var siteZ = ImmutableArray.CreateRange(Enumerable.Repeat(0.0, sites.Length));

        result.Points.Add(new Point3DSeries
        {
            Name = "Sites",
            X = siteX,
            Y = siteY,
            Z = siteZ,
            Color = "#E74C3C",
            PointSize = 6.0
        });

        UpdateBounds(result, xMin, yMin, 0.0);
        UpdateBounds(result, xMax, yMax, 0.0);
        return result;
    }

    /// <summary>Visualizes the Delaunay triangulation of a set of 2D points using incremental insertion.</summary>
    /// <param name="points">Array of point positions [x, y].</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the triangulation.</returns>
    public Plot3DResult VisualizeDelaunay(double[][] points)
    {
        var result = new Plot3DResult();
        if (points.Length < 3) return result;

        var triangles = ComputeDelaunayTriangulation(points);

        var edgeX1 = ImmutableArray.CreateBuilder<double>(triangles.Count * 3);
        var edgeY1 = ImmutableArray.CreateBuilder<double>(triangles.Count * 3);
        var edgeZ1 = ImmutableArray.CreateBuilder<double>(triangles.Count * 3);
        var edgeX2 = ImmutableArray.CreateBuilder<double>(triangles.Count * 3);
        var edgeY2 = ImmutableArray.CreateBuilder<double>(triangles.Count * 3);
        var edgeZ2 = ImmutableArray.CreateBuilder<double>(triangles.Count * 3);

        foreach (var (a, b, c) in triangles)
        {
            edgeX1.Add(points[a][0]); edgeY1.Add(points[a][1]); edgeZ1.Add(0.0);
            edgeX2.Add(points[b][0]); edgeY2.Add(points[b][1]); edgeZ2.Add(0.0);

            edgeX1.Add(points[b][0]); edgeY1.Add(points[b][1]); edgeZ1.Add(0.0);
            edgeX2.Add(points[c][0]); edgeY2.Add(points[c][1]); edgeZ2.Add(0.0);

            edgeX1.Add(points[c][0]); edgeY1.Add(points[c][1]); edgeZ1.Add(0.0);
            edgeX2.Add(points[a][0]); edgeY2.Add(points[a][1]); edgeZ2.Add(0.0);
        }

        result.Edges.Add(new Edge3DSeries
        {
            Name = "Delaunay Edges",
            X1 = edgeX1.ToImmutable(),
            Y1 = edgeY1.ToImmutable(),
            Z1 = edgeZ1.ToImmutable(),
            X2 = edgeX2.ToImmutable(),
            Y2 = edgeY2.ToImmutable(),
            Z2 = edgeZ2.ToImmutable(),
            Color = "#3498DB"
        });

        var ptX = ImmutableArray.Create(points.Select(p => p[0]).ToArray());
        var ptY = ImmutableArray.Create(points.Select(p => p[1]).ToArray());
        var ptZ = ImmutableArray.CreateRange(Enumerable.Repeat(0.0, points.Length));

        result.Points.Add(new Point3DSeries
        {
            Name = "Vertices",
            X = ptX,
            Y = ptY,
            Z = ptZ,
            Color = "#E74C3C",
            PointSize = 5.0
        });

        foreach (var p in points)
            UpdateBounds(result, p[0], p[1], 0.0);
        return result;
    }

    /// <summary>Visualizes a KD-tree by rendering its splitting planes as line segments.</summary>
    /// <param name="points">Array of point positions [x, y] or [x, y, z].</param>
    /// <param name="maxDepth">Maximum tree depth (default 10).</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the KD-tree partitioning.</returns>
    public Plot3DResult VisualizeKDTree(double[][] points, int maxDepth = 10)
    {
        var result = new Plot3DResult();
        if (points.Length == 0) return result;

        bool is3D = points[0].Length >= 3;

        double globalXMin = points.Min(p => p[0]);
        double globalXMax = points.Max(p => p[0]);
        double globalYMin = points.Min(p => p[1]);
        double globalYMax = points.Max(p => p[1]);
        double globalZMin = is3D ? points.Min(p => p[2]) : 0.0;
        double globalZMax = is3D ? points.Max(p => p[2]) : 0.0;

        BuildKDTreeVis(points, 0, maxDepth, globalXMin, globalXMax, globalYMin, globalYMax, globalZMin, globalZMax, is3D, result);

        var ptX = ImmutableArray.Create(points.Select(p => p[0]).ToArray());
        var ptY = ImmutableArray.Create(points.Select(p => p[1]).ToArray());
        var ptZ = ImmutableArray.CreateRange(is3D ? points.Select(p => p[2]) : Enumerable.Repeat(0.0, points.Length));

        result.Points.Add(new Point3DSeries
        {
            Name = "Points",
            X = ptX,
            Y = ptY,
            Z = ptZ,
            Color = "#E74C3C",
            PointSize = 4.0
        });

        UpdateBounds(result, globalXMin, globalYMin, globalZMin);
        UpdateBounds(result, globalXMax, globalYMax, globalZMax);
        return result;
    }

    /// <summary>Visualizes a triangle mesh with vertices, faces, and optional normals.</summary>
    /// <param name="vertices">Array of vertex positions [x, y, z].</param>
    /// <param name="faces">Array of face index triplets [i0, i1, i2].</param>
    /// <param name="normals">Optional per-vertex normals [nx, ny, nz].</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the mesh and edge wireframe.</returns>
    public Plot3DResult VisualizeMesh(double[][] vertices, int[][] faces, double[][]? normals = null)
    {
        var result = new Plot3DResult();
        if (vertices.Length == 0 || faces.Length == 0) return result;

        var vx = ImmutableArray.Create(vertices.Select(v => v[0]).ToArray());
        var vy = ImmutableArray.Create(vertices.Select(v => v[1]).ToArray());
        var vz = ImmutableArray.Create(vertices.Select(v => v[2]).ToArray());

        var faceIndices = ImmutableArray.Create(faces.SelectMany(f => f).ToArray());

        ImmutableArray<double>? nx = null, ny = null, nz = null;
        if (normals != null && normals.Length == vertices.Length)
        {
            nx = ImmutableArray.Create(normals.Select(n => n[0]).ToArray());
            ny = ImmutableArray.Create(normals.Select(n => n[1]).ToArray());
            nz = ImmutableArray.Create(normals.Select(n => n[2]).ToArray());
        }

        result.Meshes.Add(new Mesh3DSeries
        {
            Name = "Mesh",
            VertexX = vx,
            VertexY = vy,
            VertexZ = vz,
            FaceIndices = faceIndices,
            NormalX = nx,
            NormalY = ny,
            NormalZ = nz,
            Color = "#3498DB",
            Opacity = 0.8
        });

        // Wireframe edges
        var edgeSet = new HashSet<(int, int)>();
        var ex1 = ImmutableArray.CreateBuilder<double>();
        var ey1 = ImmutableArray.CreateBuilder<double>();
        var ez1 = ImmutableArray.CreateBuilder<double>();
        var ex2 = ImmutableArray.CreateBuilder<double>();
        var ey2 = ImmutableArray.CreateBuilder<double>();
        var ez2 = ImmutableArray.CreateBuilder<double>();

        foreach (var face in faces)
        {
            for (int i = 0; i < face.Length; i++)
            {
                int a = face[i];
                int b = face[(i + 1) % face.Length];
                var edge = a < b ? (a, b) : (b, a);
                if (edgeSet.Add(edge))
                {
                    ex1.Add(vertices[a][0]); ey1.Add(vertices[a][1]); ez1.Add(vertices[a][2]);
                    ex2.Add(vertices[b][0]); ey2.Add(vertices[b][1]); ez2.Add(vertices[b][2]);
                }
            }
        }

        if (ex1.Count > 0)
        {
            result.Edges.Add(new Edge3DSeries
            {
                Name = "Wireframe",
                X1 = ex1.ToImmutable(),
                Y1 = ey1.ToImmutable(),
                Z1 = ez1.ToImmutable(),
                X2 = ex2.ToImmutable(),
                Y2 = ey2.ToImmutable(),
                Z2 = ez2.ToImmutable(),
                Color = "#2980B9"
            });
        }

        foreach (var v in vertices)
            UpdateBounds(result, v[0], v[1], v[2]);
        return result;
    }

    /// <summary>Visualizes a Bezier curve using de Casteljau's algorithm.</summary>
    /// <param name="controlPoints">Array of control point positions [x, y, z].</param>
    /// <param name="segments">Number of evaluation segments (default 100).</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the curve and control points.</returns>
    public Plot3DResult VisualizeBezierCurve(double[][] controlPoints, int segments = 100)
    {
        var result = new Plot3DResult();
        if (controlPoints.Length == 0) return result;

        var curveX = ImmutableArray.CreateBuilder<double>(segments + 1);
        var curveY = ImmutableArray.CreateBuilder<double>(segments + 1);
        var curveZ = ImmutableArray.CreateBuilder<double>(segments + 1);

        for (int i = 0; i <= segments; i++)
        {
            double t = (double)i / segments;
            double[] pt = DeCasteljau(controlPoints, t);
            curveX.Add(pt[0]);
            curveY.Add(pt[1]);
            curveZ.Add(pt.Length >= 3 ? pt[2] : 0.0);
        }

        result.Lines.Add(new Line3DSeries
        {
            Name = "Bezier Curve",
            X = curveX.ToImmutable(),
            Y = curveY.ToImmutable(),
            Z = curveZ.ToImmutable(),
            Color = "#E67E22"
        });

        // Control polygon
        if (controlPoints.Length > 1)
        {
            var cpX = ImmutableArray.Create(controlPoints.Select(p => p[0]).ToArray());
            var cpY = ImmutableArray.Create(controlPoints.Select(p => p[1]).ToArray());
            var cpZ = ImmutableArray.Create(controlPoints.Select(p => p.Length >= 3 ? p[2] : 0.0).ToArray());

            result.Lines.Add(new Line3DSeries
            {
                Name = "Control Polygon",
                X = cpX,
                Y = cpY,
                Z = cpZ,
                Color = "#95A5A6",
                IsDashed = true
            });
        }

        // Control points
        var cpx = ImmutableArray.Create(controlPoints.Select(p => p[0]).ToArray());
        var cpy = ImmutableArray.Create(controlPoints.Select(p => p[1]).ToArray());
        var cpz = ImmutableArray.Create(controlPoints.Select(p => p.Length >= 3 ? p[2] : 0.0).ToArray());

        result.Points.Add(new Point3DSeries
        {
            Name = "Control Points",
            X = cpx,
            Y = cpy,
            Z = cpz,
            Color = "#E74C3C",
            PointSize = 7.0,
            Marker = "diamond"
        });

        foreach (var p in controlPoints)
            UpdateBounds(result, p[0], p[1], p.Length >= 3 ? p[2] : 0.0);
        return result;
    }

    /// <summary>Visualizes a Bezier surface using tensor-product evaluation.</summary>
    /// <param name="controlPoints">3D array of control points [u, v, xyz].</param>
    /// <param name="resolution">Number of subdivisions in each parametric direction (default 20).</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the surface mesh.</returns>
    public Plot3DResult VisualizeBezierSurface(double[,,] controlPoints, int resolution = 20)
    {
        var result = new Plot3DResult();

        int uCount = controlPoints.GetLength(0);
        int vCount = controlPoints.GetLength(1);

        int vertCount = (resolution + 1) * (resolution + 1);
        var vx = ImmutableArray.CreateBuilder<double>(vertCount);
        var vy = ImmutableArray.CreateBuilder<double>(vertCount);
        var vz = ImmutableArray.CreateBuilder<double>(vertCount);

        for (int iu = 0; iu <= resolution; iu++)
        {
            double u = (double)iu / resolution;
            for (int iv = 0; iv <= resolution; iv++)
            {
                double v = (double)iv / resolution;

                double px = 0, py = 0, pz = 0;
                for (int i = 0; i < uCount; i++)
                {
                    double bu = Bernstein(uCount - 1, i, u);
                    for (int j = 0; j < vCount; j++)
                    {
                        double bv = Bernstein(vCount - 1, j, v);
                        double w = bu * bv;
                        px += w * controlPoints[i, j, 0];
                        py += w * controlPoints[i, j, 1];
                        pz += w * controlPoints[i, j, 2];
                    }
                }

                vx.Add(px);
                vy.Add(py);
                vz.Add(pz);
            }
        }

        var faceIndices = ImmutableArray.CreateBuilder<int>();
        for (int iu = 0; iu < resolution; iu++)
        {
            for (int iv = 0; iv < resolution; iv++)
            {
                int i0 = iu * (resolution + 1) + iv;
                int i1 = i0 + 1;
                int i2 = i0 + (resolution + 1);
                int i3 = i2 + 1;

                faceIndices.Add(i0);
                faceIndices.Add(i2);
                faceIndices.Add(i1);

                faceIndices.Add(i1);
                faceIndices.Add(i2);
                faceIndices.Add(i3);
            }
        }

        result.Meshes.Add(new Mesh3DSeries
        {
            Name = "Bezier Surface",
            VertexX = vx.ToImmutable(),
            VertexY = vy.ToImmutable(),
            VertexZ = vz.ToImmutable(),
            FaceIndices = faceIndices.ToImmutable(),
            Color = "#9B59B6",
            Opacity = 0.7
        });

        for (int i = 0; i < vertCount; i++)
            UpdateBounds(result, vx[i], vy[i], vz[i]);
        return result;
    }

    /// <summary>Visualizes a NURBS curve.</summary>
    /// <param name="points">Control point positions [x, y, z].</param>
    /// <param name="weights">Weights for each control point.</param>
    /// <param name="knots">Knot vector.</param>
    /// <param name="degree">B-spline degree.</param>
    /// <param name="resolution">Number of evaluation segments (default 50).</param>
    /// <returns>A <see cref="Plot3DResult"/> containing the NURBS curve.</returns>
    public Plot3DResult VisualizeNURBS(double[][] points, double[] weights, double[] knots, int degree, int resolution = 50)
    {
        var result = new Plot3DResult();
        if (points.Length == 0 || weights.Length != points.Length || knots.Length < points.Length + degree + 1)
            return result;

        int dim = points[0].Length;
        double knotMin = knots[degree];
        double knotMax = knots[knots.Length - degree - 1];
        double knotSpan = knotMax - knotMin;
        if (knotSpan < 1e-15) knotSpan = 1.0;

        var curveX = ImmutableArray.CreateBuilder<double>(resolution + 1);
        var curveY = ImmutableArray.CreateBuilder<double>(resolution + 1);
        var curveZ = ImmutableArray.CreateBuilder<double>(resolution + 1);

        for (int i = 0; i <= resolution; i++)
        {
            double u = knotMin + knotSpan * i / resolution;
            double[] pt = EvaluateNURBS(points, weights, knots, degree, u);
            curveX.Add(pt[0]);
            curveY.Add(pt[1]);
            curveZ.Add(dim >= 3 ? pt[2] : 0.0);
        }

        result.Lines.Add(new Line3DSeries
        {
            Name = "NURBS Curve",
            X = curveX.ToImmutable(),
            Y = curveY.ToImmutable(),
            Z = curveZ.ToImmutable(),
            Color = "#1ABC9C"
        });

        var cpx = ImmutableArray.Create(points.Select(p => p[0]).ToArray());
        var cpy = ImmutableArray.Create(points.Select(p => p[1]).ToArray());
        var cpz = ImmutableArray.Create(points.Select(p => dim >= 3 ? p[2] : 0.0).ToArray());

        result.Lines.Add(new Line3DSeries
        {
            Name = "Control Polygon",
            X = cpx,
            Y = cpy,
            Z = cpz,
            Color = "#95A5A6",
            IsDashed = true
        });

        result.Points.Add(new Point3DSeries
        {
            Name = "Control Points",
            X = cpx,
            Y = cpy,
            Z = cpz,
            Color = "#E74C3C",
            PointSize = 6.0
        });

        foreach (var p in points)
            UpdateBounds(result, p[0], p[1], dim >= 3 ? p[2] : 0.0);
        return result;
    }

    #region Private Helpers

    private static void UpdateBounds(Plot3DResult result, double x, double y, double z)
    {
        if (x < result.BoundsMin[0]) result.BoundsMin[0] = x;
        if (y < result.BoundsMin[1]) result.BoundsMin[1] = y;
        if (z < result.BoundsMin[2]) result.BoundsMin[2] = z;
        if (x > result.BoundsMax[0]) result.BoundsMax[0] = x;
        if (y > result.BoundsMax[1]) result.BoundsMax[1] = y;
        if (z > result.BoundsMax[2]) result.BoundsMax[2] = z;
    }

    private static double[] DeCasteljau(double[][] points, double t)
    {
        int n = points.Length;
        int dim = points[0].Length;
        var temp = new double[n][];

        for (int i = 0; i < n; i++)
        {
            temp[i] = new double[dim];
            Array.Copy(points[i], temp[i], dim);
        }

        for (int k = 1; k < n; k++)
        {
            for (int i = 0; i < n - k; i++)
            {
                for (int d = 0; d < dim; d++)
                    temp[i][d] = (1.0 - t) * temp[i][d] + t * temp[i + 1][d];
            }
        }

        return temp[0];
    }

    private static double Bernstein(int n, int i, double t)
    {
        double coeff = 1.0;
        for (int j = 0; j < i; j++)
            coeff *= (double)(n - j) / (i - j) * t;
        for (int j = 0; j < n - i; j++)
            coeff *= (1.0 - t);
        return coeff;
    }

    private static double[] EvaluateNURBS(double[][] points, double[] weights, double[] knots, int degree, double u)
    {
        int n = points.Length;
        int dim = points[0].Length;
        int span = FindKnotSpan(knots, degree, u);

        var basis = new double[degree + 1];
        ComputeBasisFunctions(knots, degree, span, u, basis);

        var result = new double[dim];
        double wSum = 0.0;

        for (int i = 0; i <= degree; i++)
        {
            int idx = span - degree + i;
            if (idx < 0 || idx >= n) continue;
            double w = basis[i] * weights[idx];
            wSum += w;
            for (int d = 0; d < dim; d++)
                result[d] += w * points[idx][d];
        }

        if (wSum > 1e-15)
        {
            for (int d = 0; d < dim; d++)
                result[d] /= wSum;
        }

        return result;
    }

    private static int FindKnotSpan(double[] knots, int degree, double u)
    {
        int n = knots.Length - degree - 2;
        if (u >= knots[n + 1]) return n;
        if (u <= knots[degree]) return degree;

        int low = degree;
        int high = n + 1;
        int mid = (low + high) / 2;

        while (u < knots[mid] || u >= knots[mid + 1])
        {
            if (u < knots[mid])
                high = mid;
            else
                low = mid;
            mid = (low + high) / 2;
        }

        return mid;
    }

    private static void ComputeBasisFunctions(double[] knots, int degree, int span, double u, double[] basis)
    {
        var left = new double[degree + 1];
        var right = new double[degree + 1];

        basis[0] = 1.0;

        for (int j = 1; j <= degree; j++)
        {
            left[j] = u - knots[span + 1 - j];
            right[j] = knots[span + j] - u;
            double saved = 0.0;

            for (int r = 0; r < j; r++)
            {
                double temp = basis[r] / (right[r + 1] + left[j - r]);
                basis[r] = saved + right[r + 1] * temp;
                saved = left[j - r] * temp;
            }
            basis[j] = saved;
        }
    }

    private static List<double[]> ComputeConvexHull2D(double[][] points)
    {
        int n = points.Length;
        if (n < 3) return points.Select(p => new[] { p[0], p[1] }).ToList();

        var sorted = points
            .Select((p, i) => (Point: p, Index: i))
            .OrderBy(t => t.Point[0])
            .ThenBy(t => t.Point[1])
            .Select(t => t.Point)
            .ToArray();

        var hull = new List<double[]>();

        // Lower hull
        foreach (var p in sorted)
        {
            while (hull.Count >= 2 && Cross2D(hull[^2], hull[^1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(new[] { p[0], p[1] });
        }

        // Upper hull
        int lowerCount = hull.Count + 1;
        for (int i = sorted.Length - 2; i >= 0; i--)
        {
            while (hull.Count >= lowerCount && Cross2D(hull[^2], hull[^1], sorted[i]) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(new[] { sorted[i][0], sorted[i][1] });
        }

        hull.RemoveAt(hull.Count - 1);
        return hull;
    }

    private static List<double[]> ComputeConvexHull3D(double[][] points)
    {
        // Simple gift-wrapping: find the point with lowest X, then find adjacent hull points
        if (points.Length <= 3) return points.Select(p => new[] { p[0], p[1], p[2] }).ToList();

        // Project to 2D and compute 2D hull, then lift back for a simplified visualization
        var centroid = new double[] { 0, 0, 0 };
        foreach (var p in points)
        {
            centroid[0] += p[0];
            centroid[1] += p[1];
            centroid[2] += p[2];
        }
        centroid[0] /= points.Length;
        centroid[1] /= points.Length;
        centroid[2] /= points.Length;

        // Project onto XY plane for a quick hull (for visualization purposes)
        var projected = points.Select(p => new[] { p[0] - centroid[0], p[1] - centroid[1], p[2] - centroid[2] }).ToArray();
        var sorted = projected.OrderBy(p => System.Math.Atan2(p[1], p[0])).ToArray();

        // Convex hull on projected points
        var hull = new List<double[]>();
        foreach (var p in sorted)
        {
            while (hull.Count >= 2 && Cross2D(hull[^2], hull[^1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }
        int lowerCount = hull.Count + 1;
        for (int i = sorted.Length - 2; i >= 0; i--)
        {
            while (hull.Count >= lowerCount && Cross2D(hull[^2], hull[^1], sorted[i]) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(sorted[i]);
        }
        hull.RemoveAt(hull.Count - 1);

        // Lift back to 3D
        return hull.Select(p => new[] { p[0] + centroid[0], p[1] + centroid[1], p[2] + centroid[2] }).ToList();
    }

    private static double Cross2D(double[] o, double[] a, double[] b)
    {
        return (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0]);
    }

    private static List<double[][]> ComputeVoronoiCells(double[][] sites, double xMin, double xMax, double yMin, double yMax)
    {
        int n = sites.Length;
        var cells = new List<double[][]>();

        // Compute circumcenters for all Delaunay triangles, then assign to cells
        var triangles = ComputeDelaunayTriangulation(sites);

        // Build adjacency: for each site, collect the circumcenters of adjacent triangles
        var cellVertices = new List<List<double[]>>();
        for (int i = 0; i < n; i++)
            cellVertices.Add([]);

        var triangleCenters = new List<double[]>();
        foreach (var (a, b, c) in triangles)
        {
            double[] center = Circumcenter2D(sites[a], sites[b], sites[c]);
            triangleCenters.Add(center);

            if (center != null)
            {
                cellVertices[a].Add(center);
                cellVertices[b].Add(center);
                cellVertices[c].Add(center);
            }
        }

        // For each cell, sort vertices by angle around the site
        for (int i = 0; i < n; i++)
        {
            if (cellVertices[i].Count < 3)
            {
                // Clip to bounding box
                var clipped = ClipCellToBox(cellVertices[i].ToArray(), sites[i], xMin, xMax, yMin, yMax);
                cells.Add(clipped.Length >= 3 ? clipped : [sites[i]]);
                continue;
            }

            var center = sites[i];
            var sorted = cellVertices[i]
                .OrderBy(v => System.Math.Atan2(v[1] - center[1], v[0] - center[0]))
                .DistinctBy(v => $"{v[0]:F10},{v[1]:F10}")
                .ToArray();

            var clipped2 = ClipCellToBox(sorted, sites[i], xMin, xMax, yMin, yMax);
            cells.Add(clipped2.Length >= 3 ? clipped2 : [sites[i]]);
        }

        return cells;
    }

    private static double[] Circumcenter2D(double[] a, double[] b, double[] c)
    {
        double ax = a[0], ay = a[1];
        double bx = b[0], by = b[1];
        double cx = c[0], cy = c[1];

        double d = 2.0 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
        if (System.Math.Abs(d) < 1e-15) return null!;

        double ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
        double uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;

        return [ux, uy];
    }

    private static double[][] ClipCellToBox(double[][] vertices, double[] site, double xMin, double xMax, double yMin, double yMax)
    {
        if (vertices.Length == 0) return vertices;

        // Sutherland-Hodgman clipping against bounding box
        var output = vertices.ToList();
        double[][] edges = [[xMin, yMin], [xMax, yMin], [xMax, yMax], [xMin, yMax]];

        for (int e = 0; e < 4; e++)
        {
            if (output.Count == 0) break;
            var input = output.ToList();
            output.Clear();

            double ex1 = edges[e][0], ey1 = edges[e][1];
            double ex2 = edges[(e + 1) % 4][0], ey2 = edges[(e + 1) % 4][1];

            for (int i = 0; i < input.Count; i++)
            {
                var curr = input[i];
                var next = input[(i + 1) % input.Count];

                double currInside = (ex2 - ex1) * (curr[1] - ey1) - (ey2 - ey1) * (curr[0] - ex1);
                double nextInside = (ex2 - ex1) * (next[1] - ey1) - (ey2 - ey1) * (next[0] - ex1);

                if (currInside >= 0)
                    output.Add(curr);

                if ((currInside >= 0) != (nextInside >= 0))
                {
                    double t = currInside / (currInside - nextInside);
                    output.Add([
                        curr[0] + t * (next[0] - curr[0]),
                        curr[1] + t * (next[1] - curr[1])
                    ]);
                }
            }
        }

        return output.ToArray();
    }

    private static List<(int A, int B, int C)> ComputeDelaunayTriangulation(double[][] points)
    {
        int n = points.Length;
        var triangles = new List<(int A, int B, int C)>();
        if (n < 3) return triangles;

        // Bowyer-Watson incremental insertion with a super-triangle
        double minX = points[0][0], maxX = points[0][0];
        double minY = points[0][1], maxY = points[0][1];
        foreach (var p in points)
        {
            if (p[0] < minX) minX = p[0];
            if (p[0] > maxX) maxX = p[0];
            if (p[1] < minY) minY = p[1];
            if (p[1] > maxY) maxY = p[1];
        }

        double dx = maxX - minX;
        double dy = maxY - minY;
        double dmax = dx > dy ? dx : dy;
        double midx = (minX + maxX) * 0.5;
        double midy = (minY + maxY) * 0.5;

        double stX1 = midx - 20 * dmax;
        double stY1 = midy - dmax;
        double stX2 = midx;
        double stY2 = midy + 20 * dmax;
        double stX3 = midx + 20 * dmax;
        double stY3 = midy - dmax;

        // Super-triangle indices: n, n+1, n+2
        var allPoints = new double[n + 3][];
        for (int i = 0; i < n; i++) allPoints[i] = [points[i][0], points[i][1]];
        allPoints[n] = [stX1, stY1];
        allPoints[n + 1] = [stX2, stY2];
        allPoints[n + 2] = [stX3, stY3];

        triangles.Add((n, n + 1, n + 2));

        for (int i = 0; i < n; i++)
        {
            var p = allPoints[i];
            var badTriangles = new List<int>();

            for (int t = triangles.Count - 1; t >= 0; t--)
            {
                var tri = triangles[t];
                if (InCircumcircle(p, allPoints[tri.A], allPoints[tri.B], allPoints[tri.C]))
                    badTriangles.Add(t);
            }

            var polygon = new List<(int, int)>();

            foreach (int tIdx in badTriangles)
            {
                var tri = triangles[tIdx];
                int[] edges = [tri.A, tri.B, tri.B, tri.C, tri.C, tri.A];

                for (int e = 0; e < 6; e += 2)
                {
                    int ea = edges[e];
                    int eb = edges[e + 1];
                    bool shared = false;

                    foreach (int otherIdx in badTriangles)
                    {
                        if (otherIdx == tIdx) continue;
                        var other = triangles[otherIdx];
                        if ((ea == other.A || ea == other.B || ea == other.C) &&
                            (eb == other.A || eb == other.B || eb == other.C))
                        {
                            shared = true;
                            break;
                        }
                    }

                    if (!shared)
                        polygon.Add((ea, eb));
                }
            }

            // Remove bad triangles
            badTriangles.Sort((a, b) => b.CompareTo(a));
            foreach (int tIdx in badTriangles)
                triangles.RemoveAt(tIdx);

            foreach (var (ea, eb) in polygon)
                triangles.Add((i, ea, eb));
        }

        // Remove triangles that reference the super-triangle
        triangles.RemoveAll(t => t.A >= n || t.B >= n || t.C >= n);

        return triangles;
    }

    private static bool InCircumcircle(double[] p, double[] a, double[] b, double[] c)
    {
        double ax = a[0] - p[0], ay = a[1] - p[1];
        double bx = b[0] - p[0], by = b[1] - p[1];
        double cx = c[0] - p[0], cy = c[1] - p[1];

        double det = (ax * ax + ay * ay) * (bx * cy - cx * by)
                   - (bx * bx + by * by) * (ax * cy - cx * ay)
                   + (cx * cx + cy * cy) * (ax * by - bx * ay);

        return det > 1e-10;
    }

    private static void BuildKDTreeVis(double[][] points, int depth, int maxDepth,
        double xMin, double xMax, double yMin, double yMax, double zMin, double zMax,
        bool is3D, Plot3DResult result)
    {
        if (depth >= maxDepth || points.Length <= 1) return;

        int axis = depth % (is3D ? 3 : 2);
        var sorted = axis switch
        {
            0 => points.OrderBy(p => p[0]).ToArray(),
            1 => points.OrderBy(p => p[1]).ToArray(),
            _ => points.OrderBy(p => p[2]).ToArray()
        };

        int mid = sorted.Length / 2;

        double splitVal = sorted[mid][axis];

        // Draw splitting line
        if (axis == 0) // split by X -> vertical line
        {
            result.Lines.Add(new Line3DSeries
            {
                Name = $"KD Split {depth}",
                X = ImmutableArray.Create(splitVal, splitVal),
                Y = ImmutableArray.Create(yMin, yMax),
                Z = ImmutableArray.Create(is3D ? (zMin + zMax) * 0.5 : 0.0, is3D ? (zMin + zMax) * 0.5 : 0.0),
                Color = "#95A5A6",
                IsDashed = true
            });
        }
        else if (axis == 1) // split by Y -> horizontal line
        {
            result.Lines.Add(new Line3DSeries
            {
                Name = $"KD Split {depth}",
                X = ImmutableArray.Create(xMin, xMax),
                Y = ImmutableArray.Create(splitVal, splitVal),
                Z = ImmutableArray.Create(is3D ? (zMin + zMax) * 0.5 : 0.0, is3D ? (zMin + zMax) * 0.5 : 0.0),
                Color = "#95A5A6",
                IsDashed = true
            });
        }

        var left = sorted.Take(mid).ToArray();
        var right = sorted.Skip(mid).ToArray();

        if (axis == 0)
        {
            if (left.Length > 0) BuildKDTreeVis(left, depth + 1, maxDepth, xMin, splitVal, yMin, yMax, zMin, zMax, is3D, result);
            if (right.Length > 0) BuildKDTreeVis(right, depth + 1, maxDepth, splitVal, xMax, yMin, yMax, zMin, zMax, is3D, result);
        }
        else if (axis == 1)
        {
            if (left.Length > 0) BuildKDTreeVis(left, depth + 1, maxDepth, xMin, xMax, yMin, splitVal, zMin, zMax, is3D, result);
            if (right.Length > 0) BuildKDTreeVis(right, depth + 1, maxDepth, xMin, xMax, splitVal, yMax, zMin, zMax, is3D, result);
        }
    }

    #endregion
}
