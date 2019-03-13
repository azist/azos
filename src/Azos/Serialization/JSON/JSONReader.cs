/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Azos.CodeAnalysis.Source;
using Azos.CodeAnalysis.JSON;
using Azos.Data;

namespace Azos.Serialization.JSON
{
    /// <summary>
    /// Provides deserialization functionality from JSON format
    /// </summary>
    public static class JSONReader
    {

        /// <summary>
        /// Specifies how reader should match JSON to row field property member or backend names
        /// </summary>
        public struct NameBinding
        {
          public enum By{ CodeName = 0, BackendName}


          public static readonly NameBinding ByCode = new NameBinding(By.CodeName, null);

          public static NameBinding ByBackendName(string targetName)
          {
            return new NameBinding(By.BackendName, targetName);
          }

          private NameBinding(By by, string tagetName) { BindBy = by; TargetName = tagetName; }

          public readonly By BindBy;
          public string TargetName;
        }


        #region Public

            public static dynamic DeserializeDynamic(Stream stream, Encoding encoding = null, bool caseSensitiveMaps = true)
            {
               return deserializeDynamic( read(stream, encoding, caseSensitiveMaps));
            }

            public static dynamic DeserializeDynamic(string source, bool caseSensitiveMaps = true)
            {
               return deserializeDynamic( read(source, caseSensitiveMaps));
            }

            public static dynamic DeserializeDynamic(ISourceText source, bool caseSensitiveMaps = true)
            {
               return deserializeDynamic( read(source, caseSensitiveMaps));
            }

            public static IJSONDataObject DeserializeDataObject(Stream stream, Encoding encoding = null, bool caseSensitiveMaps = true)
            {
               return deserializeObject( read(stream, encoding, caseSensitiveMaps));
            }

            public static IJSONDataObject DeserializeDataObject(string source, bool caseSensitiveMaps = true)
            {
               return deserializeObject( read(source, caseSensitiveMaps));
            }

            public static IJSONDataObject DeserializeDataObjectFromFile(string filePath, Encoding encoding = null, bool caseSensitiveMaps = true)
            {
               using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                 return deserializeObject( read(fs, encoding, caseSensitiveMaps));
            }

            public static IJSONDataObject DeserializeDataObject(ISourceText source, bool caseSensitiveMaps = true)
            {
               return deserializeObject( read(source, caseSensitiveMaps));
            }


            /// <summary>
            /// Deserializes into Rowset or Table from JSOnDataMap, as serialized by RowsedBase.WriteAsJSON()
            /// </summary>
            public static RowsetBase ToRowset(string json, bool schemaOnly = false, bool readOnlySchema = false)
            {
              return RowsetBase.FromJSON(json, schemaOnly, readOnlySchema);
            }

            /// <summary>
            /// Deserializes into Rowset or Table from JSONDataMap, as serialized by RowsedBase.WriteAsJSON()
            /// </summary>
            public static RowsetBase ToRowset(JSONDataMap jsonMap, bool schemaOnly = false, bool readOnlySchema = false)
            {
              return RowsetBase.FromJSON(jsonMap, schemaOnly, readOnlySchema);
            }

