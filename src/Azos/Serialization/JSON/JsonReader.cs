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
using System.Linq;

namespace Azos.Serialization.JSON
{
  /// <summary>
  /// Provides deserialization functionality from JSON format
  /// </summary>
  public static class JsonReader
  {
    /// <summary>
    /// Specifies how reader should match JSON to row field property member or backend names
    /// </summary>
    public struct NameBinding
    {
      /// <summary> By Name in Code | By BackendName attribute </summary>
      public enum By{ CodeName = 0, BackendName}

      public static readonly NameBinding ByCode = new NameBinding(By.CodeName, null);

      public static NameBinding ByBackendName(string targetName) => new NameBinding(By.BackendName, targetName);

      private NameBinding(By by, string tagetName) { BindBy = by; TargetName = tagetName; }

      public readonly By BindBy;
      public readonly string TargetName;
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

    public static IJsonDataObject DeserializeDataObject(Stream stream, Encoding encoding = null, bool caseSensitiveMaps = true)
    {
        return deserializeObject( read(stream, encoding, caseSensitiveMaps));
    }

    public static IJsonDataObject DeserializeDataObject(string source, bool caseSensitiveMaps = true)
    {
        return deserializeObject( read(source, caseSensitiveMaps));
    }

    public static IJsonDataObject DeserializeDataObjectFromFile(string filePath, Encoding encoding = null, bool caseSensitiveMaps = true)
    {
        using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
          return deserializeObject( read(fs, encoding, caseSensitiveMaps));
    }

    public static IJsonDataObject DeserializeDataObject(ISourceText source, bool caseSensitiveMaps = true)
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
    public static RowsetBase ToRowset(JsonDataMap jsonMap, bool schemaOnly = false, bool readOnlySchema = false)
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
    public static TypedDoc ToDoc(Type type, JsonDataMap jsonMap, bool fromUI = true, NameBinding? nameBinding = null)
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
    public static T ToDoc<T>(JsonDataMap jsonMap, bool fromUI = true, NameBinding? nameBinding = null) where T: TypedDoc
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
    public static void ToDoc(Doc doc, JsonDataMap jsonMap, bool fromUI = true, NameBinding? nameBinding = null)
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


    private static TypedDoc toTypedDoc(Type type, NameBinding? nameBinding, JsonDataMap jsonMap, ref string field, bool fromUI)
    {
      var doc = (TypedDoc)SerializationUtils.MakeNewObjectInstance(type);
      toDoc(doc, nameBinding.HasValue ? nameBinding.Value : NameBinding.ByCode, jsonMap, ref field, fromUI);
      return doc;
    }


    private static void toDoc(Doc doc, NameBinding nameBinding, JsonDataMap jsonMap, ref string field, bool fromUI)
    {
      var amorph = doc as IAmorphousData;
      foreach (var mfld in jsonMap)
      {
        field = mfld.Key;
        var fv = mfld.Value;

        //Multitargeting for deserilization to TypedDoc from JSON
        Schema.FieldDef def;
        if (nameBinding.BindBy == NameBinding.By.CodeName)
          def = doc.Schema[field];
        else
          def = doc.Schema.TryFindFieldByTargetedBackendName(nameBinding.TargetName, field);//what about case sensitive json name?

        //No such field exists in a typed doc, try to put in amorphous data
        if (def == null)
        {
          if (amorph != null)
          {
            if (amorph.AmorphousDataEnabled)
              amorph.AmorphousData[mfld.Key] = fv;
          }
          continue;
        }

        if (fromUI && def.NonUI) continue;//skip NonUI fields

        if (fv == null)
        {
          doc.SetFieldValue(def, null);
          continue;
        }

        //fv is Never null here
        var wasset = setOneField(doc, def, fv, fromUI, nameBinding); //<------------------- field assignment

        //try to put in amorphous data if could not be set in a field
        if (!wasset && amorph != null)
        {
          if (amorph.AmorphousDataEnabled)
            amorph.AmorphousData[mfld.Key] = fv;
        }

      }//foreach field in jsonMap

      //process FORM
      var form = doc as Form;
      if (form != null)
      {
        form.FormMode = jsonMap[Form.JSON_MODE_PROPERTY].AsEnum<FormMode>(FormMode.Unspecified);
        form.CSRFToken = jsonMap[Form.JSON_CSRF_PROPERTY].AsString();
        var roundtrip = jsonMap[Form.JSON_ROUNDTRIP_PROPERTY].AsString();
        if (roundtrip.IsNotNullOrWhiteSpace())
          form.SetRoundtripBagFromJSONString(roundtrip);
      }

      if (amorph != null && amorph.AmorphousDataEnabled)
        amorph.AfterLoad("json");
    }

