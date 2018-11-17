/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

using Azos.Log;

namespace Azos.Glue
{
  /// <summary>
  /// Provides base functionality for internal glue component implementations
  /// </summary>
  public abstract class GlueDaemon : DaemonWithInstrumentation<IGlueImplementation>
  {
    internal GlueDaemon(IGlueImplementation glue) : base(glue)
    {
    }

    protected GlueDaemon(IGlueImplementation glue, string name) : base(glue)
    {
      if (string.IsNullOrWhiteSpace(name))
          name = Guid.NewGuid().ToString();

      base.Name = name;
    }

    private bool m_InstrumentationEnabled;



    public IGlueImplementation Glue => ComponentDirector;

    /// <summary>
    /// Implements IInstrumentable
    /// </summary>
    [Config(Default=false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled;}
      set { m_InstrumentationEnabled = value;}
    }

    public override string ComponentLogTopic => CoreConsts.GLUE_TOPIC;



    public void WriteLog(
        LogSrc source, MessageType type, string msg, string from = null,
        Exception exception = null, string pars = null, Guid? relatedTo = null)
    {
      var pass = false;

      if (source== LogSrc.Server) pass= type >= Glue.ServerLogLevel;
      else
      if (source== LogSrc.Client) pass= type >= Glue.ClientLogLevel;
      else
        pass = (type >= Glue.ClientLogLevel) || (type >= Glue.ServerLogLevel);


      if ( pass )
        WriteLog(type, from, msg, exception, relatedTo, pars);
    }

  }
}
