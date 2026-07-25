namespace MathVerse.Math.Distributed.GPU
{
    using System;

    /// <summary>
    /// Abstract representation of a GPU kernel (compute shader or compute function)
    /// that can be dispatched on a GPU device.
    /// </summary>
    public sealed class GPUKernel
    {
        /// <summary>
        /// Gets or sets the unique identifier for this kernel.
        /// </summary>
        public int KernelId { get; set; }

        /// <summary>
        /// Gets or sets the human-readable name of this kernel.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entry point function name in the compiled program.
        /// </summary>
        public string EntryPoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the local (workgroup) size for this kernel.
        /// This defines the number of work items in a single workgroup.
        /// </summary>
        public int[] LocalWorkSize { get; set; } = new int[] { 1, 1, 1 };

        /// <summary>
        /// Gets or sets the global (ndrange) size for this kernel.
        /// This defines the total number of work items to launch.
        /// </summary>
        public int[] GlobalWorkSize { get; set; } = new int[] { 1, 1, 1 };

        /// <summary>
        /// Gets the total number of local work items per workgroup.
        /// </summary>
        public int LocalWorkGroupSize =>
            LocalWorkSize[0] * LocalWorkSize[1] * LocalWorkSize[2];

        /// <summary>
        /// Gets the total number of global work items.
        /// </summary>
        public int TotalGlobalWorkItems =>
            GlobalWorkSize[0] * GlobalWorkSize[1] * GlobalWorkSize[2];

        /// <summary>
        /// Gets the number of workgroups that will be dispatched.
        /// </summary>
        public int WorkGroupCount
        {
            get
            {
                int groupsX = (GlobalWorkSize[0] + LocalWorkSize[0] - 1) / LocalWorkSize[0];
                int groupsY = (GlobalWorkSize[1] + LocalWorkSize[1] - 1) / LocalWorkSize[1];
                int groupsZ = (GlobalWorkSize[2] + LocalWorkSize[2] - 1) / LocalWorkSize[2];
                return groupsX * groupsY * groupsZ;
            }
        }

        /// <summary>
        /// Returns a string representation of this kernel.
        /// </summary>
        /// <returns>A string containing the kernel name and work size information.</returns>
        public override string ToString()
        {
            return $"Kernel[{KernelId}] '{Name}' - Global: [{GlobalWorkSize[0]}, {GlobalWorkSize[1]}, {GlobalWorkSize[2]}] Local: [{LocalWorkSize[0]}, {LocalWorkSize[1]}, {LocalWorkSize[2]}]";
        }
    }
}
