/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Conf;
using Azos.Data;

namespace Azos.Scripting.Expressions
{
  /// <summary>
  /// Implements a document field by name accessor operator
  /// </summary>
  public abstract class DocField<TContext, TDocument> : UnaryOperator<TContext, object, TDocument> where TDocument : IDataDoc
  {
    public DocField(string fieldName) => Field = fieldName;

    [Config] public string Field{  get; set; }

    public sealed override object Evaluate(TContext context)
    {
      var operand = Operand.NonNull(nameof(Operand));
      var fn = Field.NonBlank(nameof(Field));

      var doc = operand.Evaluate(context);
      return doc[fn];
    }
  }

  /// <summary>
  /// Implements a document field by name accessor from TDocument context
  /// </summary>
  public abstract class DocContextField<TDocument> : Expression<TDocument, object> where TDocument : IDataDoc
  {
    public DocContextField(string fieldName) => Field = fieldName;

    [Config] public string Field { get; set; }

    public sealed override object Evaluate(TDocument context)
    {
      var fn = Field.NonBlank(nameof(Field));

      return context?[fn];
    }
  }

}
