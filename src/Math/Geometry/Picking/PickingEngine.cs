namespace MathVerse.Math.Geometry.Picking;

using Geometry3D;
using SceneGraph;
using Meshes;

/// <summary>Provides picking and hit-testing functionality.</summary>
public sealed class PickingEngine
{
    /// <summary>Tests a ray against a triangle mesh.</summary>
    public HitTestResult PickMesh(Ray ray, TriangleMesh mesh, SceneNode? node = null)
    {
        var best = HitTestResult.Miss();
        var faces = mesh.GetTriangles();
        var vertices = mesh.GetVertices();
        
        for (int i = 0; i < faces.Count; i++)
        {
            var face = faces[i];
            var a = vertices[face.V0].Position;
            var b = vertices[face.V1].Position;
            var c = vertices[face.V2].Position;
            
            var tri = new Triangle3D(a, b, c);
            var line = new Line3D(ray.Origin, ray.PointAt(1000.0));
            var (hit, point) = Geometry3DOperations.Intersect(tri, line);
            
            if (hit)
            {
                double dist = ray.Origin.DistanceTo(point);
                if (dist < best.Distance)
                {
                    best = new HitTestResult
                    {
                        Hit = true,
                        Distance = dist,
                        HitPoint = point,
                        TriangleIndex = i,
                        Node = node
                    };
                }
            }
        }
        
        return best;
    }
    
    /// <summary>Tests a ray against a bounding box.</summary>
    public HitTestResult PickBoundingBox(Ray ray, BoundingBox3D box, SceneNode? node = null)
    {
        double tmin = double.MinValue, tmax = double.MaxValue;
        
        double invDx = 1.0 / (System.Math.Abs(ray.Direction.X) < 1e-15 ? 1e-15 : ray.Direction.X);
        double invDy = 1.0 / (System.Math.Abs(ray.Direction.Y) < 1e-15 ? 1e-15 : ray.Direction.Y);
        double invDz = 1.0 / (System.Math.Abs(ray.Direction.Z) < 1e-15 ? 1e-15 : ray.Direction.Z);
        
        double t1 = (box.Min.X - ray.Origin.X) * invDx;
        double t2 = (box.Max.X - ray.Origin.X) * invDx;
        if (t1 > t2) (t1, t2) = (t2, t1);
        tmin = System.Math.Max(tmin, t1);
        tmax = System.Math.Min(tmax, t2);
        
        t1 = (box.Min.Y - ray.Origin.Y) * invDy;
        t2 = (box.Max.Y - ray.Origin.Y) * invDy;
        if (t1 > t2) (t1, t2) = (t2, t1);
        tmin = System.Math.Max(tmin, t1);
        tmax = System.Math.Min(tmax, t2);
        
        t1 = (box.Min.Z - ray.Origin.Z) * invDz;
        t2 = (box.Max.Z - ray.Origin.Z) * invDz;
        if (t1 > t2) (t1, t2) = (t2, t1);
        tmin = System.Math.Max(tmin, t1);
        tmax = System.Math.Min(tmax, t2);
        
        if (tmax >= tmin && tmax >= 0)
        {
            double t = tmin >= 0 ? tmin : tmax;
            return new HitTestResult { Hit = true, Distance = t, HitPoint = ray.PointAt(t), Node = node };
        }
        
        return HitTestResult.Miss();
    }
    
    /// <summary>Tests a ray against a scene, returning the closest hit.</summary>
    public HitTestResult PickScene(Ray ray, Scene scene)
    {
        var best = HitTestResult.Miss();
        foreach (var geo in scene.GetGeometryNodes())
        {
            if (geo.Mesh == null) continue;
            var result = PickMesh(ray, geo.Mesh, geo);
            if (result.Hit && result.Distance < best.Distance)
                best = result;
        }
        return best;
    }
}
