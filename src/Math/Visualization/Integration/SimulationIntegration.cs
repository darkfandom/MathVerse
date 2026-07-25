namespace MathVerse.Math.Visualization.Integration;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

/// <summary>Represents a simulation state at a point in time.</summary>
public sealed class SimulationState
{
    /// <summary>Gets the current time.</summary>
    public double Time { get; init; }

    /// <summary>Gets the particle positions.</summary>
    public List<Vector3> Positions { get; init; } = new();

    /// <summary>Gets the particle velocities.</summary>
    public List<Vector3> Velocities { get; init; } = new();

    /// <summary>Gets the particle colors.</summary>
    public List<string> Colors { get; init; } = new();
}

/// <summary>Represents a trajectory for visualization.</summary>
public sealed class Trajectory
{
    /// <summary>Gets the trajectory name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the trajectory points.</summary>
    public List<Vector3> Points { get; init; } = new();

    /// <summary>Gets the trajectory color.</summary>
    public string Color { get; init; } = "#0000FF";
}

/// <summary>Integrates with Simulation module for simulation visualization.</summary>
public sealed class SimulationIntegration
{
    /// <summary>Creates a point cloud from simulation state particles.</summary>
    /// <param name="state">The simulation state.</param>
    /// <param name="color">The particle color.</param>
    /// <param name="pointSize">The point size.</param>
    /// <returns>A point cloud visualization.</returns>
    public static Core.PointCloud CreateParticleSystem(SimulationState state, string color = "#FF0000", double pointSize = 4.0)
    {
        var pointCloud = new Core.PointCloud
        {
            Id = "sim-particles-" + System.Guid.NewGuid().ToString("N").Substring(0, 8),
            Color = color,
            PointSize = pointSize,
            Points = new List<Vector3>(state.Positions)
        };

        return pointCloud;
    }

    /// <summary>Creates a line plot from a trajectory.</summary>
    /// <param name="trajectory">The trajectory to visualize.</param>
    /// <param name="lineWidth">The line width.</param>
    /// <returns>A line plot visualization.</returns>
    public static Core.LinePlot CreateTrajectoryPlot(Trajectory trajectory, double lineWidth = 2.0)
    {
        var linePlot = new Core.LinePlot
        {
            Id = "sim-trajectory-" + trajectory.Name,
            Color = trajectory.Color,
            LineWidth = lineWidth,
            Points = new List<Vector2>(trajectory.Points.Select(p => new Vector2(p.X, p.Y)))
        };

        return linePlot;
    }

    /// <summary>Creates multiple trajectory plots for comparison.</summary>
    /// <param name="trajectories">The trajectories to visualize.</param>
    /// <param name="lineWidth">The line width.</param>
    /// <returns>List of line plot visualizations.</returns>
    public static List<Core.LinePlot> CreateTrajectoryPlots(List<Trajectory> trajectories, double lineWidth = 2.0)
    {
        var plots = new List<Core.LinePlot>();

        foreach (var traj in trajectories)
        {
            plots.Add(CreateTrajectoryPlot(traj, lineWidth));
        }

        return plots;
    }

    /// <summary>Creates a time-series plot from simulation data.</summary>
    /// <param name="times">The time values.</param>
    /// <param name="values">The data values.</param>
    /// <param name="color">The line color.</param>
    /// <returns>A line plot for the time series.</returns>
    public static Core.LinePlot CreateTimeSeriesPlot(double[] times, double[] values, string color = "#0000FF")
    {
        var linePlot = new Core.LinePlot
        {
            Id = "sim-timeseries-" + System.Guid.NewGuid().ToString("N").Substring(0, 8),
            Color = color,
            LineWidth = 2.0,
            Points = new List<Vector2>()
        };

        int count = System.Math.Min(times.Length, values.Length);
        for (int i = 0; i < count; i++)
        {
            linePlot.Points.Add(new Vector2((float)times[i], (float)values[i]));
        }

        return linePlot;
    }

    /// <summary>Creates a phase portrait from two state variables.</summary>
    /// <param name="xValues">The X state variable values.</param>
    /// <param name="yValues">The Y state variable values.</param>
    /// <param name="color">The line color.</param>
    /// <returns>A line plot for the phase portrait.</returns>
    public static Core.LinePlot CreatePhasePortrait(double[] xValues, double[] yValues, string color = "#FF00FF")
    {
        var linePlot = new Core.LinePlot
        {
            Id = "sim-phase-" + System.Guid.NewGuid().ToString("N").Substring(0, 8),
            Color = color,
            LineWidth = 1.5,
            Points = new List<Vector2>()
        };

        int count = System.Math.Min(xValues.Length, yValues.Length);
        for (int i = 0; i < count; i++)
        {
            linePlot.Points.Add(new Vector2((float)xValues[i], (float)yValues[i]));
        }

        return linePlot;
    }

    /// <summary>Creates a vector field visualization from a grid of vectors.</summary>
    /// <param name="gridX">The X grid positions.</param>
    /// <param name="gridY">The Y grid positions.</param>
    /// <param name="fieldX">The X component of the vector field.</param>
    /// <param name="fieldY">The Y component of the vector field.</param>
    /// <param name="scale">The scale factor for arrow length.</param>
    /// <returns>Line segments representing the vector field.</returns>
    public static List<(Vector2 Start, Vector2 End)> CreateVectorField(
        double[] gridX, double[] gridY, double[] fieldX, double[] fieldY, double scale = 1.0)
    {
        var arrows = new List<(Vector2, Vector2)>();

        int count = System.Math.Min(
            System.Math.Min(gridX.Length, gridY.Length),
            System.Math.Min(fieldX.Length, fieldY.Length));

        for (int i = 0; i < count; i++)
        {
            Vector2 start = new Vector2((float)gridX[i], (float)gridY[i]);
            float dx = (float)(fieldX[i] * scale);
            float dy = (float)(fieldY[i] * scale);
            Vector2 end = start + new Vector2(dx, dy);

            arrows.Add((start, end));
        }

        return arrows;
    }

