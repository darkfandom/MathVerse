namespace MathVerse.Math.Visualization.LOD;
using System.Numerics;

/// <summary>Manages level-of-detail selection for visualization objects based on camera distance.</summary>
public sealed class LODManager
{
    private readonly float[] _distanceThresholds = [10.0f, 25.0f, 50.0f, 100.0f];

    /// <summary>Gets or sets the distance thresholds for LOD transitions.</summary>
    public ReadOnlySpan<float> DistanceThresholds => _distanceThresholds;

    /// <summary>Selects the appropriate level of detail index for an object based on its distance from the camera.</summary>
    /// <param name="objectCenter">The world-space center of the object.</param>
    /// <param name="cameraPosition">The world-space position of the camera.</param>
    /// <returns>The LOD index, where 0 is the highest detail.</returns>
    public int SelectLOD(Vector3 objectCenter, Vector3 cameraPosition)
    {
        float distance = Vector3.Distance(cameraPosition, objectCenter);

        for (int i = 0; i < _distanceThresholds.Length; i++)
        {
            if (distance < _distanceThresholds[i])
                return i;
        }

        return _distanceThresholds.Length;
    }
}
