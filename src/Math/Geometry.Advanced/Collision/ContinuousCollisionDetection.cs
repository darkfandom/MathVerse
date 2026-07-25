using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Collision;

/// <summary>
/// Represents the result of a continuous collision detection test.
/// </summary>
/// <param name="Hit">Whether a collision occurs within the time window.</param>
/// <param name="TimeOfImpact">The normalized time of impact (0 to 1).</param>
/// <param name="ContactPoint">The point of contact at time of impact.</param>
public readonly record struct CCDResult(bool Hit, double TimeOfImpact, Point3D ContactPoint);

/// <summary>
/// Provides continuous collision detection (CCD) to prevent tunneling through fast-moving objects.
/// </summary>
public static class ContinuousCollisionDetection
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Sweeps a sphere against another sphere to find the time of impact.
    /// </summary>
    /// <param name="a">The first sphere.</param>
    /// <param name="velA">The velocity of the first sphere.</param>
    /// <param name="b">The second sphere.</param>
    /// <param name="velB">The velocity of the second sphere.</param>
    /// <param name="maxTime">The maximum time to search (typically 1.0 for one frame).</param>
    /// <returns>A <see cref="CCDResult"/> indicating whether and when the spheres collide.</returns>
    public static CCDResult SweepSphereSphere(Sphere3D a, Vector3D velA, Sphere3D b, Vector3D velB, double maxTime)
    {
        Vector3D dPos = new Vector3D(
            b.Center.X - a.Center.X,
            b.Center.Y - a.Center.Y,
            b.Center.Z - a.Center.Z
        );

        Vector3D dVel = new Vector3D(
            velB.X - velA.X,
            velB.Y - velA.Y,
            velB.Z - velA.Z
        );

        double radiiSum = a.Radius + b.Radius;

        double a_coeff = dVel.X * dVel.X + dVel.Y * dVel.Y + dVel.Z * dVel.Z;
        double b_coeff = 2.0 * (dPos.X * dVel.X + dPos.Y * dVel.Y + dPos.Z * dVel.Z);
        double c_coeff = dPos.X * dPos.X + dPos.Y * dPos.Y + dPos.Z * dPos.Z - radiiSum * radiiSum;

        if (a_coeff < Tolerance)
        {
            if (c_coeff <= 0)
            {
                return new CCDResult(true, 0, a.Center);
            }

            return new CCDResult(false, 0, default);
        }

        double discriminant = b_coeff * b_coeff - 4.0 * a_coeff * c_coeff;

        if (discriminant < -Tolerance)
        {
            return new CCDResult(false, 0, default);
        }

        if (discriminant < 0)
        {
            discriminant = 0;
        }

        double sqrtDisc = System.Math.Sqrt(discriminant);
        double t = (-b_coeff - sqrtDisc) / (2.0 * a_coeff);

        if (t < 0)
        {
            t = (-b_coeff + sqrtDisc) / (2.0 * a_coeff);
        }

        if (t < 0 || t > maxTime)
        {
            return new CCDResult(false, 0, default);
        }

        Point3D contact = new Point3D(
            (a.Center.X + velA.X * t + b.Center.X + velB.X * t) * 0.5,
            (a.Center.Y + velA.Y * t + b.Center.Y + velB.Y * t) * 0.5,
            (a.Center.Z + velA.Z * t + b.Center.Z + velB.Z * t) * 0.5
        );

        return new CCDResult(true, t, contact);
    }

    /// <summary>
    /// Sweeps an axis-aligned bounding box against a stationary bounding box.
    /// </summary>
    /// <param name="moving">The moving bounding box.</param>
    /// <param name="velocity">The velocity of the moving bounding box.</param>
    /// <param name="stationary">The stationary bounding box.</param>
    /// <param name="maxTime">The maximum time to search (typically 1.0 for one frame).</param>
    /// <returns>A <see cref="CCDResult"/> indicating whether and when the boxes collide.</returns>
    public static CCDResult SweepAABB(BoundingBox3D moving, Vector3D velocity, BoundingBox3D stationary, double maxTime)
    {
        if (velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z < Tolerance)
        {
            bool overlap = moving.Min.X <= stationary.Max.X && moving.Max.X >= stationary.Min.X &&
                           moving.Min.Y <= stationary.Max.Y && moving.Max.Y >= stationary.Min.Y &&
                           moving.Min.Z <= stationary.Max.Z && moving.Max.Z >= stationary.Min.Z;

            if (overlap)
            {
                Point3D movingCenter = new Point3D(
                    (moving.Min.X + moving.Max.X) * 0.5,
                    (moving.Min.Y + moving.Max.Y) * 0.5,
                    (moving.Min.Z + moving.Max.Z) * 0.5
                );
                return new CCDResult(true, 0, movingCenter);
            }

            return new CCDResult(false, 0, default);
        }

        double tEntry = 0;
        double tExit = 1;

        if (velocity.X > Tolerance)
        {
            double t1 = (moving.Min.X - stationary.Max.X) / velocity.X;
            double t2 = (moving.Max.X - stationary.Min.X) / velocity.X;
            tEntry = System.Math.Max(tEntry, t1);
            tExit = System.Math.Min(tExit, t2);
        }
        else if (velocity.X < -Tolerance)
        {
            double t1 = (moving.Max.X - stationary.Min.X) / velocity.X;
            double t2 = (moving.Min.X - stationary.Max.X) / velocity.X;
            tEntry = System.Math.Max(tEntry, t1);
            tExit = System.Math.Min(tExit, t2);
        }
        else
        {
            if (moving.Min.X > stationary.Max.X || moving.Max.X < stationary.Min.X)
            {
                return new CCDResult(false, 0, default);
            }
        }

        if (tEntry > tExit)
        {
            return new CCDResult(false, 0, default);
        }

        if (velocity.Y > Tolerance)
        {
            double t1 = (moving.Min.Y - stationary.Max.Y) / velocity.Y;
            double t2 = (moving.Max.Y - stationary.Min.Y) / velocity.Y;
            tEntry = System.Math.Max(tEntry, t1);
            tExit = System.Math.Min(tExit, t2);
        }
        else if (velocity.Y < -Tolerance)
        {
            double t1 = (moving.Max.Y - stationary.Min.Y) / velocity.Y;
            double t2 = (moving.Min.Y - stationary.Max.Y) / velocity.Y;
            tEntry = System.Math.Max(tEntry, t1);
            tExit = System.Math.Min(tExit, t2);
        }
        else
        {
            if (moving.Min.Y > stationary.Max.Y || moving.Max.Y < stationary.Min.Y)
            {
                return new CCDResult(false, 0, default);
            }
        }

        if (tEntry > tExit)
        {
            return new CCDResult(false, 0, default);
        }

        if (velocity.Z > Tolerance)
        {
            double t1 = (moving.Min.Z - stationary.Max.Z) / velocity.Z;
            double t2 = (moving.Max.Z - stationary.Min.Z) / velocity.Z;
            tEntry = System.Math.Max(tEntry, t1);
            tExit = System.Math.Min(tExit, t2);
        }
        else if (velocity.Z < -Tolerance)
        {
            double t1 = (moving.Max.Z - stationary.Min.Z) / velocity.Z;
            double t2 = (moving.Min.Z - stationary.Max.Z) / velocity.Z;
            tEntry = System.Math.Max(tEntry, t1);
            tExit = System.Math.Min(tExit, t2);
        }
        else
        {
            if (moving.Min.Z > stationary.Max.Z || moving.Max.Z < stationary.Min.Z)
            {
                return new CCDResult(false, 0, default);
            }
        }

        if (tEntry > tExit || tEntry > maxTime || tExit < 0)
        {
            return new CCDResult(false, 0, default);
        }

        double impactTime = System.Math.Max(0, tEntry);

        Vector3D movingCenterVel = new Vector3D(
            (moving.Min.X + moving.Max.X) * 0.5 + velocity.X * impactTime,
            (moving.Min.Y + moving.Max.Y) * 0.5 + velocity.Y * impactTime,
            (moving.Min.Z + moving.Max.Z) * 0.5 + velocity.Z * impactTime
        );

        Point3D stationaryCenter = new Point3D(
            (stationary.Min.X + stationary.Max.X) * 0.5,
            (stationary.Min.Y + stationary.Max.Y) * 0.5,
            (stationary.Min.Z + stationary.Max.Z) * 0.5
        );

        Point3D contact = new Point3D(
            (movingCenterVel.X + stationaryCenter.X) * 0.5,
            (movingCenterVel.Y + stationaryCenter.Y) * 0.5,
            (movingCenterVel.Z + stationaryCenter.Z) * 0.5
        );

        return new CCDResult(true, impactTime, contact);
    }
}