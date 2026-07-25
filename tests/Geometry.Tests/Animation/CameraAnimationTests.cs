namespace MathVerse.Geometry.Tests.Animation;

/// <summary>Tests for the <see cref="CameraAnimation"/> class.</summary>
public class CameraAnimationTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies that an empty camera animation evaluates to zero position and target.</summary>
    [Fact]
    public void Empty_EvaluatesToZero()
    {
        var anim = new CameraAnimation();

        var (pos, target) = anim.Evaluate(0.0);

        pos.X.Should().BeApproximately(0.0, Tolerance);
        pos.Y.Should().BeApproximately(0.0, Tolerance);
        pos.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that SetPositionKeyframes sets position keyframes correctly.</summary>
    [Fact]
    public void SetPositionKeyframes_SetsKeyframes()
    {
        var anim = new CameraAnimation();
        var keyframes = new List<(double Time, Point3D Position)>
        {
            (0.0, new Point3D(0, 0, 0)),
            (1.0, new Point3D(10, 0, 0))
        };

        anim.SetPositionKeyframes(keyframes);

        var (pos, _) = anim.Evaluate(0.5);
        pos.X.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies that Evaluate returns correct position at t=0.</summary>
    [Fact]
    public void Evaluate_Position_AtStart()
    {
        var anim = new CameraAnimation();
        var keyframes = new List<(double Time, Point3D Position)>
        {
            (0.0, new Point3D(1, 2, 3)),
            (1.0, new Point3D(4, 5, 6))
        };

        anim.SetPositionKeyframes(keyframes);

        var (pos, _) = anim.Evaluate(0.0);
        pos.X.Should().BeApproximately(1.0, Tolerance);
        pos.Y.Should().BeApproximately(2.0, Tolerance);
        pos.Z.Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies that Evaluate returns correct position at t=1.</summary>
    [Fact]
    public void Evaluate_Position_AtEnd()
    {
        var anim = new CameraAnimation();
        var keyframes = new List<(double Time, Point3D Position)>
        {
            (0.0, new Point3D(1, 2, 3)),
            (1.0, new Point3D(4, 5, 6))
        };

        anim.SetPositionKeyframes(keyframes);

        var (pos, _) = anim.Evaluate(1.0);
        pos.X.Should().BeApproximately(4.0, Tolerance);
        pos.Y.Should().BeApproximately(5.0, Tolerance);
        pos.Z.Should().BeApproximately(6.0, Tolerance);
    }

    /// <summary>Verifies that Evaluate returns correct target at t=0.</summary>
    [Fact]
    public void Evaluate_Target_AtStart()
    {
        var anim = new CameraAnimation();
        var keyframes = new List<(double Time, Point3D Target)>
        {
            (0.0, new Point3D(10, 20, 30)),
            (1.0, new Point3D(40, 50, 60))
        };

        anim.SetTargetKeyframes(keyframes);

        var (_, target) = anim.Evaluate(0.0);
        target.X.Should().BeApproximately(10.0, Tolerance);
        target.Y.Should().BeApproximately(20.0, Tolerance);
        target.Z.Should().BeApproximately(30.0, Tolerance);
    }

    /// <summary>Verifies that Evaluate returns correct target at t=1.</summary>
    [Fact]
    public void Evaluate_Target_AtEnd()
    {
        var anim = new CameraAnimation();
        var keyframes = new List<(double Time, Point3D Target)>
        {
            (0.0, new Point3D(10, 20, 30)),
            (1.0, new Point3D(40, 50, 60))
        };

        anim.SetTargetKeyframes(keyframes);

        var (_, target) = anim.Evaluate(1.0);
        target.X.Should().BeApproximately(40.0, Tolerance);
        target.Y.Should().BeApproximately(50.0, Tolerance);
        target.Z.Should().BeApproximately(60.0, Tolerance);
    }

    /// <summary>Verifies that position and target interpolate independently.</summary>
    [Fact]
    public void Evaluate_PositionAndTarget_Independent()
    {
        var anim = new CameraAnimation();
        var posKeyframes = new List<(double Time, Point3D Position)>
        {
            (0.0, new Point3D(0, 0, 0)),
            (1.0, new Point3D(10, 0, 0))
        };
        var targetKeyframes = new List<(double Time, Point3D Target)>
        {
            (0.0, new Point3D(0, 0, 0)),
            (1.0, new Point3D(0, 10, 0))
        };

        anim.SetPositionKeyframes(posKeyframes);
        anim.SetTargetKeyframes(targetKeyframes);

        var (pos, target) = anim.Evaluate(0.5);
        pos.X.Should().BeApproximately(5.0, Tolerance);
        target.Y.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies that empty animation evaluates target at origin.</summary>
    [Fact]
    public void Empty_EvaluatesTargetAtOrigin()
    {
        var anim = new CameraAnimation();

        var (_, target) = anim.Evaluate(5.0);

        target.X.Should().BeApproximately(0.0, Tolerance);
        target.Y.Should().BeApproximately(0.0, Tolerance);
        target.Z.Should().BeApproximately(0.0, Tolerance);
    }
}
