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
  /// Saves user login entity
  /// </summary>
  public class SaveLogin : Step
  {
    public const string CONFIG_PASSWORD_SECTION = "password-protection";

    public SaveLogin(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order)
    {
      var nSource = cfg[CONFIG_PASSWORD_SECTION];
      if (nSource.Exists)
      {
        m_PasswordSubroutine = FactoryUtils.Make<StepRunner>(nSource, typeof(StepRunner), new object[] { runner.App, nSource, runner.GlobalState });
      }
    }

    protected readonly StepRunner m_PasswordSubroutine;

    [Config] public string LoginEntity { get; set; }

    protected override async Task<string> DoRunAsync(JsonDataMap state)
    {
      var logic = LoadModule.Get<IIdpUserAdminLogic>();

      var data = GetLoginEntity(state);

      await PrepareEntityPassword(data, state).ConfigureAwait(false);

      var result = await logic.SaveLoginAsync(data).ConfigureAwait(false);
      Runner.SetResult(result);

      return null;
    }

    /// <summary>
    /// Override to obfuscate password, the default implementation is based on running a sub routine <see cref="CONFIG_PASSWORD_SECTION"/> section
    /// </summary>
    protected virtual async Task PrepareEntityPassword(LoginEntity entity, JsonDataMap state)
    {
      if (m_PasswordSubroutine != null)
      {
        m_PasswordSubroutine.SetResult(Runner.Result);
        var local = await m_PasswordSubroutine.RunAsync().ConfigureAwait(false);
        state.Append(local, deep: true);

        var secPwd = m_PasswordSubroutine.Result.AsString();
        if (secPwd.IsNotNullOrWhiteSpace())
        {
          entity.Password = secPwd;
        }
      }

    }

    protected virtual LoginEntity GetLoginEntity(JsonDataMap state)
    {
      var json = Eval(LoginEntity, state);
      var result = json.IsNullOrWhiteSpace() ? new LoginEntity() : JsonReader.ToDoc<LoginEntity>(json);
      return result;
    }
  }

}
