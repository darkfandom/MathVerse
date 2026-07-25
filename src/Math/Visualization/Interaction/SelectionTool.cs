namespace MathVerse.Math.Visualization.Interaction;
using System.Numerics;
using System.Collections.Generic;
using MathVerse.Math.Visualization._2DPlotting;

/// <summary>Provides object selection interaction functionality.</summary>
public sealed class SelectionTool
{
    /// <summary>Selects the closest object intersected by a ray.</summary>
    /// <param name="objects">The objects to test.</param>
    /// <param name="ray">The selection ray.</param>
    /// <returns>The closest hit object, or null if none.</returns>
    public static Core.VisualizationObject? Select(List<Core.VisualizationObject> objects, Ray ray)
    {
        if (objects == null || objects.Count == 0)
            return null;

        Core.VisualizationObject? closest = null;
        float closestDistance = float.MaxValue;

        var hitTester = new HitTester();

        foreach (var obj in objects)
        {
            if (obj == null)
                continue;

            var bounds = ComputeObjectBounds(obj);
            if (bounds == null)
                continue;

            var result = hitTester.RayAABB(ray, bounds!);

            if (result.Hit && result.Distance < closestDistance)
            {
                closestDistance = result.Distance;
                closest = obj;
            }
        }

        return closest;
    }

    /// <summary>Selects all objects within a screen-space rectangle.</summary>
    /// <param name="objects">The objects to test.</param>
    /// <param name="screenBounds">The selection rectangle in screen coordinates.</param>
    /// <param name="viewProjection">The combined view-projection matrix.</param>
    /// <returns>Objects within the selection rectangle.</returns>
    public static List<Core.VisualizationObject> SelectBounds(
        List<Core.VisualizationObject> objects, BoundingBox2D screenBounds, Matrix4x4 viewProjection)
    {
        var selected = new List<Core.VisualizationObject>();

        if (objects == null || objects.Count == 0)
            return selected;

        Matrix4x4 inverseVP = Matrix4x4.Identity;
        bool invertible = Matrix4x4.Invert(viewProjection, out inverseVP);
        if (!invertible)
            return selected;

        Vector3 nearMin = UnProject(new Vector3((float)screenBounds.MinX, (float)screenBounds.MinY, 0), inverseVP);
        Vector3 nearMax = UnProject(new Vector3((float)screenBounds.MaxX, (float)screenBounds.MaxY, 0), inverseVP);
        Vector3 farMin = UnProject(new Vector3((float)screenBounds.MinX, (float)screenBounds.MinY, 1), inverseVP);
        Vector3 farMax = UnProject(new Vector3((float)screenBounds.MaxX, (float)screenBounds.MaxY, 1), inverseVP);

        foreach (var obj in objects)
        {
            if (obj == null)
                continue;

            Vector3? objPos = GetObjectCenter(obj);
            if (!objPos.HasValue)
                continue;

            Vector4 clipPos = Vector4.Transform(new Vector4(objPos.Value, 1.0f), viewProjection);
            if (clipPos.W <= 0)
                continue;

            Vector3 ndcPos = new Vector3(clipPos.X / clipPos.W, clipPos.Y / clipPos.W, clipPos.Z / clipPos.W);
            Vector2 screenPos = new Vector2(
                (ndcPos.X + 1.0f) * 0.5f * (float)(screenBounds.MaxX - screenBounds.MinX) + (float)screenBounds.MinX,
                (1.0f - ndcPos.Y) * 0.5f * (float)(screenBounds.MaxY - screenBounds.MinY) + (float)screenBounds.MinY
            );

            if (screenPos.X >= (float)screenBounds.MinX && screenPos.X <= (float)screenBounds.MaxX &&
                screenPos.Y >= (float)screenBounds.MinY && screenPos.Y <= (float)screenBounds.MaxY)
            {
                selected.Add(obj);
            }
        }

        return selected;
    }

    /// <summary>Computes the bounding box for a visualization object.</summary>
    /// <param name="obj">The object.</param>
    /// <returns>The axis-aligned bounding box, or null if not computable.</returns>
    public static BoundingBox? ComputeObjectBounds(Core.VisualizationObject obj)
    {
        if (obj == null)
            return null;

        float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
        bool hasPoints = false;

        if (obj is Core.LinePlot linePlot && linePlot.Points != null)
        {
            foreach (var pt in linePlot.Points)
            {
                UpdateBounds(ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ, ref hasPoints, new Vector3(pt.X, pt.Y, 0));
            }
        }
        else if (obj is Core.PointCloud pointCloud && pointCloud.Points != null)
        {
            foreach (var pt in pointCloud.Points)
            {
                UpdateBounds(ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ, ref hasPoints, pt);
            }
        }
        else if (obj is Core.MeshObject meshObj && meshObj.Vertices != null)
        {
            foreach (var v in meshObj.Vertices)
            {
                UpdateBounds(ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ, ref hasPoints, v);
            }
        }
        else if (obj.Position.HasValue)
        {
            var pos = obj.Position.Value;
            UpdateBounds(ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ, ref hasPoints, pos);
        }

        if (!hasPoints)
            return null;

        float padding = 0.01f;
        return new BoundingBox
        {
            Min = new Vector3(minX - padding, minY - padding, minZ - padding),
            Max = new Vector3(maxX + padding, maxY + padding, maxZ + padding)
        };
    }

    private static void UpdateBounds(ref float minX, ref float minY, ref float minZ,
        ref float maxX, ref float maxY, ref float maxZ, ref bool hasPoints, Vector3 pt)
    {
        hasPoints = true;
        minX = System.Math.Min(minX, pt.X);
        minY = System.Math.Min(minY, pt.Y);
        minZ = System.Math.Min(minZ, pt.Z);
        maxX = System.Math.Max(maxX, pt.X);
        maxY = System.Math.Max(maxY, pt.Y);
        maxZ = System.Math.Max(maxZ, pt.Z);
    }

    private static Vector3? GetObjectCenter(Core.VisualizationObject obj)
    {
        if (obj.Position.HasValue)
            return obj.Position.Value;

        if (obj is Core.LinePlot linePlot && linePlot.Points != null && linePlot.Points.Count > 0)
        {
            Vector3 sum = Vector3.Zero;
            foreach (var pt in linePlot.Points)
                sum += new Vector3(pt.X, pt.Y, 0);
            return sum / linePlot.Points.Count;
        }

        if (obj is Core.PointCloud pointCloud && pointCloud.Points != null && pointCloud.Points.Count > 0)
        {
            Vector3 sum = Vector3.Zero;
            foreach (var pt in pointCloud.Points)
                sum += pt;
            return sum / pointCloud.Points.Count;
        }

        return null;
    }

    private static Vector3 UnProject(Vector3 screenPos, Matrix4x4 inverseVP)
    {
        Vector4 worldPos = Vector4.Transform(new Vector4(screenPos, 1.0f), inverseVP);
        if (System.Math.Abs(worldPos.W) > 0.0001f)
            worldPos /= worldPos.W;

        return new Vector3(worldPos.X, worldPos.Y, worldPos.Z);
    }
}
