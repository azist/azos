/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Instrumentation.Analytics
{
    /// <summary>
    /// One-dimensional histogram for storing number of samples for a given
    /// dimension key
    /// </summary>
    public class Histogram<TData1> : Histogram
    {
        /// <summary>
        /// Constructs a histogram from a given array of dimensions
        /// </summary>
        /// <param name="title">Histogram title used for displaying result</param>
        /// <param name="dimension1">Dimension of the histogram dimension</param>
        public Histogram(string title, Dimension<TData1> dimension1)
            : base(title, 1, dimension1.PartitionCount)
        {
            m_Dimension1 = dimension1;
            m_Dimension1.SetIndex(0);
        }

        #region Public

            /// <summary>
            /// Number of dimensions in this histogram
            /// </summary>
            public override int DimensionCount { get { return 1; } }

            /// <summary>
            /// Return the sample count associated with given histogram keys
            /// </summary>
            public new int this[int key] { get { return this[new HistogramKeys(key)]; } }

            /// <summary>
            /// Try to get the sample count associated with the given histogram key.
            /// If the key is not present in the histogram dictionary return false
            /// </summary>
            public bool TryGet(int key, out int count) { return TryGet(new HistogramKeys(key), out count); }

            /// <summary>
            /// Increment histogram statistics for a given dimension value
            /// </summary>
            public virtual void Sample(TData1 value)
            {
                DoSample(Keys(value));
            }

            /// <summary>
            /// Convert a value to HistogramKeys struct
            /// </summary>
            public HistogramKeys Keys(TData1 value)
            {
                return new HistogramKeys(m_Dimension1[value]);
            }

            /// <summary>
            /// Returns number of samples collected for a given key.
            /// The key is obtained by mapping the given value into the dimension's partition.
            /// Return value of 0 indicates that key is not present in the histogram
            /// </summary>
            public int Value(TData1 value)
            {
                var keys = Keys(value);
                int count;
                return TryGet(keys, out count) ? count : 0;
            }

            public override IEnumerable<Dimension> Dimensions
            {
                get { yield return m_Dimension1; }
            }

        #endregion

        #region Fields

            protected Dimension<TData1> m_Dimension1;

        #endregion

    }
}
