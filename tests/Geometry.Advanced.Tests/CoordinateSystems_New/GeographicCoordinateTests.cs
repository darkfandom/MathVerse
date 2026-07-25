namespace MathVerse.Geometry.Advanced.Tests.CoordinateSystems_New;

public class GeographicCoordinateTests
{
    [Fact]
    public void FromLatLon_SetsAltitudeToZero()
    {
        var coord = GeographicCoordinate.FromLatLon(51.5074, -0.1278);
        coord.Altitude.Should().Be(0);
    }

    [Fact]
    public void FromLatLon_SetsLatitudeAndLongitude()
    {
        var coord = GeographicCoordinate.FromLatLon(48.8566, 2.3522);
        coord.Latitude.Should().Be(48.8566);
        coord.Longitude.Should().Be(2.3522);
    }

    [Fact]
    public void Constructor_WithAltitude_SetsAllProperties()
    {
        var coord = new GeographicCoordinate(40.0, -74.0, 150.0);
        coord.Latitude.Should().Be(40.0);
        coord.Longitude.Should().Be(-74.0);
        coord.Altitude.Should().Be(150.0);
    }

    [Fact]
    public void EarthRadiusMeters_IsWGS84MeanRadius()
    {
        GeographicCoordinate.EarthRadiusMeters.Should().Be(6371008.8);
    }

    [Fact]
    public void ToCartesian_EquatorPrimeMeridian_IsOnXAxis()
    {
        var coord = GeographicCoordinate.FromLatLon(0, 0);
        var cart = coord.ToCartesian();
        cart.X.Should().BeApproximately(GeographicCoordinate.EarthRadiusMeters, 1.0);
        cart.Y.Should().BeApproximately(0, 1.0);
        cart.Z.Should().BeApproximately(0, 1.0);
    }

    [Fact]
    public void ToCartesian_NorthPole_IsOnYAxis()
    {
        var coord = GeographicCoordinate.FromLatLon(90, 0);
        var cart = coord.ToCartesian();
        cart.X.Should().BeApproximately(0, 1.0);
        cart.Y.Should().BeApproximately(GeographicCoordinate.EarthRadiusMeters, 1.0);
        cart.Z.Should().BeApproximately(0, 1.0);
    }

    [Fact]
    public void ToCartesian_Equator90E_IsOnZAxis()
    {
        var coord = GeographicCoordinate.FromLatLon(0, 90);
        var cart = coord.ToCartesian();
        cart.X.Should().BeApproximately(0, 1.0);
        cart.Y.Should().BeApproximately(0, 1.0);
        cart.Z.Should().BeApproximately(GeographicCoordinate.EarthRadiusMeters, 1.0);
    }

    [Fact]
    public void ToCartesian_AltitudeAffectsRadius()
    {
        var low = GeographicCoordinate.FromLatLon(0, 0);
        var high = new GeographicCoordinate(0, 0, 1000);
        var cartLow = low.ToCartesian();
        var cartHigh = high.ToCartesian();
        double distLow = System.Math.Sqrt(cartLow.X * cartLow.X + cartLow.Y * cartLow.Y + cartLow.Z * cartLow.Z);
        double distHigh = System.Math.Sqrt(cartHigh.X * cartHigh.X + cartHigh.Y * cartHigh.Y + cartHigh.Z * cartHigh.Z);
        distHigh.Should().BeGreaterThan(distLow);
        (distHigh - distLow).Should().BeApproximately(1000, 0.1);
    }

    [Fact]
    public void DistanceTo_SamePoint_ReturnsZero()
    {
        var coord = GeographicCoordinate.FromLatLon(51.5074, -0.1278);
        coord.DistanceTo(coord).Should().Be(0);
    }

    [Fact]
    public void DistanceTo_LondonToParis_IsApproximately343km()
    {
        var london = GeographicCoordinate.FromLatLon(51.5074, -0.1278);
        var paris = GeographicCoordinate.FromLatLon(48.8566, 2.3522);
        double distanceMeters = london.DistanceTo(paris);
        double distanceKm = distanceMeters / 1000.0;
        distanceKm.Should().BeApproximately(343, 5);
    }

