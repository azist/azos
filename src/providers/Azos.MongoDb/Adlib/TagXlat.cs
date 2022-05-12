/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

using Azos.Conf;
using QDoc = Azos.Data.Access.MongoDb.Connector.Query;
using Azos.Serialization.BSON;
using Azos.Data.Access.MongoDb;
using Azos.Serialization.JSON;
using Azos.Data.AST;

namespace Azos.Data.Adlib.Server
{
  public class TagXlat : Xlat<TagXlatContext>
  {
    public TagXlat(string name = null) : base(name) { }
    public TagXlat(IConfigSectionNode conf) : base(conf) { }

    [Config] private bool m_IsCaseSensitive;

    public override bool IsCaseSensitive => m_IsCaseSensitive;

    /// <summary>
    /// Assigns an optional functor which gets called during identifier validation,
    /// this is handy in many business-specific cases to
    /// limit the column names without having to override this class
    /// </summary>
    public Func<IdentifierExpression, bool> IdentifierFilter { get; set; }


    /// <summary>
    /// Translates root AST expression for Mongo Db
    /// </summary>
    public override TagXlatContext TranslateInContext(Expression expression)
    {
      var result = new TagXlatContext(this);
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

       {"not", "$not"},//is a binary logical disjunction {field: {$not: { <operator-expression> }}}
       {"in", "$in"}, //{ field: { $in: [<value1>, <value2>, ... <valueN> ] } }
       {"nin", "$nin"},//{ field: { $nin: [ <value1>, <value2> ... <valueN> ]} }
       {"exists", "$exists"}//{ field: { $exists: <boolean> } }
    };


  }

  public class TagXlatContext : XlatContext<TagXlat>
  {
    public TagXlatContext(TagXlat xlat) : base(xlat)
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
    protected virtual string MapBinaryOperator(string oper) => MongoDbXlat.BINARY_OPS[oper];


    public override object Visit(ValueExpression value)
    {
      value.NonNull(nameof(value));
      return value.Value;
    }

    public override object Visit(ArrayValueExpression array)
    {
      return array.NonNull(nameof(array))
                  .ArrayValue.NonNull(nameof(array))
                  .Select(v => v.Accept(this) as BSONElement);
    }

    public override object Visit(IdentifierExpression id)
    {
      id.NonNull(nameof(id)).Identifier.NonBlank(nameof(id.Identifier));

      //check that id is accepted
      var f = Translator.IdentifierFilter;
      if (f != null && !f(id)) throw new ASTException(StringConsts.AST_BAD_IDENTIFIER_ERROR.Args(id.Identifier));
      return id.Identifier;
    }

    public override object Visit(UnaryExpression unary)
    {
      unary.NonNull(nameof(unary));

      if (!unary.Operator.NonBlank(nameof(unary.Operator)).IsOneOf(Translator.UnaryOperators))
        throw new ASTException(StringConsts.AST_UNSUPPORTED_UNARY_OPERATOR_ERROR.Args(unary.Operator));

      //None supported at the time

      return null;
    }

    public override object Visit(BinaryExpression binary)
    {
      binary.NonNull(nameof(binary));

      if (!binary.Operator.NonBlank(nameof(binary.Operator)).IsOneOf(Translator.BinaryOperators))
        throw new ASTException(StringConsts.AST_UNSUPPORTED_BINARY_OPERATOR_ERROR.Args(binary.Operator));

      var op = MapBinaryOperator(binary.Operator);

      var left = binary.LeftOperand
                       .NonNull(nameof(binary.LeftOperand))
                       .Accept(this);

      var isNull = (binary.RightOperand == null || binary.RightOperand is ValueExpression ve && ve.Value == null);

      if (left is string identifier) //{identifier: {$lt: value} }
      {
        if (isNull)
          return new BSONDocumentElement(identifier, new BSONDocument().Set(new BSONNullElement(op)));

        var value = binary.RightOperand.Accept(this);

        var right = new BSONDocument();
        if (value is IEnumerable vie)
        {
          //todo: need to handle array
          //   var arr = vie.Cast<object>().Select( e => new BSONElement(e));
          //   right.Set(new BSONArrayElement(op, arr));
        }
        else
        {
          try
          {
            right.Add(op, value, false, true);
          }
          catch
          {
            throwSyntaxErrorNear(binary, "unsupported RightOperand value `{0}`".Args(value == null ? "<null>" : value.GetType().Name));
          }
        }
        return new BSONDocumentElement(identifier, right);
      }

      if (left is BSONElement complex)
      {
        BSONElement right = null;
        if (isNull)
        {
          throwSyntaxErrorNear(binary, "unexpected null in compound statement");
        }
        else
        {
          right = binary.RightOperand.Accept(this) as BSONElement;
          if (right == null) throwSyntaxErrorNear(binary, "unsupported RightOperand value");
        }

        return new BSONArrayElement(op, new[] { complex, right });
      }

      return throwSyntaxErrorNear(binary, "unsupported construct");
    }

    private object throwSyntaxErrorNear(Expression expr, string cause)
      => throw new ASTException(StringConsts.AST_BAD_SYNTAX_ERROR.Args(cause, expr.ToJson(JsonWritingOptions.CompactRowsAsMap).TakeFirstChars(48)));

  }
}

