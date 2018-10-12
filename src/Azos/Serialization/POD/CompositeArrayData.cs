
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Serialization.POD
{
    /// <summary>
    /// Represents a composite (non primitive) array data stored in Portable Object Document.
    /// </summary>
    [Serializable]
    public sealed class CompositeArrayData : CompositeData
    {
        #region .ctor

            internal CompositeArrayData(PortableObjectDocument document, Array data, int metaTypeIndex = -1)
                      : base(document, data, metaTypeIndex)
            {
                if (!ExistingReference)
                    serializeArray(data);
            }

        #endregion

        #region Fields
            public object[] m_ArrayData;
            public int[] m_ArrayDims;
        #endregion


        #region Properties
            /// <summary>
            /// Returns field data that this instance contains, or null if this instance is a reference to another object
            /// </summary>
            public object[] ArrayData { get { return m_ArrayData;} }

            /// <summary>
            /// Returns array dimensions
            /// </summary>
            public int[] ArrayDims { get { return m_ArrayDims;} }


        #endregion

        #region Public



        #endregion


        #region .pvt

             private void serializeArray(Array array)
             {
                m_ArrayData = new object[array.Length];
                m_ArrayDims = new int[array.Rank];
                for(var i=0; i<array.Rank; i++)
                    m_ArrayDims[i] = array.GetLength(i);

                var idx = 0;
                SerializationUtils.WalkArrayWrite(array, elm => m_ArrayData[idx++] =  elm );
             }



        #endregion
    }
}
