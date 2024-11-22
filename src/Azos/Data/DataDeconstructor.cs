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
    public const string DECONSTRUCT_PROPERTY = "__deconstruct";


    protected DataDeconstructor(TDoc doc)
    {
      DataDoc = doc.NonNull(nameof(doc));
    }

    protected readonly TDoc DataDoc;

    public abstract bool Deconstruct(TValue value);
  }

  /// <summary>
  /// Deconstructs complex data convoluted into a string using Laconic lexer tokenization rules.
  /// <inheritdoc/>
  /// </summary>
  public abstract class StringDataDeconstructor<TDoc> : DataDeconstructor<TDoc, string> where TDoc : Doc
  {
    protected StringDataDeconstructor(TDoc doc) : base(doc)
    {
    }

    public override bool Deconstruct(string value)
    {
      if (value.IsNullOrWhiteSpace()) return false;
      var src = new StringSource(value);
      var lxr = new LaconfigLexer(src);

      //what about errors?
      var tokens = lxr.ToArray();

      return DoDeconstructFromTokens(tokens);
    }

    protected abstract bool DoDeconstructFromTokens(LaconfigToken[] tokens);
  }

  /// <summary>
  /// Deconstructs typical business data like, ZIP codes, Phones, EMails, Street addresses and Names.
  /// <inheritdoc/>
  /// </summary>
  public abstract class TypicalBusinessStringDataDeconstructor<TDoc> : StringDataDeconstructor<TDoc> where TDoc : Doc
  {
    protected TypicalBusinessStringDataDeconstructor(TDoc doc) : base(doc)
    {
    }

    public virtual bool SenseEmail  => true;
    public virtual bool SenseUsZip  => true;
    public virtual bool SensePhone  => true;
    public virtual bool SenseStreet => true;
    public virtual bool SenseNames  => true;


    protected override bool DoDeconstructFromTokens(LaconfigToken[] tokens)
    {
      throw new NotImplementedException();
    }
  }
}
