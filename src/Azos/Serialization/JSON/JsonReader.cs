/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.CodeAnalysis.Source;
using Azos.Data;

namespace Azos.Serialization.JSON
{
  /// <summary>
  /// Provides deserialization functionality from JSON format
  /// </summary>
  public static class JsonReader
  {
    /// <summary>
    /// Specifies how reader should match JSON to data document field property member or backend names along
    /// with other data document reading options such as DateTime handling
    /// </summary>
    public struct DocReadOptions
    {
      /// <summary> By Name in Code | By BackendName attribute </summary>
      public enum By{ CodeName = 0, BackendName}

      /// <summary>
      /// Bind by property names as specified in code and use UTC dates
      /// </summary>
      public static readonly DocReadOptions BindByCode = new DocReadOptions(By.CodeName, null);

      /// <summary>
      /// Bind by backend name for the specified target, Use UTC dates
      /// </summary>
      public static DocReadOptions BindByBackendName(string targetName) => new DocReadOptions(By.BackendName, targetName);

      public DocReadOptions(By by, string tagetName, bool localDates = false)
      {
        BindBy = by;
        TargetName = tagetName;
        LocalDates = localDates;
      }

      /// <summary>
      /// Specifies whether to bind by field property name in code or FieldAttribute.BackendName
      /// </summary>
      public readonly By BindBy;

      /// <summary>
      /// TargetName to bind by, used to get the right attribute for each FieldDef in a data document Schema
      /// </summary>
      public readonly string TargetName;

      /// <summary>
      /// When true converts dates to local time, default = false which adjust dates to UTC
      /// </summary>
      public readonly bool LocalDates;
    }


    private static readonly IJsonReaderBackend DEFAULT_READER_BACKEND = new Backends.JazonReaderBackend();//As of 20200302 Switched to Jazon// .ClassicJsonReaderBackend();

    private static IJsonReaderBackend s_ReaderBackend;

    private static int s_ErrorSourceDisclosureLevel;//#833

    /// <summary>
    /// Returns process global IJsonReaderBackend if one was set, or an instance of ClassicJsonReaderBackend as the default value.
    /// Null is never returned
    /// </summary>
    /// <remarks>
    /// App context and DI is not used here purposely not to couple Json framework to higher-level components.
    /// The need to switch Json parsing library at runtime multiple times is non existent, and this mechanism
    /// is provided more for special use cases like profiling with different json parsers.
    /// The Wave vNext uses JSON via a DI-installed module, whereas this is a lower-level service used in general context
    /// where DI is not even available
    /// </remarks>
    internal static IJsonReaderBackend ReaderBackend
    {
      get
      {
        var backend = s_ReaderBackend;
        return backend != null ? backend : DEFAULT_READER_BACKEND;
      }
    }

    /// <summary>
    /// System method, app developers do not call.
    /// Sets process-wide reader backend. Passing null resets the backend to DEFAULT_READER_BACKEND
    /// </summary>
    public static void ____SetReaderBackend(IJsonReaderBackend backend)
    {
      s_ReaderBackend = backend;
    }

    /// <summary>
    /// Controls the level of error disclosure in JSON deserialization exceptions.
    /// This mechanism is used for security not to disclose private data by accident.
    /// Level zero (default) does not disclose anything.
    /// Level one discloses json document path where error occurred without revealing to much data,
    /// then the next levels disclose a snippet of json string content where exception happened.
    /// Since exceptions are typically stored in logs, the inadvertent leak of sensitive data is possible, hence
    /// the json error disclosure is disabled (level zero) by default.
    /// See <see cref="____SetErrorSourceDisclosureLevel(int)"/>
    /// </summary>
    public static int ErrorSourceDisclosureLevel => s_ErrorSourceDisclosureLevel;


    /// <summary>
    /// System method, app developers do not call.
    /// Sets the level of error source (raw textual json) disclosure.
    /// By default the error source is disabled (level zero) for security purposes.
    /// You can call this method in your particular application
    /// for debugging purposes, however in sensitive applications it should be disabled
    /// <see cref="ErrorSourceDisclosureLevel"/>
    /// </summary>
    public static void ____SetErrorSourceDisclosureLevel(int level)
    {
      s_ErrorSourceDisclosureLevel = level;
    }


