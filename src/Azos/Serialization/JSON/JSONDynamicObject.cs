/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Dynamic;

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
    public sealed class JsonDynamicObject : DynamicObject
    {
        #region .ctor
            /// <summary>
            /// Creates a dynamic wrapper around existing array or map. Optionally specifies map key case sensitivity
            /// </summary>
            public JsonDynamicObject(JSONDynamicObjectKind kind, bool caseSensitiveMap = true)
            {
                m_Data = (kind==JSONDynamicObjectKind.Map) ? (IJsonDataObject)new JsonDataMap(caseSensitiveMap)
                                                           : (IJsonDataObject)new JsonDataArray();
            }

            public JsonDynamicObject(IJsonDataObject data)
            {
                m_Data = data;
            }

        #endregion

        #region Fields
            private IJsonDataObject m_Data;
        #endregion

        #region Properties

            public JSONDynamicObjectKind Kind
            {
                get
                {
                     return m_Data is JsonDataMap ? JSONDynamicObjectKind.Map
                                                  : JSONDynamicObjectKind.Array;
                }
            }

            /// <summary>
            /// Returns the underlying JSON data
            /// </summary>
            public IJsonDataObject Data
            {
                get { return m_Data; }
            }


        #endregion

        #region Public

            public override IEnumerable<string> GetDynamicMemberNames()
            {
              var map = m_Data as JsonDataMap;
              if (map!=null)
              {
                return map.Keys;
              }
              return base.GetDynamicMemberNames();
            }


            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var map = m_Data as JsonDataMap;

                if (map==null)
                {
                  var arr = m_Data as JsonDataArray;
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

                if (result is IJsonDataObject)
                    result = new JsonDynamicObject((IJsonDataObject)result);

                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                var map = m_Data as JsonDataMap;

                if (map==null)
                  return false;

                if (value is JsonDynamicObject)
                    value = ((JsonDynamicObject)value).m_Data;

                map[binder.Name] = value;
                return true;
            }

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                if (binder.Name.Equals("tojson",StringComparison.OrdinalIgnoreCase))
                {
                    result = JsonExtensions.ToJson(this);
                    return true;
                }

                return base.TryInvokeMember(binder, args, out result);
            }


            public override bool TryConvert(ConvertBinder binder, out object result)
            {
              var map = m_Data as JsonDataMap;
              if (map!=null && typeof(Data.TypedDoc).IsAssignableFrom(binder.Type))//convert dynamic->Data.doc
              {
                  result = JsonReader.ToDoc(binder.Type, map);
                  return true;
              }
              return base.TryConvert(binder, out result);
            }

            public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            {
                var arr = m_Data as JsonDataArray;

                result = null;

                if (arr==null)
                {
                  var map = m_Data as JsonDataMap;
                  if (map!=null)
                  {
                    if (indexes.Length==1 && indexes[0] is string)
                    {
                        result = map[(string)indexes[0]];

                        if (result is IJsonDataObject)
                            result = new JsonDynamicObject((IJsonDataObject)result);

                        return true;
                    }
                  }
                  return false;
                }
                if (indexes.Length!=1 || !(indexes[0] is int)) return false;

                var idx = (int)indexes[0];

                if(idx<arr.Count)
                  result = arr[idx];

                if (result is IJsonDataObject)
                    result = new JsonDynamicObject((IJsonDataObject)result);

                return true;
            }

            public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
            {
                var arr = m_Data as JsonDataArray;

                if (arr==null)
                {
                  var map = m_Data as JsonDataMap;
                  if (map!=null)
                  {
                    if (indexes.Length==1 && indexes[0] is string)
                    {
                        if (value is JsonDynamicObject)
                             value = ((JsonDynamicObject)value).m_Data;

                        map[(string)indexes[0]] = value;

                        return true;
                    }
                  }
                  return false;
                }
                if (indexes.Length!=1 || !(indexes[0] is int)) return false;

                if (value is JsonDynamicObject)
                    value = ((JsonDynamicObject)value).m_Data;

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
