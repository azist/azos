/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Azos.Conf;
using QDoc = Azos.Data.Access.MongoDb.Connector.Query;
using Azos.Serialization.BSON;
using Azos.Data.Access.MongoDb;

namespace Azos.Data.AST
{
  /// <summary>
  /// Implements expression translation for Mongo database by
  /// generating Mongo Query BSON document
  /// </summary>
  public class MongoDbXlat : Xlat<MongoDbXlatContext>
  {
    protected MongoDbXlat(string name = null) : base(name) { }
    protected MongoDbXlat(IConfigSectionNode conf) : base(conf) { }

#pragma warning disable 0649
    [Config] private bool m_IsCaseSensitive;
#pragma warning restore 0649

    public override bool IsCaseSensitive => m_IsCaseSensitive;

    /// <summary>
    /// Assigns an optional functor which gets called during identifier validation,
    /// this is handy in many business-specific cases to
    /// limit the column names without having to override this class
    /// </summary>
    public Func<IdentifierExpression, bool> IdentifierFilter { get; set; }


    /// <summary>
    /// Translates root AST expression for ORACLE RDBMS
    /// </summary>
    public override MongoDbXlatContext TranslateInContext(Expression expression)
    {
      var result = new MongoDbXlatContext(this);
      expression.Accept(result);
      return result;
    }

    /// <summary>
    /// Returns a stream of supported unary operators
    /// </summary>
    public override IEnumerable<string> UnaryOperators => UNARY_OPS.Keys;

    /// <summary>
    /// Returns a stream of supported binary operators
    /// </summary>
    public override IEnumerable<string> BinaryOperators => BINARY_OPS.Keys;


    public static readonly Dictionary<string, string> UNARY_OPS = new Dictionary<string, string>(StringComparer.Ordinal)
    {
       {"!", "$not"},
       {"not", "$not"},

       {"in", "$in"},

       {"nin", "$nin"},

       {"exists", "$exists"}
    };

    public static readonly Dictionary<string, string> BINARY_OPS = new Dictionary<string, string>(StringComparer.Ordinal)
    {
       {"=", "$eq"},
       {"==", "$eq"},

       {"!=", "$ne"},
       {"<>", "$ne"},

       {"<", "$lt"},
       {">", "$gt"},

       {"<=", "$lte"},
       {">=", "$gte"},

       {"and", "$and"},
       {"&", "$and"},
       {"&&", "$and"},

       {"or", "$or"},
       {"|", "$or"},
       {"||", "$or"},
    };


  }



  /// <summary>
  /// Provides abstraction for Mongo Db translation contexts
  /// </summary>
  public class MongoDbXlatContext : XlatContext<MongoDbXlat>
  {
    public MongoDbXlatContext(MongoDbXlat xlat) : base(xlat)
    {
      m_Query = new QDoc();
    }

    protected QDoc m_Query;
    protected BSONElement m_Operand;

    /// <summary>
    /// Returns built Mongo DB Query Document
    /// </summary>
    public QDoc Query => m_Query;


    /// <summary>
    /// Maps unary operator e.g. "!" -> "$not"
    /// </summary>
    protected virtual string MapUnaryOperator(string oper) => MongoDbXlat.UNARY_OPS[oper];

    /// <summary>
    /// Maps binary operator e.g. "and" => "$and"
    /// </summary>
    protected virtual string MapBinaryOperator(string oper, bool rhsNull) => MongoDbXlat.BINARY_OPS[oper];

    /// <summary>
    /// Constant set of types qualified as "primitive" which are inlined in SQL and do not generate parameters
    /// </summary>
    protected static readonly HashSet<Type> DEFAULT_PRIMITIVE_TYPES = new HashSet<Type> { typeof(byte), typeof(short), typeof(int), typeof(long) };

    /// <summary>
    /// Returns true if the ValueExpression represents a primitive
    /// </summary>
    protected virtual bool HandlePrimitiveValue(ValueExpression expr)
    {
      var tv = expr.Value.GetType();
      return DEFAULT_PRIMITIVE_TYPES.Contains(tv);
    }


    public override void Visit(ValueExpression value)
    {
      value.NonNull(nameof(value));

      //if (value.Value == null)
      //{
      //  m_Sql.Append(NullLiteral);
      //  return;
      //}

      //if (HandlePrimitiveValue(value))
      //{
      //  m_Sql.Append(value.Value.ToString());
      //  return;
      //}

      //var p = MakeAndAssignParameter(value);
      //m_Sql.Append(ParameterOpenSpan);
      //m_Sql.Append(p.ParameterName);
      //m_Sql.Append(ParameterCloseSpan);
      //m_Parameters.Add(p);
    }

    public override void Visit(ArrayValueExpression array)
    {
      array.NonNull(nameof(array));

    }

    public override void Visit(IdentifierExpression id)
    {
      id.NonNull(nameof(id)).Identifier.NonBlank(nameof(id.Identifier));

      //check that id is accepted
      var f = Translator.IdentifierFilter;
      if (f != null && !f(id)) throw new ASTException(StringConsts.AST_BAD_IDENTIFIER_ERROR.Args(id.Identifier));

    }

    public override void Visit(UnaryExpression unary)
    {
      unary.NonNull(nameof(unary));

      if (!unary.Operator.NonBlank(nameof(unary.Operator)).IsOneOf(Translator.UnaryOperators))
        throw new ASTException(StringConsts.AST_UNSUPPORTED_UNARY_OPERATOR_ERROR.Args(unary.Operator));

    }

    public override void Visit(BinaryExpression binary)
    {
      binary.NonNull(nameof(binary));

      if (!binary.Operator.NonBlank(nameof(binary.Operator)).IsOneOf(Translator.BinaryOperators))
        throw new ASTException(StringConsts.AST_UNSUPPORTED_BINARY_OPERATOR_ERROR.Args(binary.Operator));


      binary.LeftOperand.Accept(this);
      var left = m_Operand;

      var isNull = (binary.RightOperand == null || binary.RightOperand is ValueExpression ve && ve.Value == null);

      if (isNull)
        m_Operand = new BSONNullElement();
      else
        binary.RightOperand.Accept(this);

      var right = m_Operand;

      var op = MapBinaryOperator(binary.Operator, isNull);

      var elmBinary = new BSONArrayElement(op, new[] { left, right });

      if (m_Query.Count==0)
        m_Query.Set(elmBinary);
      else
        m_Operand = elmBinary;
    }
  }
}

