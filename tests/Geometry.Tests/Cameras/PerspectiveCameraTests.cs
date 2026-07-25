namespace MathVerse.Geometry.Tests.Cameras;

/// <summary>Tests for the <see cref="PerspectiveCamera"/> class.</summary>
public class PerspectiveCameraTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies default field of view is 60 degrees.</summary>
    [Fact]
    public void DefaultFieldOfView_IsSixtyDegrees()
    {
        var camera = new PerspectiveCamera();

        camera.FieldOfView.Should().Be(60.0);
    }

    /// <summary>Verifies default aspect ratio is 1.0.</summary>
    [Fact]
    public void DefaultAspectRatio_IsOne()
    {
        var camera = new PerspectiveCamera();

        camera.AspectRatio.Should().Be(1.0);
    }

    /// <summary>Verifies default near plane is 0.1.</summary>
    [Fact]
    public void DefaultNearPlane_IsPointOne()
    {
        var camera = new PerspectiveCamera();

        camera.NearPlane.Should().Be(0.1);
    }

    /// <summary>Verifies default far plane is 1000.</summary>
    [Fact]
    public void DefaultFarPlane_IsThousand()
    {
        var camera = new PerspectiveCamera();

        camera.FarPlane.Should().Be(1000.0);
    }

    /// <summary>Verifies GetViewMatrix returns a non-null transform.</summary>
    [Fact]
    public void GetViewMatrix_ReturnsNonNull()
    {
        var camera = new PerspectiveCamera();

        Transform3D view = camera.GetViewMatrix();

        view.Should().NotBe(default(Transform3D));
    }

    /// <summary>Verifies GetProjectionMatrix returns a non-null transform.</summary>
    [Fact]
    public void GetProjectionMatrix_ReturnsNonNull()
    {
        var camera = new PerspectiveCamera();

        Transform3D proj = camera.GetProjectionMatrix();

        proj.Should().NotBe(default(Transform3D));
    }

    /// <summary>Verifies Forward direction points from position toward target.</summary>
    [Fact]
    public void Forward_PointTowardTarget()
    {
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            Target = Point3D.Origin
        };

        Vector3D forward = camera.Forward;

        forward.Z.Should().BeApproximately(-1.0, Tolerance);
    }

    /// <summary>Verifies Right direction is perpendicular to Forward.</summary>
    [Fact]
    public void Right_IsPerpendicularToForward()
    {
        var camera = new PerspectiveCamera();

        Vector3D right = camera.Right;
        Vector3D forward = camera.Forward;

        double dot = right.Dot(forward);
        dot.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies custom field of view can be set.</summary>
    [Fact]
    public void FieldOfView_CanBeSet()
    {
        var camera = new PerspectiveCamera { FieldOfView = 90.0 };

        camera.FieldOfView.Should().Be(90.0);
    }

    /// <summary>Verifies default position is (0, 0, 5).</summary>
    [Fact]
    public void DefaultPosition_IsZeroZeroFive()
    {
        var camera = new PerspectiveCamera();

        camera.Position.X.Should().BeApproximately(0.0, Tolerance);
        camera.Position.Y.Should().BeApproximately(0.0, Tolerance);
        camera.Position.Z.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies default target is the origin.</summary>
    [Fact]
    public void DefaultTarget_IsOrigin()
    {
        var camera = new PerspectiveCamera();

        camera.Target.Should().Be(Point3D.Origin);
    }

    /// <summary>Verifies PerspectiveCamera is a Camera (inheritance).</summary>
    [Fact]
    public void PerspectiveCamera_InheritsCamera()
    {
        var camera = new PerspectiveCamera();

        camera.Should().BeAssignableTo<Camera>();
    }
}
