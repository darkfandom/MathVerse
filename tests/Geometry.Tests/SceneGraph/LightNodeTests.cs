namespace MathVerse.Geometry.Tests.SceneGraph;

/// <summary>Tests for the <see cref="LightNode"/> class.</summary>
public class LightNodeTests
{
    /// <summary>Verifies LightNode construction stores the name.</summary>
    [Fact]
    public void Constructor_StoresName()
    {
        var node = new LightNode("SunLight");

        node.Name.Should().Be("SunLight");
    }

    /// <summary>Verifies LightNode defaults to an AmbientLight when none is provided.</summary>
    [Fact]
    public void Constructor_DefaultsToAmbientLight()
    {
        var node = new LightNode();

        node.Light.Should().BeOfType<AmbientLight>();
    }

    /// <summary>Verifies LightNode stores a provided light instance.</summary>
    [Fact]
    public void Constructor_StoresProvidedLight()
    {
        Light light = new PointLight();
        var node = new LightNode("Point", light);

        node.Light.Should().BeSameAs(light);
    }

    /// <summary>Verifies Light property can be replaced after construction.</summary>
    [Fact]
    public void Light_CanBeReplaced()
    {
        var node = new LightNode();
        Light newLight = new DirectionalLight();

        node.Light = newLight;

        node.Light.Should().BeSameAs(newLight);
    }

    /// <summary>Verifies LightNode is a SceneNode (inheritance).</summary>
    [Fact]
    public void LightNode_InheritsSceneNode()
    {
        var node = new LightNode();

        node.Should().BeAssignableTo<SceneNode>();
    }

    /// <summary>Verifies LightNode with null light defaults to AmbientLight.</summary>
    [Fact]
    public void Constructor_NullLight_DefaultsToAmbient()
    {
        Light? light = null;
        var node = new LightNode("Test", light);

        node.Light.Should().BeOfType<AmbientLight>();
    }

    /// <summary>Verifies LightNode default name is empty string.</summary>
    [Fact]
    public void Constructor_DefaultNameIsEmpty()
    {
        var node = new LightNode();

        node.Name.Should().Be("");
    }
}
