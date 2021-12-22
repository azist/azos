using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Conf;
using Azos.Conf.Forest;
using Azos.Instrumentation;

namespace Azos.AuthKit.Server
{
  public sealed class IdpUserCoreDataSource : DaemonWithInstrumentation<IApplicationComponent>
  {
    public IdpUserCoreDataSource(IIdpUserCoreLogic logic) : base(logic) { }

    private bool m_InstrumentationEnabled;
    public override string ComponentLogTopic => CoreConsts.IDPUSER_TOPIC;

    /// <summary>
    /// Implements IInstrumentable
    /// </summary>
    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled; }
      set { m_InstrumentationEnabled = value; }
    }


#warning Need to provide IApplicationComponent overrides
    //TODO: add overrides for IApplicationComponent see ForestDataSource for example


  }
}
