namespace MathVerse.Geometry.Tests.Animation;

/// <summary>Tests for the <see cref="Timeline"/> class.</summary>
public class TimelineTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies that an empty timeline evaluates to zero.</summary>
    [Fact]
    public void Empty_EvaluatesToZero()
    {
        var timeline = new Timeline();

        timeline.Evaluate(0.0).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that a single keyframe timeline returns that value at any time.</summary>
    [Fact]
    public void SingleKeyframe_ReturnsValueAtAnyTime()
    {
        var timeline = new Timeline();
        timeline.AddKeyframe(1.0, 5.0);

        timeline.Evaluate(0.0).Should().BeApproximately(5.0, Tolerance);
        timeline.Evaluate(10.0).Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies that linear interpolation produces correct midpoint.</summary>
    [Fact]
    public void Linear_Midpoint()
    {
        var timeline = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(10, 20) });

        timeline.Evaluate(5.0).Should().BeApproximately(10.0, Tolerance);
    }

    /// <summary>Verifies that SmoothStep interpolation produces correct midpoint.</summary>
    [Fact]
    public void SmoothStep_Midpoint()
    {
        var timeline = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(10, 20) });
        timeline.Mode = InterpolationMode.SmoothStep;

        timeline.Evaluate(5.0).Should().BeApproximately(10.0, Tolerance);
    }

    /// <summary>Verifies that Step interpolation returns the start value before midpoint.</summary>
    [Fact]
    public void Step_BeforeMidpoint_ReturnsStartValue()
    {
        var timeline = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(10, 20) });
        timeline.Mode = InterpolationMode.Step;

        timeline.Evaluate(3.0).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that Cubic interpolation produces correct midpoint.</summary>
    [Fact]
    public void Cubic_Midpoint()
    {
        var timeline = new Timeline(new[] { new Keyframe(0, 0), new Keyframe(10, 20) });
        timeline.Mode = InterpolationMode.Cubic;

        timeline.Evaluate(5.0).Should().BeApproximately(10.0, Tolerance);
    }

    /// <summary>Verifies that evaluating before start returns the first keyframe value.</summary>
    [Fact]
    public void BeforeStart_ReturnsFirstValue()
    {
        var timeline = new Timeline(new[] { new Keyframe(5, 10), new Keyframe(10, 20) });

        timeline.Evaluate(0.0).Should().BeApproximately(10.0, Tolerance);
    }

    /// <summary>Verifies that evaluating after end returns the last keyframe value.</summary>
    [Fact]
    public void AfterEnd_ReturnsLastValue()
    {
        var timeline = new Timeline(new[] { new Keyframe(0, 10), new Keyframe(5, 20) });

        timeline.Evaluate(100.0).Should().BeApproximately(20.0, Tolerance);
    }

    /// <summary>Verifies that Duration returns the difference between end and start time.</summary>
    [Fact]
    public void Duration_ReturnsCorrectValue()
    {
        var timeline = new Timeline(new[] { new Keyframe(2, 0), new Keyframe(8, 0) });

        timeline.Duration.Should().BeApproximately(6.0, Tolerance);
    }

    /// <summary>Verifies that StartTime returns the time of the first keyframe.</summary>
    [Fact]
    public void StartTime_ReturnsFirstKeyframeTime()
    {
        var timeline = new Timeline(new[] { new Keyframe(3, 0), new Keyframe(7, 0) });

        timeline.StartTime.Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies that EndTime returns the time of the last keyframe.</summary>
    [Fact]
    public void EndTime_ReturnsLastKeyframeTime()
    {
        var timeline = new Timeline(new[] { new Keyframe(3, 0), new Keyframe(7, 0) });

        timeline.EndTime.Should().BeApproximately(7.0, Tolerance);
    }

    /// <summary>Verifies that AddKeyframe increments the count.</summary>
    [Fact]
    public void AddKeyframe_IncrementsCount()
    {
        var timeline = new Timeline();

        timeline.AddKeyframe(0, 0);
        timeline.AddKeyframe(1, 1);

        timeline.Count.Should().Be(2);
    }

    /// <summary>Verifies that AddKeyframe maintains sorted order by time.</summary>
    [Fact]
    public void AddKeyframe_MaintainsSortedOrder()
    {
        var timeline = new Timeline();

        timeline.AddKeyframe(5, 0);
        timeline.AddKeyframe(1, 0);
        timeline.AddKeyframe(3, 0);

        var kfs = timeline.GetKeyframes();
        kfs[0].Time.Should().BeApproximately(1.0, Tolerance);
        kfs[1].Time.Should().BeApproximately(3.0, Tolerance);
        kfs[2].Time.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies that an empty timeline has zero duration.</summary>
    [Fact]
    public void Empty_HasZeroDuration()
    {
        var timeline = new Timeline();

        timeline.Duration.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that constructor with keyframes initializes correctly.</summary>
    [Fact]
    public void Constructor_WithKeyframes_InitializesCorrectly()
    {
        var timeline = new Timeline(new[]
        {
            new Keyframe(1, 10),
            new Keyframe(2, 20),
            new Keyframe(3, 30)
        });

        timeline.Count.Should().Be(3);
    }

    /// <summary>Verifies that linear interpolation between multiple segments works.</summary>
    [Fact]
    public void Linear_MultipleSegments()
    {
        var timeline = new Timeline(new[]
        {
            new Keyframe(0, 0),
            new Keyframe(5, 10),
            new Keyframe(10, 0)
        });

        timeline.Evaluate(0.0).Should().BeApproximately(0.0, Tolerance);
        timeline.Evaluate(5.0).Should().BeApproximately(10.0, Tolerance);
        timeline.Evaluate(10.0).Should().BeApproximately(0.0, Tolerance);
    }
}
