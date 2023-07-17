/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;
using System.Security.Cryptography;
using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Security
{
  /// <summary>
  /// Default implementation of ICryptoManager
  /// </summary>
  public class DefaultCryptoManager : DaemonWithInstrumentation<ISecurityManagerImplementation>, ICryptoManagerImplementation
  {
    public const string CONFIG_ALGORITHM_SECTION = "algorithm";

    #region .ctor
    public DefaultCryptoManager(ISecurityManagerImplementation director) : base(director)
    {
      m_CryptoRnd = new RNGCryptoServiceProvider();
      m_MessageProtectionAlgorithms = new Registry<ICryptoMessageAlgorithmImplementation>();
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_CryptoRnd);
      cleanupAlgorithms();
    }

    private void cleanupAlgorithms()
    {
      foreach(var a in m_MessageProtectionAlgorithms) a.Dispose();
      m_MessageProtectionAlgorithms.Clear();
    }
    #endregion

    #region Fields

    private bool m_InstrumentationEnabled;
    private RNGCryptoServiceProvider m_CryptoRnd;
    private Registry<ICryptoMessageAlgorithmImplementation> m_MessageProtectionAlgorithms;

    #endregion

    #region Properties
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_PAY)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled; }
      set { m_InstrumentationEnabled = value; }
    }

    public IRegistry<ICryptoMessageAlgorithm> MessageProtectionAlgorithms => m_MessageProtectionAlgorithms;

    #endregion

    #region Public

    public byte[] GenerateRandomBytes(int count) => GenerateRandomBytes(count, count);
    public byte[] GenerateRandomBytes(int minCount, int maxCount)
    {
      //Two independent RNGs are used to avoid library implementation errors affecting entropy distribution,
      //so shall an error happen in one (highly unlikely), the other one would still ensure cryptography white noise spectrum distribution
      //the Platform.RandomGenerator is periodically fed external entropy from system and network stack
      var rnd = Platform.RandomGenerator.Instance.NextRandomBytes(minCount, maxCount);
      var rnd2 = new byte[rnd.Length];
      m_CryptoRnd.GetBytes(rnd2);
      for (var i = 1; i < rnd.Length; i++) rnd[i] = (byte)(rnd[i] ^ rnd2[i]);//both Random streams are combined using XOR
      return rnd;
    }

    public bool RegisterAlgorithm(ICryptoMessageAlgorithmImplementation algorithm)
    {
      (algorithm.NonNull(nameof(algorithm)).ComponentDirector == this).IsTrue("Algorithm directed by this manager");
      return m_MessageProtectionAlgorithms.Register(algorithm);
    }

    public bool UnregisterAlgorithm(string name)
      => m_MessageProtectionAlgorithms.Unregister(name);

    public bool UnregisterAlgorithm(ICryptoMessageAlgorithmImplementation algorithm)
      => m_MessageProtectionAlgorithms.Unregister(algorithm);

    #endregion

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      cleanupAlgorithms();

      using(var scope = new SecurityFlowScope(TheSafe.SAFE_ACCESS_FLAG))
      {
        foreach(var n in node.Children.Where(c => c.IsSameName(CONFIG_ALGORITHM_SECTION)))
        {
          var algo = FactoryUtils.MakeDirectedComponent<ICryptoMessageAlgorithmImplementation>(this, n, extraArgs: new[]{n});
          if (!m_MessageProtectionAlgorithms.Register(algo))
           throw new SecurityException("Algorithm `{0}` is already registered".Args(algo.Name));
        }
      }
    }
    #endregion
  }
}
