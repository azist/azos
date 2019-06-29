/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Security
{
  /// <summary>
  /// Default implementation of ICryptoManager
  /// </summary>
  public abstract class DefaultCryptoManager : DaemonWithInstrumentation<ISecurityManagerImplementation>, ICryptoManagerImplementation
  {
    #region .ctor
    protected DefaultCryptoManager(ISecurityManagerImplementation director) : base(director)
    {
    }
    #endregion

    #region Fields

    private bool m_InstrumentationEnabled;
    private Registry<ICryptoMessageAlgorithmImplementation> m_MessageProtectionAlgorithms = new Registry<ICryptoMessageAlgorithmImplementation>();

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

    public IRegistry<ICryptoMessageAlgorithm> MessageProtectionAlgorithms => throw new NotImplementedException();

    #endregion

    #region Public


    #endregion

    #region Protected

    #endregion
  }

}