    /// <summary>Creates a stream line from integrated velocity data.</summary>
    /// <param name="startPoint">The starting point.</param>
    /// <param name="velocityField">The velocity field function.</param>
    /// <param name="dt">The integration time step.</param>
    /// <param name="steps">The number of integration steps.</param>
    /// <returns>The stream line points.</returns>
    public static List<Vector2> CreateStreamLine(
        Vector2 startPoint, System.Func<Vector2, Vector2> velocityField,
        double dt = 0.01, int steps = 1000)
    {
        var points = new List<Vector2>();
        Vector2 current = startPoint;

        for (int i = 0; i < steps; i++)
        {
            points.Add(current);
            Vector2 velocity = velocityField(current);

            float speed = velocity.Length();
            if (speed < 1e-6f)
                break;

            current += velocity * (float)dt;

            if (float.IsNaN(current.X) || float.IsNaN(current.Y) ||
                float.IsInfinity(current.X) || float.IsInfinity(current.Y))
                break;
        }

        return points;
    }

    /// <summary>Creates an animation frame from simulation state.</summary>
    /// <param name="states">The simulation states over time.</param>
    /// <param name="frameIndex">The current frame index.</param>
    /// <returns>Visualization objects for the current frame.</returns>
    public static List<Core.VisualizationObject> CreateAnimationFrame(
        List<SimulationState> states, int frameIndex)
    {
        var objects = new List<Core.VisualizationObject>();

        if (states == null || frameIndex < 0 || frameIndex >= states.Count)
            return objects;

        var state = states[frameIndex];

        if (state.Positions.Count > 0)
        {
            var pointCloud = new Core.PointCloud
            {
                Id = $"sim-frame-{frameIndex}",
                Color = state.Colors.Count > 0 ? state.Colors[0] : "#FF0000",
                PointSize = 4.0,
                Points = new List<Vector3>(state.Positions)
            };

            objects.Add(pointCloud);
        }

        return objects;
    }

    /// <summary>Creates a trajectory visualization with trail effect.</summary>
    /// <param name="positions">The positions over time.</param>
    /// <param name="trailLength">The number of trail points to show.</param>
    /// <param name="color">The trajectory color.</param>
    /// <returns>The trail line plot.</returns>
    public static Core.LinePlot CreateTrajectoryTrail(
        List<Vector3> positions, int trailLength = 50, string color = "#0000FF")
    {
        var linePlot = new Core.LinePlot
        {
            Id = "sim-trail-" + System.Guid.NewGuid().ToString("N").Substring(0, 8),
            Color = color,
            LineWidth = 2.0,
            Points = new List<Vector2>()
        };

        int startIndex = System.Math.Max(0, positions.Count - trailLength);
        for (int i = startIndex; i < positions.Count; i++)
        {
            linePlot.Points.Add(new Vector2(positions[i].X, positions[i].Y));
        }

        return linePlot;
    }

    /// <summary>Computes kinetic energy from velocities and masses.</summary>
    /// <param name="velocities">The velocity vectors.</param>
    /// <param name="masses">The masses (scalar per particle).</param>
    /// <returns>The total kinetic energy.</returns>
    public static double ComputeKineticEnergy(List<Vector3> velocities, double[] masses)
    {
        double totalEnergy = 0;
        int count = System.Math.Min(velocities.Count, masses.Length);

        for (int i = 0; i < count; i++)
        {
            double speedSq = velocities[i].LengthSquared();
            totalEnergy += 0.5 * masses[i] * speedSq;
        }

        return totalEnergy;
    }

    /// <summary>Creates a histogram of particle speeds.</summary>
    /// <param name="velocities">The velocity vectors.</param>
    /// <param name="binCount">The number of bins.</param>
    /// <returns>Bin edges and counts.</returns>
    public static (double[] BinEdges, double[] Counts) CreateSpeedHistogram(
        List<Vector3> velocities, int binCount = 20)
    {
        if (velocities == null || velocities.Count == 0)
            return (new double[0], new double[0]);

        double[] speeds = new double[velocities.Count];
        double minSpeed = double.MaxValue;
        double maxSpeed = double.MinValue;

        for (int i = 0; i < velocities.Count; i++)
        {
            speeds[i] = velocities[i].Length();
            minSpeed = System.Math.Min(minSpeed, speeds[i]);
            maxSpeed = System.Math.Max(maxSpeed, speeds[i]);
        }

        if (System.Math.Abs(maxSpeed - minSpeed) < 1e-10)
        {
            return (new double[] { minSpeed - 0.5, maxSpeed + 0.5 }, new double[] { velocities.Count });
        }

        double binWidth = (maxSpeed - minSpeed) / binCount;
        double[] counts = new double[binCount];
        double[] edges = new double[binCount + 1];

        for (int i = 0; i <= binCount; i++)
        {
            edges[i] = minSpeed + i * binWidth;
        }

        foreach (double speed in speeds)
        {
            int bin = (int)((speed - minSpeed) / binWidth);
            bin = System.Math.Max(0, System.Math.Min(binCount - 1, bin));
            counts[bin]++;
        }

        return (edges, counts);
    }
}
