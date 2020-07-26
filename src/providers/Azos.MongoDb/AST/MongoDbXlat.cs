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
      var root = expression.Accept(result) as BSONElement;
      result.Query.Set(root);
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


    public override object Visit(ValueExpression value)
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

      return null;
    }

    public override object Visit(ArrayValueExpression array)
    {
      array.NonNull(nameof(array));
      return null;
    }

    public override object Visit(IdentifierExpression id)
    {
      id.NonNull(nameof(id)).Identifier.NonBlank(nameof(id.Identifier));

      //check that id is accepted
      var f = Translator.IdentifierFilter;
      if (f != null && !f(id)) throw new ASTException(StringConsts.AST_BAD_IDENTIFIER_ERROR.Args(id.Identifier));
      return null;
    }

    public override object Visit(UnaryExpression unary)
    {
      unary.NonNull(nameof(unary));

      if (!unary.Operator.NonBlank(nameof(unary.Operator)).IsOneOf(Translator.UnaryOperators))
        throw new ASTException(StringConsts.AST_UNSUPPORTED_UNARY_OPERATOR_ERROR.Args(unary.Operator));
      return null;
    }

    public override object Visit(BinaryExpression binary)
    {
      binary.NonNull(nameof(binary));

      if (!binary.Operator.NonBlank(nameof(binary.Operator)).IsOneOf(Translator.BinaryOperators))
        throw new ASTException(StringConsts.AST_UNSUPPORTED_BINARY_OPERATOR_ERROR.Args(binary.Operator));


      binary.LeftOperand.Accept(this);
      var left = binary.LeftOperand.Accept(this) as BSONElement;

      var isNull = (binary.RightOperand == null || binary.RightOperand is ValueExpression ve && ve.Value == null);

      BSONElement right;
      if (isNull)
        right = new BSONNullElement();
      else
        right = binary.RightOperand.Accept(this) as BSONElement;

      var op = MapBinaryOperator(binary.Operator, isNull);

      var elmBinary = new BSONArrayElement(op, new[] { left, right });

      return elmBinary;
    }
  }
}

