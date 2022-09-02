/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Apps.Hosting.Skyod.Adapters
{
  [Serialization.Bix.BixJsonHandler(ThrowOnUnresolvedType = true)]
  public abstract class AdapterDoc : AmorphousTypedDoc
  {
    [Field(Required = true)] public Guid Id { get; protected set; }

    public override bool AmorphousDataEnabled => true;

    protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
    {
      if (def.Order==0) Serialization.Bix.BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);
      base.AddJsonSerializerField(def, options, jsonMap, name, value);
    }
  }

  [Schema(Description = "Data contract for sending requests to Adapters")]
  public abstract class AdapterRequest : AdapterDoc
  {
    public static TRequest MakeNew<TRequest>() where TRequest : AdapterRequest , new()
    => new TRequest{ Id = Guid.NewGuid() };
  }

  [Schema(Description = "Data contract for adapter responses sent after processing AdapterRequests")]
  public abstract class AdapterResponse : AdapterDoc
  {
  }


  public abstract class AdapterBase : ApplicationComponent<SetComponent>
  {

    protected AdapterBase(SetComponent director) : base(director)
    {
    }

    public override string ComponentLogTopic => Sky.SysConsts.LOG_TOPIC_SKYOD;


    public abstract IEnumerable<Type> SupportedRequestTypes { get; }

    /// <summary>
    /// Executes adapter request by providing a response or null
    /// if such request has ALREADY been executed before as determined by its Guid id.
    /// This is needed to debounce the possibly chatty/recursive traffic in a distributed cloud system
    /// </summary>
    public async Task<AdapterResponse> ExecRequestAsync(AdapterRequest request)
    {
      var tc = request.NonNull(nameof(request)).GetType();
      SupportedRequestTypes.Any(one => one == tc).IsTrue("Supported request type");

      //check if command was already applied
      if (!ComponentDirector.TryRegisterNewAdapterRequest(request.Id)) return null;
      var result = await DoExecRequestAsync(request).ConfigureAwait(false);
      return result;
    }

    protected abstract Task<AdapterResponse> DoExecRequestAsync(AdapterRequest request);
  }


}
