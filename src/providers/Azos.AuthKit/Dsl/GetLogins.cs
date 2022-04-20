/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;
using Azos.Conf;
using Azos.Data;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Dsl
{
  /// <summary>
  /// Gets a list of logins for a user identified by its G_User
  /// </summary>
  public sealed class GetLogins : Step
  {
    public GetLogins(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order) { }

    [Config] public string G_User { get; set; }

    protected override async Task<string> DoRunAsync(JsonDataMap state)
    {
      var logic = LoadModule.Get<IIdpUserAdminLogic>();

      var gUser = Eval(G_User, state).AsGDID();

      var result = await logic.GetLoginsAsync(gUser).ConfigureAwait(false);
      Runner.SetResult(result);
      return null;
    }
  }

}