    #region Public

    public static dynamic DeserializeDynamic(Stream stream, Encoding encoding = null, bool caseSensitiveMaps = true)
     => deserializeDynamic( ReaderBackend.DeserializeFromJson(stream, caseSensitiveMaps, encoding));

    public static dynamic DeserializeDynamic(string source, bool caseSensitiveMaps = true)
     => deserializeDynamic(ReaderBackend.DeserializeFromJson(source, caseSensitiveMaps));

    public static dynamic DeserializeDynamic(ISourceText source, bool caseSensitiveMaps = true)
     => deserializeDynamic(ReaderBackend.DeserializeFromJson(source, caseSensitiveMaps));

    public static IJsonDataObject DeserializeDataObject(Stream stream, Encoding encoding = null, bool caseSensitiveMaps = true)
     => deserializeObject(ReaderBackend.DeserializeFromJson(stream, caseSensitiveMaps, encoding));

    public static Task<object> DeserializeAsync(Stream stream, Encoding encoding = null, bool caseSensitiveMaps = true)
     => ReaderBackend.DeserializeFromJsonAsync(stream, caseSensitiveMaps, encoding);

    public static IJsonDataObject DeserializeDataObject(string source, bool caseSensitiveMaps = true)
     => deserializeObject(ReaderBackend.DeserializeFromJson(source, caseSensitiveMaps));

    public static IJsonDataObject DeserializeDataObjectFromFile(string filePath, Encoding encoding = null, bool caseSensitiveMaps = true)
    {
        using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
          return deserializeObject(ReaderBackend.DeserializeFromJson(fs, caseSensitiveMaps, encoding));
    }

    public static async Task<object> DeserializeFromFileAsync(string filePath, Encoding encoding = null, bool caseSensitiveMaps = true)
    {
      using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
      return await ReaderBackend.DeserializeFromJsonAsync(fs, caseSensitiveMaps, encoding).ConfigureAwait(false);
    }

    public static IJsonDataObject DeserializeDataObject(ISourceText source, bool caseSensitiveMaps = true)
      => deserializeObject(ReaderBackend.DeserializeFromJson(source, caseSensitiveMaps));

    public static Task<object> DeserializeAsync(ISourceText source, bool caseSensitiveMaps = true)
      => ReaderBackend.DeserializeFromJsonAsync(source, caseSensitiveMaps);


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
    /// The requested type must be derived from <see cref="Azos.Data.TypedDoc"/>.
    /// The extra data found in JSON map will be placed in AmorphousData dictionary if the doc implements IAmorphousData, discarded otherwise.
    /// Note: This method provides "the best match" and does not guarantee that all data will/can be converted from JSON, i.e.
    ///  it can only convert one dimensional arrays and Lists of either primitive or TypeDoc-derived entries
    /// </summary>
    /// <param name="type">TypedDoc subtype to convert into</param>
    /// <param name="jsonMap">JSON data to convert into data doc</param>
    /// <param name="fromUI">When true indicates that data came from UI, hence NonUI-marked fields should be skipped. True by default</param>
    /// <param name="options">Used for backend name matching or null (any target)</param>
    public static TypedDoc ToDoc(Type type, JsonDataMap jsonMap, bool fromUI = true, DocReadOptions? options = null)
    {
      if (!typeof(TypedDoc).IsAssignableFrom(type) || jsonMap==null)
        throw new JSONDeserializationException(StringConsts.ARGUMENT_ERROR+"JSONReader.ToDoc(type|jsonMap=null)");
      var field = "";
      try
      {
        return toTypedDoc(type, options, jsonMap, ref field, fromUI);
      }
      catch(Exception error)
      {
        throw new JSONDeserializationException("JSONReader.ToDoc(jsonMap) error in '{0}': {1}".Args(field, error.ToMessageWithType()), error);
      }
    }

