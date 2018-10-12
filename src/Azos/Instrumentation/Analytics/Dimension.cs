

using System;


namespace Azos.Instrumentation.Analytics
{
    /// <summary>
    /// References a function that maps input value into a partition index
    /// in the context of a given Partitioner
    /// </summary>
    public delegate int PartitionFunc<TData>(Dimension<TData> sender, TData value);

    /// <summary>
    /// Returns a partition name for a given partition index
    /// </summary>
    public delegate string PartitionNameFunc(int idx);

    /// <summary>
    /// Base class of a histogram dimension.
    /// of data type TData partitions a range defined by MinValue and MaxValue,
    /// by mapping the given value into [0 ... PartitionCount] partitions
    /// </summary>
    public abstract class Dimension
    {
        #region Consts

            public static readonly string[] DEFAULT_PARTITION_NAMES = { "low", "medium", "high" };

        #endregion

        /// <summary>
        /// Creates histogram dimension instance. If partitionFunc is null then
        /// DoPartition must be overriden in the derived class
        /// </summary>
        /// <param name="name">Description of this dimension used in reporting</param>
        /// <param name="partCount">Total number of partitions</param>
        /// <param name="partitionNameFunc">Used to inject a function in order not to override this class</param>
        public Dimension(string name, int partCount, PartitionNameFunc partitionNameFunc = null)
        {
            if (partCount < 1)
                throw new AzosException(StringConsts.ARGUMENT_ERROR + "partCount");

            PartitionCount      = partCount;
            m_PartitionNameFunc = partitionNameFunc;
            Name                = name;
            m_Index             = -1;
        }

        #region Public

            /// <summary>
            /// Number of partitions in
            /// </summary>
            public readonly int PartitionCount;

            /// <summary>
            /// Provides meaningful name for displaying
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// Index of this dimension in the histogram
            /// </summary>
            public int Index { get { return m_Index; } }

            /// <summary>
            /// Returns the type of data that this instance partitions
            /// </summary>
            public abstract Type DataType { get; }

            /// <summary>
            /// Returns the name of a partition identified by idx
            /// </summary>
            public string GetPartitionName(int idx)
            {
                return m_PartitionNameFunc == null ? DoPartitionName(idx) : m_PartitionNameFunc(idx);
            }

        #endregion

        #region Protected/Internal

            internal void SetIndex(int idx) { m_Index = idx; }

            /// <summary>
            /// Override to implement a custom mapping logic to return the names of a given
            /// partition when its not passed in the .ctor
            /// </summary>
            protected virtual string DoPartitionName(int idx)
            {
                return DEFAULT_PARTITION_NAMES[
                    (int)(((double)idx / (double)PartitionCount) * (DEFAULT_PARTITION_NAMES.Length-1))];
            }

        #endregion

        #region Private

            private PartitionNameFunc m_PartitionNameFunc;
            private int m_Index;

        #endregion
    }

    /// <summary>
    /// A dimension of data type TData partitions a range of values
    /// by mapping the given value into [0 ... PartitionCount] partitions
    /// </summary>
    public class Dimension<TData> : Dimension
    {
        /// <summary>
        /// Creates a histogram dimension. If partitionFunc is null then DoPartition must be
        /// overriden in the derived class
        /// </summary>
        /// <param name="name">Description of dimension used in reporting</param>
        /// <param name="partCount">Total number of partitions</param>
        /// <param name="partitionFunc">Used to inject a function in order not to override this class</param>
        /// <param name="partitionNameFunc">Used to inject a function in order not to override this class</param>
        public Dimension(
            string name,
            int partCount,
            PartitionFunc<TData> partitionFunc = null,
            PartitionNameFunc partitionNameFunc = null)
            : base(name, partCount, partitionNameFunc)
        {
            if (partitionFunc == null && GetType() == typeof(Dimension<TData>))
                throw new AzosException(
                    StringConsts.ARGUMENT_ERROR + "partitionFunc required for non-derived instance");

            m_PartitionFunc = partitionFunc;
        }

        #region Public

            /// <summary>
            /// Returns the type of data that this instance partitions
            /// </summary>
            public override Type DataType { get { return typeof(TData); } }

            /// <summary>
            /// Maps a value into partition index
            /// </summary>
            public int this[TData value]
            {
                get { return m_PartitionFunc == null ? DoPartition(value) : m_PartitionFunc(this, value); }
            }

        #endregion

        #region Protected

            /// <summary>
            /// Override to implement a custom partitioning logic in case of class derivation
            /// when its not passed in the .ctor
            /// </summary>
            protected virtual int DoPartition(TData value)
            {
                return 0;
            }

            protected PartitionFunc<TData> m_PartitionFunc;

        #endregion
    }

}
