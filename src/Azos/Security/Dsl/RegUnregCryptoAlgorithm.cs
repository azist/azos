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
  /// Registers the specified cryptographic algorithm with app crypto manager
  /// </summary>
  public sealed class RegisterCryptoAlgorithm : Step
  {
    public RegisterCryptoAlgorithm(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order)
    {
    }

    [Config]
    public IConfigSectionNode Algorithm { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var cfg = StepRunnerVarResolver.WrapConfigSnippet(Algorithm.NonEmpty(nameof(Algorithm)), Runner, state);
      var crypto = App.SecurityManager.Cryptography as ICryptoManagerImplementation;
      var algo = FactoryUtils.MakeDirectedComponent<ICryptoMessageAlgorithmImplementation>(crypto, cfg, extraArgs: new[] { cfg });
      crypto.RegisterAlgorithm(algo).IsTrue("Unique algo name");
      return Task.FromResult<string>(null);
    }
  }

  /// <summary>
  /// Unregisters the specified cryptographic algorithm by name
  /// </summary>
  public sealed class UnregisterCryptoAlgorithm : Step
  {
    public UnregisterCryptoAlgorithm(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order)
    {
    }

    [Config]
    public string AlgorithmName { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var aname = Eval(AlgorithmName, state);
      var crypto = App.SecurityManager.Cryptography as ICryptoManagerImplementation;
      crypto.UnregisterAlgorithm(aname).IsTrue("Registered algorithm");
      return Task.FromResult<string>(null);
    }
  }
}
