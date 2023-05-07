/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading.Tasks;

using Azos.Text;

namespace Azos.Serialization.JSON
{
  /// <summary>
  /// Writes primitive types, JsonDataObjects, JsonDynamicObjects, IEnumerable and IDictionary - implementers into string or stream.
  /// Can also write IJsonWritable-implementing types that directly serialize their state into JSON.
  /// This class does not serialize regular CLR types (that do not implement IJsonWritable), use JsonSerializer for full functionality
  /// </summary>
  public static class JsonWriter
  {
    /// <summary>
    /// Writes JSON data to a file
    /// </summary>
    public static void WriteToFile(object data, string  fileName, JsonWritingOptions options = null, Encoding encoding = null)
    {
        using(var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            Write(data, fs, options, encoding);
    }

    /// <summary>
    /// Writes JSON data to a file
    /// </summary>
    public static Task WriteToFileAsync(object data, string fileName, JsonWritingOptions options = null, Encoding encoding = null)
    {
      using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
      return WriteAsync(data, fs, options, encoding);
    }

    /// <summary>
    /// Writes JSON data to a new byte[]
    /// </summary>
    public static byte[] WriteToBuffer(object data, JsonWritingOptions options = null, Encoding encoding = null)
    {
      using(var ms = new MemoryStream())
      {
        Write(data, ms, options, encoding);
        return ms.ToArray();
      }
    }

    /// <summary>
    /// Writes JSON data to a stream
    /// </summary>
    public static void Write(object data, Stream stream, JsonWritingOptions options = null, Encoding encoding = null)
    {
        using(var writer = new StreamWriter(stream, encoding ?? UTF8Encoding.UTF8))
            Write(data, writer, options);
    }

    /// <summary>
    /// Writes JSON data to a stream
    /// </summary>
    public static Task WriteAsync(object data, Stream stream, JsonWritingOptions options = null, Encoding encoding = null)
    {
      using var writer = new StreamWriter(stream, encoding ?? UTF8Encoding.UTF8);
      #warning Async json refactor Az #731
      Write(data, writer, options);
      return Task.CompletedTask;
    }

    /// <summary>
    /// Writes JSON data into a string
    /// </summary>
    public static string Write(object data, JsonWritingOptions options = null, IFormatProvider formatProvider = null)
    {
      if (options==null) options = JsonWritingOptions.CompactRowsAsMap;

      var sb = new StringBuilder(0xff);
      using( var wri =  formatProvider==null ?
                            new StringWriter( sb ) :
                            new StringWriter( sb, formatProvider ) )
      {
        writeAny(wri, data, 0, options);
      }

      return sb.ToString();
    }

    /// <summary>
    /// Appends JSON data into the instance of TextWriter
    /// </summary>
    public static void Write(object data, TextWriter wri, JsonWritingOptions options = null)
    {
      if (options==null) options = JsonWritingOptions.Compact;

      writeAny(wri, data, 0, options);
    }

    /// <summary>
    /// Appends JSON data into the instance of TextWriter
    /// </summary>
    public static Task WriteAsync(object data, TextWriter wri, JsonWritingOptions options = null)
    {
      if (options == null) options = JsonWritingOptions.Compact;

      #warning Json async #731
      writeAny(wri, data, 0, options);

      return Task.CompletedTask;
    }

    /// <summary>
    /// Appends JSON representation of a map(IDictionary)
    /// </summary>
    public static void WriteMap(TextWriter wri, IDictionary data, int level, JsonWritingOptions options = null)
    {
      if (options==null) options = JsonWritingOptions.Compact;

      writeMap(wri, data, level, options);
    }

    /// <summary>
    /// Appends JSON representation of a map(IDictionary)
    /// </summary>
    public static Task WriteMapAsync(TextWriter wri, IDictionary data, int level, JsonWritingOptions options = null)
    {
      if (options == null) options = JsonWritingOptions.Compact;
      #warning Json async #731
      writeMap(wri, data, level, options);
      return Task.CompletedTask;
    }

    /// <summary>
    /// Appends JSON representation of a map(IEnumerable(DictionaryEntry))
    /// </summary>
    public static void WriteMap(TextWriter wri, IEnumerable<DictionaryEntry> data, int level, JsonWritingOptions options = null)
    {
      if (options==null) options = JsonWritingOptions.Compact;

      writeMap(wri, data, level, options);
    }

    /// <summary>
    /// Appends JSON representation of a map(IEnumerable(DictionaryEntry))
    /// </summary>
    public static Task WriteMapAsync(TextWriter wri, IEnumerable<DictionaryEntry> data, int level, JsonWritingOptions options = null)
    {
      if (options == null) options = JsonWritingOptions.Compact;
      #warning Json async #731
      writeMap(wri, data, level, options);
      return Task.CompletedTask;
    }

    /// <summary>
    /// Appends JSON representation of a map(IEnumerable(DictionaryEntry))
    /// </summary>
    public static void WriteMap(TextWriter wri, int level, JsonWritingOptions options, params DictionaryEntry[] data)
    {
      if (options==null) options = JsonWritingOptions.Compact;

      writeMap(wri, data, level, options);
    }

    /// <summary>
    /// Appends JSON representation of a map(IEnumerable(DictionaryEntry))
    /// </summary>
    public static Task WriteMapAsync(TextWriter wri, int level, JsonWritingOptions options, params DictionaryEntry[] data)
    {
      if (options == null) options = JsonWritingOptions.Compact;
#warning Json async #731
      writeMap(wri, data, level, options);
      return Task.CompletedTask;
    }

    /// <summary>
    /// Appends JSON representation of an IEnumerable
    /// </summary>
    public static void WriteArray(TextWriter wri, IEnumerable data, int level, JsonWritingOptions options)
    {
        if (options==null) options = JsonWritingOptions.Compact;

        writeArray(wri, data, level, options);
    }

    /// <summary>
    /// Appends JSON representation of an IEnumerable
    /// </summary>
    public static Task WriteArrayAsync(TextWriter wri, IEnumerable data, int level, JsonWritingOptions options)
    {
      if (options == null) options = JsonWritingOptions.Compact;
#warning Json async #731
      writeArray(wri, data, level, options);
      return Task.CompletedTask;
    }


    /// <summary>
    /// Writes a string in JSON format (a la "JSON encode string") - using quotes and escaping characters that need it
    /// </summary>
    /// <param name="wri">TextWriter instance to append data into</param>
    /// <param name="data">Original string to encode as JSON</param>
    /// <param name="opt">JSONWriting options instance, if omitted then JSONWritingOptions.Compact is used</param>
    /// <param name="thint">Optional type hint</param>
    public static void EncodeString(TextWriter wri, string data, JsonWritingOptions opt = null, Atom thint = default)
    {
      if (data.IsNullOrEmpty())
      {
        wri.Write("\"\"");
        return;
      }

      if (opt==null)
      {
        opt = JsonWritingOptions.Compact;
      }

      wri.Write('"');//open string

      //#864 DKh 20230506 Type hint
      if (opt.EnableTypeHints)
      {
        if (thint.IsZero && TypeHint.StringNeedsEscape(data))
        {
          thint = TypeHint.THINT_STR;
        }

        if (!thint.IsZero)
        {
          var str_thint = thint.Value;
          (str_thint.Length == 3).IsTrue("thint.len==3");
          wri.Write(TypeHint.CHR_0);
          wri.Write(str_thint);
          wri.Write(TypeHint.CHR_4);
        }
      }//#864 DKh 20230506 Type hint

      for (int i = 0; i < data.Length; i++)
      {
        char c = data[i];
        if (c > 0x7f && opt.ASCIITarget)
        {
          wri.Write("\\u");
          wri.Write(((int)c).ToString("x4"));
          continue;
        }

        switch (c)
        {
          case '\\':  { wri.Write(@"\\"); break; }
          case '/':   { wri.Write(@"\/"); break; }
          case (char)CharCodes.Char0:     { wri.Write(@"\u0000"); break; }
          case (char)CharCodes.AlertBell: { wri.Write(@"\u"); wri.Write(((int)c).ToString("x4")); break; }
          case (char)CharCodes.Backspace: { wri.Write(@"\b"); break; }
          case (char)CharCodes.Formfeed:  { wri.Write(@"\f"); break; }
          case (char)CharCodes.LF:        { wri.Write(@"\n"); break; }
          case (char)CharCodes.CR:        { wri.Write(@"\r"); break; }
          case (char)CharCodes.Tab:       { wri.Write(@"\t"); break; }

          case '"':  { wri.Write(@"\"""); break; }

          default:
          {
            if (c < 0x20)
            {
              wri.Write("\\u");
              wri.Write(((int)c).ToString("x4"));
              break;
            }
            wri.Write(c);
            break;
          }
        }

      }//for

      wri.Write('"');//close string
    }

    /// <summary>
    /// Writes a string in JSON format (a la "JSON encode string") - using quotes and escaping characters that need it
    /// </summary>
    /// <param name="wri">TextWriter instance to append data into</param>
    /// <param name="data">Original string to encode as JSON</param>
    /// <param name="opt">JSONWriting options instance, if omitted then JSONWritingOptions.Compact is used</param>
    /// <param name="utcOffset">UTC offset override. If not supplied then offset form local time zone is used</param>
    public static void EncodeDateTime(TextWriter wri, DateTime data, JsonWritingOptions opt = null, TimeSpan? utcOffset = null)
    {
      if (opt==null) opt = JsonWritingOptions.Compact;

      if (!opt.ISODates)
      {
          wri.Write("new Date({0})".Args( data.ToMillisecondsSinceUnixEpochStart() ));
          return;
      }

      wri.Write('"');
      var year = data.Year;
      if (year>999) wri.Write(year);
      else if (year>99) { wri.Write('0'); wri.Write(year); }
      else if (year>9) { wri.Write("00"); wri.Write(year); }
      else if (year>0) { wri.Write("000"); wri.Write(year); }

      wri.Write('-');

      var month = data.Month;
      if (month>9) wri.Write(month);
      else { wri.Write('0'); wri.Write(month); }

      wri.Write('-');

      var day = data.Day;
      if (day>9) wri.Write(day);
      else { wri.Write('0'); wri.Write(day); }

      wri.Write('T');

      var hour = data.Hour;
      if (hour>9) wri.Write(hour);
      else { wri.Write('0'); wri.Write(hour); }

      wri.Write(':');

      var minute = data.Minute;
      if (minute>9) wri.Write(minute);
      else { wri.Write('0'); wri.Write(minute); }

      wri.Write(':');

      var second = data.Second;
      if (second>9) wri.Write(second);
      else { wri.Write('0'); wri.Write(second); }

      var ms = data.Millisecond;
      if (ms>0)
      {
        wri.Write('.');

        if (ms>99) wri.Write(ms);
        else if (ms>9) { wri.Write('0'); wri.Write(ms); }
        else { wri.Write("00"); wri.Write(ms); }
      }

      if (data.Kind==DateTimeKind.Utc)
      {
        wri.Write('Z');
      }
      else
      {
        var offset = utcOffset==null ? TimeZoneInfo.Local.GetUtcOffset(data) : utcOffset.Value;

        wri.Write( offset.Ticks<0 ? '-' : '+' );

        hour = Math.Abs(offset.Hours);
        if (hour>9) wri.Write(hour);
        else { wri.Write('0'); wri.Write(hour); }

        wri.Write(':');

        minute = Math.Abs(offset.Minutes);
        if (minute>9) wri.Write(minute);
        else { wri.Write('0'); wri.Write(minute); }
      }

      wri.Write('"');
    }


    #region .pvt .impl
    private static void indent(TextWriter wri, int level, JsonWritingOptions opt)
    {
      if (opt.IndentWidth==0) return;
      var total = level * opt.IndentWidth;
      for(var i=0; i < total; i++)
      {
        wri.Write(' ');
      }
    }


    private static void writeAny(TextWriter wri, object data, int level, JsonWritingOptions opt)
    {
      if (data==null)
      {
        wri.Write("null");//do NOT LOCALIZE!
        return;
      }

      if (level>opt.MaxNestingLevel)
          throw new JSONSerializationException(StringConsts.JSON_SERIALIZATION_MAX_NESTING_EXCEEDED_ERROR.Args(opt.MaxNestingLevel));

      if (data is string)
      {
        EncodeString(wri, (string)data, opt);
        return;
      }

      //20210717 - #514
      if (data is byte[] buff && buff.Length > 8)
      {
        EncodeString(wri, buff.ToWebSafeBase64(), opt);
        return;
      }

      if (data is bool)//the check is here for speed
      {
        wri.Write( ((bool)data) ? "true" : "false");//do NOT LOCALIZE!
        return;
      }

      if (data is int || data is long)//the check is here for speed
      {
        wri.Write( ((IConvertible)data).ToString(System.Globalization.CultureInfo.InvariantCulture) );
        return;
      }

      if (data is double || data is float || data is decimal)//the check is here for speed
      {
        wri.Write( ((IConvertible)data).ToString(System.Globalization.CultureInfo.InvariantCulture) );
        return;
      }

      if (data is DateTime)
      {
        EncodeDateTime(wri, (DateTime)data, opt);
        return;
      }

      if (data is TimeSpan)
      {
        var ts = (TimeSpan)data;
        wri.Write(ts.Ticks);
        return;
      }

      if (data is Guid)
      {
        var guid = (Guid)data;
        wri.Write('"');
        wri.Write(guid.ToString("D"));
        wri.Write('"');
        return;
      }

      if (data is IJsonWritable)//these types know how to directly write themselves
      {
        ((IJsonWritable)data).WriteAsJson(wri, level, opt);
        return;
      }

      if (data is JsonDynamicObject)//unwrap dynamic
      {
        writeAny(wri, ((JsonDynamicObject)data).Data, level, opt);
        return;
      }


      if (data is IDictionary)//must be BEFORE IEnumerable
      {
        writeMap(wri, (IDictionary)data, level, opt);
        return;
      }

      if (data is IEnumerable)
      {
        writeArray(wri, (IEnumerable)data, level, opt);
        return;
      }

      var tdata = data.GetType();
      if (tdata.IsPrimitive || tdata.IsEnum)
      {
        string val;
        if (data is IConvertible)
          val = ((IConvertible)data).ToString(System.Globalization.CultureInfo.InvariantCulture);
        else
          val = data.ToString();

        EncodeString(wri, val, opt);
        return;
      }

      var fields = SerializationUtils.GetSerializableFields(tdata);

      var dict = fields.Select(
      f =>
      {
        var name = f.Name;
        var iop = name.IndexOf('<');
        if (iop>=0)//handle anonymous type field name
        {
              var icl = name.IndexOf('>');
              if (icl>iop+1)
                  name = name.Substring(iop+1, icl-iop-1);
        }

        return new DictionaryEntry(name, f.GetValue(data));
      });//select


      writeMap(wri, dict, level, opt);
    }

    private struct dictEnumberable : IEnumerable<DictionaryEntry>
    {
      public dictEnumberable(IDictionary dict) { Dictionary = dict;}

      private readonly IDictionary Dictionary;

      public IEnumerator<DictionaryEntry> GetEnumerator()
      {
        return new dictEnumerator(Dictionary.GetEnumerator());
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return Dictionary.GetEnumerator();
      }
    }

    private struct dictEnumerator : IEnumerator<DictionaryEntry>
    {

      public dictEnumerator(IDictionaryEnumerator enumerator) { Enumerator = enumerator;}

      private readonly IDictionaryEnumerator Enumerator;

      public DictionaryEntry Current { get { return (DictionaryEntry)Enumerator.Current; } }

      public void Dispose() {}

      object IEnumerator.Current { get { return Enumerator.Current; } }

      public bool MoveNext() { return Enumerator.MoveNext(); }

      public void Reset() { Enumerator.Reset();}
    }


    private static void writeMap(TextWriter wri, IDictionary data, int level, JsonWritingOptions opt)
    {
        writeMap(wri, new dictEnumberable(data), level, opt);
    }

    private static void writeMap(TextWriter wri, IEnumerable<DictionaryEntry> data, int level, JsonWritingOptions opt)
    {
      if (level>0) level++;

      if (opt.ObjectLineBreak)
      {
        wri.WriteLine();
        indent(wri, level, opt);
      }

      wri.Write('{');

      //20200615 DKh #304
      if (opt.MapSortKeys)
      {
        data = data.OrderBy( kvp => (kvp.Key?.ToString()).Default(string.Empty), StringComparer.Ordinal);
      }

      var first = true;
      foreach(DictionaryEntry entry in data)
      {
        if (opt.MapSkipNulls)
        {
          if (entry.Value == null) continue;

          // NLSMap is a special type which is treated as a ref type for perf optimization
          if (entry.Value is NLSMap && ((NLSMap)entry.Value).Count == 0) continue;
        }

        if (!first)
          wri.Write(opt.SpaceSymbols ? ", " : ",");

        if (opt.MemberLineBreak)
        {
          wri.WriteLine();
          indent(wri, level+1, opt);
        }
        EncodeString(wri, entry.Key.ToString(), opt);
        wri.Write(opt.SpaceSymbols ? ": " : ":");
        writeAny(wri, entry.Value, level+1, opt);
        first = false;
      }

      if (!first && opt.MemberLineBreak)
      {
        wri.WriteLine();
        indent(wri, level, opt);
      }

      wri.Write('}');
    }

    private static void writeArray(TextWriter wri, IEnumerable data, int level, JsonWritingOptions opt)
    {
      wri.Write('[');

      var first = true;
      foreach(var elm in data)
      {
        if (!first)
        {
          wri.Write(opt.SpaceSymbols ? ", " : ",");
        }

        writeAny(wri, elm, level+1, opt);

        first = false;
      }

      wri.Write(']');
    }
    #endregion
  }
}
