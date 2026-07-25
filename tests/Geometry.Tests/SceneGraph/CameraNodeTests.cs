namespace MathVerse.Geometry.Tests.SceneGraph;

/// <summary>Tests for the <see cref="CameraNode"/> class.</summary>
public class CameraNodeTests
{
    /// <summary>Verifies CameraNode construction stores the name.</summary>
    [Fact]
    public void Constructor_StoresName()
    {
        var node = new CameraNode("MainCamera");

        node.Name.Should().Be("MainCamera");
    }

    /// <summary>Verifies CameraNode defaults to a PerspectiveCamera when none is provided.</summary>
    [Fact]
    public void Constructor_DefaultsToPerspectiveCamera()
    {
        var node = new CameraNode();

        node.Camera.Should().BeOfType<PerspectiveCamera>();
    }

    /// <summary>Verifies CameraNode stores a provided camera instance.</summary>
    [Fact]
    public void Constructor_StoresProvidedCamera()
    {
        Camera camera = new OrthographicCamera();
        var node = new CameraNode("Ortho", camera);

        node.Camera.Should().BeSameAs(camera);
    }

    /// <summary>Verifies Camera property can be replaced after construction.</summary>
    [Fact]
    public void Camera_CanBeReplaced()
    {
        var node = new CameraNode();
        Camera newCam = new OrthographicCamera();

        node.Camera = newCam;

        node.Camera.Should().BeSameAs(newCam);
    }

    /// <summary>Verifies CameraNode is a SceneNode (inheritance).</summary>
    [Fact]
    public void CameraNode_InheritsSceneNode()
    {
        var node = new CameraNode();

        node.Should().BeAssignableTo<SceneNode>();
    }

    /// <summary>Verifies CameraNode with null camera defaults to PerspectiveCamera.</summary>
    [Fact]
    public void Constructor_NullCamera_DefaultsToPerspective()
    {
        Camera? cam = null;
        var node = new CameraNode("Test", cam);

        node.Camera.Should().BeOfType<PerspectiveCamera>();
    }

    /// <summary>Verifies CameraNode default name is empty string.</summary>
    [Fact]
    public void Constructor_DefaultNameIsEmpty()
    {
        var node = new CameraNode();

        node.Name.Should().Be("");
    }
}
