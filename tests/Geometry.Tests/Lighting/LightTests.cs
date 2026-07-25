namespace MathVerse.Geometry.Tests.Lighting;

/// <summary>Tests for all light types and Material.</summary>
public class LightTests
{
    private const double Tolerance = 1e-10;

    // AmbientLight tests

    /// <summary>Verifies AmbientLight default color is white.</summary>
    [Fact]
    public void AmbientLight_DefaultColor_IsWhite()
    {
        var light = new AmbientLight();

        light.Color.R.Should().Be(1.0);
        light.Color.G.Should().Be(1.0);
        light.Color.B.Should().Be(1.0);
    }

    /// <summary>Verifies AmbientLight default intensity is 1.0.</summary>
    [Fact]
    public void AmbientLight_DefaultIntensity_IsOne()
    {
        var light = new AmbientLight();

        light.Intensity.Should().Be(1.0);
    }

    /// <summary>Verifies AmbientLight color can be customized.</summary>
    [Fact]
    public void AmbientLight_CustomColor()
    {
        var light = new AmbientLight { Color = (0.5, 0.5, 0.5) };

        light.Color.R.Should().Be(0.5);
        light.Color.G.Should().Be(0.5);
        light.Color.B.Should().Be(0.5);
    }

    // DirectionalLight tests

    /// <summary>Verifies DirectionalLight default direction is (0, -1, 0).</summary>
    [Fact]
    public void DirectionalLight_DefaultDirection_IsDown()
    {
        var light = new DirectionalLight();

        light.Direction.X.Should().BeApproximately(0.0, Tolerance);
        light.Direction.Y.Should().BeApproximately(-1.0, Tolerance);
        light.Direction.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies DirectionalLight default color is white.</summary>
    [Fact]
    public void DirectionalLight_DefaultColor_IsWhite()
    {
        var light = new DirectionalLight();

        light.Color.R.Should().Be(1.0);
        light.Color.G.Should().Be(1.0);
        light.Color.B.Should().Be(1.0);
    }

    /// <summary>Verifies DirectionalLight direction can be set.</summary>
    [Fact]
    public void DirectionalLight_Direction_CanBeSet()
    {
        var light = new DirectionalLight
        {
            Direction = new Vector3D(1, 1, 0)
        };

        light.Direction.X.Should().BeApproximately(1.0, Tolerance);
        light.Direction.Y.Should().BeApproximately(1.0, Tolerance);
    }

    // PointLight tests

    /// <summary>Verifies PointLight default position is origin.</summary>
    [Fact]
    public void PointLight_DefaultPosition_IsOrigin()
    {
        var light = new PointLight();

        light.Position.Should().Be(Point3D.Origin);
    }

    /// <summary>Verifies PointLight default range is MaxValue.</summary>
    [Fact]
    public void PointLight_DefaultRange_IsMaxValue()
    {
        var light = new PointLight();

        light.Range.Should().Be(double.MaxValue);
    }

    /// <summary>Verifies PointLight attenuation defaults are correct.</summary>
    [Fact]
    public void PointLight_DefaultAttenuation()
    {
        var light = new PointLight();

        light.ConstantAttenuation.Should().Be(1.0);
        light.LinearAttenuation.Should().Be(0.0);
        light.QuadraticAttenuation.Should().Be(0.0);
    }

    /// <summary>Verifies PointLight range can be customized.</summary>
    [Fact]
    public void PointLight_Range_CanBeSet()
    {
        var light = new PointLight { Range = 50.0 };

        light.Range.Should().Be(50.0);
    }

    // SpotLight tests

    /// <summary>Verifies SpotLight default inner angle is 30 degrees.</summary>
    [Fact]
    public void SpotLight_DefaultInnerAngle_IsThirtyDegrees()
    {
        var light = new SpotLight();

        light.InnerAngle.Should().Be(30.0);
    }

    /// <summary>Verifies SpotLight default outer angle is 45 degrees.</summary>
    [Fact]
    public void SpotLight_DefaultOuterAngle_IsFortyFiveDegrees()
    {
        var light = new SpotLight();

        light.OuterAngle.Should().Be(45.0);
    }

    /// <summary>Verifies SpotLight default falloff is 1.0.</summary>
    [Fact]
    public void SpotLight_DefaultFalloff_IsOne()
    {
        var light = new SpotLight();

        light.Falloff.Should().Be(1.0);
    }

    /// <summary>Verifies SpotLight default position is origin and default direction is down.</summary>
    [Fact]
    public void SpotLight_DefaultPositionAndDirection()
    {
        var light = new SpotLight();

        light.Position.Should().Be(Point3D.Origin);
        light.Direction.Y.Should().BeApproximately(-1.0, Tolerance);
    }

    // Material tests

    /// <summary>Verifies Material default properties.</summary>
    [Fact]
    public void Material_DefaultProperties()
    {
        var material = new Material();

        material.Name.Should().Be("default");
        material.Shininess.Should().Be(32.0);
        material.Opacity.Should().Be(1.0);
    }

    /// <summary>Verifies Material default colors.</summary>
    [Fact]
    public void Material_DefaultColors()
    {
        var material = new Material();

        material.AmbientColor.Should().Be((0.2, 0.2, 0.2));
        material.DiffuseColor.Should().Be((0.8, 0.8, 0.8));
        material.SpecularColor.Should().Be((1.0, 1.0, 1.0));
        material.EmissiveColor.Should().Be((0.0, 0.0, 0.0));
    }

    /// <summary>Verifies Material properties can be customized.</summary>
    [Fact]
    public void Material_CustomProperties()
    {
        var material = new Material
        {
            Name = "gold",
            DiffuseColor = (1.0, 0.84, 0.0),
            Shininess = 64.0,
            Opacity = 0.9
        };

        material.Name.Should().Be("gold");
        material.DiffuseColor.Should().Be((1.0, 0.84, 0.0));
        material.Shininess.Should().Be(64.0);
        material.Opacity.Should().Be(0.9);
    }

    // Light base class tests

    /// <summary>Verifies Light default enabled is true.</summary>
    [Fact]
    public void Light_DefaultEnabled_IsTrue()
    {
        var light = new AmbientLight();

        light.Enabled.Should().BeTrue();
    }

    /// <summary>Verifies Light name can be set.</summary>
    [Fact]
    public void Light_Name_CanBeSet()
    {
        var light = new DirectionalLight { Name = "Sun" };

        light.Name.Should().Be("Sun");
    }

    /// <summary>Verifies Light enabled can be toggled.</summary>
    [Fact]
    public void Light_Enabled_CanBeToggled()
    {
        var light = new PointLight { Enabled = false };

        light.Enabled.Should().BeFalse();
    }
}
