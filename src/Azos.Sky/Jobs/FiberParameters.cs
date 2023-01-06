/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Sky.Jobs
{
  [BixJsonHandler(ThrowOnUnresolvedType = true)]
  public abstract class JobParameterBase : TransientModel
  {
    /// <summary>
    /// Adds type code using BIX, so the system will add Guids from <see cref="Azos.Serialization.Bix.BixAttribute"/>
    /// which are used for both binary and json polymorphism
    /// </summary>
    protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
    {
      if (def?.Order == 0)
      {
        BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);
      }

      base.AddJsonSerializerField(def, options, jsonMap, name, value);
    }
  }

  [Schema(Description = "Immutable bag of values supplied at the job creation")]
  public abstract class FiberParameters : JobParameterBase
  {
  }

  [Schema(Description = "Immutable bag of values created as the result of job execution")]
  public abstract class FiberResult : JobParameterBase
  {
  }

}
