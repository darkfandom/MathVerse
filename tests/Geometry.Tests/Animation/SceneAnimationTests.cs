namespace MathVerse.Geometry.Tests.Animation;

/// <summary>Tests for the <see cref="SceneAnimation"/> class.</summary>
public class SceneAnimationTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies that an empty scene animation has zero channels.</summary>
    [Fact]
    public void Empty_HasZeroChannels()
    {
        var anim = new SceneAnimation();

        anim.ChannelCount.Should().Be(0);
    }

    /// <summary>Verifies that AddTranslationChannel adds three channels per node.</summary>
    [Fact]
    public void AddTranslationChannel_AddsThreeChannels()
    {
        var anim = new SceneAnimation();
        var node = new SceneNode("test");
        var tx = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 5) });
        var ty = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 10) });
        var tz = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 15) });

        anim.AddTranslationChannel("node1", node, tx, ty, tz);

        anim.ChannelCount.Should().Be(3);
    }

    /// <summary>Verifies that Evaluate moves the node's transform.</summary>
    [Fact]
    public void Evaluate_MovesNode()
    {
        var anim = new SceneAnimation();
        var node = new SceneNode("test");
        var tx = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 10) });
        var ty = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 20) });
        var tz = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 30) });

        anim.AddTranslationChannel("node1", node, tx, ty, tz);
        anim.Evaluate(1.0);

        node.LocalTransform[0, 3].Should().BeApproximately(10.0, Tolerance);
        node.LocalTransform[1, 3].Should().BeApproximately(20.0, Tolerance);
        node.LocalTransform[2, 3].Should().BeApproximately(30.0, Tolerance);
    }

    /// <summary>Verifies that Evaluate at t=0 with zero keyframes does not change transform.</summary>
    [Fact]
    public void Evaluate_AtZero_NoChange()
    {
        var anim = new SceneAnimation();
        var node = new SceneNode("test");
        var tx = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 10) });
        var ty = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 20) });
        var tz = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 30) });

        anim.AddTranslationChannel("node1", node, tx, ty, tz);
        anim.Evaluate(0.0);

        node.LocalTransform[0, 3].Should().BeApproximately(0.0, Tolerance);
        node.LocalTransform[1, 3].Should().BeApproximately(0.0, Tolerance);
        node.LocalTransform[2, 3].Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that Evaluate with multiple nodes processes all nodes.</summary>
    [Fact]
    public void Evaluate_MultipleNodes()
    {
        var anim = new SceneAnimation();
        var node1 = new SceneNode("n1");
        var node2 = new SceneNode("n2");
        var tx1 = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 5) });
        var ty1 = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 0) });
        var tz1 = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 0) });
        var tx2 = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 10) });
        var ty2 = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 0) });
        var tz2 = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(1, 0) });

        anim.AddTranslationChannel("n1", node1, tx1, ty1, tz1);
        anim.AddTranslationChannel("n2", node2, tx2, ty2, tz2);

        anim.Evaluate(1.0);

        node1.LocalTransform[0, 3].Should().BeApproximately(5.0, Tolerance);
        node2.LocalTransform[0, 3].Should().BeApproximately(10.0, Tolerance);
    }

    /// <summary>Verifies that ChannelCount returns the number of registered channels.</summary>
    [Fact]
    public void ChannelCount_ReturnsCorrectNumber()
    {
        var anim = new SceneAnimation();
        var node = new SceneNode("test");
        var tx = new Timeline();
        var ty = new Timeline();
        var tz = new Timeline();

        anim.AddTranslationChannel("node1", node, tx, ty, tz);

        anim.ChannelCount.Should().Be(3);
    }

    /// <summary>Verifies that Evaluate at midpoint interpolates correctly.</summary>
    [Fact]
    public void Evaluate_AtMidpoint_InterpolatesCorrectly()
    {
        var anim = new SceneAnimation();
        var node = new SceneNode("test");
        var tx = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(10, 20) });
        var ty = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(10, 0) });
        var tz = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(10, 0) });

        anim.AddTranslationChannel("node1", node, tx, ty, tz);
        anim.Evaluate(5.0);

        node.LocalTransform[0, 3].Should().BeApproximately(10.0, Tolerance);
    }

    /// <summary>Verifies that StartTime is zero for empty animation.</summary>
    [Fact]
    public void Empty_StartTime_IsZero()
    {
        var anim = new SceneAnimation();

        anim.StartTime.Should().BeApproximately(0.0, Tolerance);
    }
}