            /// <summary>
            /// Converts JSONMap into typed data document of the requested type.
            /// The requested type must be derived from Azos.Data.TypedDoc.
            /// The extra data found in JSON map will be placed in AmorphousData dictionary if the row implements IAmorphousData, discarded otherwise.
            /// Note: This method provides "the best match" and does not guarantee that all data will/can be converted from JSON, i.e.
            ///  it can only convert one dimensional arrays and Lists of either primitive or TypeRow-derived entries
            /// </summary>
            /// <param name="type">TypedDoc subtype to convert into</param>
            /// <param name="jsonMap">JSON data to convert into data doc</param>
            /// <param name="fromUI">When true indicates that data came from UI, hence NonUI-marked fields should be skipped. True by default</param>
            /// <param name="nameBinding">Used for backend name matching or null (any target)</param>
            public static TypedDoc ToDoc(Type type, JSONDataMap jsonMap, bool fromUI = true, NameBinding? nameBinding = null)
            {
              if (!typeof(TypedDoc).IsAssignableFrom(type) || jsonMap==null)
               throw new JSONDeserializationException(StringConsts.ARGUMENT_ERROR+"JSONReader.ToDoc(type|jsonMap=null)");
              var field = "";
              try
              {
                return toTypedDoc(type, nameBinding, jsonMap, ref field, fromUI);
              }
              catch(Exception error)
              {
                throw new JSONDeserializationException("JSONReader.ToDoc(jsonMap) error in '{0}': {1}".Args(field, error.ToMessageWithType()), error);
              }
            }

            /// <summary>
            /// Generic version of ToDoc(Type, JSONDataMap, bool)/>
            /// </summary>
            /// <typeparam name="T">TypedDoc</typeparam>
            public static T ToDoc<T>(JSONDataMap jsonMap, bool fromUI = true, NameBinding? nameBinding = null) where T: TypedDoc
            {
              return ToDoc(typeof(T), jsonMap, fromUI, nameBinding) as T;
            }


            /// <summary>
            /// Converts JSONMap into supplied row instance.
            /// The extra data found in JSON map will be placed in AmorphousData dictionary if the row implements IAmorphousData, discarded otherwise.
            /// Note: This method provides "the best match" and does not guarantee that all data will/can be converted from JSON, i.e.
            ///  it can only convert one dimensional arrays and Lists of either primitive or TypeRow-derived entries
            /// </summary>
            /// <param name="doc">Data document instance to convert into</param>
            /// <param name="jsonMap">JSON data to convert into row</param>
            /// <param name="fromUI">When true indicates that data came from UI, hence NonUI-marked fields should be skipped. True by default</param>
            /// <param name="nameBinding">Used for backend name matching or null (any target)</param>
            public static void ToDoc(Doc doc, JSONDataMap jsonMap, bool fromUI = true, NameBinding? nameBinding = null)
            {
              if (doc == null || jsonMap == null)
                throw new JSONDeserializationException(StringConsts.ARGUMENT_ERROR + "JSONReader.ToDoc(doc|jsonMap=null)");
              var field = "";
              try
              {
                toDoc(doc, nameBinding.HasValue ? nameBinding.Value : NameBinding.ByCode, jsonMap, ref field, fromUI);
              }
              catch (Exception error)
              {
                throw new JSONDeserializationException("JSONReader.ToDoc(doc, jsonMap) error in '{0}': {1}".Args(field, error.ToMessageWithType()), error);
              }
            }


