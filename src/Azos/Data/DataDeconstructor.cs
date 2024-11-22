/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.CodeAnalysis.Laconfig;
using Azos.CodeAnalysis.Source;
using Azos.Text;

namespace Azos.Data
{
  /// <summary>
  /// Provides handy extension methods for writing data deconstruction heuristics
  /// </summary>
  public static class DataDeconstructionExtensions
  {
    public const string DECONSTRUCT_PROPERTY = "__deconstruct";

    /// <summary> Returns true if a string value could be deemed as a telephone number </summary>
    public static bool IsPhone(this object v) => v is string sv && DataEntryUtils.CheckTelephone(sv);
    /// <summary> Returns true if a token value could be deemed as a telephone number </summary>
    public static bool IsPhone(this LaconfigToken t) => t != null && (t.IsLiteral || t.IsIdentifier) && t.Value.IsPhone();


    /// <summary> Returns true if a string value could be deemed as an email </summary>
    public static bool IsEmail(this object v) => v is string sv && DataEntryUtils.CheckEMail(sv);
    /// <summary> Returns true if a token value could be deemed as a telephone number </summary>
    public static bool IsEmail(this LaconfigToken t) => t != null && (t.IsLiteral || t.IsIdentifier) && t.Value.IsEmail();

    /// <summary> Returns true if a string value could be deemed as a screen name/id </summary>
    public static bool IsScreenName(this object v) => v is string sv && DataEntryUtils.CheckScreenName(sv);
    /// <summary> Returns true if a token value could be deemed as a screen name </summary>
    public static bool IsScreenName(this LaconfigToken t) => t != null && (t.IsLiteral || t.IsIdentifier) && t.Value.IsScreenName();


    /// <summary> Returns true if a string value could be deemed as a zip code </summary>
    public static bool IsUsZip(this object v)
      => (v is string sv && sv.IsNotNullOrWhiteSpace() && sv.Length == 5 && int.TryParse(sv, out var iv) && iv < 99999) ||
         (v is int iv2 && iv2 > 0 && iv2 < 99999) ||
         (v is long lv && lv > 0 && lv < 99999);

    /// <summary> Returns true if a long value could be deemed as a zip code </summary>
    public static bool IsUsZip(this LaconfigToken t) => t != null && (t.IsLiteral || t.IsIdentifier) && t.Value.IsUsZip();



    /// <summary> Returns true if a value could be deemed as a real number </summary>
    public static bool IsInteger(this object v)
      => v is string sv ? sv.IsNotNullOrWhiteSpace() && int.TryParse(sv, out var _) :
         v is int ? true :
         v is long ? true :
         v is uint ? true :
         v is ulong ? true : false;

    /// <summary> Returns true if a token value could be deemed as a real number </summary>
    public static bool IsInteger(this LaconfigToken t) => t != null && (t.IsNumericLiteral || t.IsLiteral || t.IsIdentifier) && t.Value.IsInteger();

    /// <summary> Returns true if a value could be deemed as a real number </summary>
    public static bool IsReal(this object v)
      => v is string sv ? sv.IsNotNullOrWhiteSpace() && decimal.TryParse(sv, out var _) :
         v is double ? true :
         v is float ? true :
         v is decimal ? true : false;

    /// <summary> Returns true if a token value could be deemed as a real number </summary>
    public static bool IsReal(this LaconfigToken t) => t != null && (t.IsNumericLiteral || t.IsLiteral || t.IsIdentifier) && t.Value.IsReal();

    public static string NormalizePhone(this string v) => DataEntryUtils.NormalizeUSPhone(v);
    public static string NormalizePhone(this LaconfigToken t) => t != null && (t.IsLiteral || t.IsIdentifier) ?  DataEntryUtils.NormalizeUSPhone(t.Value.AsString()) : null;

