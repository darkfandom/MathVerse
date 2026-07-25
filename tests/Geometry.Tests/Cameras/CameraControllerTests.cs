namespace MathVerse.Geometry.Tests.Cameras;

/// <summary>Tests for the <see cref="CameraController"/> class.</summary>
public class CameraControllerTests
{
    private const double Tolerance = 1e-6;

    /// <summary>Verifies MoveForward shifts position along forward direction.</summary>
    [Fact]
    public void MoveForward_ShiftsPosition()
    {
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            Target = Point3D.Origin
        };
        var controller = new CameraController(camera);
        double initialZ = camera.Position.Z;

        controller.MoveForward(1.0);

        controller.Camera.Position.Z.Should().NotBeApproximately(initialZ, Tolerance);
    }

    /// <summary>Verifies MoveRight shifts position along right direction.</summary>
    [Fact]
    public void MoveRight_ShiftsPosition()
    {
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            Target = Point3D.Origin
        };
        var controller = new CameraController(camera);

        controller.MoveRight(2.0);

        double dx = controller.Camera.Position.X - camera.Position.X;
        System.Math.Abs(dx).Should().BeGreaterThan(0);
    }

    /// <summary>Verifies MoveUp shifts position along up direction.</summary>
    [Fact]
    public void MoveUp_ShiftsPosition()
    {
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            Target = Point3D.Origin
        };
        var controller = new CameraController(camera);

        controller.MoveUp(1.0);

        double dy = controller.Camera.Position.Y - camera.Position.Y;
        System.Math.Abs(dy).Should().BeGreaterThan(0);
    }

    /// <summary>Verifies Rotate changes the target position.</summary>
    [Fact]
    public void Rotate_ChangesTarget()
    {
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            Target = Point3D.Origin
        };
        var controller = new CameraController(camera);

        controller.Rotate(0.5, 0.0);

        controller.Camera.Target.Should().NotBe(Point3D.Origin);
    }

    /// <summary>Verifies LookAt sets the target to the specified point.</summary>
    [Fact]
    public void LookAt_SetsTarget()
    {
        var camera = new PerspectiveCamera();
        var controller = new CameraController(camera);
        var target = new Point3D(10, 20, 30);

        controller.LookAt(target);

        controller.Camera.Target.Should().Be(target);
    }

    /// <summary>Verifies Reset restores the original position and target.</summary>
    [Fact]
    public void Reset_RestoresOriginalState()
    {
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            Target = Point3D.Origin
        };
        var controller = new CameraController(camera);
        controller.MoveForward(10);
        controller.Rotate(1.0, 0.5);

        controller.Reset();

        controller.Camera.Position.X.Should().BeApproximately(0.0, Tolerance);
        controller.Camera.Position.Y.Should().BeApproximately(0.0, Tolerance);
        controller.Camera.Position.Z.Should().BeApproximately(5.0, Tolerance);
        controller.Camera.Target.Should().Be(Point3D.Origin);
    }

    /// <summary>Verifies MoveForward with negative distance moves backward.</summary>
    [Fact]
    public void MoveForward_NegativeDistance_MovesBackward()
    {
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            Target = Point3D.Origin
        };
        var controller = new CameraController(camera);

        controller.MoveForward(-1.0);

        controller.Camera.Position.Z.Should().BeGreaterThan(5.0);
    }

    /// <summary>Verifies MoveRight with positive distance moves right (positive X with default camera).</summary>
    [Fact]
    public void MoveRight_PositiveDistance_MovesInPositiveX()
    {
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            Target = Point3D.Origin
        };
        var controller = new CameraController(camera);

        controller.MoveRight(1.0);

        controller.Camera.Position.X.Should().BeGreaterThan(0.0);
    }

    /// <summary>Verifies Camera property can be replaced.</summary>
    [Fact]
    public void Camera_CanBeReplaced()
    {
        var camera1 = new PerspectiveCamera();
        var controller = new CameraController(camera1);
        var camera2 = new OrthographicCamera();

        controller.Camera = camera2;

        controller.Camera.Should().BeSameAs(camera2);
    }

    /// <summary>Verifies Rotate with zero angles does not change target.</summary>
    [Fact]
    public void Rotate_ZeroAngles_NoChange()
    {
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            Target = Point3D.Origin
        };
        var controller = new CameraController(camera);

        controller.Rotate(0.0, 0.0);

        controller.Camera.Target.Should().Be(Point3D.Origin);
    }

    /// <summary>Verifies multiple forward moves accumulate.</summary>
    [Fact]
    public void MoveForward_MultipleTimes_Accumulates()
    {
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            Target = Point3D.Origin
        };
        var controller = new CameraController(camera);

        controller.MoveForward(1.0);
        controller.MoveForward(1.0);

        double totalDist = controller.Camera.Position.DistanceTo(new Point3D(0, 0, 5));
        totalDist.Should().BeGreaterThan(1.0);
    }

    /// <summary>Verifies LookAt with origin sets target to origin.</summary>
    [Fact]
    public void LookAt_Origin_SetsTargetToOrigin()
    {
        var camera = new PerspectiveCamera
        {
            Target = new Point3D(5, 5, 5)
        };
        var controller = new CameraController(camera);

        controller.LookAt(Point3D.Origin);

        controller.Camera.Target.Should().Be(Point3D.Origin);
    }
}
