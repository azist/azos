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
  /// Saves user entity
  /// </summary>
  public class SaveUser : Step
  {
    public SaveUser(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order) { }

    [Config] public string UserEntity { get; set; }

    protected override async Task<string> DoRunAsync(JsonDataMap state)
    {
      var logic = LoadModule.Get<IIdpUserAdminLogic>();

      var data = GetUserEntity(state);

      var result = await logic.SaveUserAsync(data).ConfigureAwait(false);
      Runner.SetResult(result);

      return null;
    }

    protected virtual UserEntity GetUserEntity(JsonDataMap state)
    {
      var json = Eval(UserEntity, state);
      var result = json.IsNullOrWhiteSpace() ? new UserEntity() : JsonReader.ToDoc<UserEntity>(json);
      return result;
    }
  }

}