    /// <summary>
    /// Deconstructs complex data vectors, such as a data filter vectors, from their simplified convoluted representations,
    /// such as a strings or key/value pairs.
    /// For example, given a specific deconstructor implementation passed as `Func` for a `PersonFilter` object,
    /// an implementation will deconstruct as string like "Johnson 90210" into a `PersonFilter` object having its field values set to "LastName='Johnson' and AddressZip='90210'"
    /// as decosntructed from a flat string. Deconstruction is usually based on simple decision-tree based heuristics process.
    /// Server side deconstruction is used to process simple filter strings which look like Google queries - a single line of text coming from clients.
    /// Decosntruction applies heuristics to compose a more complex filter objects based on a simple assumptions,
    ///  such as "a string value which matches Email pattern is most likely an email address"
    /// </summary>
    public static bool DeconstructStringData<TDoc>(this TDoc doc, string value, Func<TDoc, LaconfigToken[], bool> fBody) where TDoc : Doc
    {
      fBody.NonNull(nameof(fBody));
      if (doc == null) return false;
      if (value.IsNullOrWhiteSpace()) return false;
      try
      {
        var src = new StringSource(value);
        var lxr = new LaconfigLexer(src, throwErrors: true);
        var tokens = lxr.ToArray();
        return fBody(doc, tokens);
      }
      catch (Exception cause)
      {
        throw new ValidationException($"Bad deconstruction string clause: {cause.ToMessageWithType()}", cause);
      }
    }

    /// <summary>
    /// Deconstructs complex data vectors, such as a data filter vectors, from their simplified convoluted representations,
    /// such as a strings or key/value pairs.
    /// For example, given a specific deconstructor implementation passed as `Func` for a `PersonFilter` object,
    /// an implementation will deconstruct as string like "Johnson 90210" into a `PersonFilter` object having its field values set to "LastName='Johnson' and AddressZip='90210'"
    /// as decosntructed from a flat string. Deconstruction is usually based on simple decision-tree based heuristics process.
    /// Server side deconstruction is used to process simple filter strings which look like Google queries - a single line of text coming from clients.
    /// Decosntruction applies heuristics to compose a more complex filter objects based on a simple assumptions,
    ///  such as "a string value which matches Email pattern is most likely an email address"
    /// </summary>
    public static bool DeconstructAmorphousData<TDoc>(this TDoc doc, Func<TDoc, object, bool> fBody, string amorphousPropertyName = null) where TDoc : Doc, IAmorphousData
    {
      fBody.NonNull(nameof(fBody));
      if (doc == null) return false;
      if (!doc.AmorphousDataEnabled) return false;
      if (!doc.AmorphousData.TryGetValue(amorphousPropertyName.Default(DECONSTRUCT_PROPERTY), out var value)) return false;
      if (value == null) return false;

      return fBody(doc, value);
    }

    /// <summary>
    /// Deconstructs complex data vectors, such as a data filter vectors, from their simplified convoluted string representations, which are extracted from an amorphous
    /// data named key (See <see cref="DECONSTRUCT_PROPERTY"/> used by default).
    /// For example, given a specific deconstructor implementation passed as `Func` for a `PersonFilter` object,
    /// an implementation will deconstruct as string like "Johnson 90210" into a `PersonFilter` object having its field values set to "LastName='Johnson' and AddressZip='90210'"
    /// as decosntructed from a flat string using Laconfig lexer (tokenizer).
    /// Deconstruction is usually based on simple decision-tree based heuristics process.
    /// Server side deconstruction is used to process simple filter strings which look like Google queries - a single line of text coming from clients.
    /// Decosntruction applies heuristics to compose a more complex filter objects based on a simple assumptions,
    ///  such as "a string value which matches Email pattern is most likely an email address"
    /// </summary>
    public static bool DeconstructAmorphousStringData<TDoc>(this TDoc doc, Func<TDoc, LaconfigToken[], bool> fBody, string amorphousPropertyName = null) where TDoc : Doc, IAmorphousData
    {
      fBody.NonNull(nameof(fBody));
      if (doc == null) return false;
      if (!doc.AmorphousDataEnabled) return false;
      if (!doc.AmorphousData.TryGetValue(amorphousPropertyName.Default(DECONSTRUCT_PROPERTY), out var value)) return false;
      if (value == null) return false;

      var svalue = value.AsString();

      return doc.DeconstructStringData(svalue, fBody);
    }
  }

}
