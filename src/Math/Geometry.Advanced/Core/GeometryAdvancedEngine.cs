using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced;

/// <summary>
/// The main entry point for the advanced geometry processing engine.
/// Provides access to all geometry sub-engines through dedicated facade properties,
/// each delegating to the corresponding static implementation classes.
/// Construct with optional <see cref="GeometryAdvancedOptions"/> to customize behavior.
/// </summary>
public sealed class GeometryAdvancedEngine
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeometryAdvancedEngine"/> class
    /// with the specified options. If no options are provided, default settings are used.
    /// </summary>
    /// <param name="options">Optional configuration options. When null, <see cref="GeometryAdvancedOptions"/> defaults are used.</param>
    public GeometryAdvancedEngine(GeometryAdvancedOptions? options = null)
    {
        Options = options ?? new GeometryAdvancedOptions();
        ConvexHull = new ConvexHullEngine();
        Voronoi = new VoronoiEngine();
        Delaunay = new DelaunayEngine();
        Polygons = new PolygonEngine();
        Boolean = new BooleanEngine();
        SweepLine = new SweepLineEngine();
        Spatial = new SpatialEngine();
        MeshProcessing = new MeshProcessingEngine();
        Surfaces = new SurfaceEngine();
        Curves = new CurveEngine();
        Intersection = new IntersectionEngine();
        Distance = new DistanceEngine();
        Collision = new CollisionEngine();
        Optimization = new OptimizationEngine();
        Topology = new TopologyEngine();
        Serialization = new SerializationEngine();
    }

    /// <summary>Gets the configuration options for this engine instance.</summary>
    public GeometryAdvancedOptions Options { get; }

    /// <summary>Gets the convex hull sub-engine for computing 2D and 3D convex hulls.</summary>
    public ConvexHullEngine ConvexHull { get; }

    /// <summary>Gets the Voronoi diagram sub-engine.</summary>
    public VoronoiEngine Voronoi { get; }

    /// <summary>Gets the Delaunay triangulation sub-engine.</summary>
    public DelaunayEngine Delaunay { get; }

    /// <summary>Gets the polygon processing sub-engine for triangulation, decomposition, clipping, and offsetting.</summary>
    public PolygonEngine Polygons { get; }

    /// <summary>Gets the boolean geometry sub-engine for union, intersection, and difference operations.</summary>
    public BooleanEngine Boolean { get; }

    /// <summary>Gets the sweep line sub-engine for line arrangement processing.</summary>
    public SweepLineEngine SweepLine { get; }

    /// <summary>Gets the spatial indexing sub-engine for R-tree, KD-tree, quadtree, octree, and BSP operations.</summary>
    public SpatialEngine Spatial { get; }

    /// <summary>Gets the mesh processing sub-engine for vertex welding, normal computation, and mesh analysis.</summary>
    public MeshProcessingEngine MeshProcessing { get; }

    /// <summary>Gets the surface sub-engine for Bezier, bicubic, and parametric surface evaluation.</summary>
    public SurfaceEngine Surfaces { get; }

    /// <summary>Gets the curve sub-engine for Bezier, B-spline, and parametric curve operations.</summary>
    public CurveEngine Curves { get; }

    /// <summary>Gets the intersection sub-engine for computing geometric intersections.</summary>
    public IntersectionEngine Intersection { get; }

    /// <summary>Gets the distance sub-engine for computing geometric distances.</summary>
    public DistanceEngine Distance { get; }

    /// <summary>Gets the collision detection sub-engine.</summary>
    public CollisionEngine Collision { get; }

    /// <summary>Gets the geometry optimization sub-engine for vertex welding and simplification.</summary>
    public OptimizationEngine Optimization { get; }

    /// <summary>Gets the topology sub-engine for mesh topology analysis and validation.</summary>
    public TopologyEngine Topology { get; }

    /// <summary>Gets the serialization sub-engine for reading and writing geometry in various file formats.</summary>
    public SerializationEngine Serialization { get; }

    /// <summary>
    /// Provides access to 2D and 3D convex hull computation algorithms.
    /// </summary>
    public sealed class ConvexHullEngine
    {
        internal ConvexHullEngine() { }

        /// <summary>Computes the 2D convex hull using the Graham scan algorithm.</summary>
        /// <param name="points">The input 2D point set.</param>
        /// <returns>The convex hull vertices in counter-clockwise order.</returns>
        public ImmutableArray<Point2D> GrahamScan(ImmutableArray<Point2D> points) =>
            global::MathVerse.Math.Geometry.Advanced.ConvexHull.GrahamScan.Compute(points);

        /// <summary>Computes the 3D convex hull by projecting onto the best-fit plane.</summary>
        /// <param name="points">The input 3D point set.</param>
        /// <returns>The 3D convex hull vertices.</returns>
        public ImmutableArray<Point3D> GrahamScan3D(ImmutableArray<Point3D> points) =>
            global::MathVerse.Math.Geometry.Advanced.ConvexHull.GrahamScan.Compute3D(points);

        /// <summary>Computes the 2D convex hull using Andrew's monotone chain algorithm.</summary>
        /// <param name="points">The input 2D point set.</param>
        /// <returns>The convex hull vertices in counter-clockwise order.</returns>
        public ImmutableArray<Point2D> AndrewMonotoneChain(ImmutableArray<Point2D> points) =>
            global::MathVerse.Math.Geometry.Advanced.ConvexHull.AndrewMonotoneChain.Compute(points);

        /// <summary>Computes the 2D convex hull using the Jarvis march (gift wrapping) algorithm.</summary>
        /// <param name="points">The input 2D point set.</param>
        /// <returns>The convex hull vertices in counter-clockwise order.</returns>
        public ImmutableArray<Point2D> JarvisMarch(ImmutableArray<Point2D> points) =>
            global::MathVerse.Math.Geometry.Advanced.ConvexHull.JarvisMarch.Compute(points);

        /// <summary>Computes the 3D convex hull using plane-based gift wrapping.</summary>
        /// <param name="points">The input 3D point set.</param>
        /// <returns>The 3D convex hull vertices.</returns>
        public ImmutableArray<Point3D> JarvisMarch3D(ImmutableArray<Point3D> points) =>
            global::MathVerse.Math.Geometry.Advanced.ConvexHull.JarvisMarch.Compute3D(points);

        /// <summary>Computes the 2D convex hull using the QuickHull divide-and-conquer algorithm.</summary>
        /// <param name="points">The input 2D point set.</param>
        /// <returns>The convex hull vertices in counter-clockwise order.</returns>
        public ImmutableArray<Point2D> QuickHull(ImmutableArray<Point2D> points) =>
            global::MathVerse.Math.Geometry.Advanced.ConvexHull.QuickHull.Compute(points);

        /// <summary>Computes the 2D convex hull using Chan's O(n log h) algorithm.</summary>
        /// <param name="points">The input 2D point set.</param>
        /// <returns>The convex hull vertices in counter-clockwise order.</returns>
        public ImmutableArray<Point2D> Chan(ImmutableArray<Point2D> points) =>
            global::MathVerse.Math.Geometry.Advanced.ConvexHull.ChanAlgorithm.Compute(points);
    }

    /// <summary>
    /// Provides access to Voronoi diagram computation algorithms.
    /// </summary>
    public sealed class VoronoiEngine
    {
        internal VoronoiEngine() { }

        /// <summary>Computes the full Voronoi diagram using Fortune's sweep-line algorithm.</summary>
        /// <param name="sites">The input site points.</param>
        /// <returns>A tuple of sites, edges, and cells of the Voronoi diagram.</returns>
        public (ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Voronoi.VoronoiSite> Sites, ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Voronoi.VoronoiEdge> Edges, ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Voronoi.VoronoiCell> Cells) Compute(ImmutableArray<Point2D> sites) =>
            global::MathVerse.Math.Geometry.Advanced.Voronoi.FortuneAlgorithm.Compute(sites);
    }

    /// <summary>
    /// Provides access to Delaunay triangulation algorithms.
    /// </summary>
    public sealed class DelaunayEngine
    {
        internal DelaunayEngine() { }

        /// <summary>Computes the Delaunay triangulation using the Bowyer-Watson algorithm.</summary>
        /// <param name="points">The input 2D point set.</param>
        /// <returns>A tuple of vertices and Delaunay triangles.</returns>
        public (ImmutableArray<Point2D> Vertices, ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Delaunay.DelaunayTriangle> Triangulation) Triangulate(ImmutableArray<Point2D> points) =>
            global::MathVerse.Math.Geometry.Advanced.Delaunay.BowyerWatson.Triangulate(points);

        /// <summary>Extracts unique edges from a set of Delaunay triangles.</summary>
        /// <param name="triangles">The Delaunay triangles.</param>
        /// <returns>An immutable array of unique edges.</returns>
        public ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Delaunay.DelaunayEdge> ComputeEdges(ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Delaunay.DelaunayTriangle> triangles) =>
            global::MathVerse.Math.Geometry.Advanced.Delaunay.NeighborGraph.ComputeEdges(triangles);

        /// <summary>Builds a vertex-to-neighbors adjacency map from Delaunay triangles.</summary>
        /// <param name="triangles">The Delaunay triangles.</param>
        /// <returns>A dictionary mapping vertex indices to neighbor arrays.</returns>
        public Dictionary<int, ImmutableArray<int>> ComputeAdjacency(ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Delaunay.DelaunayTriangle> triangles) =>
            global::MathVerse.Math.Geometry.Advanced.Delaunay.NeighborGraph.ComputeAdjacency(triangles);
    }

    /// <summary>
    /// Provides access to polygon processing operations including triangulation,
    /// convex decomposition, clipping, and offsetting.
    /// </summary>
    public sealed class PolygonEngine
    {
        internal PolygonEngine() { }

        /// <summary>Triangulates a simple polygon using ear clipping.</summary>
        /// <param name="polygon">The polygon vertices in winding order.</param>
        /// <returns>Triangle indices in groups of three.</returns>
        public ImmutableArray<int> TriangulateEarClipping(ImmutableArray<Point2D> polygon) =>
            global::MathVerse.Math.Geometry.Advanced.PolygonAlgorithms.EarClipping.Triangulate(polygon);

        /// <summary>Partitions a simple polygon into Y-monotone sub-polygons.</summary>
        /// <param name="polygon">The polygon vertices in winding order.</param>
        /// <returns>An array of monotone sub-polygons.</returns>
        public ImmutableArray<ImmutableArray<Point2D>> PartitionToMonotone(ImmutableArray<Point2D> polygon) =>
            global::MathVerse.Math.Geometry.Advanced.PolygonAlgorithms.MonotonePartitioner.Partition(polygon);

        /// <summary>Triangulates a Y-monotone polygon in linear time.</summary>
        /// <param name="polygon">A Y-monotone polygon.</param>
        /// <returns>Triangle indices in groups of three.</returns>
        public ImmutableArray<int> TriangulateMonotone(ImmutableArray<Point2D> polygon) =>
            global::MathVerse.Math.Geometry.Advanced.PolygonAlgorithms.MonotonePartitioner.TriangulateMonotone(polygon);

        /// <summary>Decomposes a simple polygon into convex sub-polygons using the Hertel-Mehlhorn algorithm.</summary>
        /// <param name="polygon">The polygon vertices in winding order.</param>
        /// <returns>An array of convex sub-polygons.</returns>
        public ImmutableArray<ImmutableArray<Point2D>> ConvexDecompose(ImmutableArray<Point2D> polygon) =>
            global::MathVerse.Math.Geometry.Advanced.PolygonAlgorithms.ConvexDecomposer.Decompose(polygon);

        /// <summary>Clips a subject polygon against a convex clip polygon using Sutherland-Hodgman.</summary>
        /// <param name="subject">The subject polygon.</param>
        /// <param name="clip">The convex clip polygon.</param>
        /// <returns>The clipped polygon.</returns>
        public ImmutableArray<Point2D> ClipSutherlandHodgman(ImmutableArray<Point2D> subject, ImmutableArray<Point2D> clip) =>
            global::MathVerse.Math.Geometry.Advanced.PolygonAlgorithms.PolygonClipper.SutherlandHodgman(subject, clip);

        /// <summary>Clips two polygons using the Weiler-Atherton algorithm.</summary>
        /// <param name="subject">The subject polygon.</param>
        /// <param name="clip">The clipping polygon.</param>
        /// <returns>The clipped region.</returns>
        public ImmutableArray<Point2D> ClipWeilerAtherton(ImmutableArray<Point2D> subject, ImmutableArray<Point2D> clip) =>
            global::MathVerse.Math.Geometry.Advanced.PolygonAlgorithms.PolygonClipper.WeilerAtherton(subject, clip);

        /// <summary>Determines whether a point is inside a polygon using ray casting.</summary>
        /// <param name="point">The test point.</param>
        /// <param name="polygon">The polygon to test against.</param>
        /// <returns>True if the point is inside the polygon.</returns>
        public bool IsPointInPolygon(Point2D point, ImmutableArray<Point2D> polygon) =>
            global::MathVerse.Math.Geometry.Advanced.PolygonAlgorithms.PolygonClipper.IsPointInPolygon(point, polygon);

        /// <summary>Offsets a polygon by the specified distance using Minkowski sum computation.</summary>
        /// <param name="polygon">The polygon vertices.</param>
        /// <param name="distance">The offset distance (positive for outward, negative for inward).</param>
        /// <returns>The offset polygon vertices.</returns>
        public ImmutableArray<Point2D> Offset(ImmutableArray<Point2D> polygon, double distance) =>
            global::MathVerse.Math.Geometry.Advanced.PolygonAlgorithms.PolygonOffsetter.Offset(polygon, distance);
    }

    /// <summary>
    /// Provides access to boolean geometry operations (union, intersection, difference).
    /// </summary>
    public sealed class BooleanEngine
    {
        internal BooleanEngine() { }

        /// <summary>Computes the union of two 2D polygons.</summary>
        /// <param name="a">The first polygon.</param>
        /// <param name="b">The second polygon.</param>
        /// <returns>The union polygon vertices.</returns>
        public ImmutableArray<Point2D> Union(ImmutableArray<Point2D> a, ImmutableArray<Point2D> b) =>
            global::MathVerse.Math.Geometry.Advanced.PolygonAlgorithms.PolygonClipper.WeilerAtherton(a, b);

        /// <summary>Computes the intersection of two 2D polygons using Sutherland-Hodgman clipping.</summary>
        /// <param name="a">The first polygon.</param>
        /// <param name="b">The second polygon (must be convex).</param>
        /// <returns>The intersection polygon vertices.</returns>
        public ImmutableArray<Point2D> Intersection(ImmutableArray<Point2D> a, ImmutableArray<Point2D> b) =>
            global::MathVerse.Math.Geometry.Advanced.PolygonAlgorithms.PolygonClipper.SutherlandHodgman(a, b);
    }

    /// <summary>
    /// Provides access to sweep line algorithms for line arrangement processing.
    /// </summary>
    public sealed class SweepLineEngine
    {
        internal SweepLineEngine() { }

        /// <summary>Computes the Voronoi diagram using Fortune's sweep-line algorithm.</summary>
        /// <param name="sites">The input site points.</param>
        /// <returns>A tuple of sites, edges, and cells.</returns>
        public (ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Voronoi.VoronoiSite> Sites, ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Voronoi.VoronoiEdge> Edges, ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Voronoi.VoronoiCell> Cells) ComputeVoronoi(ImmutableArray<Point2D> sites) =>
            global::MathVerse.Math.Geometry.Advanced.Voronoi.FortuneAlgorithm.Compute(sites);
    }

    /// <summary>
    /// Provides access to spatial indexing structures including R-tree, KD-tree, quadtree, octree, and BSP tree.
    /// </summary>
    public sealed class SpatialEngine
    {
        internal SpatialEngine() { }

        /// <summary>Creates a new 2D R-tree spatial index with the specified maximum entries per node.</summary>
        /// <param name="maxEntries">The maximum entries per node before splitting.</param>
        /// <returns>A new R-tree instance.</returns>
        public global::MathVerse.Math.Geometry.Advanced.Spatial.RTree2D CreateRTree(int maxEntries = 16) =>
            new global::MathVerse.Math.Geometry.Advanced.Spatial.RTree2D(maxEntries);

        /// <summary>Builds a balanced 2D KD-tree from the given points.</summary>
        /// <param name="points">The points to index.</param>
        /// <returns>A new KD-tree instance.</returns>
        public global::MathVerse.Math.Geometry.Advanced.Spatial.KDTree2D CreateKDTree(ImmutableArray<Point2D> points) =>
            new global::MathVerse.Math.Geometry.Advanced.Spatial.KDTree2D(points);

        /// <summary>Creates a new 2D quadtree with the specified bounds and capacity.</summary>
        /// <param name="bounds">The spatial bounds of the root node.</param>
        /// <param name="maxCapacity">Max entries per node before subdivision.</param>
        /// <returns>A new quadtree instance.</returns>
        public global::MathVerse.Math.Geometry.Advanced.Spatial.QuadTree2D CreateQuadTree(BoundingBox2D bounds, int maxCapacity = 4) =>
            new global::MathVerse.Math.Geometry.Advanced.Spatial.QuadTree2D(bounds, maxCapacity);

        /// <summary>Creates a new 3D octree with the specified bounds and capacity.</summary>
        /// <param name="bounds">The spatial bounds of the root node.</param>
        /// <param name="maxCapacity">Max entries per node before subdivision.</param>
        /// <returns>A new octree instance.</returns>
        public global::MathVerse.Math.Geometry.Advanced.Spatial.Octree3D CreateOctree(BoundingBox3D bounds, int maxCapacity = 8) =>
            new global::MathVerse.Math.Geometry.Advanced.Spatial.Octree3D(bounds, maxCapacity);

        /// <summary>Builds a balanced 2D BSP tree from the given points.</summary>
        /// <param name="points">The points to partition.</param>
        /// <returns>The root BSP node.</returns>
        public global::MathVerse.Math.Geometry.Advanced.Spatial.BSPNode2D BuildBSPTree(ImmutableArray<Point2D> points) =>
            global::MathVerse.Math.Geometry.Advanced.Spatial.BSPTree2D.Build(points);
    }

    /// <summary>
    /// Provides access to mesh processing operations including vertex welding and analysis.
    /// </summary>
    public sealed class MeshProcessingEngine
    {
        internal MeshProcessingEngine() { }

        /// <summary>Welds 3D vertices within the specified tolerance using spatial hash grid.</summary>
        /// <param name="vertices">The input vertex positions.</param>
        /// <param name="indices">The triangle index buffer.</param>
        /// <param name="tolerance">Maximum distance between vertices to merge.</param>
        /// <returns>A tuple of welded vertices, updated indices, and removed count.</returns>
        public (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices, int RemovedCount) WeldVertices(
            ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, double tolerance) =>
            global::MathVerse.Math.Geometry.Advanced.Optimization.VertexWelder.Weld(vertices, indices, tolerance);

        /// <summary>Welds 2D vertices within the specified tolerance using spatial hash grid.</summary>
        /// <param name="vertices">The input 2D vertex positions.</param>
        /// <param name="indices">The triangle index buffer.</param>
        /// <param name="tolerance">Maximum distance between vertices to merge.</param>
        /// <returns>A tuple of welded vertices, updated indices, and removed count.</returns>
        public (ImmutableArray<Point2D> Vertices, ImmutableArray<int> Indices, int RemovedCount) WeldVertices2D(
            ImmutableArray<Point2D> vertices, ImmutableArray<int> indices, double tolerance) =>
            global::MathVerse.Math.Geometry.Advanced.Optimization.VertexWelder.Weld2D(vertices, indices, tolerance);
    }

    /// <summary>
    /// Provides access to surface evaluation and tessellation for Bezier and bicubic surfaces.
    /// </summary>
    public sealed class SurfaceEngine
    {
        internal SurfaceEngine() { }

        /// <summary>Evaluates a Bezier surface at the specified parametric coordinates.</summary>
        /// <param name="controlPoints">The control point grid defining the surface.</param>
        /// <param name="u">The u parametric coordinate.</param>
        /// <param name="v">The v parametric coordinate.</param>
        /// <returns>The 3D point on the surface.</returns>
        public Point3D EvaluateBezier(ImmutableArray<ImmutableArray<Point3D>> controlPoints, double u, double v)
        {
            var surface = new global::MathVerse.Math.Geometry.Advanced.Surfaces.BezierSurfaceAdvanced(controlPoints);
            return surface.Evaluate(u, v);
        }

        /// <summary>Generates a tessellated mesh from a Bezier surface.</summary>
        /// <param name="controlPoints">The control point grid.</param>
        /// <param name="uSegments">Number of subdivisions in the u-direction.</param>
        /// <param name="vSegments">Number of subdivisions in the v-direction.</param>
        /// <returns>An array of surface points with positions and normals.</returns>
        public ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Surfaces.SurfacePoint> TessellateBezier(
            ImmutableArray<ImmutableArray<Point3D>> controlPoints, int uSegments, int vSegments)
        {
            var surface = new global::MathVerse.Math.Geometry.Advanced.Surfaces.BezierSurfaceAdvanced(controlPoints);
            return surface.Tessellate(uSegments, vSegments);
        }

        /// <summary>Evaluates a bicubic Hermite surface at the specified parametric coordinates.</summary>
        /// <param name="points">A 4x4 grid of position values.</param>
        /// <param name="tangentsU">A 4x4 grid of u-direction tangent vectors.</param>
        /// <param name="tangentsV">A 4x4 grid of v-direction tangent vectors.</param>
        /// <param name="crossTangents">A 4x4 grid of cross-tangent (twist) vectors.</param>
        /// <param name="u">The u parametric coordinate.</param>
        /// <param name="v">The v parametric coordinate.</param>
        /// <returns>The 3D point on the surface.</returns>
        public Point3D EvaluateBicubic(
            ImmutableArray<ImmutableArray<Point3D>> points,
            ImmutableArray<ImmutableArray<Vector3D>> tangentsU,
            ImmutableArray<ImmutableArray<Vector3D>> tangentsV,
            ImmutableArray<ImmutableArray<Vector3D>> crossTangents,
            double u, double v)
        {
            var surface = new global::MathVerse.Math.Geometry.Advanced.Surfaces.BicubicSurface(points, tangentsU, tangentsV, crossTangents);
            return surface.Evaluate(u, v);
        }

        /// <summary>Generates a tessellated mesh from a bicubic Hermite surface.</summary>
        /// <param name="points">A 4x4 grid of position values.</param>
        /// <param name="tangentsU">A 4x4 grid of u-direction tangent vectors.</param>
        /// <param name="tangentsV">A 4x4 grid of v-direction tangent vectors.</param>
        /// <param name="crossTangents">A 4x4 grid of cross-tangent (twist) vectors.</param>
        /// <param name="uSegments">Number of subdivisions in the u-direction.</param>
        /// <param name="vSegments">Number of subdivisions in the v-direction.</param>
        /// <returns>An array of surface points with positions and normals.</returns>
        public ImmutableArray<global::MathVerse.Math.Geometry.Advanced.Surfaces.SurfacePoint> TessellateBicubic(
            ImmutableArray<ImmutableArray<Point3D>> points,
            ImmutableArray<ImmutableArray<Vector3D>> tangentsU,
            ImmutableArray<ImmutableArray<Vector3D>> tangentsV,
            ImmutableArray<ImmutableArray<Vector3D>> crossTangents,
            int uSegments, int vSegments)
        {
            var surface = new global::MathVerse.Math.Geometry.Advanced.Surfaces.BicubicSurface(points, tangentsU, tangentsV, crossTangents);
            return surface.Tessellate(uSegments, vSegments);
        }
    }

    /// <summary>
    /// Provides access to parametric curve evaluation and tessellation.
    /// </summary>
    public sealed class CurveEngine
    {
        internal CurveEngine() { }
    }

    /// <summary>
    /// Provides access to geometric intersection computation.
    /// </summary>
    public sealed class IntersectionEngine
    {
        internal IntersectionEngine() { }
    }

    /// <summary>
    /// Provides access to geometric distance computation.
    /// </summary>
    public sealed class DistanceEngine
    {
        internal DistanceEngine() { }
    }

    /// <summary>
    /// Provides access to collision detection operations.
    /// </summary>
    public sealed class CollisionEngine
    {
        internal CollisionEngine() { }
    }

    /// <summary>
    /// Provides access to geometry optimization operations including vertex welding.
    /// </summary>
    public sealed class OptimizationEngine
    {
        internal OptimizationEngine() { }

        /// <summary>Welds 3D vertices within the specified tolerance using spatial hash grid.</summary>
        /// <param name="vertices">The input vertex positions.</param>
        /// <param name="indices">The triangle index buffer.</param>
        /// <param name="tolerance">Maximum distance between vertices to merge.</param>
        /// <returns>A tuple of welded vertices, updated indices, and removed count.</returns>
        public (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices, int RemovedCount) Weld(
            ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, double tolerance) =>
            global::MathVerse.Math.Geometry.Advanced.Optimization.VertexWelder.Weld(vertices, indices, tolerance);

        /// <summary>Welds 2D vertices within the specified tolerance using spatial hash grid.</summary>
        /// <param name="vertices">The input 2D vertex positions.</param>
        /// <param name="indices">The triangle index buffer.</param>
        /// <param name="tolerance">Maximum distance between vertices to merge.</param>
        /// <returns>A tuple of welded vertices, updated indices, and removed count.</returns>
        public (ImmutableArray<Point2D> Vertices, ImmutableArray<int> Indices, int RemovedCount) Weld2D(
            ImmutableArray<Point2D> vertices, ImmutableArray<int> indices, double tolerance) =>
            global::MathVerse.Math.Geometry.Advanced.Optimization.VertexWelder.Weld2D(vertices, indices, tolerance);
    }

    /// <summary>
    /// Provides access to mesh topology analysis and validation.
    /// </summary>
    public sealed class TopologyEngine
    {
        internal TopologyEngine() { }
    }

    /// <summary>
    /// Provides access to geometry serialization and deserialization across multiple file formats.
    /// </summary>
    public sealed class SerializationEngine
    {
        internal SerializationEngine() { }

        /// <summary>Serializes 3D geometry to the specified format.</summary>
        /// <param name="vertices">The vertex positions.</param>
        /// <param name="indices">The triangle index buffer.</param>
        /// <param name="format">The target file format.</param>
        /// <returns>The serialized geometry string.</returns>
        public string Serialize(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, global::MathVerse.Math.Geometry.Advanced.Serialization.GeometryFormatType format) =>
            global::MathVerse.Math.Geometry.Advanced.Serialization.SerializationRegistry.Serialize(vertices, indices, format);

        /// <summary>Deserializes 3D geometry from the specified format.</summary>
        /// <param name="content">The serialized geometry data.</param>
        /// <param name="format">The source file format.</param>
        /// <returns>A tuple of vertices and triangle indices.</returns>
        public (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) Deserialize(string content, global::MathVerse.Math.Geometry.Advanced.Serialization.GeometryFormatType format) =>
            global::MathVerse.Math.Geometry.Advanced.Serialization.SerializationRegistry.Deserialize(content, format);

        /// <summary>Serializes 3D geometry to the Wavefront OBJ format.</summary>
        /// <param name="vertices">The vertex positions.</param>
        /// <param name="indices">The triangle index buffer.</param>
        /// <returns>The OBJ-formatted string.</returns>
        public string SerializeOBJ(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices) =>
            global::MathVerse.Math.Geometry.Advanced.Serialization.OBJSerializer.Serialize(vertices, indices);

        /// <summary>Serializes 3D geometry to the ASCII STL format.</summary>
        /// <param name="vertices">The vertex positions.</param>
        /// <param name="indices">The triangle index buffer.</param>
        /// <returns>The ASCII STL-formatted string.</returns>
        public string SerializeSTL(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices) =>
            global::MathVerse.Math.Geometry.Advanced.Serialization.STLSerializer.Serialize(vertices, indices);

        /// <summary>Serializes 3D geometry to the OFF format.</summary>
        /// <param name="vertices">The vertex positions.</param>
        /// <param name="indices">The triangle index buffer.</param>
        /// <returns>The OFF-formatted string.</returns>
        public string SerializeOFF(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices) =>
            global::MathVerse.Math.Geometry.Advanced.Serialization.OFFSerializer.Serialize(vertices, indices);

        /// <summary>Serializes 3D geometry to the PLY ASCII format.</summary>
        /// <param name="vertices">The vertex positions.</param>
        /// <param name="indices">The triangle index buffer.</param>
        /// <returns>The PLY-formatted string.</returns>
        public string SerializePLY(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices) =>
            global::MathVerse.Math.Geometry.Advanced.Serialization.PLYSerializer.Serialize(vertices, indices);

        /// <summary>Serializes a 2D polygon to an SVG document.</summary>
        /// <param name="polygon">The polygon vertices.</param>
        /// <param name="width">The SVG viewport width.</param>
        /// <param name="height">The SVG viewport height.</param>
        /// <returns>The SVG document string.</returns>
        public string SerializeSVG(ImmutableArray<Point2D> polygon, double width, double height) =>
            global::MathVerse.Math.Geometry.Advanced.Serialization.SVGSerializer.SerializePolygon2D(polygon, width, height);

        /// <summary>Serializes a 2D point to the WKT POINT format.</summary>
        /// <param name="point">The 2D point.</param>
        /// <returns>The WKT string.</returns>
        public string SerializeWKTPoint(Point2D point) =>
            global::MathVerse.Math.Geometry.Advanced.Serialization.WKTSerializer.SerializePoint2D(point);

        /// <summary>Serializes a 2D polygon to the WKT POLYGON format.</summary>
        /// <param name="polygon">The polygon vertices.</param>
        /// <returns>The WKT string.</returns>
        public string SerializeWKTPolygon(ImmutableArray<Point2D> polygon) =>
            global::MathVerse.Math.Geometry.Advanced.Serialization.WKTSerializer.SerializePolygon2D(polygon);

        /// <summary>Serializes a 2D point to a GeoJSON Feature.</summary>
        /// <param name="point">The 2D point.</param>
        /// <returns>The GeoJSON Feature string.</returns>
        public string SerializeGeoJSONPoint(Point2D point) =>
            global::MathVerse.Math.Geometry.Advanced.Serialization.GeoJSONSerializer.SerializePoint(point);

        /// <summary>Serializes a 2D polygon to a GeoJSON Feature.</summary>
        /// <param name="polygon">The polygon vertices.</param>
        /// <returns>The GeoJSON Feature string.</returns>
        public string SerializeGeoJSONPolygon(ImmutableArray<Point2D> polygon) =>
            global::MathVerse.Math.Geometry.Advanced.Serialization.GeoJSONSerializer.SerializePolygon(polygon);
    }
}
