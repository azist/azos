/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Collections;
using Azos.Conf;

namespace Azos.Data.AST
{
  /// <summary>
  /// Translates an expressions per concrete technology, e.g. MsSql or MongoDb
  /// </summary>
  public abstract class Xlat : INamed
  {
    protected Xlat(string name = null)
    {
      m_Name = name;
      if (m_Name.IsNullOrWhiteSpace()) m_Name = Guid.NewGuid().ToString();
    }

    protected Xlat(IConfigSectionNode conf)
    {
      ConfigAttribute.Apply(this, conf);
      if (m_Name.IsNullOrWhiteSpace()) m_Name = Guid.NewGuid().ToString();
    }

    [Config] private string m_Name;

    /// <summary>
    /// Returns true of the translation handles identifiers as case-sensitive
    /// </summary>
    public abstract bool IsCaseSensitive { get; }

    /// <summary>
    /// Returns a stream of supported unary operators
    /// </summary>
    public abstract IEnumerable<string> UnaryOperators{ get; }

    /// <summary>
    /// Returns a stream of supported binary operators
    /// </summary>
    public abstract IEnumerable<string> BinaryOperators { get; }


    /// <summary>
    /// Name for this translator instance
    /// </summary>
    public string Name => m_Name;

    /// <summary>
    /// Translates expression into base XlatContext
    /// </summary>
    public abstract XlatContext Translate(Expression expression);
  }

  /// <summary>
  /// Generically typed Xlat - derive your custom translations from this class
  /// </summary>
  public abstract class Xlat<TContext> : Xlat where TContext : XlatContext
  {
    protected Xlat(string name = null) : base(name) { }
    protected Xlat(IConfigSectionNode conf) : base(conf) { }

    public sealed override XlatContext Translate(Expression expression)
      => TranslateInContext(expression);

    /// <summary>
    /// Translates expression into specific TContext type
    /// </summary>
    public abstract TContext TranslateInContext(Expression expression);
  }

  /// <summary>
  /// Translation context base
  /// </summary>
  public abstract class XlatContext
  {
    protected XlatContext(Xlat xlat)
    {
      m_Translator = xlat.NonNull(nameof(xlat));
    }

    private Xlat m_Translator;

    /// <summary>
    /// References Xlat that this context is for
    /// </summary>
    public Xlat Translator => m_Translator;

    /// <summary> Implements visitor pattern for ValueExpression</summary>
    public abstract object Visit(ValueExpression value);

    /// <summary> Implements visitor pattern for ArrayValueExpression</summary>
    public abstract object Visit(ArrayValueExpression array);

    /// <summary> Implements visitor pattern for IdentifierExpression</summary>
    public abstract object Visit(IdentifierExpression id);

    /// <summary> Implements visitor pattern for UnaryExpression</summary>
    public abstract object Visit(UnaryExpression unary);

    /// <summary> Implements visitor pattern for BinaryExpression</summary>
    public abstract object Visit(BinaryExpression binary);
  }

  /// <summary>
  /// Generically typed xlat context base - derive your implementations form this class
  /// </summary>
  public abstract class XlatContext<TXlat> : XlatContext where TXlat : Xlat
  {
    protected XlatContext(TXlat xlat) : base (xlat){ }

    /// <summary>
    /// References Xlat of TXlat type that this context is for
    /// </summary>
    public new TXlat Translator => (TXlat)base.Translator;
  }

}
