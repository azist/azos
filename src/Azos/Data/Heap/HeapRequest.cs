﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Provides a base for writing command requests sent into heap area (e.g. data queries).
  /// A Request is a data document having its type represent an action akin to "stored procedure"
  /// and its fields represent query/request parameters.
  /// A system may support a requests that take <see cref="AST.Expression"/> which
  /// is a similar concept to GraphQL where a query "shapes" data.
  /// Requests are eventually mapped to server-side processor/handler via [HeapProc] attribute.
  /// A concrete request type maps to one and only one handler per area, however
  /// the reverse is not true: the same server procedure/handler may handle different requests types
  /// mapped to the same proc name, thus allowing for server procedure/handler argument polymorphism
  /// </summary>
  [BixJsonHandler]
  public abstract class HeapRequest : AmorphousTypedDoc
  {
    /// <summary>
    /// Heap queries support amorphous data to implement gradual query schema changes
    /// </summary>
    public override bool AmorphousDataEnabled => true;

    protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
    {
      if (def?.Order == 0)
      {
        BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);
      }

      base.AddJsonSerializerField(def, options, jsonMap, name, value);
    }
  }

  /// <summary>
  /// HeapReuqest yielding `TResponse` as the resulting response
  /// </summary>
  public abstract class HeapRequest<TResponse> : HeapRequest {  }
}
