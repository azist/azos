/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Abstract base declaration of event document
  /// </summary>
  public abstract class EventDocument : AmorphousTypedDoc
  {
    /// <summary>
    /// EventDocuments enable amorphous data by default
    /// </summary>
    public override bool AmorphousDataEnabled => true;

    /// <summary>
    /// Specifies parameters how this event should be routed for processing: what namespace, queue and partition should be used.
    /// The function is called in the specified target context which may be passed from caller.
    /// This is an instance property because its value may depend on event state (other doc fields)
    /// </summary>
    public abstract Route GetEventRoute(string target);

    /// <summary>
    /// Returns headers which should be included in enqueued event.
    /// The function is called in the specified target context which may be passed from caller.
    /// This is an instance property because its value may depend on event state (other doc fields)
    /// </summary>
    public virtual string GetEventHeaders(string target)
    {
      return null;
    }

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
}
