namespace MathVerse.Geometry.Tests.Cameras;

/// <summary>Tests for the <see cref="OrthographicCamera"/> class.</summary>
public class OrthographicCameraTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies default half-width is 10.0.</summary>
    [Fact]
    public void DefaultHalfWidth_IsTen()
    {
        var camera = new OrthographicCamera();

        camera.HalfWidth.Should().Be(10.0);
    }

    /// <summary>Verifies default half-height is 10.0.</summary>
    [Fact]
    public void DefaultHalfHeight_IsTen()
    {
        var camera = new OrthographicCamera();

        camera.HalfHeight.Should().Be(10.0);
    }

    /// <summary>Verifies default field of view is 60 (inherited from Camera base).</summary>
    [Fact]
    public void DefaultFieldOfView_IsSixtyDegrees()
    {
        var camera = new OrthographicCamera();

        camera.FieldOfView.Should().Be(60.0);
    }

    /// <summary>Verifies GetViewMatrix returns a non-null transform.</summary>
    [Fact]
    public void GetViewMatrix_ReturnsNonNull()
    {
        var camera = new OrthographicCamera();

        Transform3D view = camera.GetViewMatrix();

        view.Should().NotBe(default(Transform3D));
    }

    /// <summary>Verifies GetProjectionMatrix returns a non-null transform.</summary>
    [Fact]
    public void GetProjectionMatrix_ReturnsNonNull()
    {
        var camera = new OrthographicCamera();

        Transform3D proj = camera.GetProjectionMatrix();

        proj.Should().NotBe(default(Transform3D));
    }

    /// <summary>Verifies custom half-width and half-height can be set.</summary>
    [Fact]
    public void CustomHalfWidthAndHeight_CanBeSet()
    {
        var camera = new OrthographicCamera
        {
            HalfWidth = 20.0,
            HalfHeight = 15.0
        };

        camera.HalfWidth.Should().Be(20.0);
        camera.HalfHeight.Should().Be(15.0);
    }

    /// <summary>Verifies OrthographicCamera is a Camera (inheritance).</summary>
    [Fact]
    public void OrthographicCamera_InheritsCamera()
    {
        var camera = new OrthographicCamera();

        camera.Should().BeAssignableTo<Camera>();
    }

    /// <summary>Verifies default aspect ratio is 1.0.</summary>
    [Fact]
    public void DefaultAspectRatio_IsOne()
    {
        var camera = new OrthographicCamera();

        camera.AspectRatio.Should().Be(1.0);
    }

    /// <summary>Verifies default position is (0, 0, 5).</summary>
    [Fact]
    public void DefaultPosition_IsZeroZeroFive()
    {
        var camera = new OrthographicCamera();

        camera.Position.Z.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies default target is the origin.</summary>
    [Fact]
    public void DefaultTarget_IsOrigin()
    {
        var camera = new OrthographicCamera();

        camera.Target.Should().Be(Point3D.Origin);
    }
}
