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

  public abstract class Xlat<TContext> : Xlat where TContext : XlatContext
  {
    public sealed override XlatContext Translate(Expression expression)
      => TranslateInContext(expression);

    public abstract TContext TranslateInContext(Expression expression);
  }


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

  public abstract class XlatContext<TXlat> : XlatContext where TXlat : Xlat
  {
    protected XlatContext(TXlat xlat) : base (xlat)
    {
    }

    private Xlat m_Translator;

    public new TXlat Translator => base.Translator as TXlat;
  }

}
