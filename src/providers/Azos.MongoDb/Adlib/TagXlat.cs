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

    public override TagXlatContext TranslateInContext(Expression expression)
    {
      var result = new TagXlatContext(this);
      var root = expression.Accept(result) as BSONElement;
      result.Query.Set(root);
      return result;
    }

    public override IEnumerable<string> UnaryOperators => UNARY_OPS.Keys;
    public override IEnumerable<string> BinaryOperators => BINARY_OPS.Keys;


    public static readonly Dictionary<string, string> UNARY_OPS = new Dictionary<string, string>(StringComparer.Ordinal)
    {
    };

    public static readonly Dictionary<string, string> BINARY_OPS = new Dictionary<string, string>(StringComparer.Ordinal)
    {
       {"=", "$eq"},   //{ Name: { $eq: 'Jack' } }
       {"==", "$eq"},

       {"!=", "$ne"},  //{ Age: { $ne: 10 } }
       {"<>", "$ne"},

       {"<", "$lt"},  //{ Age: { $lt: 10 } }
       {">", "$gt"},

       {"<=", "$lte"}, //{ Age: { $lte: 10 } }
       {">=", "$gte"},

       {"and", "$and"}, // { $and: [{ Name: { $ne: 'Jack' } }, { Name: { $ne: 'Jim' } }] }
       {"&", "$and"},
       {"&&", "$and"},

       {"or", "$or"},  // { $or: [{ Name: 'Jack' }, { Age: 10 }] }
       {"|", "$or"},
       {"||", "$or"},

       {"not", "$not"}//is a binary logical disjunction {field: {$not: { <operator-expression> }}}
    };


  }

  public class TagXlatContext : XlatContext<TagXlat>
  {
    public TagXlatContext(TagXlat xlat) : base(xlat)
    {
      m_Query = new QDoc();
    }

    protected QDoc m_Query;
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

      var v = value.Value;
      var t = v.GetType();

      var supported = t == typeof(string) ||
                      t == typeof(ulong)  ||  t == typeof(long) ||
                      t == typeof(int)    ||  t == typeof(uint);
      supported.IsTrue("ValueExpression of supported types: string|long|ulong|int|uint");
      return v;
    }

    public override object Visit(ArrayValueExpression array)
    {
      throw new ASTException(StringConsts.AST_UNSUPPORTED_ERROR.Args(nameof(ArrayValueExpression)));
    }

    public override object Visit(IdentifierExpression id)
    {
      id.NonNull(nameof(id))
        .Identifier
        .NonBlank(nameof(id.Identifier));

      //check that id is accepted: encodable as an Atom
      Atom.TryEncode(id.Identifier, out var atom).IsTrue("Atom identifier");
      return atom;
    }

    public override object Visit(UnaryExpression unary)
    {
      unary.NonNull(nameof(unary));

      if (!unary.Operator.NonBlank(nameof(unary.Operator)).IsOneOf(Translator.UnaryOperators))
        throw new ASTException(StringConsts.AST_UNSUPPORTED_UNARY_OPERATOR_ERROR.Args(unary.Operator));

      //None supported at the time

      return null;
    }

    //{p: {$eq: #color}, v: {$eq: 'green'}},
    private BSONDocument propPair(Atom prop, string op, object v)
    {
      var doc = new BSONDocument();
      //1
      doc.Set(new BSONDocumentElement("p", new BSONDocument().Set(new BSONInt64Element("$eq", (long)prop.ID))));

          BSONElement ev;
          if (v == null) ev = new BSONNullElement(op);
          else if (v is string sv) ev = new BSONStringElement(op, sv);
          else
          {
            try
            {
              var ul = Convert.ToUInt64(v);
              ev = new BSONInt64Element(op, (long)ul);
            }
            catch
            {
              throw new ASTException(StringConsts.AST_BAD_SYNTAX_ERROR.Args("value must be string or ulong-convertible", "{0} {1} {2}".Args(prop, op, v)).TakeFirstChars(48));
            }
          }

      //2
      doc.Set(new BSONDocumentElement("v", new BSONDocument().Set(ev)));

      return doc;
    }


    public override object Visit(BinaryExpression binary)
    {
      binary.NonNull(nameof(binary));

      if (!binary.Operator.NonBlank(nameof(binary.Operator)).IsOneOf(Translator.BinaryOperators))
        throw new ASTException(StringConsts.AST_UNSUPPORTED_BINARY_OPERATOR_ERROR.Args(binary.Operator));

      var op = MapBinaryOperator(binary.Operator); // '!=' -> '$ne'

      var left = binary.LeftOperand
                       .NonNull(nameof(binary.LeftOperand))
                       .Accept(this);

      var isNull = (binary.RightOperand == null || binary.RightOperand is ValueExpression ve && ve.Value == null);

//"color" == 'green' and "age" < 100
//==================================
//{$and: [
//  {p: {$eq: #color}, v: {$eq: 'green'}},
//  {p: {$eq: #age"}, v: {$lt: 100}}
//]}

      if (left is Atom identifier) //{identifier: {$lt: value} }
      {
        if (isNull) return propPair(identifier, op, null);
        var value = binary.RightOperand.Accept(this);
        return propPair(identifier, op, value);
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

