
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

using Azos.CodeAnalysis.JSON;


namespace Azos.Serialization.JSON
{

    /// <summary>
    /// Specifies what kind the dynamic object is - map or array
    /// </summary>
    public enum JSONDynamicObjectKind {Map, Array}

    /// <summary>
    /// Implements a JSON dynamic object that shapes itself at runtime
    /// </summary>
    [Serializable]
    public sealed class JSONDynamicObject : DynamicObject
    {
        #region .ctor
            /// <summary>
            /// Creates a dynamic wrapper around existing array or map. Optionally specifies map key case sensitivity
            /// </summary>
            public JSONDynamicObject(JSONDynamicObjectKind kind, bool caseSensitiveMap = true)
            {
                m_Data = (kind==JSONDynamicObjectKind.Map) ? (IJSONDataObject)new JSONDataMap(caseSensitiveMap)
                                                           : (IJSONDataObject)new JSONDataArray();
            }

            public JSONDynamicObject(IJSONDataObject data)
            {
                m_Data = data;
            }

        #endregion

        #region Fields
            private IJSONDataObject m_Data;
        #endregion

        #region Properties

            public JSONDynamicObjectKind Kind
            {
                get
                {
                     return m_Data is JSONDataMap ? JSONDynamicObjectKind.Map
                                                  : JSONDynamicObjectKind.Array;
                }
            }

            /// <summary>
            /// Returns the underlying JSON data
            /// </summary>
            public IJSONDataObject Data
            {
                get { return m_Data; }
            }


        #endregion

        #region Public

            public override IEnumerable<string> GetDynamicMemberNames()
            {
              var map = m_Data as JSONDataMap;
              if (map!=null)
              {
                return map.Keys;
              }
              return base.GetDynamicMemberNames();
            }


            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var map = m_Data as JSONDataMap;

                if (map==null)
                {
                  var arr = m_Data as JSONDataArray;
                  if (arr!=null)
                  {
                      if (binder.Name=="Count" || binder.Name=="Length")
                      {
                        result = arr.Count;
                        return true;
                      }

                      if (binder.Name=="List")
                      {
                        result = arr;
                        return true;
                      }

                  }

                  result = null;
                  return false;
                }

                if (map.ContainsKey(binder.Name))
                  result = map[binder.Name];
                else
                  return base.TryGetMember(binder, out result);

                if (result is IJSONDataObject)
                    result = new JSONDynamicObject((IJSONDataObject)result);

                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                var map = m_Data as JSONDataMap;

                if (map==null)
                  return false;

                if (value is JSONDynamicObject)
                    value = ((JSONDynamicObject)value).m_Data;

                map[binder.Name] = value;
                return true;
            }

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                if (binder.Name.Equals("tojson",StringComparison.OrdinalIgnoreCase))
                {
                    result = JSONExtensions.ToJSON(this);
                    return true;
                }

                return base.TryInvokeMember(binder, args, out result);
            }


            public override bool TryConvert(ConvertBinder binder, out object result)
            {
              var map = m_Data as JSONDataMap;
              if (map!=null && typeof(NFX.DataAccess.CRUD.TypedRow).IsAssignableFrom(binder.Type))//convert dynamic->CRUD.row
              {
                  result = JSONReader.ToRow(binder.Type, map);
                  return true;
              }
              return base.TryConvert(binder, out result);
            }

            public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            {
                var arr = m_Data as JSONDataArray;

                result = null;

                if (arr==null)
                {
                  var map = m_Data as JSONDataMap;
                  if (map!=null)
                  {
                    if (indexes.Length==1 && indexes[0] is string)
                    {
                        result = map[(string)indexes[0]];

                        if (result is IJSONDataObject)
                            result = new JSONDynamicObject((IJSONDataObject)result);

                        return true;
                    }
                  }
                  return false;
                }
                if (indexes.Length!=1 || !(indexes[0] is int)) return false;

                var idx = (int)indexes[0];

                if(idx<arr.Count)
                  result = arr[idx];

                if (result is IJSONDataObject)
                    result = new JSONDynamicObject((IJSONDataObject)result);

                return true;
            }

            public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
            {
                var arr = m_Data as JSONDataArray;

                if (arr==null)
                {
                  var map = m_Data as JSONDataMap;
                  if (map!=null)
                  {
                    if (indexes.Length==1 && indexes[0] is string)
                    {
                        if (value is JSONDynamicObject)
                             value = ((JSONDynamicObject)value).m_Data;

                        map[(string)indexes[0]] = value;

                        return true;
                    }
                  }
                  return false;
                }
                if (indexes.Length!=1 || !(indexes[0] is int)) return false;

                if (value is JSONDynamicObject)
                    value = ((JSONDynamicObject)value).m_Data;

                var idx = (int)indexes[0];

                //autogrow
                while(idx>=arr.Count)
                 arr.Add(null);

                arr[idx] = value;
                return true;
            }

        #endregion
    }
}
