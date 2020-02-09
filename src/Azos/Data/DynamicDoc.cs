/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

namespace Azos.Data
{
    /// <summary>
    /// Represents a data document having schema only known at run-time.
    /// Dynamic docs store data in object[] internally, providing better flexibility(ability to define schema at runtime) than TypedDocs
    /// at the expense of performance. This class is not sealed so implementors may override configuration persistence
    /// </summary>
    [Serializable]
    public class DynamicDoc : Doc
    {
        #region .ctor
            protected DynamicDoc()//used by serializer
            {

            }

            public DynamicDoc(Schema schema)
            {
               __ctor(schema);
            }

            /// <summary>
            /// Developers do not call, lazily injects the statr of this object,
            /// this is needed for speed and other optimizations
            /// </summary>
            internal void __ctor(Schema schema)
            {
               m_Schema = schema;
               m_Data = new object[schema.FieldCount];
            }
        #endregion

        #region Fields

            private Schema m_Schema;
            private object[] m_Data;

        #endregion

        #region Properties

            /// <summary>
            /// References a schema for a table that this row is a part of
            /// </summary>
            public override Schema Schema
            {
                get { return m_Schema;}
            }

            /// <summary>
            /// Provides access to raw array of values that this row contains
            /// </summary>
            public object[] Data
            {
                get { return m_Data;}
            }


        #endregion

        #region Public

            public override object GetFieldValue(Schema.FieldDef fdef)
            {
                var result = m_Data[ fdef.Order ];

                if (result==DBNull.Value) result = null;

                return result;
            }

            public override void SetFieldValue(Schema.FieldDef fdef, object value)
            {
                value = ConvertFieldValueToDef(fdef, value);
                m_Data[ fdef.Order ] = value;
            }

        #endregion


        #region .pvt


        #endregion
    }


    /// <summary>
    /// Represents a data document which has a schema only known at run-time that also implements IAmorphousData
    /// interface that allows this row to store "extra" data that does not comply with the current schema.
    /// Dynamic docs store data in object[] internally, providing better flexibility(ability to define schema at runtime) than TypedDocs at the expense of performance.
    /// This class is not sealed so implementors may override configuration persistence
    /// </summary>
    [Serializable]
    public class AmorphousDynamicDoc : DynamicDoc, IAmorphousData
    {
      public AmorphousDynamicDoc(Schema schema) : base(schema)
      {
      }

      private Dictionary<string, object> m_AmorphousData;


      /// <summary>
      /// True by default for rows
      /// </summary>
      public virtual bool AmorphousDataEnabled => true;

      /// <summary>
      /// Returns data that does not comply with known schema (dynamic data). The field names are NOT case-sensitive
      /// </summary>
      public IDictionary<string, object> AmorphousData
      {
        get
        {
          if (m_AmorphousData==null)
          m_AmorphousData = new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase);

          return m_AmorphousData;
        }
      }


      void IAmorphousData.BeforeSave(string targetName) => DoBeforeSave(targetName);
      void IAmorphousData.AfterLoad(string targetName) => DoAfterLoad(targetName);


      /// <summary>
      /// Invoked to allow the doc to transform its state into AmorphousData bag.
      /// For example, this may be useful to store extra data that is not a part of established business schema.
      /// The operation is performed per particular targetName (name of physical backend). Simply put, this method allows
      ///  business code to "specify what to do before object gets saved in THE PARTICULAR TARGET backend store"
      /// </summary>
      protected virtual void DoBeforeSave(string targetName)
      {

      }

      /// <summary>
      /// Invoked to allow the doc to hydrate its fields/state from AmorphousData bag.
      /// For example, this may be used to reconstruct some temporary object state that is not stored as a part of established business schema.
      /// The operation is performed per particular targetName (name of physical backend).
      /// Simply put, this method allows business code to "specify what to do after object gets loaded from THE PARTICULAR TARGET backend store".
      /// An example: suppose current MongoDB collection stores 3 fields for name, and we want to collapse First/Last/Middle name fields into one field.
      /// If we change schema then it will only contain 1 field which is not present in the database, however those 'older' fields will get populated
      /// into AmorphousData giving us an option to merge older 3 fields into 1 within AfterLoad() implementation
      /// </summary>
      protected virtual void DoAfterLoad(string targetName)
      {

      }
    }
}
