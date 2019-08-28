/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using Azos.Data;

namespace Azos
{
  /// <summary>
  /// Provides Web-related utility functions used by the majority of projects
  /// </summary>
  public static class WebUtils
  {
    public static readonly char[] PATH_JOIN_TRIM_CHARS = new char[] { '/', ' ', '\\' };

    /// <summary>
    /// JavaScript reserved words
    /// </summary>
    public static readonly HashSet<string> JS_RESERVED_WORDS = new HashSet<string>(StringComparer.Ordinal)
    {
      "abstract"  ,"arguments"   ,"await"          ,"boolean"     ,
      "break"     ,"byte"        ,"case"           ,"catch"       ,
      "char"      ,"class"       ,"const"          ,"continue"    ,
      "debugger"  ,"default"     ,"delete"         ,"do"          ,
      "double"    ,"else"        ,"enum"           ,"eval"        ,
      "export"    ,"extends"     ,"false"          ,"final"       ,
      "finally"   ,"float"       ,"for"            ,"function"    ,
      "goto"      ,"if"          ,"implements"     ,"import"      ,
      "in"        ,"instanceof"  ,"int"            ,"interface"   ,
      "let"       ,"long"        ,"native"         ,"new"         ,
      "null"      ,"package"     ,"private"        ,"protected"   ,
      "public"    ,"return"      ,"short"          ,"static"      ,
      "super"     ,"switch"      ,"synchronized"   ,"this"        ,
      "throw"     ,"throws"      ,"transient"      ,"true"        ,
      "try"       ,"typeof"      ,"var"            ,"void"        ,
      "volatile"  ,"while"       ,"with"           ,"yield"       ,
    };


    /// <summary>
    /// Strip simple html from string
    /// </summary>
    public static string StripSimpleHtml(this string str)
    {
      return WebUtility.HtmlDecode(Regex.Replace(str, @"<[^>]+>|&nbsp;", "").Trim());
    }

    /// <summary>
    /// Returns true when supplied name can be used for JS identifier naming (var names, func/class names etc..)
    /// </summary>
    public static bool IsValidJSIdentifier(this string id)
    {
      if (id.IsNullOrWhiteSpace()) return false;

      if (JS_RESERVED_WORDS.Contains(id)) return false;

      for (int i = 0; i < id.Length; i++)
      {
        char c = id[i];
        if (c == '$' || c == '_') continue;
        if (!Char.IsLetterOrDigit(c) || (i == 0 && !Char.IsLetter(c))) return false;
      }

      return true;
    }

    /// <summary>
    /// Escapes JS literal, replacing / \ \r \n " ' &lt; &gt; &amp; chars with their hex codes
    /// </summary>
    public static string EscapeJSLiteral(this string value)
    {
      if (value == null) return null;
      if (value.Length == 0) return string.Empty;

      var sb = new StringBuilder();
      for (var i = 0; i < value.Length; i++)
      {
        var c = value[i];
        if (c < 0x20 || //space
            c == '\'' || c == '"' ||
            c == '/' || c == '\\' ||
            c == '&' || c == '<' || c == '>')
        {
          sb.Append(@"\x");
          var nibble = (c >> 4) & 0x0f; //this works faster than int to string hex
          sb.Append((char)(nibble <= 9 ? '0' + nibble : 'A' + (nibble - 10)));
          nibble = c & 0x0f;
          sb.Append((char)(nibble <= 9 ? '0' + nibble : 'A' + (nibble - 10)));
          continue;
        }

        sb.Append(c);
      }
      return sb.ToString();
    }


    /// <summary>
    /// Joins URI path segments with "/". This function just concats strings, it does not evaluate relative paths etc.
    /// The first segment may or may not start with '/'
    /// </summary>
    public static string JoinPathSegs(params string[] segments)
    {
      if (segments == null || segments.Length == 0) return string.Empty;

      var sb = new StringBuilder();

      var first = true;
      for (var i = 0; i < segments.Length; i++)
      {
        var seg = segments[i];
        if (seg == null) continue;

        seg = (first ? seg.TrimStart(' ') : seg.TrimStart(PATH_JOIN_TRIM_CHARS)).TrimEnd(PATH_JOIN_TRIM_CHARS);

        if (seg.IsNullOrWhiteSpace()) continue;

        if (!first) sb.Append('/');
        sb.Append(seg);
        first = false;
      }

      return sb.ToString();
    }

