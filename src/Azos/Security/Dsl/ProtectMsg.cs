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
  /// Protects/Unprotects message using cryptographic APIs
  /// </summary>
  public sealed class ProtectMsg : Step
  {
    public ProtectMsg(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order)
    {
    }

    [Config]
    public string Data { get; set; }

    [Config]
    public string Algorithm { get; set; }

    [Config]
    public bool Protect { get ; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var algName = Eval(Algorithm, state);
      var alg = algName.IsNullOrWhiteSpace() ? CryptoMessageUtils.GetDefaultPublicCipher(App.SecurityManager)
                                             : App.SecurityManager.Cryptography.MessageProtectionAlgorithms[algName];
      alg.NonNull("existing algorithm `{0}`".Args(algName));

      var msg = Eval(Data, state);

      var result = Protect ? (object)CryptoMessageUtils.ProtectAsString(alg, msg)
                           : CryptoMessageUtils.UnprotectObject(alg, msg);
      Runner.SetResult(result);

      return Task.FromResult<string>(null);
    }
  }
}
