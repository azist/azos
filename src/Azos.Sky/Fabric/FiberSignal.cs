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

namespace Azos.Sky.Fabric
{
  [BixJsonHandler(ThrowOnUnresolvedType = true)]
  public abstract class FiberSignalBase : TransientModel
  {

    [Field(Required = true,
           Description = "Unique JobId obtained from a call to `AllocateJobId()`. This job must not have started yet")]
    public FiberId FiberId { get; set; }

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

  /// <summary>
  /// A command object sent to fibers to perform some action immediately.
  /// Depending on a fiber state, it can process the signal and generate <see cref="FiberSignalResult"/>
  /// or not process it if the fiber is not found, in suspended state or does not process this type of signal
  /// </summary>
  public abstract class FiberSignal : FiberSignalBase
  {
  }

  public abstract class FiberSignalResult : FiberSignalBase
  {
  }

}
