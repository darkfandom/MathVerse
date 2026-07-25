namespace MathVerse.Math.Geometry.CoordinateSystems;

/// <summary>Represents a geographic coordinate using latitude, longitude, and optional altitude (WGS84).</summary>
public readonly record struct GeographicCoordinate(double Latitude, double Longitude, double Altitude)
{
    /// <summary>Earth's mean radius in meters (WGS84).</summary>
    public const double EarthRadiusMeters = 6371008.8;

    /// <summary>The latitude in degrees (-90 to 90).</summary>
    public double Latitude { get; } = Latitude;

    /// <summary>The longitude in degrees (-180 to 180).</summary>
    public double Longitude { get; } = Longitude;

    /// <summary>The altitude above mean sea level in meters.</summary>
    public double Altitude { get; } = Altitude;

    /// <summary>Creates a geographic coordinate from latitude and longitude only.</summary>
    public static GeographicCoordinate FromLatLon(double lat, double lon) => new(lat, lon, 0);

    /// <summary>Converts to 3D Cartesian coordinates (ECEF - Earth-Centered, Earth-Fixed).</summary>
    public CartesianCoordinate ToCartesian()
    {
        double latRad = Latitude * System.Math.PI / 180.0;
        double lonRad = Longitude * System.Math.PI / 180.0;
        double r = EarthRadiusMeters + Altitude;
        double cosLat = System.Math.Cos(latRad);
        return new CartesianCoordinate(
            r * cosLat * System.Math.Cos(lonRad),
            r * System.Math.Sin(latRad),
            r * cosLat * System.Math.Sin(lonRad));
    }

    /// <summary>Computes the great-circle distance to another geographic coordinate using the Haversine formula.</summary>
    /// <param name="other">The target coordinate.</param>
    /// <returns>The distance in meters.</returns>
    public double DistanceTo(GeographicCoordinate other)
    {
        double lat1 = Latitude * System.Math.PI / 180.0;
        double lat2 = other.Latitude * System.Math.PI / 180.0;
        double dLat = (other.Latitude - Latitude) * System.Math.PI / 180.0;
        double dLon = (other.Longitude - Longitude) * System.Math.PI / 180.0;

        double a = System.Math.Sin(dLat * 0.5) * System.Math.Sin(dLat * 0.5) +
                   System.Math.Cos(lat1) * System.Math.Cos(lat2) *
                   System.Math.Sin(dLon * 0.5) * System.Math.Sin(dLon * 0.5);
        double c = 2.0 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1.0 - a));
        return EarthRadiusMeters * c;
    }

    /// <summary>Computes the initial bearing (forward azimuth) to another coordinate in degrees.</summary>
    public double BearingTo(GeographicCoordinate other)
    {
        double lat1 = Latitude * System.Math.PI / 180.0;
        double lat2 = other.Latitude * System.Math.PI / 180.0;
        double dLon = (other.Longitude - Longitude) * System.Math.PI / 180.0;

        double y = System.Math.Sin(dLon) * System.Math.Cos(lat2);
        double x = System.Math.Cos(lat1) * System.Math.Sin(lat2) -
                   System.Math.Sin(lat1) * System.Math.Cos(lat2) * System.Math.Cos(dLon);
        double bearing = System.Math.Atan2(y, x) * 180.0 / System.Math.PI;
        return (bearing + 360.0) % 360.0;
    }

    /// <summary>Returns a string representation.</summary>
    public override string ToString() => $"({Latitude:F6}, {Longitude:F6}, {Altitude:F1}m)";
}