    /// <summary>
    /// Converts JSONMap into typed data document of the requested type.
    /// The requested type must be derived from <see cref="Azos.Data.TypedDoc"/>.
    /// The extra data found in JSON map will be placed in AmorphousData dictionary if the doc implements IAmorphousData, discarded otherwise.
    /// Note: This method provides "the best match" and does not guarantee that all data will/can be converted from JSON, i.e.
    ///  it can only convert one dimensional arrays and Lists of either primitive or TypeRow-derived entries
    /// </summary>
    /// <param name="type">TypedDoc subtype to convert into</param>
    /// <param name="json">JSON data to convert into data doc</param>
    /// <param name="fromUI">When true indicates that data came from UI, hence NonUI-marked fields should be skipped. True by default</param>
    /// <param name="options">Used for backend name matching or null (any target)</param>
    public static TypedDoc ToDoc(Type type, string json, bool fromUI = true, DocReadOptions? options = null)
    {
      var map =  (json.NonBlank(nameof(json)).JsonToDataObject(true) as JsonDataMap).NonNull("json is not a map");

      return ToDoc(type, map, fromUI, options);
    }

    /// <summary>
    /// Generic version of ToDoc(Type...)
    /// </summary>
    /// <typeparam name="T">TypedDoc</typeparam>
    public static T ToDoc<T>(JsonDataMap jsonMap, bool fromUI = true, DocReadOptions? options = null) where T: TypedDoc
    {
      return ToDoc(typeof(T), jsonMap, fromUI, options) as T;
    }

    /// <summary>
    /// Generic version of ToDoc(Type, JSONDataMap, DocReadOptions)
    /// </summary>
    /// <typeparam name="T">TypedDoc</typeparam>
    public static T ToDoc<T>(string json, bool fromUI = true, DocReadOptions? options = null) where T : TypedDoc
    {
      var map = (json.NonBlank(nameof(json)).JsonToDataObject(true) as JsonDataMap).NonNull("json is not a map");
      return ToDoc(typeof(T), map, fromUI, options) as T;
    }


    /// <summary>
    /// Converts JSONMap into supplied data document instance.
    /// The extra data found in JSON map will be placed in AmorphousData dictionary if the doc implements IAmorphousData, discarded otherwise.
    /// Note: This method provides "the best match" and does not guarantee that all data will/can be converted from JSON, i.e.
    ///  it can only convert one dimensional arrays and Lists of either primitive or TypeRow-derived entries
    /// </summary>
    /// <param name="doc">Data document instance to convert into</param>
    /// <param name="json">JSON data to convert into row</param>
    /// <param name="fromUI">When true indicates that data came from UI, hence NonUI-marked fields should be skipped. True by default</param>
    /// <param name="options">Used for backend name matching or null (any target)</param>
    public static void ToDoc(Doc doc, string json, bool fromUI = true, DocReadOptions? options = null)
    {
      var map = (json.NonBlank(nameof(json)).JsonToDataObject(true) as JsonDataMap).NonNull("json is not a map");
      ToDoc(doc, map, fromUI, options);
    }


    /// <summary>
    /// Converts JSONMap into supplied row instance.
    /// The extra data found in JSON map will be placed in AmorphousData dictionary if the doc implements IAmorphousData, discarded otherwise.
    /// Note: This method provides "the best match" and does not guarantee that all data will/can be converted from JSON, i.e.
    ///  it can only convert one dimensional arrays and Lists of either primitive or TypeRow-derived entries
    /// </summary>
    /// <param name="doc">Data document instance to convert into</param>
    /// <param name="jsonMap">JSON data to convert into row</param>
    /// <param name="fromUI">When true indicates that data came from UI, hence NonUI-marked fields should be skipped. True by default</param>
    /// <param name="options">Used for backend name matching or null (any target)</param>
    public static void ToDoc(Doc doc, JsonDataMap jsonMap, bool fromUI = true, DocReadOptions? options = null)
    {
      if (doc == null || jsonMap == null)
        throw new JSONDeserializationException(StringConsts.ARGUMENT_ERROR + "JSONReader.ToDoc(doc|jsonMap=null)");

      var field = "";
      try
      {
        var tDoc = doc.GetType();
        var customHandler = JsonHandlerAttribute.TryFind(tDoc);
        if (customHandler != null)
        {
          var castResult = customHandler.TypeCastOnRead(jsonMap, tDoc, fromUI, options ?? DocReadOptions.BindByCode);

          //only reacts to ChangeSource because this method works on pre-allocated doc
          if (castResult.Outcome == JsonHandlerAttribute.TypeCastOutcome.ChangedSourceValue)
          {
            jsonMap = (castResult.Value as JsonDataMap).NonNull("Changed source value must be of JsonDataMap type for root data documents");
          }
        }

        toDoc(doc, options.HasValue ? options.Value : DocReadOptions.BindByCode, jsonMap, ref field, fromUI);
      }
      catch (Exception error)
      {
        throw new JSONDeserializationException("JSONReader.ToDoc(doc, jsonMap) error in '{0}': {1}".Args(field, error.ToMessageWithType()), error);
      }
    }