            private static TypedDoc toTypedDoc(Type type, NameBinding? nameBinding, JSONDataMap jsonMap, ref string field, bool fromUI)
            {
              var doc = (TypedDoc)Activator.CreateInstance(type);
              toDoc(doc, nameBinding.HasValue ? nameBinding.Value : NameBinding.ByCode, jsonMap, ref field, fromUI);
              return doc;
            }

//todo Refactor to lower method complexity
            private static void toDoc(Doc doc, NameBinding nameBinding, JSONDataMap jsonMap, ref string field, bool fromUI)
            {
              var amorph = doc as IAmorphousData;
              foreach(var mfld in jsonMap)
              {
                    field = mfld.Key;
                    var fv = mfld.Value;

                    //20170420 DKh+Ogee multitargeting for deserilization to ROW from JSON
                    Schema.FieldDef rfd;
                    if (nameBinding.BindBy==NameBinding.By.CodeName)
                       rfd = doc.Schema[field];
                    else
                      rfd = doc.Schema.TryFindFieldByTargetedBackendName(nameBinding.TargetName, field);//what about case sensitive json name?

                    if (rfd==null)
                    {
                        if (amorph!=null)
                        {
                          if (amorph.AmorphousDataEnabled)
                            amorph.AmorphousData[mfld.Key] = fv;
                        }
                        continue;
                    }

                    if (fromUI && rfd.NonUI) continue;//skip NonUI fields

                    if (fv==null)
                      doc.SetFieldValue(rfd, null);
                    else
                    if (fv is JSONDataMap)
                    {
                          if (typeof(TypedDoc).IsAssignableFrom(rfd.Type))
                            doc.SetFieldValue(rfd, ToDoc(rfd.Type, (JSONDataMap)fv, fromUI, nameBinding));
                          else
                            doc.SetFieldValue(rfd, fv);//try to set row's field to MAP directly
                    }
                    else if (rfd.NonNullableType==typeof(TimeSpan) && (fv is ulong || fv is long || fv is int || fv is uint))
                    {
                         var lt = Convert.ToInt64(fv);
                         doc.SetFieldValue(rfd, TimeSpan.FromTicks(lt));
                    }
                    else if (fv is int || fv is long || fv is ulong || fv is double || fv is bool)
                          doc.SetFieldValue(rfd, fv);
                    else if (fv is byte[] && rfd.Type==typeof(byte[]))//optimization byte array assignment without copies
                    {
                          var passed = (byte[])fv;
                          var arr = new byte[passed.Length];
                          Array.Copy(passed, arr, passed.Length);
                          doc.SetFieldValue(rfd, arr);
                    }
                    else if (fv is JSONDataArray || fv.GetType().IsArray)
                    {
                          JSONDataArray arr;
                          if (fv is JSONDataArray)
                            arr = (JSONDataArray)fv;
                          else
                          {
                            arr = new JSONDataArray(((Array)fv).Length);
                            foreach(var elm in (System.Collections.IEnumerable)fv) arr.Add(elm);
                          }

                          if (rfd.Type.IsArray)
                          {
                                var raet = rfd.Type.GetElementType();//row array element type
                                if (typeof(TypedDoc).IsAssignableFrom(raet))
                                {
                                  var narr = Array.CreateInstance(raet, arr.Count);
                                  for(var i=0; i<narr.Length; i++)
                                    narr.SetValue( ToDoc(raet, arr[i] as JSONDataMap, fromUI, nameBinding), i);
                                  doc.SetFieldValue(rfd, narr);
                                }//else primitives
                                else
                                {
                                  var narr = Array.CreateInstance(raet, arr.Count);
                                  for(var i=0; i<narr.Length; i++)
                                    if (arr[i]!=null)
                                      narr.SetValue( StringValueConversion.AsType(arr[i].ToString(), raet, false), i);

                                  doc.SetFieldValue(rfd, narr);
                                }
                          }
                          else if (rfd.Type.IsGenericType && rfd.Type.GetGenericTypeDefinition() == typeof(List<>))//List
                          {
                                var gat = rfd.Type.GetGenericArguments()[0];

                                var lst = Activator.CreateInstance(rfd.Type) as System.Collections.IList;

                                if (typeof(TypedDoc).IsAssignableFrom(gat))
                                {
                                  for(var i=0; i<arr.Count; i++)
                                    if (arr[i] is JSONDataMap)
                                      lst.Add( ToDoc(gat, arr[i] as JSONDataMap, fromUI, nameBinding) );
                                    else
                                      lst.Add(null);
                                }
                                else if (gat==typeof(object))
                                {
                                  for(var i=0; i<arr.Count; i++)
                                      lst.Add( arr[i] );
                                }
                                else if (gat==typeof(string))
                                {
                                  for(var i=0; i<arr.Count; i++)
                                    if (arr[i]!=null)
                                      lst.Add( arr[i].ToString() );
                                    else
                                      lst.Add( null );
                                }
                                else if (gat.IsPrimitive ||
                                    gat==typeof(Data.GDID) ||
                                    gat==typeof(Guid) ||
                                    gat==typeof(DateTime))
                                {
                                  for(var i=0; i<arr.Count; i++)
                                    if (arr[i]!=null)
                                      lst.Add( StringValueConversion.AsType(arr[i].ToString(), gat, false) );
                                }
                                else if (gat.IsGenericType && gat.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    var nt = gat.GetGenericArguments()[0];
                                    if (nt.IsPrimitive ||
                                        nt==typeof(Data.GDID) ||
                                        nt==typeof(Guid) ||
                                        nt==typeof(DateTime))
                                    {

                                    for(var i=0; i<arr.Count; i++)
                                      if (arr[i]!=null)
                                        lst.Add( StringValueConversion.AsType(arr[i].ToString(), gat, false) );
                                      else
                                        lst.Add( null);
                                    }
                                }

                                doc.SetFieldValue(rfd, lst);

                          }
                    }
                    else
                    {
                          //Try to get String containing JSON
                          if (fv is string)
                          {
                            var sfv = (string)fv;
                            if (rfd.Type==typeof(string))
                            {
                              doc.SetFieldValue(rfd, sfv);
                              continue;
                            }

                            if (typeof(TypedDoc).IsAssignableFrom(rfd.Type))
                            {
                             if (sfv.IsNotNullOrWhiteSpace())
                                doc.SetFieldValue(rfd, ToDoc(rfd.Type, (JSONDataMap)deserializeObject( read(sfv, true)), fromUI, nameBinding));
                             continue;
                            }
                            if (typeof(IJSONDataObject).IsAssignableFrom(rfd.Type))
                            {
                             if (sfv.IsNotNullOrWhiteSpace())
                                doc.SetFieldValue(rfd, deserializeObject( read(sfv, true)));//try to set row's field to MAP directly
                             continue;
                            }
                          }

                          doc.SetFieldValue(rfd, StringValueConversion.AsType(fv.ToString(), rfd.Type, false));//<--Type conversion
                    }
              }//foreach

              //20140914 DKh
              var form = doc as Form;
              if (form != null)
              {
                form.FormMode  = jsonMap[Form.JSON_MODE_PROPERTY].AsEnum<FormMode>(FormMode.Unspecified);
                form.CSRFToken = jsonMap[Form.JSON_CSRF_PROPERTY].AsString();
                var roundtrip  = jsonMap[Form.JSON_ROUNDTRIP_PROPERTY].AsString();
                if (roundtrip.IsNotNullOrWhiteSpace())
                 form.SetRoundtripBagFromJSONString(roundtrip);
              }

              if (amorph!=null && amorph.AmorphousDataEnabled )
                amorph.AfterLoad("json");
            }


        #endregion

        #region .pvt

          private static dynamic deserializeDynamic(object root)
          {
              var data = deserializeObject(root);
              if (data == null) return null;

              return new JSONDynamicObject( data );
          }

          private static IJSONDataObject deserializeObject(object root)
          {
              if (root==null) return null;

              var data = root as IJSONDataObject;

              if (data == null)
                data = new JSONDataMap{{"value", root}};

              return data;
          }

          private static object read(Stream stream, Encoding encoding, bool caseSensitiveMaps)
          {
              using(var source = encoding==null ? new StreamSource(stream, JSONLanguage.Instance)
                                                : new StreamSource(stream, encoding, JSONLanguage.Instance))
                  return read(source, caseSensitiveMaps);
          }

          private static object read(string data, bool caseSensitiveMaps)
          {
              var source = new StringSource(data, JSONLanguage.Instance);
              return read(source, caseSensitiveMaps);
          }

          private static object read(ISourceText source, bool caseSensitiveMaps)
          {
              var lexer = new JSONLexer(source, throwErrors: true);
              var parser = new JSONParser(lexer, throwErrors: true, caseSensitiveMaps: caseSensitiveMaps);

              parser.Parse();

              return parser.ResultContext.ResultObject;
          }
        #endregion



    }
}
