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

namespace Azos.Security.Dsl
{
  /// <summary>
  /// Returns password strength 0-100%
  /// </summary>
  public sealed class CalculatePasswordScore : Step
  {
    public CalculatePasswordScore(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order){ }

    [Config]
    public string PlainPassword { get; set; }

    [Config]
    public string PasswordFamily { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var family = Eval(PasswordFamily, state).AsEnum(Security.PasswordFamily.Text);
      var pwd = Eval(PlainPassword, state);
      var spwd = IDPasswordCredentials.PlainPasswordToSecureBuffer(pwd);

      var result = App.SecurityManager.PasswordManager.CalculateStrenghtPercent(family, spwd);
      Runner.SetResult(result);

      return Task.FromResult<string>(null);
    }
  }


  /// <summary>
  /// Protects plain password into protected vector placed in Runner.Result
  /// </summary>
  public sealed class ProtectPassword : Step
  {
    public ProtectPassword(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order)
    {
    }

    [Config]
    public string PlainPassword { get; set; }

    [Config]
    public string Algorithm { get; set; }

    [Config]
    public string PasswordFamily { get; set; }

    [Config]
    public string IsUnicodeResult {  get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var algName = Eval(Algorithm, state).Default("KDF");
      var alg = App.SecurityManager.PasswordManager.Algorithms[algName];
      alg.NonNull("existing algorithm `{0}`".Args(algName));

      var family = Eval(PasswordFamily, state).AsEnum(Security.PasswordFamily.Text);
      var pwd = Eval(PlainPassword, state);
      var spwd = IDPasswordCredentials.PlainPasswordToSecureBuffer(pwd);

      var hashed = alg.ComputeHash(family, spwd);

      var isunicode = Eval(IsUnicodeResult, state).AsBool(true);

      var resultJson = hashed.ToJson(isunicode ? JsonWritingOptions.Compact : JsonWritingOptions.CompactASCII);

      Runner.SetResult(resultJson);

      return Task.FromResult<string>(null);
    }
  }
}
