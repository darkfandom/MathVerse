namespace MathVerse.Geometry.Tests.Rendering;

/// <summary>Tests for the <see cref="RenderState"/> record.</summary>
public class RenderStateTests
{
    /// <summary>Verifies that default render state has black background color.</summary>
    [Fact]
    public void Default_BackgroundColor_IsBlack()
    {
        var state = new RenderState();

        state.BackgroundColor.Should().Be(Color.Black);
    }

    /// <summary>Verifies that the BackgroundColor property can be set.</summary>
    [Fact]
    public void BackgroundColor_CanBeSet()
    {
        var state = new RenderState { BackgroundColor = Color.White };

        state.BackgroundColor.Should().Be(Color.White);
    }

    /// <summary>Verifies that default render state has Wireframe disabled.</summary>
    [Fact]
    public void Default_Wireframe_IsFalse()
    {
        var state = new RenderState();

        state.Wireframe.Should().BeFalse();
    }

    /// <summary>Verifies that Wireframe can be toggled.</summary>
    [Fact]
    public void Wireframe_CanBeToggled()
    {
        var state = new RenderState { Wireframe = true };

        state.Wireframe.Should().BeTrue();
    }

    /// <summary>Verifies that default render state has BackfaceCulling enabled.</summary>
    [Fact]
    public void Default_BackfaceCulling_IsTrue()
    {
        var state = new RenderState();

        state.BackfaceCulling.Should().BeTrue();
    }

    /// <summary>Verifies that default render state has DepthTest enabled.</summary>
    [Fact]
    public void Default_DepthTest_IsTrue()
    {
        var state = new RenderState();

        state.DepthTest.Should().BeTrue();
    }

    /// <summary>Verifies that default render state has AlphaBlending disabled.</summary>
    [Fact]
    public void Default_AlphaBlending_IsFalse()
    {
        var state = new RenderState();

        state.AlphaBlending.Should().BeFalse();
    }

    /// <summary>Verifies that the LineWidth property can be set.</summary>
    [Fact]
    public void LineWidth_CanBeSet()
    {
        var state = new RenderState { LineWidth = 3.5 };

        state.LineWidth.Should().BeApproximately(3.5, 1e-10);
    }
}
