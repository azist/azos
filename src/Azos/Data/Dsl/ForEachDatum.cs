/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

using Azos.Collections;
using Azos.Conf;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.Data.Dsl
{
  /// <summary>
  /// Iterates through an enumerable object in named data source
  /// </summary>
  public sealed class ForEachDatum : Step
  {
    public const string CONFIG_BODY_SECTION = "body";

    public ForEachDatum(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx)
    {
      var nSource = cfg[CONFIG_BODY_SECTION].NonEmpty();
      m_Body = FactoryUtils.Make<StepRunner>(nSource, typeof(StepRunner), new object[] { runner.App, nSource, runner.GlobalState });
    }

    private StepRunner m_Body;

    [Config]
    public string From { get; set;}

    [Config]
    public string Into { get; set; }

    protected override async Task<string> DoRunAsync(JsonDataMap state)
    {
      var dsn = Eval(From, state);
      var source = DataLoader.Get(dsn);
      var enumerable = source.ObjectData as IEnumerable;

      enumerable.NonNull("Enumerable data");
      Into.NonBlank(nameof(Into));

      var bodyArgs = new JsonDataMap(true);
      foreach(var o in enumerable)
      {
        bodyArgs.Clear();
        bodyArgs[Into] = o;
        await m_Body.RunAsync(state: bodyArgs).ConfigureAwait(false);
      }
      return null;
    }
  }

}
