using System;
using System.Collections.Generic;
using System.Text;

using Azos.Collections;

namespace Azos.Data.AST
{
  /// <summary>
  /// Translates the expressions per concrete technology, e.g. MsSql or MongoDb
  /// </summary>
  public abstract class Xlat : INamed
  {

    public abstract bool IsCaseSensitive { get; }
    public abstract IEnumerable<string> UnaryOperators{ get; }
    public abstract IEnumerable<string> BinaryOperators { get; }

    public string Name => throw new NotImplementedException();

    public abstract XlatContext Translate(Expression expression);
  }

  /// <summary>
  /// Generically typed Xlat - derive your custom translations from this class
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  public abstract class Xlat<TContext> : Xlat where TContext : XlatContext
  {
    public sealed override XlatContext Translate(Expression expression)
      => TranslateInContext(expression);

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

    public Xlat Translator => m_Translator;

    public abstract void Visit(ValueExpression value);
    public abstract void Visit(IdentifierExpression value);
    public abstract void Visit(UnaryExpression value);
    public abstract void Visit(BinaryExpression value);
  }

  /// <summary>
  /// Generically typed xlat context base - derive your implementations form this class
  /// </summary>
  /// <typeparam name="TXlat"></typeparam>
  public abstract class XlatContext<TXlat> : XlatContext where TXlat : Xlat
  {
    protected XlatContext(TXlat xlat) : base (xlat)
    {
    }

    public new TXlat Translator => (TXlat)base.Translator;
  }

}
