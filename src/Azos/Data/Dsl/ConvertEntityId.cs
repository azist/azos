/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

using Azos.Conf;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.Data.Dsl
{
  /// <summary>
  /// Converts an EntityId value represented in canonical form into a parsable tuple of (sys, tp, sch, adr).
  /// If an input is a tuple then converts it back to EntityId.
  /// </summary>
  public sealed class ConvertEntityId : Step
  {
    public ConvertEntityId(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order)
    {
    }

    [Config] public string From { get; set; }
    [Config] public string Into { get; set;}

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var from = Eval(From, state);
      var into = Eval(Into, state);

      object val = null;
      if (from.IsNotNullOrWhiteSpace())
      {
        if (EntityId.TryParse(from, out var eid))
        {
          val = new {sys = eid.System, tp = eid.Type, sch = eid.Schema, adr = eid.Address}.ToJson(JsonWritingOptions.CompactRowsAsMap);
        }
        else
        {
          var map = JsonReader.DeserializeDataObject(from) as JsonDataMap;
          map.NonNull("Json representation of EntityId");
          val = new EntityId(map["sys"].AsAtom(),
                             map["tp"].AsAtom(),
                             map["sch"].AsAtom(),
                             map["adr"].AsString());
        }
      }

      if (into.IsNotNullOrWhiteSpace())
      {
        state[into] = val;
      }

      return Task.FromResult<string>(null);
    }
  }
}
