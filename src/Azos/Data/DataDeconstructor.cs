/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.CodeAnalysis.Laconfig;
using Azos.CodeAnalysis.Source;

namespace Azos.Data
{
  /// <summary>
  /// Facilitates creation of classes which deconstruct complex data vectors, such as a data filter vectors,
  /// from their simplified convoluted representations, such as a strings or key/value pairs.
  /// For example, given a specific deconstructor implementation for a `PersonFilter` object,
  /// an implementation will deconstruct as string like "Johnson 90210" into a `PersonFilter` object having its field values set to "LastName='Johnson' and AddressZip='90210'"
  /// as decosntructed from a flat string. Deconstruction is usually based on simple decision-tree based heuristics process.
  /// Server side deconstruction is used to process simple filter strings which look like Google queries - a single line of text coming from clients.
  /// Decosntruction applies heuristics to compose a more complex filter objects based on a simple assumptions,
  ///  such as "a string value which matches Email pattern is most likely an email address"
  /// </summary>
  public abstract class DataDeconstructor<TDoc, TValue> where TDoc : Doc
  {
    protected DataDeconstructor(TDoc doc)
    {
      m_Doc = doc.NonNull(nameof(doc));
    }

    protected readonly TDoc m_Doc;

    public TDoc DataDoc => m_Doc;

    /// <summary>
    /// Deconstructs a value into this document. Returns true if value was deconstructed, or false when nothing was decosntructed
    /// e.g., when the data was logically empty/blank.
    /// Errors are thrown wrapped in <see cref="ValidationException"/> (e.g. invalid syntax)
    /// </summary>
    public abstract bool Deconstruct(TValue value);
  }

  /// <summary>
  /// Deconstructs complex data convoluted into a string using Laconic lexer tokenization rules.
  /// <inheritdoc/>
  /// </summary>
  public abstract class StringDataDeconstructor<TDoc> : DataDeconstructor<TDoc, string> where TDoc : Doc
  {
    public StringDataDeconstructor(TDoc doc) : base(doc)
    {
    }


    /// <inheritdoc/>
    public override bool Deconstruct(string value)
    {
      if (value.IsNullOrWhiteSpace()) return false;
      try
      {
        var src = new StringSource(value);
        var lxr = new LaconfigLexer(src, throwErrors: true);
        var tokens = lxr.ToArray();
        return DoDeconstructFromTokens(tokens);
      }
      catch (Exception cause)
      {
        throw new ValidationException($"Bad `{this.GetType().DisplayNameWithExpandedGenericArgs()}` clause: {cause.ToMessageWithType()}", cause);
      }
    }

    protected abstract bool DoDeconstructFromTokens(LaconfigToken[] tokens);
  }


  /// <summary>
  /// Provides handy extension methods for writing data deconstruction heuristics
  /// </summary>
  public static class DataDeconstructionExtensions
  {
    public const string DECONSTRUCT_PROPERTY = "__deconstruct";

    public static bool IsPhone(this string v) => false;
    public static bool IsEmail(this string v) => false;
    public static bool IsZip(this string v) => false;
    public static bool IsInteger(this string v) => false;
    public static bool IsReal(this string v) => false;




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
        throw new ValidationException($"Bad deconstruction clause: {cause.ToMessageWithType()}", cause);
      }
    }


    public static bool DeconstructAmorphousData<TDoc>(this TDoc doc, Func<TDoc, object, bool> fBody, string amorphousPropertyName = null) where TDoc : Doc, IAmorphousData
    {
      fBody.NonNull(nameof(fBody));
      if (doc == null) return false;
      if (!doc.AmorphousDataEnabled) return false;
      if (!doc.AmorphousData.TryGetValue(amorphousPropertyName.Default(DECONSTRUCT_PROPERTY), out var value)) return false;
      if (value == null) return false;

      return fBody(doc, value);
    }
  }

}