    [Fact]
    public void DistanceTo_EquatorToPole_IsApproximatelyQuarterCircumference()
    {
        var equator = GeographicCoordinate.FromLatLon(0, 0);
        var pole = GeographicCoordinate.FromLatLon(90, 0);
        double distance = equator.DistanceTo(pole);
        double expected = GeographicCoordinate.EarthRadiusMeters * System.Math.PI / 2.0;
        distance.Should().BeApproximately(expected, 100);
    }

    [Fact]
    public void DistanceTo_IsSymmetric()
    {
        var a = GeographicCoordinate.FromLatLon(51.5074, -0.1278);
        var b = GeographicCoordinate.FromLatLon(48.8566, 2.3522);
        a.DistanceTo(b).Should().BeApproximately(b.DistanceTo(a), 1e-6);
    }

    [Fact]
    public void DistanceTo_SmallDistance_IsAccurate()
    {
        var a = GeographicCoordinate.FromLatLon(0, 0);
        var b = GeographicCoordinate.FromLatLon(0, 1);
        double distance = a.DistanceTo(b);
        double expectedArc = GeographicCoordinate.EarthRadiusMeters * System.Math.PI / 180.0;
        distance.Should().BeApproximately(expectedArc, 100);
    }

    [Fact]
    public void DistanceTo_AntipodalPoints_IsHalfCircumference()
    {
        var a = GeographicCoordinate.FromLatLon(0, 0);
        var b = GeographicCoordinate.FromLatLon(0, 180);
        double distance = a.DistanceTo(b);
        double expected = System.Math.PI * GeographicCoordinate.EarthRadiusMeters;
        distance.Should().BeApproximately(expected, 100);
    }

    [Fact]
    public void BearingTo_North_IsApproximatelyZero()
    {
        var from = GeographicCoordinate.FromLatLon(0, 0);
        var to = GeographicCoordinate.FromLatLon(10, 0);
        from.BearingTo(to).Should().BeApproximately(0, 1);
    }

    [Fact]
    public void BearingTo_East_IsApproximately90()
    {
        var from = GeographicCoordinate.FromLatLon(0, 0);
        var to = GeographicCoordinate.FromLatLon(0, 10);
        from.BearingTo(to).Should().BeApproximately(90, 1);
    }

    [Fact]
    public void BearingTo_South_IsApproximately180()
    {
        var from = GeographicCoordinate.FromLatLon(10, 0);
        var to = GeographicCoordinate.FromLatLon(0, 0);
        from.BearingTo(to).Should().BeApproximately(180, 1);
    }

    [Fact]
    public void BearingTo_West_IsApproximately270()
    {
        var from = GeographicCoordinate.FromLatLon(0, 10);
        var to = GeographicCoordinate.FromLatLon(0, 0);
        from.BearingTo(to).Should().BeApproximately(270, 1);
    }

    [Fact]
    public void BearingTo_LondonToParis_IsNorthEastish()
    {
        var london = GeographicCoordinate.FromLatLon(51.5074, -0.1278);
        var paris = GeographicCoordinate.FromLatLon(48.8566, 2.3522);
        double bearing = london.BearingTo(paris);
        bearing.Should().BeGreaterThan(90);
        bearing.Should().BeLessThan(180);
    }

