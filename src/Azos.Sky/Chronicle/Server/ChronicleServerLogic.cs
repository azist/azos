/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;
using Azos.Serialization.JSON;


namespace Azos.Sky.Chronicle.Server
{
  /// <summary>
  /// Provides server implementation for ILogChronicle and  IInstrumentationChronicle
  /// </summary>
  public sealed class ChronicleServerLogic : ModuleBase, ILogChronicleLogic, IInstrumentationChronicleLogic
  {
    public const string CONFIG_SERVICE_SECTION = "service";

    public ChronicleServerLogic(IApplication application) : base(application) { }
    public ChronicleServerLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
    //  DisposeAndNull(ref m_Server);
      base.Destructor();
    }

   // private LogStore m_Log;
   // private InstrumentationStore m_Instrumentation;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.INSTRUMENTATION_TOPIC;



    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      //DisposeAndNull(ref m_Server);
      if (node == null) return;

      //m_Server = FactoryUtils.MakeDirectedComponent<HttpService>(this,
      //                                                           node[CONFIG_SERVICE_SECTION],
      //                                                           typeof(HttpService),
      //                                                           new object[] { node });
    }

    protected override bool DoApplicationAfterInit()
    {
      //m_Server.NonNull("Service not configured");
      return base.DoApplicationAfterInit();
    }


    public async Task WriteAsync(LogBatch data)
    {
      throw new NotImplementedException();
    }

    public async Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter)
    {
      throw new NotImplementedException();
    }

    public async Task WriteAsync(InstrumentationBatch data)
    {
      throw new NotImplementedException();
    }

    public async Task<IEnumerable<JsonDataMap>> GetAsync(InstrumentationChronicleFilter filter)
    {
      throw new NotImplementedException();
    }

  }
}