    private static TypedDoc toTypedDoc(Type type, DocReadOptions? options, JsonDataMap jsonMap, ref string field, bool fromUI)
    {
      var toAllocate = type;

      var customHandler = JsonHandlerAttribute.TryFind(type);
      if (customHandler != null)
      {
        var castResult = customHandler.TypeCastOnRead(jsonMap, type, fromUI, options ?? DocReadOptions.BindByCode);

        if (castResult.Outcome >= JsonHandlerAttribute.TypeCastOutcome.ChangedTargetType)
        {
          toAllocate = castResult.ToType.IsOfType(type, "Changed type must be a subtype of specific document type");
        }

        if (castResult.Outcome == JsonHandlerAttribute.TypeCastOutcome.ChangedSourceValue)
        {
          jsonMap = (castResult.Value as JsonDataMap).NonNull("Changed source value must be of JsonDataMap type for root data documents");
        }
        else if (castResult.Outcome == JsonHandlerAttribute.TypeCastOutcome.HandledCast)
        {
          return castResult.Value.ValueIsOfType(type, "HandledCast must return TypedDoc for root data documents") as TypedDoc;
        }
      }

      if (toAllocate.IsAbstract)
        throw new JSONDeserializationException(StringConsts.JSON_DESERIALIZATION_ABSTRACT_TYPE_ERROR.Args(toAllocate.Name, nameof(JsonHandlerAttribute)));

      var doc = (TypedDoc)SerializationUtils.MakeNewObjectInstance(toAllocate);
      toDoc(doc, options.HasValue ? options.Value : DocReadOptions.BindByCode, jsonMap, ref field, fromUI);
      return doc;
    }