    /// <summary>
    /// Performs escaping plus sign in URL into its hex value
    /// </summary>
    public static string EscapeURIStringWithPlus(this string uri)
    {
      const string PQ = "%2B";

      if (uri.IsNullOrWhiteSpace()) return string.Empty;

      var builder = new StringBuilder();
      for (int i = 0; i < uri.Length; i++)
      {
        var c = uri[i];
        if (c == '+') builder.Append(PQ);
        else builder.Append(c);
      }

      return builder.ToString();
    }

    /// <summary>
    /// Performs URI.EscapeDataString with additional replacement of " and ' chars with their hex equivalents.
    /// This method is suitable for escaping client-side intelligent keys that may have single/double quotes
    /// </summary>
    public static string EscapeURIDataStringWithQuotes(this string uri)
    {
      const string SQ = "%27";
      const string DQ = "%22";

      if (uri.IsNullOrWhiteSpace()) return string.Empty;

      var ds = Uri.EscapeDataString(uri);
      var result = new StringBuilder();
      for (var i = 0; i < ds.Length; i++)
      {
        var c = ds[i];
        if (c == '\'') result.Append(SQ);
        else
        if (c == '"') result.Append(DQ);
        else
          result.Append(c);
      }

      return result.ToString();
    }


    /// <summary>
    /// Escapes all chars except latin A..Z, 0..9, and . - _
    /// This function is based on EscapeDataString but does not depend on .NET 4/4.5 differences encoding ! * ( ) and others...
    /// </summary>
    public static string EscapeAllDataStringChars(this string str)
    {
      if (str.IsNullOrWhiteSpace()) return string.Empty;

      str = Uri.EscapeDataString(str);//.NET <4.5.2 does not encode ! * ( ) properly

      var sb = new StringBuilder();
      for (var i = 0; i < str.Length; i++)
      {
        var ch = str[i];
        if ((ch >= 'A' && ch <= 'Z') ||
            (ch >= 'a' && ch <= 'z') ||
            (ch >= '0' && ch <= '9') ||
            (ch == '%' || ch == '-' || ch == '_' || ch == '.')
           )
        {
          sb.Append(ch);
          continue;
        }
        sb.Append(Uri.HexEscape(ch));
      }

      return sb.ToString();
    }

    /// <summary>
    /// Returns a URL query string representation of KVP[string, object]
    /// </summary>
    public static string ComposeURLQueryString(IDictionary<string, object> qParams)
    {
      if (qParams == null || qParams.Count < 1)
        return string.Empty;

      var builder = new StringBuilder();
      foreach (var kvp in qParams)
      {
        if (kvp.Key.IsNullOrWhiteSpace()) continue;

        builder.Append(EscapeAllDataStringChars(kvp.Key));
        if (kvp.Value != null)
          builder.Append('=').Append(EscapeAllDataStringChars(kvp.Value.AsString()));
        builder.Append('&');
      }

      if (builder.Length > 0) builder.Length--;

      return builder.ToString();
    }


  }

  /// <summary>
  /// Facilitates construction of URI queries escaping all values as needed
  /// </summary>
  public sealed class UriQueryBuilder : IEnumerable<KeyValuePair<string, object>>
  {
    public UriQueryBuilder(){ }
    public UriQueryBuilder(string uri) { Uri = uri; }


    public readonly string Uri;
    public readonly Dictionary<string, object> Data = new Dictionary<string, object>();

    public void Add(string key, object value) => Data.Add(key, value);

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();

    public override string ToString()
    {
      var query = WebUtils.ComposeURLQueryString(Data);
      return Uri.IsNotNullOrEmpty() ? Uri + (query.IsNotNullOrWhiteSpace()? "?" : "") + query : query;
    }
  }


}
