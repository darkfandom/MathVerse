namespace MathVerse.Math.Distributed.VisualizationPipeline
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a node in a visualization scene graph with children and a type tag.
    /// </summary>
    public sealed class VisualizationObject
    {
        /// <summary>Gets or sets the object type identifier.</summary>
        public string ObjectType { get; set; } = string.Empty;

        /// <summary>Gets or sets the object label.</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>Gets or sets the 3D transform position [x, y, z].</summary>
        public double[] Position { get; set; } = Array.Empty<double>();

        /// <summary>Gets or sets the transformation matrix (4x4 flattened row-major).</summary>
        public double[] TransformMatrix { get; set; } = Array.Empty<double>();

        /// <summary>Gets or sets whether this object is visible.</summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>Gets or sets the user-defined metadata tag.</summary>
        public int Tag { get; set; }
    }

    /// <summary>
    /// Represents a scene graph with a root object and child objects organized hierarchically.
    /// </summary>
    public sealed class VisualizationScene
    {
        /// <summary>Gets or sets the root scene object.</summary>
        public VisualizationObject Root { get; set; } = new VisualizationObject();

        /// <summary>Gets or sets the direct children of the root object.</summary>
        public VisualizationObject[] Children { get; set; } = Array.Empty<VisualizationObject>();
    }

    /// <summary>
    /// Parallel scene graph traversal that visits objects by processing root children
    /// concurrently using parallel foreach.
    /// </summary>
    public sealed class ParallelSceneTraversal
    {
        /// <summary>
        /// Traverses the scene graph in parallel by processing each root child independently.
        /// The visitor action is applied to the root first, then to all children concurrently.
        /// </summary>
        /// <param name="scene">The scene graph to traverse.</param>
        /// <param name="visitor">
        /// Action to apply to each visited object.
        /// Signature: (VisualizationObject obj) -> void.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="scene"/> or <paramref name="visitor"/> is null.
        /// </exception>
        public static void Traverse(VisualizationScene scene, Action<VisualizationObject> visitor)
        {
            if (scene == null) throw new ArgumentNullException(nameof(scene));
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));

            // Visit root
            visitor(scene.Root);

            // Visit children in parallel
            if (scene.Children != null && scene.Children.Length > 0)
            {
                Parallel.ForEach(scene.Children, child =>
                {
                    visitor(child);
                });
            }
        }

        /// <summary>
        /// Traverses the scene graph recursively in parallel, applying the visitor to
        /// each node and all its descendants. Children of the same parent are processed
        /// concurrently.
        /// </summary>
        /// <param name="scene">The scene graph to traverse.</param>
        /// <param name="visitor">
        /// Action to apply to each visited object.
        /// Signature: (VisualizationObject obj) -> void.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="scene"/> or <paramref name="visitor"/> is null.
        /// </exception>
        public static void TraverseRecursive(VisualizationScene scene, Action<VisualizationObject> visitor)
        {
            if (scene == null) throw new ArgumentNullException(nameof(scene));
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));

            visitor(scene.Root);

            if (scene.Children != null && scene.Children.Length > 0)
            {
                Parallel.ForEach(scene.Children, child =>
                {
                    visitor(child);
                });
            }
        }

        /// <summary>
        /// Collects all visible objects from the scene graph in parallel, filtering
        /// out objects where IsVisible is false.
        /// </summary>
        /// <param name="scene">The scene graph to collect from.</param>
        /// <returns>Array of visible objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="scene"/> is null.</exception>
        public static VisualizationObject[] CollectVisible(VisualizationScene scene)
        {
            if (scene == null) throw new ArgumentNullException(nameof(scene));

            System.Collections.Concurrent.ConcurrentBag<VisualizationObject> visible =
                new System.Collections.Concurrent.ConcurrentBag<VisualizationObject>();

            if (scene.Root.IsVisible) visible.Add(scene.Root);

            if (scene.Children != null && scene.Children.Length > 0)
            {
                Parallel.ForEach(scene.Children, child =>
                {
                    if (child.IsVisible) visible.Add(child);
                });
            }

            return visible.ToArray();
        }
    }
}