    private static bool setOneField(Doc doc, Schema.FieldDef def, object fv, bool fromUI, NameBinding nameBinding)
    {
      var converted = cast(fv, def.Type, fromUI, nameBinding);
      ////Console.WriteLine($"{def.Name} = {converted} ({(converted!=null ? converted.GetType().Name : "null")})");

      if (converted!=null)
      {
        doc.SetFieldValue(def, converted);
        return true;
      }

      return false;
    }//setOneField


    //Returns non null on success; may return null for collection sub-element in which case null=null and does not indicate failure
    private static object cast(object v, Type toType, bool fromUI, NameBinding nameBinding)
    {
      //used only for collections inner calls
      if (v==null) return null;

      //object goes as is
      if (toType == typeof(object)) return v;

      //IJSONDataObject
      if (toType == typeof(IJsonDataObject))
      {
        if (v is IJsonDataObject) return v;//goes as is
        if (v is string s)//string containing embedded JSON
        {
          var jo = s.JsonToDataObject();
          return jo;
        }
      }

      //IJSONDataMap
      if (toType == typeof(JsonDataMap))
      {
        if (v is JsonDataMap) return v;//goes as is
        if (v is string s)//string containing embedded JSON
        {
          var jo = s.JsonToDataObject() as JsonDataMap;
          return jo;
        }
      }

      //IJSONDataArray
      if (toType == typeof(JsonDataArray))
      {
        if (v is JsonDataArray) return v;//goes as is
        if (v is string s)//string containing embedded JSON
        {
          var jo = s.JsonToDataObject() as JsonDataArray;
          return jo;
        }
      }

      var nntp = toType;
      if (toType.IsGenericType && nntp.GetGenericTypeDefinition() == typeof(Nullable<>))
        nntp = toType.GetGenericArguments()[0];


      //Custom JSON Readable
      if (typeof(IJsonReadable).IsAssignableFrom(nntp))
      {
        IJsonReadable newval = SerializationUtils.MakeNewObjectInstance(nntp) as IJsonReadable;
        var got = newval.ReadAsJson(v, fromUI, nameBinding);//this me re-allocate the result based of newval
        return got.match ? got.self : null;
      }

      //byte[] direct assignment w/o copies
      if (nntp == typeof(byte[]) && v is byte[] passed)
      {
        return passed;
      }


      //field def = []
      if (toType.IsArray)
      {
        var fvseq = v as IEnumerable<object>;
        if (fvseq == null) return null;//can not set non enumerable into array

        var arr = fvseq.Select(e => cast(e, toType.GetElementType(), fromUI, nameBinding)).ToArray();
        var newval = Array.CreateInstance(toType.GetElementType(), arr.Length);
        for(var i=0; i<newval.Length; i++)
          newval.SetValue(arr[i], i);

        return newval;
      }

      //field def = List<t>
      if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(List<>))
      {
        var fvseq = v as IEnumerable<object>;
        if (fvseq == null) return false;//can not set non enumerable into List<t>

        var arr = fvseq.Select(e => cast(e, toType.GetGenericArguments()[0], fromUI, nameBinding)).ToArray();
        var newval = SerializationUtils.MakeNewObjectInstance(toType) as System.Collections.IList;
        for (var i = 0; i < arr.Length; i++)
          newval.Add(arr[i]);

        return newval;
      }

      //last resort
      return StringValueConversion.AsType(v.ToString(), toType, false);
    }

  #endregion

    #region .pvt

    private static dynamic deserializeDynamic(object root)
    {
        var data = deserializeObject(root);
        if (data == null) return null;

        return new JsonDynamicObject( data );
    }

    private static IJsonDataObject deserializeObject(object root)
    {
        if (root==null) return null;

        var data = root as IJsonDataObject;

        if (data == null)
          data = new JsonDataMap{{"value", root}};

        return data;
    }

    private static object read(Stream stream, Encoding encoding, bool caseSensitiveMaps)
    {
        using(var source = encoding==null ? new StreamSource(stream, JsonLanguage.Instance)
                                          : new StreamSource(stream, encoding, JsonLanguage.Instance))
            return read(source, caseSensitiveMaps);
    }

    private static object read(string data, bool caseSensitiveMaps)
    {
        var source = new StringSource(data, JsonLanguage.Instance);
        return read(source, caseSensitiveMaps);
    }

    private static object read(ISourceText source, bool caseSensitiveMaps)
    {
        var lexer = new JsonLexer(source, throwErrors: true);
        var parser = new JsonParser(lexer, throwErrors: true, caseSensitiveMaps: caseSensitiveMaps);

        parser.Parse();

        return parser.ResultContext.ResultObject;
    }
    #endregion

  }
}
