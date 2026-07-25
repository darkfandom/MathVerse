namespace MathVerse.Math.Distributed.GeometryParallelism
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel boolean operations on triangle meshes, supporting union, intersection,
    /// and difference operations by parallelizing triangle-triangle intersection tests.
    /// </summary>
    public sealed class ParallelBooleanOps
    {
        /// <summary>
        /// Performs a boolean operation on two triangle meshes in parallel.
        /// Triangle-triangle intersection tests are distributed across threads for performance.
        /// </summary>
        /// <param name="verticesA">Vertices of the first mesh.</param>
        /// <param name="facesA">Faces (triangle indices) of the first mesh.</param>
        /// <param name="verticesB">Vertices of the second mesh.</param>
        /// <param name="facesB">Faces (triangle indices) of the second mesh.</param>
        /// <param name="operation">
        /// Boolean operation type: 0 = union, 1 = intersection, 2 = difference (A - B).
        /// </param>
        /// <returns>
        /// A tuple of (resultVertices, resultFaces) representing the output mesh.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown when meshes are empty or operation is invalid.</exception>
        public static (double[][] vertices, int[][] faces) ComputeBooleanOp(
            double[][] verticesA,
            int[][] facesA,
            double[][] verticesB,
            int[][] facesB,
            int operation)
        {
            if (verticesA == null) throw new ArgumentNullException(nameof(verticesA));
            if (facesA == null) throw new ArgumentNullException(nameof(facesA));
            if (verticesB == null) throw new ArgumentNullException(nameof(verticesB));
            if (facesB == null) throw new ArgumentNullException(nameof(facesB));
            if (verticesA.Length == 0) throw new ArgumentException("Mesh A vertices must not be empty.", nameof(verticesA));
            if (facesA.Length == 0) throw new ArgumentException("Mesh A faces must not be empty.", nameof(facesA));
            if (verticesB.Length == 0) throw new ArgumentException("Mesh B vertices must not be empty.", nameof(verticesB));
            if (facesB.Length == 0) throw new ArgumentException("Mesh B faces must not be empty.", nameof(facesB));
            if (operation < 0 || operation > 2)
                throw new ArgumentException("Operation must be 0 (union), 1 (intersection), or 2 (difference).", nameof(operation));

            // Precompute triangle bounding boxes for mesh B
            AABBox[] boxesB = new AABBox[facesB.Length];
            for (int f = 0; f < facesB.Length; f++)
            {
                boxesB[f] = ComputeBoundingBox(verticesB, facesB[f]);
            }

            // For each face in A, test intersection with all faces in B in parallel
            bool[] faceAIntersects = new bool[facesA.Length];
            List<int>[] intersectingPairsA = new List<int>[facesA.Length];

            Parallel.For(0, facesA.Length, fA =>
            {
                intersectingPairsA[fA] = new List<int>();
                AABBox boxA = ComputeBoundingBox(verticesA, facesA[fA]);

                for (int fB = 0; fB < facesB.Length; fB++)
                {
                    if (AABBoxOverlap(boxA, boxesB[fB]))
                    {
                        if (TrianglesIntersect(verticesA, facesA[fA], verticesB, facesB[fB]))
                        {
                            faceAIntersects[fA] = true;
                            intersectingPairsA[fA].Add(fB);
                        }
                    }
                }
            });

            // Classify faces based on operation
            List<double[]> resultVertices = new List<double[]>();
            List<int[]> resultFaces = new List<int[]>();

            // Add all vertices from both meshes
            int offsetA = 0;
            int offsetB = verticesA.Length;

            for (int i = 0; i < verticesA.Length; i++)
            {
                resultVertices.Add(new double[] { verticesA[i][0], verticesA[i][1], verticesA[i][2] });
            }
            for (int i = 0; i < verticesB.Length; i++)
            {
                resultVertices.Add(new double[] { verticesB[i][0], verticesB[i][1], verticesB[i][2] });
            }

            switch (operation)
            {
                case 0: // Union: include non-intersecting faces from both
                    for (int f = 0; f < facesA.Length; f++)
                    {
                        if (!faceAIntersects[f])
                        {
                            int[] remapped = RemapFace(facesA[f], offsetA);
                            resultFaces.Add(remapped);
                        }
                    }
                    for (int f = 0; f < facesB.Length; f++)
                    {
                        bool intersectsAny = false;
                        for (int fA = 0; fA < facesA.Length; fA++)
                        {
                            if (intersectingPairsA[fA].Contains(f))
                            {
                                intersectsAny = true;
                                break;
                            }
                        }
                        if (!intersectsAny)
                        {
                            int[] remapped = RemapFace(facesB[f], offsetB);
                            resultFaces.Add(remapped);
                        }
                    }
                    break;

                case 1: // Intersection: include intersecting faces from both
                    for (int f = 0; f < facesA.Length; f++)
                    {
                        if (faceAIntersects[f])
                        {
                            int[] remapped = RemapFace(facesA[f], offsetA);
                            resultFaces.Add(remapped);
                        }
                    }
                    for (int f = 0; f < facesB.Length; f++)
                    {
                        bool isIncluded = false;
                        for (int fA = 0; fA < facesA.Length; fA++)
                        {
                            if (intersectingPairsA[fA].Contains(f))
                            {
                                isIncluded = true;
                                break;
                            }
                        }
                        if (isIncluded)
                        {
                            int[] remapped = RemapFace(facesB[f], offsetB);
                            resultFaces.Add(remapped);
                        }
                    }
                    break;

                case 2: // Difference (A - B): include non-intersecting A faces
                    for (int f = 0; f < facesA.Length; f++)
                    {
                        if (!faceAIntersects[f])
                        {
                            int[] remapped = RemapFace(facesA[f], offsetA);
                            resultFaces.Add(remapped);
                        }
                    }
                    break;
            }

            return (resultVertices.ToArray(), resultFaces.ToArray());
        }

        /// <summary>
        /// Remaps face vertex indices by adding an offset.
        /// </summary>
        private static int[] RemapFace(int[] face, int offset)
        {
            int[] remapped = new int[face.Length];
            for (int i = 0; i < face.Length; i++)
            {
                remapped[i] = face[i] + offset;
            }
            return remapped;
        }

        /// <summary>
        /// Computes an axis-aligned bounding box for a triangle.
        /// </summary>
        private static AABBox ComputeBoundingBox(double[][] vertices, int[] face)
        {
            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            for (int i = 0; i < face.Length; i++)
            {
                double[] v = vertices[face[i]];
                if (v[0] < minX) minX = v[0];
                if (v[1] < minY) minY = v[1];
                if (v[2] < minZ) minZ = v[2];
                if (v[0] > maxX) maxX = v[0];
                if (v[1] > maxY) maxY = v[1];
                if (v[2] > maxZ) maxZ = v[2];
            }

            return new AABBox { MinX = minX, MinY = minY, MinZ = minZ, MaxX = maxX, MaxY = maxY, MaxZ = maxZ };
        }

        /// <summary>
        /// Tests overlap between two axis-aligned bounding boxes.
        /// </summary>
        private static bool AABBoxOverlap(AABBox a, AABBox b)
        {
            return a.MinX <= b.MaxX && a.MaxX >= b.MinX
                && a.MinY <= b.MaxY && a.MaxY >= b.MinY
                && a.MinZ <= b.MaxZ && a.MaxZ >= b.MinZ;
        }

        /// <summary>
        /// Tests whether two triangles intersect using a separating axis approach.
        /// </summary>
        private static bool TrianglesIntersect(
            double[][] vertsA, int[] faceA,
            double[][] vertsB, int[] faceB)
        {
            double[] a0 = vertsA[faceA[0]];
            double[] a1 = vertsA[faceA[1]];
            double[] a2 = vertsA[faceA[2]];
            double[] b0 = vertsB[faceB[0]];
            double[] b1 = vertsB[faceB[1]];
            double[] b2 = vertsB[faceB[2]];

            // Compute edge vectors
            double[] aE1 = Sub(a1, a0);
            double[] aE2 = Sub(a2, a0);
            double[] bE1 = Sub(b1, b0);
            double[] bE2 = Sub(b2, b0);

            // Test separating axes
            double[][] axes = new double[9][];
            axes[0] = Cross(aE1, aE2);
            axes[1] = Cross(bE1, bE2);
            axes[2] = Cross(aE1, bE1);
            axes[3] = Cross(aE1, bE2);
            axes[4] = Cross(aE2, bE1);
            axes[5] = Cross(aE2, bE2);
            axes[6] = Cross(Cross(aE1, aE2), Cross(bE1, bE2));
            axes[7] = Cross(aE1, Sub(b0, a0));
            axes[8] = Cross(aE2, Sub(b0, a0));

            for (int i = 0; i < axes.Length; i++)
            {
                if (Magnitude(axes[i]) < 1e-10) continue;

                if (!ProjectsOverlap(axes[i], a0, a1, a2, b0, b1, b2))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tests whether two triangles' projections onto an axis overlap.
        /// </summary>
        private static bool ProjectsOverlap(
            double[] axis,
            double[] a0, double[] a1, double[] a2,
            double[] b0, double[] b1, double[] b2)
        {
            double aMin = Dot(a0, axis);
            double aMax = aMin;
            double val;

            val = Dot(a1, axis); if (val < aMin) aMin = val; else if (val > aMax) aMax = val;
            val = Dot(a2, axis); if (val < aMin) aMin = val; else if (val > aMax) aMax = val;

            double bMin = Dot(b0, axis);
            double bMax = bMin;

            val = Dot(b1, axis); if (val < bMin) bMin = val; else if (val > bMax) bMax = val;
            val = Dot(b2, axis); if (val < bMin) bMin = val; else if (val > bMax) bMax = val;

            return aMax >= bMin && bMax >= aMin;
        }

        private static double[] Sub(double[] a, double[] b)
        {
            return new double[] { a[0] - b[0], a[1] - b[1], a[2] - b[2] };
        }

        private static double[] Cross(double[] a, double[] b)
        {
            return new double[]
            {
                a[1] * b[2] - a[2] * b[1],
                a[2] * b[0] - a[0] * b[2],
                a[0] * b[1] - a[1] * b[0]
            };
        }

        private static double Dot(double[] a, double[] b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        private static double Magnitude(double[] v)
        {
            return System.Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        }

        /// <summary>
        /// Represents an axis-aligned bounding box.
        /// </summary>
        private struct AABBox
        {
            public double MinX, MinY, MinZ;
            public double MaxX, MaxY, MaxZ;
        }
    }
}
