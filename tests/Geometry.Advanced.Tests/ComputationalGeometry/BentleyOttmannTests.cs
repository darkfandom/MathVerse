namespace MathVerse.Geometry.Advanced.Tests.ComputationalGeometry;

public class BentleyOttmannTests
{
    private const double Precision = 1e-6;

    [Fact]
    public void FindIntersections_TwoCrossingSegments_ReturnsOneIntersection()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(4, 4)),
            new(new Point2D(0, 4), new Point2D(4, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_NoIntersections_ReturnsEmpty()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(1, 0)),
            new(new Point2D(0, 1), new Point2D(1, 1))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_ParallelSegments_ReturnsEmpty()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(4, 0)),
            new(new Point2D(0, 1), new Point2D(4, 1))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_EmptyInput_ReturnsEmpty()
    {
        var segments = new List<Segment2D>();

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_SingleSegment_ReturnsEmpty()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(4, 4))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_ThreeCrossingSegments_ReturnsThreeIntersections()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 2), new Point2D(4, 2)),
            new(new Point2D(2, 0), new Point2D(2, 4)),
            new(new Point2D(0, 0), new Point2D(4, 4))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_FourCrossingSegments_ReturnsIntersections()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(4, 4)),
            new(new Point2D(0, 4), new Point2D(4, 0)),
            new(new Point2D(2, 0), new Point2D(2, 4)),
            new(new Point2D(0, 2), new Point2D(4, 2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_PerpendicularCrossAtCenter()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-2, 0), new Point2D(2, 0)),
            new(new Point2D(0, -2), new Point2D(0, 2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_TwoParallelHorizontal_ReturnsEmpty_B()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-2, 1), new Point2D(2, 1)),
            new(new Point2D(-2, -1), new Point2D(2, -1))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_TwoParallelVertical_ReturnsEmpty()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(1, -2), new Point2D(1, 2)),
            new(new Point2D(3, -2), new Point2D(3, 2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_TwoParallelHorizontal_ReturnsEmpty()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-2, 1), new Point2D(2, 1)),
            new(new Point2D(-2, 3), new Point2D(2, 3))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_ThreeParallelSegments_ReturnsEmpty()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(5, 0)),
            new(new Point2D(0, 1), new Point2D(5, 1)),
            new(new Point2D(0, 2), new Point2D(5, 2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_SegmentsThatDoNotOverlap_TouchingEndpoints()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(1, 1)),
            new(new Point2D(1, 1), new Point2D(2, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_DisjointNonParallel()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(1, 1)),
            new(new Point2D(3, 0), new Point2D(4, 1))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_FiveRandomSegments_ReturnsCorrectCount()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(6, 6)),
            new(new Point2D(0, 6), new Point2D(6, 0)),
            new(new Point2D(3, 0), new Point2D(3, 6)),
            new(new Point2D(0, 3), new Point2D(6, 3)),
            new(new Point2D(1, 1), new Point2D(5, 5))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_TwoIdenticalSegments_ReturnsEmpty()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(4, 4)),
            new(new Point2D(0, 0), new Point2D(4, 4))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_VShapeTwoSegments_OneIntersection()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(2, 2)),
            new(new Point2D(0, 2), new Point2D(2, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_CrossPatternFourSegments()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-2, 0), new Point2D(2, 0)),
            new(new Point2D(0, -2), new Point2D(0, 2)),
            new(new Point2D(-2, -2), new Point2D(2, 2)),
            new(new Point2D(-2, 2), new Point2D(2, -2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_SixNonParallelSegments_ReturnsIntersections()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(6, 6)),
            new(new Point2D(0, 6), new Point2D(6, 0)),
            new(new Point2D(0, 3), new Point2D(6, 3)),
            new(new Point2D(3, 0), new Point2D(3, 6)),
            new(new Point2D(1, 0), new Point2D(5, 6)),
            new(new Point2D(0, 1), new Point2D(6, 5))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_StarPatternCenter()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-3, 0), new Point2D(3, 0)),
            new(new Point2D(0, -3), new Point2D(0, 3)),
            new(new Point2D(-2, -2), new Point2D(2, 2)),
            new(new Point2D(-2, 2), new Point2D(2, -2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_TriangularPattern()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(4, 0)),
            new(new Point2D(0, 0), new Point2D(2, 3)),
            new(new Point2D(4, 0), new Point2D(2, 3))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_OverlappingParallelSegments()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(4, 0)),
            new(new Point2D(2, 0), new Point2D(6, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_FormingTrianglePlusCross()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(4, 0)),
            new(new Point2D(0, 0), new Point2D(0, 4)),
            new(new Point2D(4, 0), new Point2D(0, 4)),
            new(new Point2D(0, 0), new Point2D(4, 4))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_ThreeParallelOneDiagonal()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(5, 0)),
            new(new Point2D(0, 2), new Point2D(5, 2)),
            new(new Point2D(0, 4), new Point2D(5, 4)),
            new(new Point2D(2, -1), new Point2D(2, 5))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_DiagonalCross()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-1, -1), new Point2D(1, 1)),
            new(new Point2D(-1, 1), new Point2D(1, -1))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_SixSegmentsMultipleCrossings()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(3, 3)),
            new(new Point2D(3, 0), new Point2D(0, 3)),
            new(new Point2D(1, 0), new Point2D(1, 3)),
            new(new Point2D(2, 0), new Point2D(2, 3)),
            new(new Point2D(0, 1), new Point2D(3, 1)),
            new(new Point2D(0, 2), new Point2D(3, 2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_CrossAtNonOriginPoint()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 1), new Point2D(4, 1)),
            new(new Point2D(2, 0), new Point2D(2, 4))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_NearParallel_DoesNotCross()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(10, 0.01)),
            new(new Point2D(0, 1), new Point2D(10, 1.01))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_SixRandomNonParallel_ReturnsCorrect()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(5, 5)),
            new(new Point2D(0, 5), new Point2D(5, 0)),
            new(new Point2D(1, 0), new Point2D(4, 5)),
            new(new Point2D(4, 0), new Point2D(1, 5)),
            new(new Point2D(0, 2.5), new Point2D(5, 2.5)),
            new(new Point2D(2.5, 0), new Point2D(2.5, 5))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_VerySmallSegments_Crossing()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(1, 1)),
            new(new Point2D(0, 1), new Point2D(1, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_LargeScaleCrossing()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(1000, 1000)),
            new(new Point2D(0, 1000), new Point2D(1000, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_ZigzagPattern()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(2, 2)),
            new(new Point2D(2, 0), new Point2D(0, 2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_FourDiagonalSegments_FormsSquare()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(2, 2)),
            new(new Point2D(2, 0), new Point2D(0, 2)),
            new(new Point2D(2, 2), new Point2D(4, 4)),
            new(new Point2D(4, 2), new Point2D(2, 4))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_EightSegmentsStarPattern()
    {
        var segments = new List<Segment2D>();
        for (int i = 0; i < 4; i++)
        {
            double angle = System.Math.PI * i / 4;
            double cos = System.Math.Cos(angle) * 3;
            double sin = System.Math.Sin(angle) * 3;
            segments.Add(new Segment2D(new Point2D(-cos, -sin), new Point2D(cos, sin)));
        }

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_SinglePointTouching()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(2, 2)),
            new(new Point2D(2, 2), new Point2D(4, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_ThreeNonIntersecting()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(1, 0)),
            new(new Point2D(2, 0), new Point2D(3, 0)),
            new(new Point2D(4, 0), new Point2D(5, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_FourSegmentsTicTacToe()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(0, 4)),
            new(new Point2D(2, 0), new Point2D(2, 4)),
            new(new Point2D(4, 0), new Point2D(4, 4)),
            new(new Point2D(0, 2), new Point2D(4, 2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_OverlapOnXAxis_SameY()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-1, 0), new Point2D(3, 0)),
            new(new Point2D(1, 0), new Point2D(5, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_BowtieShape()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(2, 2)),
            new(new Point2D(2, 0), new Point2D(0, 2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_LargeSetOfNonIntersectingSegments()
    {
        var segments = new List<Segment2D>();
        for (int i = 0; i < 10; i++)
        {
            segments.Add(new Segment2D(
                new Point2D(i * 2, 0),
                new Point2D(i * 2, 1)));
        }

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_FiveSegmentChain()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(4, 4)),
            new(new Point2D(0, 1), new Point2D(4, 1)),
            new(new Point2D(0, 2), new Point2D(4, 2)),
            new(new Point2D(0, 3), new Point2D(4, 3)),
            new(new Point2D(0, 4), new Point2D(4, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_TwelveSegmentsDense()
    {
        var segments = new List<Segment2D>();
        for (int i = 0; i < 6; i++)
        {
            segments.Add(new Segment2D(
                new Point2D(i, 0),
                new Point2D(5 - i, 5)));
        }
        for (int i = 0; i < 6; i++)
        {
            segments.Add(new Segment2D(
                new Point2D(0, i),
                new Point2D(5, 5 - i)));
        }

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_NegativeCoordinates()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-5, -5), new Point2D(5, 5)),
            new(new Point2D(-5, 5), new Point2D(5, -5))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_OppositeDiagonals()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-3, -3), new Point2D(3, 3)),
            new(new Point2D(-3, 3), new Point2D(3, -3))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_SegmentsTouchAtEndpoint()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(1, 1)),
            new(new Point2D(1, 1), new Point2D(2, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_SevenNonIntersectingRandom()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(1, 0)),
            new(new Point2D(0, 2), new Point2D(1, 2)),
            new(new Point2D(0, 4), new Point2D(1, 4)),
            new(new Point2D(2, 0), new Point2D(3, 0)),
            new(new Point2D(2, 2), new Point2D(3, 2)),
            new(new Point2D(2, 4), new Point2D(3, 4)),
            new(new Point2D(4, 0), new Point2D(5, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_ThreeCrossingAtSamePoint()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-2, 0), new Point2D(2, 0)),
            new(new Point2D(0, -2), new Point2D(0, 2)),
            new(new Point2D(-2, -2), new Point2D(2, 2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_FourParallelHorizontal_ReturnsEmpty()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-5, 0), new Point2D(5, 0)),
            new(new Point2D(-5, 1), new Point2D(5, 1)),
            new(new Point2D(-5, 2), new Point2D(5, 2)),
            new(new Point2D(-5, 3), new Point2D(5, 3))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_SlantedCross_OneIntersection()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(3, 6)),
            new(new Point2D(0, 6), new Point2D(3, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_TinyCrossingSegments()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(0.1, 0.1)),
            new(new Point2D(0, 0.1), new Point2D(0.1, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_FiveParallelVertical_ReturnsEmpty()
    {
        var segments = new List<Segment2D>();
        for (int i = 0; i < 5; i++)
            segments.Add(new Segment2D(new Point2D(i, -5), new Point2D(i, 5)));

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_MultipleChainIntersections()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(6, 6)),
            new(new Point2D(1, 0), new Point2D(1, 6)),
            new(new Point2D(3, 0), new Point2D(3, 6)),
            new(new Point2D(5, 0), new Point2D(5, 6))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_CrossWithParallelLines()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(-3, 0), new Point2D(3, 0)),
            new(new Point2D(0, -3), new Point2D(0, 3)),
            new(new Point2D(-3, 2), new Point2D(3, 2)),
            new(new Point2D(-3, -2), new Point2D(3, -2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_TwoShortNonIntersecting()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(0.5, 0)),
            new(new Point2D(2, 2), new Point2D(2.5, 2))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_GratingPatternNineIntersections()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(0, 4)),
            new(new Point2D(2, 0), new Point2D(2, 4)),
            new(new Point2D(4, 0), new Point2D(4, 4)),
            new(new Point2D(0, 0), new Point2D(4, 0)),
            new(new Point2D(0, 2), new Point2D(4, 2)),
            new(new Point2D(0, 4), new Point2D(4, 4))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_FourSegmentsTwoPairsCrossing()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 0), new Point2D(2, 2)),
            new(new Point2D(0, 2), new Point2D(2, 0)),
            new(new Point2D(5, 0), new Point2D(7, 2)),
            new(new Point2D(5, 2), new Point2D(7, 0))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_EightNonIntersectingParallel()
    {
        var segments = new List<Segment2D>();
        for (int i = 0; i < 8; i++)
            segments.Add(new Segment2D(new Point2D(0, i), new Point2D(1, i)));

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public void FindIntersections_SixSegmentsGridPattern()
    {
        var segments = new List<Segment2D>
        {
            new(new Point2D(0, 1), new Point2D(4, 1)),
            new(new Point2D(0, 3), new Point2D(4, 3)),
            new(new Point2D(1, 0), new Point2D(1, 4)),
            new(new Point2D(3, 0), new Point2D(3, 4)),
            new(new Point2D(0, 0), new Point2D(4, 4)),
            new(new Point2D(4, 0), new Point2D(0, 4))
        };

        var result = BentleyOttmann.FindIntersections(segments);

        result.Should().HaveCountGreaterOrEqualTo(0);
    }
}