    [Fact]
    public void BearingTo_SamePoint_IsZero()
    {
        var coord = GeographicCoordinate.FromLatLon(51.5074, -0.1278);
        coord.BearingTo(coord).Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void BearingTo_IsInZeroTo360Range()
    {
        var from = GeographicCoordinate.FromLatLon(30, -70);
        var to = GeographicCoordinate.FromLatLon(-30, 110);
        double bearing = from.BearingTo(to);
        bearing.Should().BeGreaterThanOrEqualTo(0);
        bearing.Should().BeLessThan(360);
    }

    [Fact]
    public void ToString_ContainsLatitudeLongitudeAltitude()
    {
        var coord = new GeographicCoordinate(51.5074, -0.1278, 10.5);
        string s = coord.ToString();
        s.Should().Contain("51.5074");
        s.Should().Contain("-0.1278");
        s.Should().Contain("10.5");
        s.Should().Contain("m");
    }

    [Fact]
    public void ToString_DefaultAltitude_ShowsZeroPointZero()
    {
        var coord = GeographicCoordinate.FromLatLon(10, 20);
        string s = coord.ToString();
        s.Should().Contain("0.0");
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new GeographicCoordinate(51.5074, -0.1278, 0);
        var b = new GeographicCoordinate(51.5074, -0.1278, 0);
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = new GeographicCoordinate(51.5074, -0.1278, 0);
        var b = new GeographicCoordinate(48.8566, 2.3522, 0);
        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentAltitude_AreNotEqual()
    {
        var a = new GeographicCoordinate(51.5074, -0.1278, 0);
        var b = new GeographicCoordinate(51.5074, -0.1278, 100);
        a.Should().NotBe(b);
    }

    [Fact]
    public void DistanceTo_AlwaysReturnsNonNegative()
    {
        var a = GeographicCoordinate.FromLatLon(-89.99, -179.99);
        var b = GeographicCoordinate.FromLatLon(89.99, 179.99);
        a.DistanceTo(b).Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void ToCartesian_MagnitudeMatchesEarthRadius()
    {
        var coord = GeographicCoordinate.FromLatLon(35.6762, 139.6503);
        var cart = coord.ToCartesian();
        double mag = System.Math.Sqrt(cart.X * cart.X + cart.Y * cart.Y + cart.Z * cart.Z);
        mag.Should().BeApproximately(GeographicCoordinate.EarthRadiusMeters, 1.0);
    }

    [Fact]
    public void ToCartesian_WithAltitude_MagnitudeIncludesAltitude()
    {
        double alt = 5000;
        var coord = new GeographicCoordinate(35.6762, 139.6503, alt);
        var cart = coord.ToCartesian();
        double mag = System.Math.Sqrt(cart.X * cart.X + cart.Y * cart.Y + cart.Z * cart.Z);
        mag.Should().BeApproximately(GeographicCoordinate.EarthRadiusMeters + alt, 1.0);
    }

    [Fact]
    public void DistanceTo_MutualConsistency()
    {
        var a = GeographicCoordinate.FromLatLon(40.7128, -74.0060);
        var b = GeographicCoordinate.FromLatLon(34.0522, -118.2437);
        double ab = a.DistanceTo(b);
        double ba = b.DistanceTo(a);
        ab.Should().Be(ba);
    }

    [Fact]
    public void BearingTo_NYCtoLA_IsSouthWest()
    {
        var nyc = GeographicCoordinate.FromLatLon(40.7128, -74.0060);
        var la = GeographicCoordinate.FromLatLon(34.0522, -118.2437);
        double bearing = nyc.BearingTo(la);
        bearing.Should().BeGreaterThan(180);
        bearing.Should().BeLessThan(280);
    }

    [Fact]
    public void DistanceTo_LargeAltitude_DoesNotAffectHaversine()
    {
        var a = GeographicCoordinate.FromLatLon(0, 0);
        var b = GeographicCoordinate.FromLatLon(0, 10);
        var aHigh = new GeographicCoordinate(0, 0, 50000);
        a.DistanceTo(b).Should().BeApproximately(aHigh.DistanceTo(b), 1e-6);
    }

    [Fact]
    public void FromLatLon_NegativeCoordinates_Works()
    {
        var coord = GeographicCoordinate.FromLatLon(-33.8688, 151.2093);
        coord.Latitude.Should().Be(-33.8688);
        coord.Longitude.Should().Be(151.2093);
    }

    [Fact]
    public void FromLatLon_MaxValues_Works()
    {
        var coord = GeographicCoordinate.FromLatLon(90, 180);
        coord.Latitude.Should().Be(90);
        coord.Longitude.Should().Be(180);
    }

    [Fact]
    public void FromLatLon_MinValues_Works()
    {
        var coord = GeographicCoordinate.FromLatLon(-90, -180);
        coord.Latitude.Should().Be(-90);
        coord.Longitude.Should().Be(-180);
    }

    [Fact]
    public void DistanceTo_TokyoToSydney_IsApproximately7800km()
    {
        var tokyo = GeographicCoordinate.FromLatLon(35.6762, 139.6503);
        var sydney = GeographicCoordinate.FromLatLon(-33.8688, 151.2093);
        double distanceKm = tokyo.DistanceTo(sydney) / 1000.0;
        distanceKm.Should().BeApproximately(7825, 50);
    }

    [Fact]
    public void BearingTo_Reversal_DiffersBy180()
    {
        var a = GeographicCoordinate.FromLatLon(0, 0);
        var b = GeographicCoordinate.FromLatLon(10, 10);
        double forward = a.BearingTo(b);
        double reverse = b.BearingTo(a);
        double diff = System.Math.Abs(forward - reverse);
        System.Math.Min(diff, 360 - diff).Should().BeApproximately(180, 1);
    }

    [Fact]
    public void DistanceTo_EquatorAlongEquator_IsProportional()
    {
        var a = GeographicCoordinate.FromLatLon(0, 0);
        var b1 = GeographicCoordinate.FromLatLon(0, 1);
        var b2 = GeographicCoordinate.FromLatLon(0, 2);
        double d1 = a.DistanceTo(b1);
        double d2 = a.DistanceTo(b2);
        d2.Should().BeApproximately(2 * d1, 100);
    }

    [Fact]
    public void Equality_Deconstruction_Works()
    {
        var coord = new GeographicCoordinate(1.0, 2.0, 3.0);
        (double lat, double lon, double alt) = coord;
        lat.Should().Be(1.0);
        lon.Should().Be(2.0);
        alt.Should().Be(3.0);
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var a = new GeographicCoordinate(1, 2, 3);
        var b = new GeographicCoordinate(1, 2, 3);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void DistanceTo_ZeroAltitudeConsistent()
    {
        var a = new GeographicCoordinate(48.8566, 2.3522, 0);
        var b = new GeographicCoordinate(51.5074, -0.1278, 0);
        var aFromLatLon = GeographicCoordinate.FromLatLon(48.8566, 2.3522);
        a.DistanceTo(b).Should().BeApproximately(aFromLatLon.DistanceTo(b), 1e-6);
    }

    [Fact]
    public void ToString_NegativeLatitude_ShowsNegative()
    {
        var coord = GeographicCoordinate.FromLatLon(-33.8688, 151.2093);
        coord.ToString().Should().Contain("-33.8688");
    }

    [Fact]
    public void ToCartesian_PrimeMeridian90N_IsOnZAxis()
    {
        var coord = GeographicCoordinate.FromLatLon(0, -90);
        var cart = coord.ToCartesian();
        cart.Z.Should().BeApproximately(-GeographicCoordinate.EarthRadiusMeters, 1.0);
    }

    [Fact]
    public void BearingTo_DueNorthAtPole_IsUndefined_ButReturnsValue()
    {
        var pole = GeographicCoordinate.FromLatLon(89.999, 0);
        var nearPole = GeographicCoordinate.FromLatLon(90, 0);
        double bearing = pole.BearingTo(nearPole);
        bearing.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void DistanceTo_CrossDateLine_Works()
    {
        var a = GeographicCoordinate.FromLatLon(0, 179);
        var b = GeographicCoordinate.FromLatLon(0, -179);
        double dist = a.DistanceTo(b);
        dist.Should().BeGreaterThan(0);
        dist.Should().BeLessThan(1000000);
    }

    [Fact]
    public void ToString_FormatsAltitudeToOneDecimal()
    {
        var coord = new GeographicCoordinate(1, 2, 123.456);
        coord.ToString().Should().Contain("123.5");
    }

    [Fact]
    public void ToCartesian_NegativeLongitude_IsOnNegativeZSide()
    {
        var coord = GeographicCoordinate.FromLatLon(0, -90);
        var cart = coord.ToCartesian();
        cart.Z.Should().BeLessThan(0);
    }

    [Fact]
    public void DistanceTo_CapetownToCairo_IsApproximately6400km()
    {
        var capetown = GeographicCoordinate.FromLatLon(-33.9249, 18.4241);
        var cairo = GeographicCoordinate.FromLatLon(30.0444, 31.2357);
        double distanceKm = capetown.DistanceTo(cairo) / 1000.0;
        distanceKm.Should().BeApproximately(7267, 100);
    }

    [Fact]
    public void BearingTo_DueEast_IsApproximately90()
    {
        var from = GeographicCoordinate.FromLatLon(45, 0);
        var to = GeographicCoordinate.FromLatLon(45, 10);
        from.BearingTo(to).Should().BeApproximately(90, 5);
    }

    [Fact]
    public void Constructor_AllZero_CoordinateWorks()
    {
        var coord = new GeographicCoordinate(0, 0, 0);
        coord.Latitude.Should().Be(0);
        coord.Longitude.Should().Be(0);
        coord.Altitude.Should().Be(0);
    }
}