    private static void toDoc(Doc doc, DocReadOptions options, JsonDataMap jsonMap, ref string field, bool fromUI)
    {
      var amorph = doc as IAmorphousData;
      var schema = doc.Schema;
      foreach (var mfld in jsonMap)
      {
        field = mfld.Key;
        var fv = mfld.Value;

        //Multi-targeting for deserialization to TypedDoc from JSON
        Schema.FieldDef def;
        if (options.BindBy == DocReadOptions.By.CodeName)
          def = schema[field];
        else
          def = schema.TryFindFieldByTargetedBackendName(options.TargetName, field);//what about case sensitive json name?

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

        if (def.GetOnly) continue;//do not try to read get-only fields

        if (fromUI && def.NonUI) continue;//skip NonUI fields

        //weed out NULLS here
        if (fv == null)
        {
          doc.SetFieldValue(def, null);
          continue;
        }

        //fv is NEVER NULL here
        var wasset = setOneField(doc, def, fv, fromUI, options); //<------------------- field assignment

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
        amorph.AfterLoad(options.TargetName ?? "json");
    }

    private static bool setOneField(Doc doc, Schema.FieldDef def, object fv, bool fromUI, DocReadOptions options)
    {
      var fieldCustomHandler = JsonHandlerAttribute.TryFind(def.MemberInfo);
      var converted = cast(fv, def.Type, fromUI, options, fieldCustomHandler);

      if (converted != null)
      {
        doc.SetFieldValue(def, converted);
        return true;
      }

      return false;
    }//setOneField


    //Returns non null on success; may return null for collection sub-element in which case null=null and does not indicate failure
    private static object cast(object v, Type toType, bool fromUI, DocReadOptions options, JsonHandlerAttribute fieldCustomHandler = null)
    {
      //See #264 - the collection inability to cast has no amorphous data so it MUST throw
      //used only for collections inner calls
      if (v==null) return null;

      var customHandler = fieldCustomHandler ?? JsonHandlerAttribute.TryFind(toType);
      if (customHandler != null)
      {
        var castResult = customHandler.TypeCastOnRead(v, toType, fromUI, options);

        if (castResult.Outcome >= JsonHandlerAttribute.TypeCastOutcome.ChangedTargetType) toType = castResult.ToType;

        if (castResult.Outcome == JsonHandlerAttribute.TypeCastOutcome.ChangedSourceValue) v = castResult.Value;
        else if (castResult.Outcome == JsonHandlerAttribute.TypeCastOutcome.HandledCast) return castResult.Value;
      }

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
      if (nntp.IsGenericType && nntp.GetGenericTypeDefinition() == typeof(Nullable<>))
        nntp = toType.GetGenericArguments()[0];

      //20191217 DKh
      if (nntp==typeof(DateTime))
      {
        if (options.LocalDates)
        {
          var d = v.AsDateTime(System.Globalization.DateTimeStyles.AssumeLocal);
          return d;
        }
        else //UTC (the default)
        {
          var d = v.AsDateTime(System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal);
          return d;
        }
      }
      //20191217 DKh


      //Custom JSON Readable (including config)
      if (typeof(IJsonReadable).IsAssignableFrom(nntp) || typeof(IConfigSectionNode).IsAssignableFrom(nntp))
      {
        var toAllocate = nntp;

        //Configuration requires special handling because nodes do not exist as independent entities and there
        //is master/detail relationship between them
        if (toAllocate == typeof(Configuration) ||
            toAllocate == typeof(ConfigSectionNode) ||
            toAllocate == typeof(IConfigSectionNode)) toAllocate = typeof(MemoryConfiguration);

        if (toAllocate.IsAbstract)
          throw new JSONDeserializationException(StringConsts.JSON_DESERIALIZATION_ABSTRACT_TYPE_ERROR.Args(toAllocate.Name, nameof(JsonHandlerAttribute)));

        var newval = SerializationUtils.MakeNewObjectInstance(toAllocate) as IJsonReadable;
        var got = newval.ReadAsJson(v, fromUI, options);//this may re-allocate the result based of newval

        if (!got.match) return null;

        if (typeof(IConfigSectionNode).IsAssignableFrom(nntp)) return (got.self as Configuration)?.Root;

        return got.self;
      }

      //byte[] direct assignment w/o copies
      if (nntp == typeof(byte[]))
      {
        if (v is byte[] passed) return passed;
        //20210717 - #514
        if (v is string str && str.IsNotNullOrWhiteSpace())
        {
          var buff = str.TryFromWebSafeBase64();
          if (buff != null) return buff;
        }
      }


      //field def = []
      if (toType.IsArray)
      {
        var fvseq = v as IEnumerable;
        if (fvseq == null) return null;//can not set non enumerable into array

        var arr = fvseq.Cast<object>().Select(e => cast(e, toType.GetElementType(), fromUI, options, fieldCustomHandler)).ToArray();
        var newval = Array.CreateInstance(toType.GetElementType(), arr.Length);
        for(var i=0; i<newval.Length; i++)
          newval.SetValue(arr[i], i);

        return newval;
      }

      //field def = List<t>
      if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(List<>))
      {
        var fvseq = v as IEnumerable;
        if (fvseq == null) return false;//can not set non enumerable into List<t>

        var arr = fvseq.Cast<object>().Select(e => cast(e, toType.GetGenericArguments()[0], fromUI, options, fieldCustomHandler)).ToArray();
        var newval = SerializationUtils.MakeNewObjectInstance(toType) as IList;
        for (var i = 0; i < arr.Length; i++)
          newval.Add(arr[i]);

        return newval;
      }

      //last resort
      try
      {
        return StringValueConversion.AsType(v.ToString(), toType, false);
      }
      catch
      {
        return null;//the value could not be converted, and is going to go into amorphous bag if it is enabled
      }
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
          data = new JsonDataMap{ {"value", root} };

        return data;
    }

    #endregion
  }
}
